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
            CelestialBody transit = null;

            // Cribbed with love from RemoteTech.  I have no head for vectors.

            Vector3d opos = GetPositionAtUT(bOrigin, time);
            Vector3d dpos = GetPositionAtUT(bDestination,time);
            foreach (CelestialBody rock in FlightGlobals.Bodies)
            {
                if (rock == bOrigin || rock == bDestination)
                    continue;


                Vector3d bodyFromOrigin = GetPositionAtUT(rock,time) - opos;
                Vector3d destFromOrigin = dpos - opos;

                // ∠ Destination-Origin-Body should be <180
                if (Vector3d.Dot(bodyFromOrigin, destFromOrigin) <= 0)
                {
#if DEBUG
                    if (rock.name == "Mun" || (hour == 0 && minute == 0))
                        Debug.Log("time: " + time_str + "  " + rock.name + " >180deg");
#endif
                    continue;
                }

                //  (why projection to origin-destination? body should be closer then destination.     
                //  OT<OD, but OB>OD.  if OB>OD then there isn't eclipse)
                //                                                                         Dest (sun)
                //                                                                           |
                //                                                                           |
                //    Body (outher_planet)---------------------------------------------------|T
                //                                                                          ||
                //                                                                        Origin (kerbin)

                /*
                // projection to origin-destination
                Vector3d destFromOriginNorm = destFromOrigin.normalized;
                if (Vector3d.Dot(bodyFromOrigin, destFromOriginNorm) >= destFromOrigin.magnitude)
                {
                    continue;
                }
                */

                // body should be closer then destination. OB>OD = no eclipse 
                if (bodyFromOrigin.magnitude >= destFromOrigin.magnitude)
                {
#if DEBUG
                    if (rock.name == "Mun" || (hour == 0 && minute == 0))
                        Debug.Log("time: " + time_str + "  " + rock.name + " rock futher then sun");
#endif
                    if (eclipseReq)
                        continue;
                    else
                        transit = rock;
                }


                // is the body big enough?
                double rr = rock.Radius;
                double sr_proj = bDestination.Radius * ((bodyFromOrigin.magnitude - bOrigin.Radius) / (destFromOrigin.magnitude - bOrigin.Radius)); // from surface of origin
                //if (rr < sr_proj)
                //{
                //    continue;
                //}

                Vector3d destFromOriginNorm = destFromOrigin.normalized;
                Vector3d lateralOffset = bodyFromOrigin - Vector3d.Dot(bodyFromOrigin, destFromOriginNorm) * destFromOriginNorm;
                double limbo = rock.Radius * 1.025; // Make it slightly larger

#if DEBUG
                Vector3d BOtime = GetPositionAtUT(rock,time) - opos;
               // Vector3d BO = rock.position - opos;

                double BOmtime = BOtime.magnitude;
               // double BOm = BO.magnitude;
                double Or_r = bOrigin.Radius;
                double De_r = bDestination.Radius;

                double De_r_proj = De_r * ((BOmtime - Or_r) / (destFromOrigin.magnitude - Or_r)); // from surface of origin
                double ratio = rr / De_r_proj;

                // What is wrong there: while Mun circles around kerbin, distanse from kerbin don't changes. 
                // So 
                //    BOtime.m = (rock.getPositionAtUT(time) - opos).magnitude 
                // should not changes. And ratio for Mun should be near 1.

                // P.S. Also 
                //    BO.m = (rock.position - opos).magnitude 
                // should change, because kerbin moves around sun in time, and rock.position don't correspont that. rock.getPositionAtUT(time) correspont.  

                // time: 5d  0:1, Mun,  ratio 0.7  latOffset.magn   13555.2k > 205.0k   BOtime.m 16.2M   BO.m 16.1M
                // time: 5d  0:5, Mun,  ratio 0.8  latOffset.magn   11231.0k > 205.0k   BOtime.m 14.4M   BO.m 14.4M
                // time: 5d 0:10, Mun,  ratio 0.9  latOffset.magn    8324.6k > 205.0k   BOtime.m 12.3M   BO.m 12.3M
                // time: 5d 0:15, Mun,  ratio 1.0  latOffset.magn    5416.8k > 205.0k   BOtime.m 10.7M   BO.m 10.7M
                // time: 5d 0:20, Mun,  ratio 1.1  latOffset.magn    2507.6k > 205.0k   BOtime.m 9.7M    BO.m 9.5M
                // time: 5d 0:24, Mun,  ratio 1.2  latOffset.magn     179.3k < 205.0k   BOtime.m 9.4M    BO.m 9.1M

                //Vector3d LOff = BO - Vector3d.Dot(BO, destFromOriginNorm) * destFromOriginNorm;
                Vector3d LOffTime = BOtime - Vector3d.Dot(BOtime, destFromOriginNorm) * destFromOriginNorm;

                if (rock.name == "Mun" || (hour == 0 && minute == 0))
                {
                    Hire.Log.Info("bOrigin: " + bOrigin.name + "   bOrigin.pos: " + opos +  ", bDestination.pos: " + dpos  + ",    rock.position: " + GetPositionAtUT(rock,time) );
                    Debug.Log(String.Format("time: {0} {1} ratio {2:f1}(>=1 for total eclipse) " +
                        "loffT {3:f1}k    lim {4:f1}k  " +
                        "BOt {5:f1}M    BOt-Or {6:f1}M   " +
                        "Or_r {7:f1} De_r {8:f1}M R_r {9:f1}",

                        time_str, rock.name, ratio,

                        LOffTime.magnitude / 1000, limbo / 1000,

                        BOmtime / 1000000, (BOmtime - Or_r) / 1000000,

                        Or_r, De_r / 1000000, rr));

                }
#endif

                //Debug.Log("time: " + time + ".  rock: "+  rock.name + ",   limbo: " + limbo + ",   lateralOffset: " + lateralOffset.magnitude);

                // is the body really between destination and origin?
                if (lateralOffset.magnitude < limbo)
                {
#if DEBUG
                    if (transit != null)
                        Hire.Log.Info("Transit found at " + time_str);
                    else
                        Debug.Log("Eclipse found at " + time_str);
#endif

                    //
                    // Code for determining transits is not yet completed
                    //
                    if (relative == Relative.any)
                        return rock;
                    bool useThis = true;
                    if (eclipser != null)
                    {
                        if (relative == Relative.closest)
                        {
                            if (bodyFromOrigin.magnitude > eclipserDistance)
                            {
                                useThis = false;
                            }
                        }
                        else
                        {
                            if (bodyFromOrigin.magnitude > eclipserDistance)
                            {
                                useThis = false;
                            }

                        }
                    }
                    if (useThis)
                    {
                        eclipser = rock;
                        eclipserDistance = bodyFromOrigin.magnitude;

                    }
                }
            }
            return eclipser;
        }
    }
}