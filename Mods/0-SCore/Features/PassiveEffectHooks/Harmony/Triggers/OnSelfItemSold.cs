using UnityEngine;

public static class OnSell
{
    public static void SellItem(string traderName, int value)
    {
        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            ItemValue = ItemValue.None,
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };
        var currentTrader = GameManager.Instance.World.GetEntity(minEventParams.TileEntity.EntityId) as EntityNPC;
        minEventParams.Other = currentTrader;

        // We want to set the _item_value to something, just case this is the first time.        
        var previousItemValue = minEventParams.Self.Buffs.GetCustomVar("_item_value");
        if (previousItemValue == 0)
            previousItemValue = 1f;

        minEventParams.Self.Buffs.AddCustomVar("_last_item_value", previousItemValue);
        minEventParams.Self.Buffs.AddCustomVar("_item_value", value);
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfItemSold);
    }
    
    public static void SellItem(ItemStack itemStack, int count, int sellPrice)
    {
        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            ItemValue = itemStack.itemValue
        };
        
        minEventParams.Self.Buffs.AddCustomVar("_totalSold", count);
        minEventParams.Self.Buffs.AddCustomVar("_sellPrice", sellPrice);

        var currentTrader = GameManager.Instance.World.GetEntity(minEventParams.TileEntity.EntityId) as EntityNPC;
        minEventParams.Other = currentTrader;
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfItemSold);
        minEventParams.Self.Buffs.AddCustomVar("_totalSold", 0f);
        minEventParams.Self.Buffs.AddCustomVar("_sellPrice", 0f);

    }
}