using HarmonyLib;
using UnityEngine;

public static class OnBought
{
    public static void BoughtItem(string traderName, int value)
    {
        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            ItemValue = ItemValue.None
        };

        var currentTrader = GameManager.Instance.World.GetEntity(minEventParams.TileEntity.EntityId) as EntityNPC;
        minEventParams.Other = currentTrader;

        // We want to set the _item_value to something, just in case this is the first time.        
        var previousItemValue = minEventParams.Self.Buffs.GetCustomVar("_item_value");
        if (previousItemValue == 0)
            previousItemValue = 1f;
        minEventParams.Self.Buffs.AddCustomVar("_last_item_value", previousItemValue);
        minEventParams.Self.Buffs.AddCustomVar("_item_value", value);
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfItemBought);
    }
    
    public static void BoughtItem(ItemStack itemStack, int count, int buyPrice)
    {
        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            ItemValue = itemStack.itemValue
        };

        minEventParams.Self.Buffs.AddCustomVar("_totalBought", count);
        minEventParams.Self.Buffs.AddCustomVar("_buyPrice", buyPrice);

        var currentTrader = GameManager.Instance.World.GetEntity(minEventParams.TileEntity.EntityId) as EntityNPC;
        minEventParams.Other = currentTrader;
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfItemBought);
        minEventParams.Self.Buffs.AddCustomVar("_totalBought", 0f);
        minEventParams.Self.Buffs.AddCustomVar("_buyPrice", 0);

    }

}