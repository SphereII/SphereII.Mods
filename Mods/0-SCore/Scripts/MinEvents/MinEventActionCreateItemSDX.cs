using System.Xml.Linq;
using UnityEngine;

// <triggered_effect trigger="onSelfBuffRemove" action="CreateItemSDX, SCore" item="drinkJarCoffee" count="2"/>
// <triggered_effect trigger="onSelfBuffRemove" action="CreateItemSDX, SCore" lootgroup="2" count="1" />
// <triggered_effect trigger="onSelfBuffRemove" action="CreateItemSDX, SCore" lootgroup2="LootList" count="1" />


public class MinEventActionCreateItemSDX : MinEventActionBase
{
    private string _createItem = string.Empty;
    private int _createItemCount = 1;
    private string _lootGroup = "";
    private string _lootGroup2 = "";

    public override void Execute(MinEventParams _params)
    {
        var entityPlayer = _params.Self as EntityPlayerLocal;

        // Loot group
        if (_params.Self as EntityPlayerLocal != null && !string.IsNullOrEmpty(_lootGroup))
        {
            if (!string.IsNullOrEmpty(_lootGroup2))
            {
                var item2 = LootContainer.GetRewardItem(_lootGroup2, 1f);
                if (!LocalPlayerUI.GetUIForPlayer(entityPlayer).xui.PlayerInventory.AddItem(item2, true))
                    entityPlayer.world.gameManager.ItemDropServer(item2, entityPlayer.GetPosition(), Vector3.zero);
                return;
            }
            var container = LootContainer.GetLootContainer(_lootGroup);
            if (container == null) return;

            var array = container.Spawn(_params.Self.rand, _createItemCount, (float)_params.Self.Progression.GetLevel(), 0f, null, new FastTags<TagGroup.Global>(), container.UniqueItems, false);
            foreach (var t in array)
                if (!LocalPlayerUI.GetUIForPlayer(entityPlayer).xui.PlayerInventory.AddItem(t, true))
                    entityPlayer.world.gameManager.ItemDropServer(t, entityPlayer.GetPosition(), Vector3.zero);
            return;
        }

        // item value.
        if (_params.Self as EntityPlayerLocal == null || _createItem == null || _createItemCount <= 0) return;

        var item = ItemClass.GetItem(_createItem);
        if (item == null)
        {
            Log.Out($"MinEventActionCreateItemSDX:: No such item: {_createItem}");
            return;
        }
        var itemStack = new ItemStack(item, _createItemCount);
        if (!LocalPlayerUI.GetUIForPlayer(entityPlayer).xui.PlayerInventory.AddItem(itemStack, true))
            entityPlayer.world.gameManager.ItemDropServer(itemStack, entityPlayer.GetPosition(), Vector3.zero);
    }

    public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
    {
        return base.CanExecute(_eventType, _params) && _params.Self as EntityPlayerLocal != null;
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;

        switch (name)
        {
            case null:
                return flag;
            case "item":
                _createItem = _attribute.Value;
                return true;
            case "lootgroup":
                _lootGroup = _attribute.Value;
                return true;
            case "lootgroup2":
                _lootGroup2 = _attribute.Value;
                return true;
            case "count":
                _createItemCount = (int)StringParsers.ParseFloat(_attribute.Value);
                return true;
            default:
                return false;
        }
    }
}