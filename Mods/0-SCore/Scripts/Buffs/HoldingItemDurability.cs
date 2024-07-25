// Happens when the item is 100% broken
// 	<requirement name="HoldingItemDurability, SCore" operation="Equals" value="1"/>

// Happens when the item is 50% broken
// 	<requirement name="HoldingItemDurability, SCore" operation="Equals" value="0.5"/>

using UnityEngine;

public class HoldingItemDurability : TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!this.ParamsValid(_params))
            return false;

        if (this.target == null)
            return false;

        var itemValue = this.target.inventory.holdingItemItemValue;
        var percent = itemValue.UseTimes / itemValue.MaxUseTimes;
//        Debug.Log($"Item UseTime: {itemValue.UseTimes}  MaxUser Time: {itemValue.MaxUseTimes}  Percent: {percent}, Operation: {operation.ToString()} Value: {value}");
        return compareValues(percent, this.operation, value);
    }
}