/// <summary>
/// Sets the guard position to where the leader or owner is standing.
/// If there is no leader or owner, for example if the order was given by a pathing cube,
/// then it sets the guard position to where the entity is standing.
/// 
/// <example>
/// <code>
/// &lt;triggered_effect trigger="onSelfBuffUpdate" action="GuardHere, SCore" /&gt;
/// </code>
/// </example>
/// </summary>
public class MinEventActionGuardHere : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        if (!(_params.Self is IEntityOrderReceiverSDX entityOrderReceiver)) return;

        if (EntityUtilities.GetLeaderOrOwner(_params.Self.entityId) is EntityAlive leader)
        {
            // When setting the guard position from a leader, use the center of the block they're
            // standing on to avoid (literal) edge cases with pathing.
            entityOrderReceiver.GuardPosition = EntityUtilities.CenterPosition(leader.position);

            entityOrderReceiver.GuardLookPosition = leader.position + leader.GetForwardVector();

            return;
        }
        
        // When setting the guard position from our own position, we want to go back to exactly
        // that position, even if it's not the center of the block we're standing on. Otherwise
        // we may end up in weird places, like on top of a railing next to where we're standing.
        entityOrderReceiver.GuardPosition = _params.Self.position;

        entityOrderReceiver.GuardLookPosition = _params.Self.position + _params.Self.GetLookVector();
    }
}