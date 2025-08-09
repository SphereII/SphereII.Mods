// 				<requirement name="!RequirementIsProgressionLocked, SCore" progression_name="attPerception" />

using UnityEngine;

public class RequirementIsProgressionLocked : ProgressionLevel
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
        {
            return false;
        }

        if (_params.Self.Progression == null) return false;
        pv = _params.Self.Progression.GetProgressionValue(this.progressionId);
        if (pv == null) return false;
        var result = pv.CanPurchase(_params.Self, pv.Level);
        if (invert)
        {
            return !result;
        }

        return result;
    }
}
