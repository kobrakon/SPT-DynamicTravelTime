using Newtonsoft.Json;
using System.Collections.Generic;

namespace r1ft.DynamicTimeCyle
{
    class Reader
    {
        public static PTTConfig.DTCCProfile GetOffRaidPosition(out bool err, out string pos)
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

        public static PTTConfig.MainConfig GetConfig(string pttconfig)
        {                
            System.IO.FileStream f_pttconfig = new System.IO.FileStream(pttconfig, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_pttconfig = new System.IO.StreamReader(f_pttconfig);

            var lst = new List<string>();

            while (!s_pttconfig.EndOfStream)
                lst.Add(s_pttconfig.ReadLine());

            var jsonstring = "";

            foreach (var line in lst.ToArray())
            {
                jsonstring += line.Trim();
            }

            return JsonConvert.DeserializeObject<PTTConfig.MainConfig>(jsonstring);
        }

        public static List<PTTConfig.Locations> GetConfig(PTTConfig.MainConfig pttconfigMain, string pttconfig)
        {
            System.IO.FileStream f_pttconfig = new System.IO.FileStream(pttconfig, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_pttconfig = new System.IO.StreamReader(f_pttconfig);

            var lst = new List<string>();

            while (!s_pttconfig.EndOfStream)
                lst.Add(s_pttconfig.ReadLine());

            var jsonstring = "";

            foreach (var line in lst.ToArray())
            {
                jsonstring += line.Trim();
            }

            var config = JsonConvert.DeserializeObject<PTTConfig.Locations[]>(jsonstring);
            var list = new List<PTTConfig.Locations>();

            foreach (var option in config)
                list.Add(option);

            return list;
        }
    }
}
