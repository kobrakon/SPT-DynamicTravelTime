using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace r1ft.DynamicTimeCyle
{
    class Config
    {
        private static readonly string _dtcconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\r1ft-PTTDynamicTimeCycle\\cfg\\traveltime.json");
        private static readonly string _pttconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\ZTrap-PathToTarkov\\config\\config.json");

        public static PTTConfig.MainConfig GetPTTConfig()
        {
            System.IO.FileStream f_pttconfig = new System.IO.FileStream(_pttconfig, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

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

        public static List<PTTConfig.Locations> GetDTCConfig()
        {
            System.IO.FileStream f_pttconfig = new System.IO.FileStream(_dtcconfig, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

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
