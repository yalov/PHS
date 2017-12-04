using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Hire
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class Orbital : MonoBehaviour
    {
        static double lastDayChecked = 0; // Will be the last second of the last day which was checked
        public static int HoursPerDay { get { return GameSettings.KERBIN_TIME ? 6 : 24; } }
        public static int SecsPerDay { get { return 3600 * HoursPerDay; } }
        static CelestialBody currentEclipsingbody = null;

        void Start()
        {
            DontDestroyOnLoad(this);
        }

        public static CelestialBody eclipseToday
        {
            get
            {
                // No need to check if the current day was already checked, so just return the last body

                double currentDay = Math.Floor(Planetarium.GetUniversalTime() / SecsPerDay) * SecsPerDay;
                if (lastDayChecked == currentDay)
                    return currentEclipsingbody;
                lastDayChecked = currentDay;

                double endOfDay = currentDay + SecsPerDay;


                // For now, check at 1 minute intervals
                Debug.Log("currentDay: " + currentDay + ",  lastDayChecked: " + lastDayChecked);
                for (double checkTime = currentDay; checkTime < endOfDay; checkTime += 60f)
                {
                    // The chances of two bodies starting an eclipse at the same time are minimal
                    var hcp = HasClearPath(checkTime, Planetarium.fetch.Home, Planetarium.fetch.Sun, Relative.closest);
                    if (hcp != null)
                    {
                        currentEclipsingbody = hcp;
                        return hcp;
                    }
                }
                currentEclipsingbody = null;
                return null;
            }
        }

        /// <summary>
        /// Return the Vector3d of the actual position of the body, converted to Scaled Space
        /// </summary>
        /// <param name="body"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        static Vector3d   GetPositionAtUT(CelestialBody body, double time)
        {
            return Vector3.Scale(ScaledSpace.LocalToScaledSpace(body.getPositionAtUT(time)), new Vector3d(ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor));
        }

        public enum Relative { closest, furthest, any };

        /// <summary>
        /// Finds if the path between two bodies is obstructed, and if so, by what body.  Can also return either the closest body or furthest body
        /// </summary>
        /// <param name="time"></param>
        /// <param name="bOrigin"></param>
        /// <param name="bDestination"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        public static CelestialBody HasClearPath(double time, CelestialBody bOrigin, CelestialBody bDestination, Relative relative, bool eclipseReq = true)
        {
#if DEBUG
            int day = (int)(time / SecsPerDay);
            int hour = (int)((time - (day * SecsPerDay)) / 3600);
            int minute = (int)((time - (day * SecsPerDay) - hour * 3600) / 60);
            string time_str = (day + 1) + " day, " + hour.ToString("D2") + ":" + minute.ToString("D2");
#endif

            CelestialBody eclipser = null;
            double eclipserDistance = 0;

            // Cribbed with love from RemoteTech.  I have no head for vectors.

            Vector3d opos = GetPositionAtUT(bOrigin, time);
            Vector3d dpos = GetPositionAtUT(bDestination,time);
            foreach (CelestialBody rock in FlightGlobals.Bodies)
            {
                if (rock == bOrigin || rock == bDestination)
                    continue;


                Vector3d rpos = GetPositionAtUT(rock, time) + opos; //rock.getPositionAtUT(time) + opos;

                Vector3d rockFromOrigin = rpos - opos;
                Vector3d destFromOrigin = dpos - opos;

                // I believe what math below is correct. Problem is in getting rpos, opos, dpos above and therefore bodyFromOrigin and destFromOrigin.
                // It's something about global/local coordinates and centers of coordinate grids.
                //
                // RO, DO correspond to distance Mun-Kerbin and Sun-Kerbin, 
                // therefore they should be constant all the time from 0:00 to 5:59 (for Mun) Otherwise code is wrong.
                // ratio should be ~ 1 for Mun from 0:00 to 5:59.
                //
                // 
                // It's became better with     rpos = GetPositionAtUT(rock, time) + opos     but not perfect.   
                // For test run
                //         0.8664 < ratio < 0.8719,        // I think it's really >0.95
                //         12525k < RO < 12600k            // RO == AltitudeOfMun + Kerbin_radius = 11400,000 + 600,000 = 12,000,000
                // 13,599,840,095 < DO < 13,599,840,101    // DO == altitudeOfKerbin + Sun_radius = 13,338,240,256 + 261,600,000 = 13,599,840,256. Complete.
                //
                // eclipse vs transit is about ratio

                double RO = rockFromOrigin.magnitude;
                double DO = destFromOrigin.magnitude;
                double orad = bOrigin.Radius;
                double drad = bDestination.Radius;
                double rrad = rock.Radius;
                
                double drad_proj = drad * ((RO - orad) / (DO - orad)); // from surface of origin
                double ratio = rrad / drad_proj;
                const double RATIO_EDGE = 0.8; // should be 0.95 when math is correct. Now it is 0.8.

                // is the body big enough for possible eclipse or it is possible transit
                bool big_enough_for_eclipse = (ratio > RATIO_EDGE);

                // ∠ Destination-Origin-Rock should be <180° for eclipse/transit
                if (Vector3d.Dot(rockFromOrigin, destFromOrigin) <= 0)
                {
#if DEBUG
                    if (rock.name == "Mun" || (hour == 0 && minute == 0))
                        Debug.Log("time: " + time_str + "  " + rock.name + " ratio="+ ratio.ToString("F4") + " >180deg");
#endif
                    continue;
                }

                // body should be closer then destination. BO>DO = no eclipse/transit 
                if (RO >= DO)
                {
#if DEBUG
                    if (rock.name == "Mun" || (hour == 0 && minute == 0))
                        Debug.Log("time: " + time_str + "  " + rock.name + " ratio=" + ratio.ToString("F4") + " rock futher then sun");
#endif
                    continue;
                }                

                Vector3d destFromOriginNorm = destFromOrigin.normalized;
                Vector3d lateralOffset = rockFromOrigin - Vector3d.Dot(rockFromOrigin, destFromOriginNorm) * destFromOriginNorm;
                double limbo = rock.Radius * 1.025; // Make it slightly larger

#if DEBUG
                if (rock.name == "Mun" || (hour == 0 && minute == 0))
                {
                    //Hire.Log.Info("bOrigin: " + bOrigin.name + "   bOrigin.pos: " + opos +  ", bDestination.pos: " + dpos  + ",    rock.position: " + GetPositionAtUT(rock,time) );

                    Debug.Log(String.Format("time: {0} {1} ratio {2:f4}   " +
                        "loff {3:f1}k    lim {4:f1}k  " +
                        "RO {5:f0}   DO {6:f0}   " +
                       // "Rads {7:f0}k {8:f0}M {9:f0}k   " +
                        "Opos {10:f0} Dpos {11:f0}k Rpos {12:f0}",

                        time_str, rock.name, ratio,
                        lateralOffset.magnitude / 1000, limbo / 1000,
                        RO, DO,
                        orad /1000, drad / 1000000, rrad/1000,
                        opos.magnitude, dpos.magnitude / 1000, rpos.magnitude));
                }
#endif

                // is the body between destination and origin (eclipse/transit)  
                if (lateralOffset.magnitude < limbo)
                {

                    if (big_enough_for_eclipse || !eclipseReq)
                    {
#if DEBUG
                        if (big_enough_for_eclipse) Hire.Log.Info("Eclipse found at " + time_str);
                        if (!big_enough_for_eclipse && !eclipseReq) Hire.Log.Info("Transit found at " + time_str);
#endif
                        if (relative == Relative.any)
                            return rock;

                        bool useThis = true;
                        if (eclipser != null)
                        {
                            if (relative == Relative.closest)
                            {
                                if (rockFromOrigin.magnitude > eclipserDistance)
                                {
                                    useThis = false;
                                }
                            }
                            else  // Relative.furthest
                            {
                                if (rockFromOrigin.magnitude < eclipserDistance)
                                {
                                    useThis = false;
                                }

                            }
                        }
                        if (useThis)
                        {
                            eclipser = rock;
                            eclipserDistance = rockFromOrigin.magnitude;
                        }
                    }
                }
            }
            return eclipser;
        }
    }
}