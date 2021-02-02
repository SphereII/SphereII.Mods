using HarmonyLib;



/**
 * SphereII_XPIconRemover
 *
 * This class includes a Harmony patch hide the IP Notification pop up
 */
[HarmonyPatch(typeof(XUiC_CollectedItemList))]
[HarmonyPatch("AddIconNotification")]
public class SphereII_XPIconRemover
{
    private static readonly string AdvFeatureClass = "AdvancedUI";
    private static readonly string Feature = "DisableXPIconNotification";

    static bool Prefix()
    {
        // Check if this feature is enabled.
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            return true;
        return false;
    }
}

