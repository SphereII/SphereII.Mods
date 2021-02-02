
using HarmonyLib;


/**
 * SphereII_XuiC_TargetBar
 *
 * This class includes a Harmony patch to reduce the fade time.
 */
public class SphereII_XuiC_TargetBar
{
    private static readonly string AdvFeatureClass = "AdvancedUI";
    private static readonly string Feature = "UnlockEnemyHealthBar";

    [HarmonyPatch(typeof(XUiC_TargetBar))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_TargetBar_Prefix
    {
        public static bool Prefix(XUiC_TargetBar __instance, ref XUiView ___viewComponent, ref float ___noTargetFadeTimeMax)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) // if disables, don't execute the base Update()
                return false;

            ___noTargetFadeTimeMax = 0.1f;
            return true;
        }


    }

}