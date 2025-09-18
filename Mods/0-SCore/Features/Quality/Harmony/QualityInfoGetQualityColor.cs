using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    
    [HarmonyPatch(typeof(QualityInfo))]
    [HarmonyPatch(nameof(QualityInfo.GetQualityColor))]
    public class QualityInfoGetQualityColor
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";
        
        public static bool Prefix(ref Color __result,  int _quality)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
            if (_quality == 0) return true;
            __result = QualityUtils.GetColor(_quality);
            return false;
        }
    }
}

