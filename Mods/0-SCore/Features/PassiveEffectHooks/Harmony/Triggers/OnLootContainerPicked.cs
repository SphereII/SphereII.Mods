using HarmonyLib;
using UnityEngine;

public static class OnLootContainerPicked
{
    public static void onLootContainerPicked(BlockValue blockValue)
    {
        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            BlockValue = blockValue,
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };

        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfLockpickSuccess);

    }
}