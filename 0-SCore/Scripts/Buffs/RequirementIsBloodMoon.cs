// 	<requirement name="RequirementIsBloodMoonDMT, SCore" invert="true" />

using System.Xml.Linq;

public class RequirementIsBloodMoonDMT : RequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        var isBloodMood = SkyManager.IsBloodMoonVisible();
        if (invert) return !isBloodMood;
        return isBloodMood;

    }
}