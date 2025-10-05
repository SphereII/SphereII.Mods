using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(XUiC_CraftingInfoWindow))]
[HarmonyPatch(nameof(XUiC_CraftingInfoWindow.AddQualityButton_OnPress))]
public class XUiCCraftingInfoAddQualityButton_OnPress
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "CustomQualityLevels";

    public static bool Prefix(XUiC_CraftingInfoWindow __instance)
    {
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

        var value = QualityUtils.GetQualityStage();
        var maxQuality = QualityUtils.GetMaxQuality();

        var proposedTier = __instance.selectedCraftingTier + value;
        var finalTier = Mathf.Min(proposedTier, maxQuality);
        if (__instance.selectedCraftingTier != finalTier)
        {
            __instance.selectedCraftingTier = finalTier;
            __instance.IsDirty = true; // Mark dirty only on actual change
        }

        return false;
    }
}
