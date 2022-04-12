
namespace r1ft.DynamicTimeCyle
{
    class Writer
    { 
        public static void WritePersistance(double hour, double min, bool hideout)
        {
            Aki.Common.Http.RequestHandler.GetData($"/pttdynamictravel/post/{hour}/{min}/{hideout}");
            return;
        }
    }
}
