// 	<requirement name="RequirementIsBloodMoonDMT, SCore" />

public class RequirementIsBloodMoonDMT : RequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        if (SkyManager.IsBloodMoonVisible())
            return true;
        return false;
    }
}