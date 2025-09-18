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
        _quality = Mathf.Clamp(_quality / 100, 0, QualityUtils.GetMaxQuality());
        return true;
    }
}