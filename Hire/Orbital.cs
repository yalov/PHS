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
        static double  lastDayChecked = 0; // Will be the last second of the last day which was checked
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
                    var hcp = HasClearPath(checkTime, FlightGlobals.GetHomeBody(), Planetarium.fetch.Sun, Relative.closest);
                    if (hcp!= null)
                    {
                        currentEclipsingbody = hcp;
                        return hcp;
                    }
                }
                currentEclipsingbody = null;
                return null;
            }
        }

        public enum Relative  { closest, furthest, any };

        // Finds if the path between two bodies is obstructed, and if so, by what body.  Can also return either the closest body or furthest body
        public static  CelestialBody HasClearPath(double time, CelestialBody bOrigin, CelestialBody bDestination, Relative relative)
        {
            CelestialBody eclipser = null;
            double eclipserDistance = 0;

            // Cribbed with love from RemoteTech.  I have no head for vectors.

            Vector3d opos = bOrigin.getPositionAtUT(time);
            Vector3d dpos = bDestination.getPositionAtUT(time);
            foreach (CelestialBody rock in FlightGlobals.Bodies)
            {
                if (rock == bOrigin || rock == bDestination)
                    continue;
                Vector3d bodyFromOrigin = rock.position - opos;
                Vector3d destFromOrigin = dpos - opos;

                // is the destingation behind the origin?
                if (Vector3d.Dot(bodyFromOrigin, destFromOrigin) <= 0)
                {
                    continue;
                }
                Vector3d destFromOriginNorm = destFromOrigin.normalized;
                if (Vector3d.Dot(bodyFromOrigin, destFromOriginNorm) >= destFromOrigin.magnitude)
                {
                    continue;
                }
                Vector3d lateralOffset = bodyFromOrigin - Vector3d.Dot(bodyFromOrigin, destFromOriginNorm) * destFromOriginNorm;

             
                double limbo = rock.Radius * 1.025; // Make it slightly larger

                //Debug.Log("time: " + time + ".  rock: "+  rock.name + ",   limbo: " + limbo + ",   lateralOffset: " + lateralOffset.magnitude);

                if (lateralOffset.magnitude < limbo)
                {
#if DEBUG
                    double day = Math.Floor(time / SecsPerDay);
                    double hour = Math.Floor((time - (day * SecsPerDay)) / 3600);
                    double minute = Math.Floor((time - (day * SecsPerDay) - hour * 3600) /60);
                    Debug.Log("Eclipse found at " + day + " day, " + hour + ":" + minute);
#endif
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
                        } else
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
