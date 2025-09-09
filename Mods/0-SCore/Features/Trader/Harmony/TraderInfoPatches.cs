using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;

namespace Features.Trader
{
  
        // Hijack the Getter for CurrencyItem, and check to see if they exist in our alt_currency dictionary.
        [HarmonyPatch(typeof(TraderInfo))]
        [HarmonyPatch(nameof(TraderInfo.CurrencyItem), MethodType.Getter)]
        public class TraderInfoCurrentItemPatch
        {
            public static void Postfix(ref string __result, TraderInfo __instance)
            {
                var id = TraderUtils.GetCurrentTraderID();
                if (TraderCurrencyManager.HasCustomCurrency(id))
                {
                    __result = TraderCurrencyManager.GetTraderCurrency(id);
                }
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
        
        
        // Grabs the default currency
        [HarmonyPatch(typeof(TradersFromXml))]
        [HarmonyPatch(nameof(TradersFromXml.ParseNode))]
        public class TradersFromXMLPatchParseNode
        {
            public static void Postfix(XElement e)
            {
                if (e.HasAttribute("currency_item"))
                {
                    TraderCurrencyManager.DefaultCurrency = e.GetAttribute("currency_item");
                }
               
            }
        }
        
        
        // Updates the currency display on the player's backpack.
        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch(nameof(XUiM_PlayerInventory.RefreshCurrency))]
        public class XUiMPlayerInventoryRefreshCurrency
        {
            public static bool Prefix(XUiM_PlayerInventory __instance)
            {
                var id = TraderUtils.GetCurrentTraderID();
                if (id == -1) return true;
                if (!TraderCurrencyManager.HasCustomCurrency(id)) return true;
                
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