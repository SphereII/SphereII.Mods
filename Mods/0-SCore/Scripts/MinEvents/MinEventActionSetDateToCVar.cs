using UnityEngine;

//  <triggered_effect trigger="onSelfBuffStart" action="SetDateToCVar, SCore" target="self"/> 

public class MinEventActionSetDateToCVar : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {

        var day = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
        _params.Self.Buffs.AddCustomVar("$CurrentDay", day);

        base.Execute(_params);
    }
}
