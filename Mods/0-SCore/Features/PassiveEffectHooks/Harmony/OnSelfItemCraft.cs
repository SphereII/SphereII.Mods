using HarmonyLib;
using UnityEngine;


public static class OnCraft
{
    public static void CheckForCrafting(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty()) return;

        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            ItemValue = stack.itemValue,
            Self = GameManager.Instance.World.GetPrimaryPlayer()
        };

        stack.itemValue.ItemClass.FireEvent(MinEventTypes.onSelfItemCrafted, minEventParams);
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfItemCrafted);
    }
}