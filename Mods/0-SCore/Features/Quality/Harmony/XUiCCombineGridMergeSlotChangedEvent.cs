
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiC_CombineGrid))]
    [HarmonyPatch(nameof(XUiC_CombineGrid.Merge_SlotChangedEvent))]
    public class XUiCCombineGridMergeSlotChangedEvent
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
            }

            return codes;
        }
    }
}