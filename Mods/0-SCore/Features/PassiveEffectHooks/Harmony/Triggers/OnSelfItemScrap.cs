using HarmonyLib;
using UnityEngine;


public static class OnSelfItemScrap
{
    public static void CheckForScrapping(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty()) return;

        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            ItemValue = stack.itemValue,
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };

        
        stack.itemValue.ItemClass.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfScrapItem, minEventParams);
        // minEventParams.Self.MinEventContext = minEventParams;
        // minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfScrapItem);
    }
}