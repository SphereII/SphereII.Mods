using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(XUiC_CraftingInfoWindow))]
[HarmonyPatch(nameof(XUiC_CraftingInfoWindow.SubtractQualityButton_OnPress))]
public class XUiCCraftingInfoSubtractQualityButton_OnPress
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "CustomQualityLevels";

    public static bool Prefix(XUiC_CraftingInfoWindow __instance)
    {
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
        var value = QualityUtils.GetQualityStage(); 
        var proposedTier = __instance.selectedCraftingTier - value;
        var finalTier = Mathf.Max(proposedTier, value); 
        if (__instance.selectedCraftingTier != finalTier)
        {
            __instance.selectedCraftingTier = finalTier;
            __instance.IsDirty = true; 
        }
        return false;
    }
}
