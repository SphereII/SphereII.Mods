// 				<requirement name="CanPurchasePerk, SCore" progression_name="attPerception" />

using UnityEngine;

public class CanPurchasePerk : ProgressionLevel
{
    public override bool IsValid(MinEventParams _params)
    {
        // Consolidate all early exit conditions into a single check.
        if (_params.Self.Progression == null) return false;

        pv = _params.Self.Progression.GetProgressionValue(this.progressionId);
        if (pv == null) return false;
        
        var result = pv.CanPurchase(_params.Self, pv.level);
        var maxLevel = ProgressionClass.GetCalculatedMaxLevel(_params.Self, pv);

        // Check if the current level is below the maximum allowed level.
        if (maxLevel < pv.Level + 1)
        {
            result = false;
        }

        // Return the final result, gated by the 'invert' flag.
        return invert ? !result : result;
    }
}