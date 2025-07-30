using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

// <!-- If the property exists, and has that value -->
// 	<requirement name="ItemHasProperty, SCore" property="UnlockedBy" prop_value="craftingHarvestingTools" />

// <!-- Just in general, does the property exist. -->
// <requirement name="ItemHasProperty, SCore" property="UnlockedBy" />

public class ItemHasProperty : TargetedCompareRequirementBase
{
    private string propertyName = "";
    private string propertyValue = "";

    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params)) return false;
        ItemClass itemClass;
        if (_params.ItemValue == null)
            itemClass = _params.BlockValue.ToItemValue().ItemClass;
        else
        {
            itemClass = _params.ItemValue.ItemClass;
            if (_params.ItemValue.HasMetadata("OriginalItemName"))
            {
                if (_params.ItemValue.GetMetadata("OriginalItemName") is string itemName)
                    itemClass = ItemClass.GetItemClass(itemName, true);
            }
        }

        var result = false;
        // No expected value, so just check if it's there at all.
        if (string.IsNullOrEmpty(propertyValue))
        {
            if (itemClass.Properties.Contains(propertyName))
                result = true;
        }
        else
        {
            if (!itemClass.Properties.Contains(propertyName)) return false;

            var propStringValue = itemClass.Properties.GetStringValue(propertyName);
            result = propStringValue.EqualsCaseInsensitive(propertyValue);
        }

        if (!invert)
        {
            return result;
        }

        return !result;
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        string localName = _attribute.Name.LocalName;
        if (localName == "property")
        {
            propertyName = _attribute.Value.ToString();
            return true;
        }

        if (localName == "prop_value")
        {
            propertyValue = _attribute.Value.ToString();
            return true;
        }

        bool flag = base.ParseXAttribute(_attribute);
        return flag;
    }
}