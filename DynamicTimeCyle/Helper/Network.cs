using Newtonsoft.Json;

namespace r1ft.DynamicTimeCyle
{
    class Network
    {
        private static DTCConfig.DTCCProfile RetreiveProfile(out bool err, out string pos)
        {
            pos = "";
            err = false;
            try
            {
                var posrequst = Aki.Common.Http.RequestHandler.GetJson("/pttdynamictravel/offraidPosition");
                var ptt = JsonConvert.DeserializeObject<DTCConfig.PTTProfile>(posrequst);
                pos = ptt.offraidPosition;
                if (pos == "null")
                    err = true;

                var request = Aki.Common.Http.RequestHandler.GetJson("/pttdynamictravel/config");
                return JsonConvert.DeserializeObject<DTCConfig.DTCCProfile>(request);
            }
            catch
            {
                err = true;
                return new DTCConfig.DTCCProfile();
            }
        }

        private static void WritePersistance(double hour, double min, bool hideout)
        {
            Aki.Common.Http.RequestHandler.GetData($"/pttdynamictravel/post/{hour}/{min}/{hideout}");
            return;
        }

        public static bool SetOffRaidPosition(DTCConfig.MainConfig main, double inhour, double inmin, out string pos, out double hour, out double min, out bool hideout)
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

            WritePersistance(hour, min, hideout);
            return true;
        }
    }
}
