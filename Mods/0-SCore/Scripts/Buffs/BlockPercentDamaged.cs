// Happens when the item is 100% broken
// 	<requirement name="HoldingItemDurability, SCore" value="1"/>

// Happens when the item is 50% broken
// 	<requirement name="BlockPercentDamaged, SCore" operation="GTE" value="0.5"/>


using System.Xml.Linq;
using UnityEngine;

public class BlockPercentDamaged : TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        var damaged = _params.BlockValue.damage;
        float percentDamaged = (float)damaged / _params.BlockValue.Block.MaxDamage;
        if (!invert)
        {
            return compareValues(percentDamaged, operation, value);
        }
        return !compareValues(percentDamaged, operation, value);

    }
  
}