// using HarmonyLib;
// using UnityEngine;
//
// [HarmonyPatch(typeof(XUiC_CraftingInfoWindow))]
// [HarmonyPatch(nameof(XUiC_CraftingInfoWindow.AddQualityButton_OnPress))]
// public class XUiCCraftingInfoAddQualityButton_OnPress
// {
//     private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
//     private static readonly string Feature = "CustomQualityLevels";
//
//     public static bool Prefix(XUiC_CraftingInfoWindow __instance)
//     {
//         if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
//         Debug.Log($"AddQualityButton: {__instance.selectedCraftingTier} : Max Quality: {QualityUtils.GetMaxQuality()}");
//         if (__instance.selectedCraftingTier < QualityUtils.GetMaxQuality())
//         {
//             __instance.selectedCraftingTier =+ 100;
//             __instance.IsDirty = true;
//         }
//         return false;
//     }
// }
