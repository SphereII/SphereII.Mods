// Happens when the item is 100% broken
// 	<requirement name="ItemPercentUsed, SCore" operation="Equals" value="1"/>

// Happens when the item is 50% broken
// 	<requirement name="ItemPercentUsed, SCore" operation="Equals" value="0.5"/>
//
// optional: Display a log line if item is specified for debugging only.
// 	<requirement name="ItemPercentUsed, SCore" operation="Equals" value="0.5" tracked_item="meleeToolFlashlight02"/>

using System;
using System.Xml.Linq;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;

public class ItemPercentUsed : TargetedCompareRequirementBase
{
    private string tracked_item = "";

    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params)) return false;

        if (_params.ItemValue == null) return false;

        var percent = ItemDegradationHelpers.GetPercentUsed(_params.ItemValue);
        if ( !string.IsNullOrEmpty(tracked_item) && tracked_item.EqualsCaseInsensitive(_params.ItemValue.ItemClass.GetItemName()))
            Log.Out($"ItemValue: {_params.ItemValue.ItemClass.GetItemName()} :: {_params.ItemValue.UseTimes} / {ItemDegradationHelpers.GetMaxUseTimes(_params.ItemValue)} = {percent}");
        
        
        return compareValues(percent, operation, value);
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        string localName = _attribute.Name.LocalName;
        if (localName == "tracked_item")
        {
            tracked_item = _attribute.Value.ToString();
            return true;
        }

        return base.ParseXAttribute(_attribute);
    }
}