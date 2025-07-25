using HarmonyLib;
using UnityEngine;

public static class OnRepair
{
    public static void CheckForDegradation(ItemValue stack)
    {
        if (stack == null || stack.IsEmpty()) return;

        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            ItemValue = stack,
            Self = GameManager.Instance.World.GetPrimaryPlayer()
        };
        stack.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfItemRepaired);
    }
}