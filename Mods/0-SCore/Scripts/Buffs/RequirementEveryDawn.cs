//  Happens at the 7th hour
// 	<requirement name="RequirementEveryDawn, SCore" value="7"/>

public class RequirementEveryDawn : RequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        var hour = GameUtils.WorldTimeToHours(GameManager.Instance.World.GetWorldTime());
        if (hour == value)
            return true;
        return false;
    }
}