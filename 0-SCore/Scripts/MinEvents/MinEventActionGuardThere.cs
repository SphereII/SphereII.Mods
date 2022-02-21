

// sets the guard position to where the entity is standing.
// <triggered_effect trigger="onSelfBuffUpdate" action="GuardThere, SCore" />
using UnityEngine;

public class MinEventActionGuardThere : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var entityAliveSDX = _params.Self as EntityAliveSDX;
        if (entityAliveSDX == null) return;

        entityAliveSDX.guardPosition = _params.Self.position;
        entityAliveSDX.guardLookPosition = _params.Self.position + _params.Self.GetLookVector();
    }
}