// using HarmonyLib;
// using UnityEngine;
//
// [HarmonyPatch(typeof(XUiC_CraftingInfoWindow))]
// [HarmonyPatch(nameof(XUiC_CraftingInfoWindow.SubtractQualityButton_OnPress))]
// public class XUiCCraftingInfoSubtractQualityButton_OnPress
// {
//     private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
//     private static readonly string Feature = "CustomQualityLevels";
//
//     public static bool Prefix(XUiC_CraftingInfoWindow __instance)
//     {
//         if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
//         Debug.Log($"SubtractQualityButton: {__instance.selectedCraftingTier} : Max Quality: {QualityUtils.GetMaxQuality()}");
//         if (__instance.selectedCraftingTier >= 100 )
//         {
//             __instance.selectedCraftingTier =- 100;
//             __instance.IsDirty = true;
//         }
//         return false;
//     }
// }
