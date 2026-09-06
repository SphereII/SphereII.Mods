using HarmonyLib;
using UnityEngine;

public class QualityInfoGetQualityLevelName
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "CustomQualityLevels";

    [HarmonyPatch(typeof(QualityInfo))]
    [HarmonyPatch(nameof(QualityInfo.GetQualityLevelName))]
    public static bool Prefix(ref int _quality, bool _useQualityColor = false)
    {
        // Check if this feature is enabled.
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
        if (_quality == 0) return true;
        // Same banding as the colour lookup, so the name and the colour never disagree.
        _quality = QualityUtils.CalculateTier(_quality);
        return true;
    }
}