using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace r1ft.DynamicTimeCyle
{
    class DynamicTime
    {
        public static void ReturnFactoryTime(string location, out double hour, out double min)
        {
            hour = 0;
            min = 0;

            if (location == "factory4_day")
            {
                hour = 15;
                min = 28;
            }

            if (location == "factory4_night")
            {
                hour = 3;
                min = 28;
            }
        }

        public static void ReturnMapTime(double inhour, double inmin, string pos, PTTConfig.Locations[] locations, out double hour, out double min)
        {
            hour = inhour;
            min = inmin;
            if (pos != "")
            {
                foreach (var location in locations)
                {
                    if (location.OffraidPosition != pos)
                        continue;

                    min += location.TravelTime;
                    break;
                }

                if (min > 60)
                {
                    hour += Mathf.RoundToInt((int)min / 60);
                    min -= Mathf.RoundToInt((int)min / 60) * 60;
                }
            }
        }
    }
}
