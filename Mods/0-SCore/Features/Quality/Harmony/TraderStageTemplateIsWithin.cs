using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    
    [HarmonyPatch(typeof(TraderStageTemplate))]
    [HarmonyPatch(nameof(TraderStageTemplate.IsWithin))]
    public class TraderStageTemplateIsWithin
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";
        
        public static bool Prefix(ref bool __result, TraderStageTemplate __instance,int traderStage, int quality)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

            // The trader stage check remains the same.
            var traderStageCheck = (__instance.Min == -1 || __instance.Min <= traderStage) && (__instance.Max == -1 || __instance.Max >= traderStage);
            // 1. The quality requirement is a wildcard (-1), OR
            // 2. The tier of the input 'quality' is the same as the tier of 'this.Quality'.
            var qualityCheck = (__instance.Quality == -1) || (QualityUtils.CalculateTier(quality) == QualityUtils.CalculateTier(__instance.Quality));

            __result= traderStageCheck && qualityCheck;
            return false;
        }
    }
}