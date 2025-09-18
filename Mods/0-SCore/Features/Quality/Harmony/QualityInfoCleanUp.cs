// using HarmonyLib;
// using UnityEngine;
//
// namespace SCore.Features.Quality.Harmony
// {
//     
//     [HarmonyPatch(typeof(QualityInfo))]
//     [HarmonyPatch(nameof(QualityInfo.Cleanup))]
//     public class QualityInfoCleanUp
//     {
//         private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
//         private static readonly string Feature = "CustomQualityLevels";
//         
//         public static void Postfix()
//         {
//             // Check if this feature is enabled.
//             if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return ;
//
//             var countQualityColors = QualityInfo.qualityColors.Length;
//             var countHexColors = QualityInfo.hexColors.Length;
//             QualityInfo.qualityColors = new Color[countQualityColors];
//             QualityInfo.hexColors = new string[countHexColors];
//         }
//     }
// }
//
