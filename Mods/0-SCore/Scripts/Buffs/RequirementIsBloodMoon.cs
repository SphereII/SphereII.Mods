// 	<requirement name="RequirementIsBloodMoonDMT, SCore" invert="true" />

public class RequirementIsBloodMoonDMT : RequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        var dayCount = (int)SkyManager.dayCount;
        var bloodMoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
        // Is today a blood moon day, and it's after 10? Or is it the day after blood moon, but before dawn
        return (dayCount == bloodMoonDay && SkyManager.TimeOfDay() >= 22f) || (dayCount > 1 && dayCount == bloodMoonDay + 1 && SkyManager.TimeOfDay() <= 4f);
        
        var isBloodMood = SkyManager.IsBloodMoonVisible();
        if (invert) return !isBloodMood;
        return isBloodMood;

    }
}