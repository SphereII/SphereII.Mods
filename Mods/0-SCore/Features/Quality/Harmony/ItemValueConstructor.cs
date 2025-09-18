using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(ItemValue))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(string[]), typeof(float) })]
public class ItemValueConstructor
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "CustomQualityLevels";
        
    public static bool Prefix(ref int minQuality, ref int maxQuality)
    {
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true ;
        minQuality = QualityUtils.GetMinQuality();
        maxQuality = QualityUtils.GetMaxQuality();
        return true;
    }
}