using HarmonyLib;

namespace Harmony.Progression
{
    /**
     * SCoreProgression
     * 
     * This class includes a Harmony patch to disable all XP from all events.
     */
    public class SCoreProgression
    {
        private static readonly string AdvFeatureClass = "AdvancedProgression";
        private static readonly string Feature = "ZeroXP";

        [HarmonyPatch(typeof(global::Progression))]
        [HarmonyPatch("AddLevelExpRecursive")]
        public class ProgressionAddLevelExpRecursive
        {
            private static bool Prefix()
            {
                // Check if this feature is enabled.
                return !Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
            }
        }


        [HarmonyPatch(typeof(global::Progression))]
        [HarmonyPatch("AddLevelExp")]
        public class ProgressionAddLevelExp
        {
            private static bool Prefix()
            {
                // Check if this feature is enabled.
                return !Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
            }
        }
    }
}