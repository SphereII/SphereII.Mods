
// Moves the entity to where the player is standing
//        <triggered_effect trigger="onSelfBuffUpdate" action="GuardHere, SCore" />
using UnityEngine;

public class MinEventActionGuardHere : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        if (!(_params.Self is IEntityOrderReceiverSDX entityOrderReceiver)) return;

        var leader = EntityUtilities.GetLeaderOrOwner(_params.Self.entityId) as EntityAlive;
        if ( leader == null) return;

        entityOrderReceiver.GuardPosition = leader.position;

        entityOrderReceiver.GuardLookPosition = leader.position + leader.GetForwardVector();
    }
}