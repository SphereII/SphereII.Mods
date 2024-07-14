using HarmonyLib;

namespace Harmony.UI
{
    /**
     * SCoreXPIconRemover
     * 
     * This class includes a Harmony patch hide the IP Notification pop up
     */
    [HarmonyPatch(typeof(XUiC_CollectedItemList))]
    [HarmonyPatch("AddIconNotification")]
    public class XPIconRemover
    {
        private static readonly string AdvFeatureClass = "AdvancedUI";
        private static readonly string Feature = "DisableXPIconNotification";

        private static bool Prefix()
        {
            // Check if this feature is enabled.
            return !Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
        }
    }
}