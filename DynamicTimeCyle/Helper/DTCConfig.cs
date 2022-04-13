using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace r1ft.DynamicTimeCyle
{
    class DTCConfig
    {
        private static readonly string _dtcconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\r1ft-DynamicTimeCycle\\cfg\\config.json");
        private static readonly string _pttconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\ZTrap-PathToTarkov\\config\\config.json");

        public static PTTConfig GetPTTConfig()
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

            return JsonConvert.DeserializeObject<PTTConfig>(jsonstring);
        }

        public static List<Locations> GetDTCConfig(out bool pttenabled)
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

            var config = JsonConvert.DeserializeObject<DTCMainConfig>(jsonstring);
            pttenabled = config.PTTEnabled;

            var locations = config.Locations;
            var list = new List<Locations>();

            foreach (var option in locations)
                list.Add(option);

            return list;
        }


        public class DTCMainConfig
        {
            public bool PTTEnabled { get; set; }
            public Locations[] Locations { get; set; }
        }

        public class Locations
        {
            public string OffraidPosition { get; set; }
            public int TravelTime { get; set; }
        }

        public class PTTProfile
        {
            public string mainStashId { get; set; }
            public string offraidPosition { get; set; }
        }

        public class DTCCProfile
        {
            public double hour { get; set; }
            public double min { get; set; }
            public bool hideout { get; set; }
        }

        public class PTTConfig
        {
            public bool enabled { get; set; }
            public string initial_offraid_position { get; set; }
            public bool reset_offraid_position_on_player_die { get; set; }
            public bool hideout_multistash_enabled { get; set; }
            public bool laboratory_access_restriction { get; set; }
            public string[] laboratory_access_via { get; set; }
            public bool player_scav_move_offraid_position { get; set; }
            public bool bypass_exfils_override { get; set; }
            public bool bypass_uninstall_procedure { get; set; }
            public bool bypass_luas_custom_spawn_points_tweak { get; set; }
            public object restrictions_in_raid { get; set; }
            public object offraid_regen_config { get; set; }
            public string[] hideout_main_stash_access_via { get; set; }
            public object[] hideout_secondary_stashes { get; set; }
            public bool traders_access_restriction { get; set; }
            public object traders_config { get; set; }
            public object exfiltrations { get; set; }
            public object infiltrations { get; set; }
        }
    }
}