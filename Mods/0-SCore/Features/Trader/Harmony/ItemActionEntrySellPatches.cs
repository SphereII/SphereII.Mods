using HarmonyLib;

public class ItemActionEntrySellPatches
{
    // Blocks selling the trader's custom currency.
    [HarmonyPatch(typeof(ItemActionEntrySell))]
    [HarmonyPatch(nameof(ItemActionEntrySell.RefreshEnabled))]
    public class ItemActionEntrySellRefreshEnabled
    {
        public static void Postfix(ItemActionEntrySell __instance)
        {
            if (__instance.Enabled == false) return;

            var id = TraderUtils.GetCurrentTraderID();
            if (!TraderCurrencyManager.HasCustomCurrency(id)) return;

            var customCurrency= TraderCurrencyManager.GetTraderCurrency(id);
            var xuiC_ItemStack = (XUiC_ItemStack)__instance.ItemController;
            if (xuiC_ItemStack.itemStack.IsEmpty()) return;
            var itemStack2 = xuiC_ItemStack.ItemStack.Clone();
            var itemClass = itemStack2.itemValue.ItemClass;
            if (itemClass.GetItemName().EqualsCaseInsensitive(customCurrency))
            {
                __instance.state = ItemActionEntrySell.StateTypes.ItemNotSellable;
                __instance.Enabled= false;
            }
        }
    }
}
