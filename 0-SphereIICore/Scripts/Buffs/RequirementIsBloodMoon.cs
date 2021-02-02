// 	<requirement name="RequirementIsBloodMoonDMT, Mods" />
public class RequirementIsBloodMoonDMT : RequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        if ( SkyManager.BloodMoon())
            return true;
        return false;
    }
}

