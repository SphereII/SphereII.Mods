using HarmonyLib;
using UnityEngine;


namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(ItemActionEntrySell))]
    [HarmonyPatch(nameof(ItemActionEntrySell.OnActivated))]
    public class ItemActionEntrySellOnActivated
    {
        // Used to keep track of the item before and after.
        private static ItemStack _currentItemStack;
        private static int _sellPrice;

        public static bool Prefix(ItemActionEntrySell __instance)
        {
            var xuiCItemStack = (XUiC_ItemStack)__instance.ItemController;

            _currentItemStack = xuiCItemStack.itemStack;
            var num = xuiCItemStack.InfoWindow.BuySellCounter.Count;
            var forId = ItemClass.GetForId(_currentItemStack.itemValue.type);
            _sellPrice =
                XUiM_Trader.GetSellPrice(__instance.ItemController.xui, _currentItemStack.itemValue, num, forId);

            return true;
        }

        public static void Postfix(ItemActionEntryPurchase __instance)
        {
            var xuiCItemStack = (XUiC_ItemStack)__instance.ItemController;
            if (xuiCItemStack.itemStack.IsEmpty())
            {
                OnSell.SellItem(_currentItemStack, _currentItemStack.count, _sellPrice);
                return;
            }

            if (_currentItemStack.itemValue.type == xuiCItemStack.itemStack.itemValue.type)
            {
                // Same item, but different quantity
                if (_currentItemStack.count != xuiCItemStack.itemStack.count)
                {
                    OnSell.SellItem(_currentItemStack, _currentItemStack.count - xuiCItemStack.itemStack.count, _sellPrice);
                }

                return;
            }

            OnSell.SellItem(_currentItemStack, _currentItemStack.count, _sellPrice);
        }
    }


    [HarmonyPatch(typeof(ItemActionEntryPurchase))]
    [HarmonyPatch(nameof(ItemActionEntryPurchase.OnActivated))]
    public class ItemActionEntryPurchaseOnActivated
    {
        // Used to keep track of the item before and after.
        private static ItemStack _currentItemStack;
        private static int _buyPrice;

        public static bool Prefix(ItemActionEntryPurchase __instance)
        {
            _currentItemStack = ((XUiC_TraderItemEntry)__instance.ItemController).Item.Clone();
            var num = ((XUiC_TraderItemEntry)__instance.ItemController).InfoWindow.BuySellCounter.Count;
            var forId = ItemClass.GetForId(_currentItemStack.itemValue.type);
            _buyPrice = XUiM_Trader.GetBuyPrice(__instance.ItemController.xui, _currentItemStack.itemValue, num, forId,
                ((XUiC_TraderItemEntry)__instance.ItemController).SlotIndex);

            return true;
        }

        public static void Postfix(ItemActionEntryPurchase __instance)
        {
            var currentItem = ((XUiC_TraderItemEntry)__instance.ItemController).Item;
           
            if (currentItem == null || currentItem.IsEmpty())
            {
                OnBought.BoughtItem(_currentItemStack, _currentItemStack.count, _buyPrice);
                return;
            }

            if (_currentItemStack.itemValue.type == currentItem.itemValue.type)
            {
                // Same item, but different quantity
                if (_currentItemStack.count != currentItem.count)
                {
                    OnBought.BoughtItem(_currentItemStack, _currentItemStack.count - currentItem.count, _buyPrice);
                }

                return;
            }

            OnBought.BoughtItem(_currentItemStack, _currentItemStack.count, _buyPrice);
        }
    }
}