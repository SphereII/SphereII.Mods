
// Moves the entity to where the player is standing
//        <triggered_effect trigger="onSelfBuffUpdate" action="GuardHere, SCore" />
using UnityEngine;

public class MinEventActionGuardHere : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var entityAliveSDX = _params.Self as EntityAliveSDX;
        if (entityAliveSDX == null) return;

        var leader = EntityUtilities.GetLeaderOrOwner(_params.Self.entityId) as EntityAlive;
        if ( leader == null) return;

        entityAliveSDX.guardPosition = leader.position;

        entityAliveSDX.guardLookPosition = leader.position + leader.GetForwardVector();
    }
}