// 				<requirement name="!RequirementIsProgressionLocked, SCore" progression_name="attPerception" />

using UnityEngine;

public class RequirementIsProgressionLocked : ProgressionLevel
{
    public override bool IsValid(MinEventParams _params)
    {
        if (_params.Self.Progression == null) return false;
        pv = _params.Self.Progression.GetProgressionValue(this.progressionId);

        if (pv == null)
        {
            return false;
        }
        var result = pv.IsLocked(_params.Self);
        if (invert)
        {
            return !result;
        }

        return result;
    }
    
    
}
