using Newtonsoft.Json;

namespace r1ft.DynamicTimeCyle
{
    class Network
    {
        public static PTTConfig.DTCCProfile RetreiveProfile(out bool err, out string pos)
        {
            pos = "";
            err = false;
            try
            {
                var posrequst = Aki.Common.Http.RequestHandler.GetJson("/pttdynamictravel/offraidPosition");
                var ptt = JsonConvert.DeserializeObject<PTTConfig.PTTProfile>(posrequst);
                pos = ptt.offraidPosition;
                if (pos == "null")
                    err = true;

                var request = Aki.Common.Http.RequestHandler.GetJson("/pttdynamictravel/config");
                return JsonConvert.DeserializeObject<PTTConfig.DTCCProfile>(request);
            }
            catch
            {
                err = true;
                return new PTTConfig.DTCCProfile();
            }
        }

        public static void WritePersistance(double hour, double min, bool hideout)
        {
            Aki.Common.Http.RequestHandler.GetData($"/pttdynamictravel/post/{hour}/{min}/{hideout}");
            return;
        }

        public static bool SetOffRaidPosition(PTTConfig.MainConfig main, double inhour, double inmin, out string pos, out double hour, out double min, out bool hideout)
        {
            hideout = false;
            hour = inhour;
            min = inmin;
            var config = Network.RetreiveProfile(out var err, out pos);
            if (err)
                return false;

            if (inhour == 99)
            {
                hideout = config.hideout;
                hour = config.hour;
                min = config.min;
            }


            foreach (var hideoutposition in main.hideout_main_stash_access_via)
            {
                if (hideoutposition != pos)
                    continue;

                hideout = true;
                break;
            }

            Network.WritePersistance(hour, min, hideout);
#if DEBUG
PreloaderUI.Instance.Console.AddLog("SAVED PERSISTANCE", "[DEBUG]");
PreloaderUI.Instance.Console.AddLog($"Hour : {_cacheTimeHour}", "[DEBUG]");
PreloaderUI.Instance.Console.AddLog($"Min : {_cacheTimeMin}", "[DEBUG]");
PreloaderUI.Instance.Console.AddLog($"Position : {_offraidPos}", "[DEBUG]");
PreloaderUI.Instance.Console.AddLog($"Hideout : {_hideout}", "[DEBUG]");
#endif
            return true;
        }
    }
}
