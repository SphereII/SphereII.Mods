using System.Collections.Generic;
using HarmonyLib;

namespace SCore.Features.Quality.Harmony
{
    // See TraderQualityPriceTranspiler in XUiMTraderGetBuyPrice.cs. The old version of this patch
    // replaced only the first 5f, which is the per-item TraderQualityMinMod/MaxMod branch, so items
    // priced by the global TraderInfo mods (most weapons and tools) still capped at quality 6.
    [HarmonyPatch(typeof(XUiM_Trader))]
    [HarmonyPatch(nameof(XUiM_Trader.GetSellPrice))]
    public class XUiMTraderGetSellPrice
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return TraderQualityPriceTranspiler.Rewrite(instructions, nameof(XUiM_Trader.GetSellPrice));
        }
    }
}
