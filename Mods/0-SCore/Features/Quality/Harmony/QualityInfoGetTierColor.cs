using System;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(QualityInfo))]
    [HarmonyPatch(nameof(QualityInfo.GetTierColor))]
    public class QualityInfoGetTierColor
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static bool Prefix(ref Color __result,  int _tier)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
            if (_tier == 0) return true;
            __result = QualityUtils.GetColor(_tier);
            return false;
        }
    }
}