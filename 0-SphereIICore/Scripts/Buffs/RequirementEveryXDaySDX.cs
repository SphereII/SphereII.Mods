//  Happens every 7th day
// 	<requirement name="RequirementEveryXDaySDX, Mods" value="7"/>
// 	<requirement name="RequirementEveryXDaySDX, Mods" value="0"/>  <- trigger on the blood moon
public class RequirementEveryXDaySDX : RequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        int day = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
        float currentValue = value;

        // If the value is 0, then read the Blood Moon day event.
        if (value == 0)
        {
            currentValue = GameStats.GetInt(EnumGameStats.BloodMoonDay);
            if (day == currentValue)
                return true;
        }
        if (day % currentValue == 0) // Blood Moon Day Events
            return true;


        return false;
    }
}
