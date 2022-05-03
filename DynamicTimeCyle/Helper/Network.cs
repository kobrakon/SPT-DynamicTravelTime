using Comfort.Common;
using EFT;
using EFT.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace r1ft.DynamicTimeCyle
{
    class Network
    {
        private static int _deathCount = 0;

        private static Config.DTCProfile RetreiveProfile(bool pttEnabled, out bool err, out string pos)
        {
            pos = "";
            err = false;
            try
            {
                if (pttEnabled)
                {
                    var posrequst = Aki.Common.Http.RequestHandler.GetJson("/dynamictimecycle/offraidPosition");
                    pos = posrequst.ToString().Replace("\"", "");
                    if (pos == "")
                        err = true;

                    var request = Aki.Common.Http.RequestHandler.GetJson("/dynamictimecycle/config");
                    return JsonConvert.DeserializeObject<Config.DTCProfile>(request);
                }
                else
                {
                    var gameWorld = Singleton<GameWorld>.Instance;
                    if (gameWorld != null)
                    {
                        if (gameWorld.AllPlayers != null)
                        {
                            if (gameWorld.AllPlayers.Count != 0)
                            {
                                pos = gameWorld.AllPlayers[0].Location;
                            }
                        }
                    }
                    var request = Aki.Common.Http.RequestHandler.GetJson("/dynamictimecycle/config");
                    return JsonConvert.DeserializeObject<Config.DTCProfile>(request);
                }
            }
            catch
            {
                err = true;
                return new Config.DTCProfile();
            }
        }

        private static void WritePersistance(double hour, double min, bool hideout)
        {
            Aki.Common.Http.RequestHandler.GetData($"/dynamictimecycle/post/{hour}/{min}/{hideout}");
            return;
        }

        public static bool GetPttAvailiable()
        {
            var data = Aki.Common.Http.RequestHandler.GetJson("/dynamictimecycle/ptt");
            return bool.Parse(data.ToString());
        }

        public static int GetDeathCount()
        {
            var data = Aki.Common.Http.RequestHandler.GetJson("/dynamictimecycle/deathcount");
            return int.Parse(data.ToString());
        }

        public static bool SetOffRaidPosition(bool pttEnabled, Config.PTTConfig main, double inhour, double inmin, bool nightAccel, double nightAccelTime, out string pos, out double hour, out double min, out bool hideout)
        {
            if (pttEnabled)
            {
                hideout = false;
                hour = inhour;
                min = inmin;
                var config = RetreiveProfile(pttEnabled, out var err, out pos);
                if (err)
                    return false;

                if (inhour == 99)
                {
                    hour = config.hour;
                    min = config.min;
                }
                else
                {
                    if (nightAccel)
                    {
                        if (inhour >= 22 || inhour < 6)
                        {
                            min += nightAccelTime;

                            if (min > 60)
                            {
                                hour += Mathf.RoundToInt((int)min / 60);
                                min -= Mathf.RoundToInt((int)min / 60) * 60;

                                if (hour > 24)
                                    hour -= 24;
                            }
                        }
                    }
                }

                foreach (var hideoutposition in main.hideout_main_stash_access_via)
                {
                    if (hideoutposition != pos)
                        continue;

                    hideout = true;
                    break;
                }

                WritePersistance(hour, min, hideout);
                return true;
            }
            else
            {
                hideout = false;
                hour = inhour;
                min = inmin;
                var config = RetreiveProfile(pttEnabled, out var err, out pos);
                if (err)
                    return false;

                if (inhour == 99)
                {
                    if (config.hour == 99)
                        hideout = true;
                    else
                        hideout = config.hideout;

                    hour = config.hour;
                    min = config.min;
                }

                var deathcount = GetDeathCount();
                if (_deathCount < deathcount)
                {
                    _deathCount = deathcount;
                    hideout = true;
                }

                WritePersistance(hour, min, hideout);
                return true;
            }
        }
    }
}
