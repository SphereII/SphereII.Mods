using HarmonyLib;

namespace Harmony.UI
{
    /**
     * SCoreXuiC_TargetBar
     * 
     * This class includes a Harmony patch to reduce the fade time.
     */
    public class TargetBar
    {
        [HarmonyPatch(typeof(XUiC_TargetBar))]
        [HarmonyPatch("Update")]
        public class TargetBarPrefix
        {
            public static bool Prefix(XUiC_TargetBar __instance, ref XUiView ___viewComponent, ref float ___noTargetFadeTimeMax)
            {
                ___noTargetFadeTimeMax = 0.1f;
                return true;
            }
        }
    }
}