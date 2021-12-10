using HarmonyLib;

namespace Harmony.ErrorHandling
{
    /**
     * SCoreNoExceptionHijack
     * 
     * This class includes a Harmony patch to disable the console from popping down when a Null ref or other critical error occurs. This is mainly to allow
     * modders to keep control of their game despite erroring out while testing.
     */
    public class NoExceptionHijack
    {
        private static readonly string AdvFeatureClass = "ErrorHandling";
        private static readonly string Feature = "NoExceptionHijack";

        [HarmonyPatch(typeof(GUIWindowConsole))]
        [HarmonyPatch("openConsole")]
        public class DisableExceptionHijack
        {
            private static bool Prefix()
            {
                return !Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
            }
        }
    }
}