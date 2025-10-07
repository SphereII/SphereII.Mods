using System.Collections.Generic;
using System.Reflection.Emit;
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
        if (minQuality == maxQuality) return true;
        minQuality = QualityUtils.GetMinQuality();
        maxQuality = QualityUtils.GetMaxQuality();
        return true;
    }
}

[HarmonyPatch(typeof(ItemValue))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new[] { typeof(int),typeof(bool)})]
public class ItemValueConstructor3
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "CustomQualityLevels";
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return codes;
        foreach (var t in codes)
        {
            if (t.opcode == OpCodes.Ldc_I4_6)
            {
                t.opcode = OpCodes.Ldc_I4;
                t.operand = QualityUtils.GetMaxQuality();
            }
            if (t.opcode == OpCodes.Ldc_I4_1)
            {
                t.opcode = OpCodes.Ldc_I4;
                t.operand = QualityUtils.GetMinQuality();
            }
        }

        return codes;
    }
}
