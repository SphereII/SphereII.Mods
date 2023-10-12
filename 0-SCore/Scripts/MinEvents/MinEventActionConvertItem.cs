//        <triggered_effect trigger="onSelfPrimaryActionEnd" action="ConvertItem, SCore" downgradeItem="meleeClub" maxUsage="10" />

using System;
using System.Xml;
using System.Xml.Linq;
using Audio;
using UnityEngine;

public class MinEventActionConvertItem : MinEventActionTargetedBase
{
    private string _itemName = string.Empty;
    private int _maxUsage = -1;

    public override void Execute(MinEventParams _params)
    {
        // No effect when it's -1
        if (_maxUsage == -1) return;

        var currentItem = _params.Self.inventory.holdingItemItemValue;
        if (!currentItem.HasMetadata("MaxUsage"))
            currentItem.SetMetadata("MaxUsage", _maxUsage, TypedMetadataValue.TypeTag.Integer);

        var currentUsage = (int) currentItem.GetMetadata("MaxUsage");
        currentUsage--;
        if (currentUsage <= 0)
        {
            var item = ItemValue.None;
            if ( !string.IsNullOrEmpty(_itemName))
                item = ItemClass.GetItem(_itemName);
            
            Manager.BroadcastPlay(_params.Self, "itembreak");
            _params.Self.inventory.SetItem(_params.Self.inventory.holdingItemIdx, item, 1);
            return;
        }
       
        currentItem.SetMetadata("MaxUsage", currentUsage, TypedMetadataValue.TypeTag.Integer);
        
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;
        if (name == "downgradeItem")
        {
            _itemName = _attribute.Value;
        }

        if (name == "maxUsage")
        {
            _maxUsage = StringParsers.ParseSInt32(_attribute.Value);
        }

        return true;
    }
}