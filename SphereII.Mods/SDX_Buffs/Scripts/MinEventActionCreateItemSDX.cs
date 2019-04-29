using System;
using System.Globalization;
using System.Xml;
using UnityEngine;

// <triggered_effect trigger="onSelfBuffRemove" action="CreateItem" item="drinkJarCoffee" count="2"/>

public class MinEventActionCreateItemSDX : MinEventActionBase
{
    public override void Execute(MinEventParams _params)
    {
        EntityPlayerLocal entityPlayer = _params.Self as EntityPlayerLocal;
        if (_params.Self as EntityPlayerLocal != null && this.CreateItem != null && this.CreateItemCount > 0)
        {
            ItemStack itemStack = new ItemStack(ItemClass.GetItem(this.CreateItem, false), this.CreateItemCount);
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
                    this.CreateItem = _attribute.Value;
                    return true;
                }
                if (name == "count")
                {
                    this.CreateItemCount = (int)StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
                    return true;
                }
            }
        }
        return flag;
    }

    private string CreateItem = string.Empty;

    private int CreateItemCount = 1;
}
