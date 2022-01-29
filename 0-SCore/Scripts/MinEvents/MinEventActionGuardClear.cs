

// Clears the guard position
// <triggered_effect trigger="onSelfBuffUpdate" action="GuardClear, SCore" />
using UnityEngine;

public class MinEventActionGuardClear : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var entityAliveSDX = _params.Self as EntityAliveSDX;
        if (entityAliveSDX == null) return;

        entityAliveSDX.guardPosition = Vector3.zero;
        entityAliveSDX.guardLookPosition = Vector3.zero;
    }
}