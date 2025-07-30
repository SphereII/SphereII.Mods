// Happens when the item is 100% broken
// 	<requirement name="HoldingItemDurability, SCore" value="1"/>

// Happens when the item is 50% broken
// 	<requirement name="ItemPercentDamaged, SCore" operation="GTE" value="0.5"/>


using System.Xml.Linq;
using UnityEngine;

public class ItemPercentDamaged : TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
            return false;

        if (target == null)
            return false;


        var itemValue = _params.ItemValue;
        if (itemValue == null || itemValue.Equals(ItemValue.None)) return false;
        if (!itemValue.HasMetadata("PercentDamaged")) return false;
        if (itemValue.GetMetadata("PercentDamaged") is not float percentDamaged) return false;

        if (!invert)
        {
            return compareValues(percentDamaged, operation, value);
        }
        return !compareValues(percentDamaged, operation, value);

    }
  
}