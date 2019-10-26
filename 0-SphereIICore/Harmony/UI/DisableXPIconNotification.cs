using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;

[HarmonyPatch(typeof(XUiC_CollectedItemList))]
[HarmonyPatch("AddIconNotification")]
public class SphereII_XPIconRemover
{
    private static string AdvFeatureClass = "AdvancedUI";
    private static string Feature = "DisableXPIconNotification";

    static bool Prefix()
    {
        // Check if this feature is enabled.
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            return true;
        return false;
    }
}

