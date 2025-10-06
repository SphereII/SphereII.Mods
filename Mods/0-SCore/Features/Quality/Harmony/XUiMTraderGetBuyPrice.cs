using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiM_Trader))]
    [HarmonyPatch(nameof(XUiM_Trader.GetBuyPrice))]
    public class XUiMTraderGetBuyPrice
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return codes;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float floatValue && floatValue == 5f)
                {
                    codes.RemoveAt(i);
                    var getMaxQualityMethod = AccessTools.Method(typeof(QualityUtils), nameof(QualityUtils.GetMaxQuality));
                    if (getMaxQualityMethod == null)
                    {
                        Debug.LogError("Harmony Transpiler Error: QualityUtils.GetMaxQuality method not found!");
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldc_R4, 5f));
                        continue; // Skip to the next instruction
                    }

                    codes.Insert(i, new CodeInstruction(OpCodes.Call, getMaxQualityMethod));
                    i++; 
                    codes.Insert(i, new CodeInstruction(OpCodes.Conv_R4));
                }
            }

            return codes;
        }
    }
}