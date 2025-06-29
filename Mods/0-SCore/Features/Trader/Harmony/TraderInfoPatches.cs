using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;

namespace Features.Trader
{
    public class TraderInfoPatches
    {
        // Helper method to try to get the current trader ID.
        private static int GetCurrentTraderID(){
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null) return -1;
            if (player.playerUI?.xui?.Trader?.TraderTileEntity?.TraderData == null) return -1;
            return player.playerUI.xui.Trader.TraderTileEntity.TraderData.TraderID;
        }
        
        // Hijack the Getter for CurrencyItem, and check to see if they exist in our alt_currency dictionary.
        [HarmonyPatch(typeof(TraderInfo))]
        [HarmonyPatch(nameof(TraderInfo.CurrencyItem), MethodType.Getter)]
        public class TraderInfoCurrentItemPatch
        {
            public static void Postfix(ref string __result, TraderInfo __instance)
            {
                var id = GetCurrentTraderID();
                if (id == -1) return;
                
                __result = TraderCurrencyManager.GetTraderCurrency(id);
            }
        }

        // Stores the alt_currency from xml into the dictionary, so we can reference it later.
        [HarmonyPatch(typeof(TradersFromXml))]
        [HarmonyPatch(nameof(TradersFromXml.ParseTraderInfo))]
        public class TradersFromXMLPatch
        {
            public static void Postfix(XElement e)
            {
                if (!e.HasAttribute("id")) return;
                if (!int.TryParse(e.GetAttribute("id"), out var traderId)) return;

                if (!e.HasAttribute("alt_currency")) return;
                TraderCurrencyManager.SetTraderCurrency(traderId, e.GetAttribute("alt_currency"));
            }
        }
        
        
        // Updates the currency display on the player's backpack.
        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch(nameof(XUiM_PlayerInventory.RefreshCurrency))]
        public class XUiMPlayerInventoryRefreshCurrency
        {
            public static bool Prefix(XUiM_PlayerInventory __instance)
            {
                var id = GetCurrentTraderID();
                if (id == -1) return true; 
                var currencyItemString = TraderCurrencyManager.GetTraderCurrency(id);
                var currencyItem = ItemClass.GetItem(currencyItemString);
                var itemCount = __instance.GetItemCount(currencyItem);
                if (itemCount != __instance.CurrencyAmount)
                {
                    __instance.CurrencyAmount = itemCount;
                }
                return false;
            }
        }
    }
}