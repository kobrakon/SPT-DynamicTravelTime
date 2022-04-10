using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using EFT.UI;
using System;

namespace r1ft.DynamicTimeCyle
{
    class Reader
    {
        public static string[] GetOffRaidPositions(PTTConfig.MainConfig pttconfig)
        {
            var returnlist = new List<string>();

            var infils = pttconfig.infiltrations.ToString();

            List<string> lst = new List<string>();

            using (System.IO.StringReader reader = new System.IO.StringReader(infils))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lst.Add(line);
                }
            }

            foreach (var line in lst)
            {
                if (!line.Contains(": {"))
                    continue;

                var linesplit = line.Split(':');
                var returnline = linesplit[0].Replace("\"", "").Trim();
                returnlist.Add(returnline);
            }

            return returnlist.ToArray();
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

        public static PTTConfig.Persistance ReadPersistance(string pttconfig)
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

            return JsonConvert.DeserializeObject<PTTConfig.Persistance>(jsonstring);
        }

        public static List<PTTConfig.Locations> GetLocation(PTTConfig.MainConfig pttconfigMain, string pttconfig)
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

        public static string GetOffRaidPos(PTTConfig.MainConfig pttconfig, string serverlog, out bool found)
        {
            System.IO.FileStream f_serverlog = new System.IO.FileStream(serverlog, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_serverlog = new System.IO.StreamReader(f_serverlog);

            List<string> lst = new List<string>();

            var offraidpos = "";

            while (!s_serverlog.EndOfStream)
                lst.Add(s_serverlog.ReadLine());

            var tmpArray = lst.ToArray();
            Array.Reverse(tmpArray);
            found = false;
            foreach (var hit in tmpArray)
            {
                if (!hit.Contains("=> PathToTarkov: player offraid position changed to"))
                    continue;


                var split = hit.Split(' ');
                offraidpos = split[split.Length - 1].Trim();
                offraidpos = offraidpos.Replace("'", " ").Trim();
                found = true;
                break;
            }

            if (offraidpos == "")
            {
                offraidpos = pttconfig.initial_offraid_position;
                offraidpos = offraidpos.Replace("'", " ").Trim();
            }

            return offraidpos;
        }
    }
}
