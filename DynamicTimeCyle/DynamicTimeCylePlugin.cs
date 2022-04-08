using UnityEngine;
using BepInEx;

namespace r1ft.DynamicTimeCyle
{
    [BepInPlugin("com.r1ft.DynamicTimeCyle", "r1ft.DynamicTimeCyle", "2.2.0")]
    public class DynamicTimeCyle : BaseUnityPlugin
    {
        public static GameObject Hook;

        private void Awake()
        {
            Logger.LogInfo("Loading: r1fT-DynamicTimeCyle");
            Hook = new GameObject("DynamicTimeCyle");
            Hook.AddComponent<DynamicTimeCyleController>();
            DontDestroyOnLoad(Hook);
        }
    }
}
