
namespace r1ft.DynamicTimeCyle
{
    class PTTConfig
    {
        public class Locations
        {
            public string OffraidPosition { get; set; }
            public int TravelTime { get; set; }
        }

        public class Persistance
        {
            public string currentLocation { get; set; }
            public double currentHour { get; set; }
            public double currentMin { get; set; }
            public bool hideout { get; set; }
        }


        public class MainConfig
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