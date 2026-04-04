/// <summary>
/// Owns per-tick combat-state maintenance and acts as a gatekeeper for
/// SetRevengeTarget / SetAttackTarget changes.
///
/// Key improvement over EntityAliveSDX:
///   - Alert-flag clearing reads from the pre-populated NPCFrameCache instead of calling
///     GetAttackOrRevengeTarget a second time inside OnUpdateLive.
/// </summary>
public class NPCCombatComponent : INPCComponent
{
    private EntityAliveSDXV4 _entity;

    // -----------------------------------------------------------------------
    // INPCComponent
    // -----------------------------------------------------------------------

    public void Initialize(EntityAliveSDXV4 entity) => _entity = entity;

    public void OnUpdateLive(ref NPCFrameCache cache)
    {
        // If alerted but the cached target is already dead, clear the alert flag.
        // Uses the frame-cache value — no extra GetAttackOrRevengeTarget call.
        if (_entity.IsAlert
            && cache.AttackOrRevengeTarget != null
            && cache.AttackOrRevengeTarget.IsDead())
        {
            _entity.bReplicatedAlertFlag = false;
        }
    }

    public void OnDead()   { }
    public void OnUnload() { }

    // -----------------------------------------------------------------------
    // Target validation helpers — called from the entity's overridden methods
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns true if the proposed revenge target is valid for this NPC.
    /// Blocks friendly fire from leader and allied NPCs, and ignores dead targets.
    /// </summary>
    public bool ShouldAllowRevengeTarget(EntityAlive proposed)
    {
        if (proposed == null)     return true;  // clearing the target is always allowed
        if (proposed.IsDead())    return false;

        var myLeader = EntityUtilities.GetLeaderOrOwner(_entity.entityId) as EntityAlive;
        if (myLeader != null && myLeader.entityId == proposed.entityId)
            return false;

        return !EntityUtilities.IsAnAlly(_entity.entityId, proposed.entityId);
    }

    /// <summary>
    /// Returns true if the proposed attack target should be applied.
    /// Blocks clearing a living target (some AI tasks reset on stun — we preserve the target).
    /// </summary>
    public bool ShouldAllowAttackTarget(EntityAlive proposed)
    {
        if (proposed == null)
        {
            // Prevent clearing the target while the current victim is still alive.
            return _entity.attackTarget == null || !_entity.attackTarget.IsAlive();
        }

        return !proposed.IsDead();
    }
}
