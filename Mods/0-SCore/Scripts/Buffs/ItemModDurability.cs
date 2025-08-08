// Happens when the item is 100% broken
// 	<requirement name="ItemModDurability, SCore" operation="Equals" value="1"/>

// Happens when the item is 50% broken
// 	<requirement name="ItemModDurability, SCore" operation="Equals" value="0.5"/>

public class ItemModDurability : TargetedCompareRequirementBase
{
    private string item_modifier;
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
            return false;

        return compareValues(_params.ItemValue.PercentUsesLeft, operation, value);
    }

}