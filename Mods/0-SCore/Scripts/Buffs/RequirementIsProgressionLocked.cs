// 				<requirement name="!RequirementIsProgressionLocked, SCore" progression_name="attPerception" />

public class RequirementIsProgressionLocked : ProgressionLevel
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
        {
            return false;
        }

        if (target.Progression == null) return false;
        pv = target.Progression.GetProgressionValue(this.progressionId);
        if (pv == null) return false;
        var result = pv.IsLocked(_params.Self);
        if (invert)
        {
            return !result;
        }

        return result;
    }
}
