// Happens when the item is 100% broken
// 	<requirement name="ItemPercentUsed, SCore" operation="Equals" value="1"/>

// Happens when the item is 50% broken
// 	<requirement name="ItemPercentUsed, SCore" operation="Equals" value="0.5"/>

using System;
using UnityEngine;

public class ItemPercentUsed : TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params)) return false;
        if (_params.ItemValue == null) return false;
        var percent = _params.ItemValue.UseTimes / _params.ItemValue.MaxUseTimes;
        return compareValues(percent, operation, value);

    }

}