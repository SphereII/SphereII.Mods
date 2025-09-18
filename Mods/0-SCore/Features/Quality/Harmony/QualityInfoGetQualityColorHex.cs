using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    
    [HarmonyPatch(typeof(QualityInfo))]
    [HarmonyPatch(nameof(QualityInfo.GetQualityColorHex))]
    public class QualityInfoGetQualityColorHex
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static bool Prefix(ref string __result, int _quality)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

            __result = QualityUtils.GetColorHex(_quality);
            return false;
        }
    }
}

