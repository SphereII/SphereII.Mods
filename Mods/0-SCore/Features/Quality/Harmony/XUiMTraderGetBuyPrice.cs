using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace SCore.Features.Quality.Harmony
{
    // GetBuyPrice and GetSellPrice both scale quality-bearing items with
    //     Mathf.Lerp(minMod, maxMod, ((float)Quality - 1f) / 5f)
    // twice: once for items with their own TraderQualityMinMod/MaxMod, once for the global
    // TraderInfo.QualityMinMod/MaxMod. With CustomQualityLevels, every quality of 6 and above clamps
    // to the top of the lerp, so a 155 and a 555 price the same. Both lerps are rewritten to use
    // QualityUtils.GetTraderQualityFraction, which normalises against the configured range.
    public static class TraderQualityPriceTranspiler
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static IEnumerable<CodeInstruction> Rewrite(IEnumerable<CodeInstruction> instructions, string methodName)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return codes;

            var getQuality = AccessTools.PropertyGetter(typeof(ItemValue), nameof(ItemValue.Quality));
            var fraction = AccessTools.Method(typeof(QualityUtils), nameof(QualityUtils.GetTraderQualityFraction));

            var replaced = 0;
            for (var i = 0; i + 5 < codes.Count; i++)
            {
                // callvirt get_Quality; conv.r4; ldc.r4 1; sub; ldc.r4 5; div
                if (!codes[i].Calls(getQuality)) continue;
                if (codes[i + 1].opcode != OpCodes.Conv_R4) continue;
                if (!IsFloat(codes[i + 2], 1f)) continue;
                if (codes[i + 3].opcode != OpCodes.Sub) continue;
                if (!IsFloat(codes[i + 4], 5f)) continue;
                if (codes[i + 5].opcode != OpCodes.Div) continue;

                // Edit in place so any labels on these instructions survive.
                codes[i + 2].opcode = OpCodes.Call;
                codes[i + 2].operand = fraction;
                for (var j = i + 3; j <= i + 5; j++)
                {
                    codes[j].opcode = OpCodes.Nop;
                    codes[j].operand = null;
                }

                replaced++;
                i += 5;
            }

            if (replaced != 2)
                Log.Warning($"[SCore] CustomQualityLevels: expected 2 quality price lerps in XUiM_Trader.{methodName}, rewrote {replaced}. Trader prices may not scale with quality.");

            return codes;
        }

        private static bool IsFloat(CodeInstruction code, float value)
        {
            return code.opcode == OpCodes.Ldc_R4 && code.operand is float f && f == value;
        }
    }

    [HarmonyPatch(typeof(XUiM_Trader))]
    [HarmonyPatch(nameof(XUiM_Trader.GetBuyPrice))]
    public class XUiMTraderGetBuyPrice
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return TraderQualityPriceTranspiler.Rewrite(instructions, nameof(XUiM_Trader.GetBuyPrice));
        }
    }
}
