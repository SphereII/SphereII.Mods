//        <triggered_effect trigger = "onSelfBuffUpdate" action="SetHomePosition, SCore"  />

public class MinEventActionSetHomePosition : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        _params.Self.setHomeArea(new Vector3i(_params.Self.position), 1);
    }
}