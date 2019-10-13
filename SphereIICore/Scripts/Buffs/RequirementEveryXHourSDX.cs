using System;
using System.Xml;

using UnityEngine;
//  Happens at the 7th hour
// 	<requirement name="RequirementEveryXHourSDX, Mods" value="7"/>
public class RequirementEveryXHourSDX : RequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        int hour = GameUtils.WorldTimeToHours(GameManager.Instance.World.GetWorldTime());
        if (hour == this.value)
        {
            Debug.Log("Requirement hour match: " + hour);
            return true;
        }
        return false;
    }
}
