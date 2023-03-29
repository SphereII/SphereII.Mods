// Happens when the item is 100% broken
// 	<requirement name="HoldingItemDurability, SCore" value="1"/>

// Happens when the item is 50% broken
// 	<requirement name="HoldingItemDurability, SCore" value="0.5"/>

public class RequirementHoldingItemDurability : TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!this.ParamsValid(_params))
            return false;

        if (this.target == null)
            return false;

        var itemValue = this.target.inventory.holdingItemItemValue;
        var percent = itemValue.UseTimes / itemValue.MaxUseTimes;
        return !compareValues(percent, this.operation, value);
    }
}