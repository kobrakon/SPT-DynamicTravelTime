using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace r1ft.DynamicTimeCyle
{
    class Writer
    { 
        public static void WritePersistance(PTTConfig.Persistance persistance, string persistancefile)
        {
            var writestr = JsonConvert.SerializeObject(persistance);
            File.WriteAllText(persistancefile, writestr);
            return;
        }
    }
}
