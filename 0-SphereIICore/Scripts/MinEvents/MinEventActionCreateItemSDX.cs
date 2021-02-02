using System.Globalization;
using System.Xml;
using UnityEngine;

// <triggered_effect trigger="onSelfBuffRemove" action="CreateItem" item="drinkJarCoffee" count="2"/>
// <triggered_effect trigger="onSelfBuffRemove" action="CreateItem" lootgroup="2" count="1" />

public class MinEventActionCreateItemSDX : MinEventActionBase
{
    public override void Execute(MinEventParams _params)
    {
        EntityPlayerLocal entityPlayer = _params.Self as EntityPlayerLocal;

        // Loot group
        if (_params.Self as EntityPlayerLocal != null && lootgroup > 0)
        {
            ItemStack[] array = LootContainer.lootList[lootgroup].Spawn(GameManager.Instance.lootManager.Random, CreateItemCount, EffectManager.GetValue(PassiveEffects.LootGamestage, null, entityPlayer.HighestPartyGameStage, entityPlayer, null, default(FastTags), true, true, true, true, 1, true), 0f, entityPlayer, new FastTags());
            for (int i = 0; i < array.Length; i++)
            {
                if (!LocalPlayerUI.GetUIForPlayer(entityPlayer).xui.PlayerInventory.AddItem(array[i], true))
                    entityPlayer.world.gameManager.ItemDropServer(array[i], entityPlayer.GetPosition(), Vector3.zero, -1, 60f, false);
            }
            return;
        }

        // item value.
        if (_params.Self as EntityPlayerLocal != null && CreateItem != null && CreateItemCount > 0)
        {
            ItemStack itemStack = new ItemStack(ItemClass.GetItem(CreateItem, false), CreateItemCount);
            if (!LocalPlayerUI.GetUIForPlayer(entityPlayer).xui.PlayerInventory.AddItem(itemStack, true))
            {
                entityPlayer.world.gameManager.ItemDropServer(itemStack, entityPlayer.GetPosition(), Vector3.zero, -1, 60f, false);
            }
        }
    }

    public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
    {
        return base.CanExecute(_eventType, _params) && _params.Self as EntityPlayerLocal != null;
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "item")
                {

                    CreateItem = _attribute.Value;
                    return true;
                }

                if (name == "lootgroup")
                {
                    lootgroup = (int)StringParsers.ParseSInt64(_attribute.Value, 0, -1, NumberStyles.Any);
                    return true;
                }
                if (name == "count")
                {
                    CreateItemCount = (int)StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
                    return true;
                }
            }
        }
        return flag;
    }

    private string CreateItem = string.Empty;
    private int lootgroup = -1;
    private int CreateItemCount = 1;
}
