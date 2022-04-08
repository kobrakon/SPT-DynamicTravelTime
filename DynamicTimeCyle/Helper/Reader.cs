using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace r1ft.DynamicTimeCyle
{
    class Reader
    {
        public static List<PTTConfig.Locations> ReadPTTConfig(string pttconfig)
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

        public static string GetOffRaidPos(string serverlog)
        {
            System.IO.FileStream f_serverlog = new System.IO.FileStream(serverlog, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_serverlog = new System.IO.StreamReader(f_serverlog);

            List<string> lst = new List<string>();

            var offraidpos = "";

            while (!s_serverlog.EndOfStream)
                lst.Add(s_serverlog.ReadLine());

            foreach (var line in lst.ToArray())
            {
                if (!line.Contains("=> PathToTarkov: player offraid position changed to"))
                    continue;

                var splitchar = " ";
                var split = line.Split(splitchar.ToCharArray()[0]);
                offraidpos = split[split.Length - 1].Trim();
                var removechar = "'";
                offraidpos = offraidpos.Replace(removechar.ToCharArray()[0], splitchar.ToCharArray()[0]).Trim();
                break;
            }

            return offraidpos;
        }
    }
}
