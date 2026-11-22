using HarmonyLib;
using UnityEngine;

namespace Features.Trader
{
    // Scales trader prices by the alt_currency item's CurrencyScale property.
    //
    // This is applied to the final price, after vanilla has applied the trader markup and the
    // bartering perks. Setting OverrideBuyMarkup / OverrideSellMarkdown on the trader instead
    // makes vanilla skip the bartering perks entirely, which is what this replaces.
    //
    // GetBuyPrice and GetSellPrice call themselves for each mod on a modded item, and Harmony
    // patches those inner calls too. The depth counter makes sure only the outermost call is
    // scaled, so a modded item isn't scaled once per mod and again for the total.
    public static class XUiMTraderCurrencyScale
    {
        private static int _buyDepth;
        private static int _sellDepth;

        [HarmonyPatch(typeof(XUiM_Trader))]
        [HarmonyPatch(nameof(XUiM_Trader.GetBuyPrice))]
        public class GetBuyPrice
        {
            public static void Prefix() => _buyDepth++;

            public static void Postfix(ref int __result, XUi _xui)
            {
                if (_buyDepth != 1 || __result <= 0) return;
                var traderId = _xui?.Trader?.TraderData?.TraderID ?? -1;
                if (!TraderCurrencyManager.TryGetCurrencyScale(traderId, out var buyScale, out _)) return;

                // Round up so a scaled price is never free.
                __result = Mathf.Max(1, Mathf.CeilToInt(__result * buyScale));
            }

            public static void Finalizer() => _buyDepth--;
        }

        [HarmonyPatch(typeof(XUiM_Trader))]
        [HarmonyPatch(nameof(XUiM_Trader.GetSellPrice))]
        public class GetSellPrice
        {
            public static void Prefix() => _sellDepth++;

            public static void Postfix(ref int __result, XUi _xui)
            {
                if (_sellDepth != 1 || __result <= 0) return;
                var traderId = _xui?.Trader?.TraderData?.TraderID ?? -1;
                if (!TraderCurrencyManager.TryGetCurrencyScale(traderId, out _, out var sellScale)) return;

                // Round down so selling can't mint currency; vanilla treats a 0 price as unsellable.
                __result = Mathf.FloorToInt(__result * sellScale);
            }

            public static void Finalizer() => _sellDepth--;
        }
    }
}
