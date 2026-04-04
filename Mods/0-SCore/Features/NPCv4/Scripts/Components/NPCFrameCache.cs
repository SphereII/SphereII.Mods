using System.Collections.Generic;
using UAI;
using UnityEngine;

/// <summary>
/// Populated exactly once at the top of EntityAliveSDXV4.OnUpdateLive and passed by
/// reference to every component.  Ensures that expensive world/entity lookups such as
/// GetLeaderOrOwner and GetAttackOrRevengeTarget happen only once per tick regardless of
/// how many components or methods need that data.
/// </summary>
public struct NPCFrameCache
{
    /// <summary>The entity's current leader/owner, or null.</summary>
    public EntityAlive  Leader;

    /// <summary>Leader cast to EntityPlayer, or null when the leader is not a player.</summary>
    public EntityPlayer LeaderAsPlayer;

    /// <summary>Pre-calculated distance to the leader. Zero when there is no leader.</summary>
    public float        DistanceToLeader;

    /// <summary>
    /// Squared distance to the leader (no sqrt cost).  Use for comparisons where the
    /// exact distance is not needed — e.g. <c>if (cache.DistanceSqToLeader > threshold * threshold)</c>.
    /// Zero when there is no leader.
    /// </summary>
    public float        DistanceSqToLeader;

    /// <summary>True when a leader exists.</summary>
    public bool         HasLeader;

    /// <summary>True when the leader is the local player on this machine.</summary>
    public bool         IsLeaderLocalPlayer;

    /// <summary>The entity's current attack or revenge target, or null.</summary>
    public EntityAlive  AttackOrRevengeTarget;

    /// <summary>
    /// Pre-computed search bounds centred on the entity with a radius of
    /// <see cref="AIConstants.DefaultNearbySearchRadius"/>.  Used by V4 utility methods
    /// that need a bounds for entity queries so the allocation happens once per tick.
    /// </summary>
    public Bounds SearchBounds;

    /// <summary>
    /// Shared, pooled buffer for entity-search results.  V4 utility methods clear this
    /// list before use rather than allocating a new one each call, eliminating per-call
    /// GC pressure from <c>GetEntitiesInBounds</c> queries.
    /// <para>
    /// <b>Not thread-safe.</b>  All AI updates in 7 Days to Die run on the main thread,
    /// so sharing a single static instance is safe.  Callers must call
    /// <c>EntityBuffer.Clear()</c> before passing it to any world query.
    /// </para>
    /// </summary>
    public static readonly List<Entity> EntityBuffer
        = new List<Entity>(AIConstants.EntityBufferInitialCapacity);

    /// <summary>
    /// Fills every field of the cache for the current tick.
    /// Call this once at the start of OnUpdateLive before updating any component.
    /// </summary>
    public void Populate(EntityAliveSDXV4 entity)
    {
        Leader              = EntityUtilities.GetLeaderOrOwner(entity.entityId) as EntityAlive;
        HasLeader           = Leader != null;
        LeaderAsPlayer      = Leader as EntityPlayer;
        // Compute squared distance first (no sqrt); derive DistanceToLeader only if needed.
        DistanceSqToLeader  = HasLeader ? (entity.position - Leader.position).sqrMagnitude : 0f;
        DistanceToLeader    = HasLeader ? UnityEngine.Mathf.Sqrt(DistanceSqToLeader) : 0f;
        IsLeaderLocalPlayer = HasLeader && GameManager.Instance.World.IsLocalPlayer(Leader.entityId);
        AttackOrRevengeTarget = EntityUtilities.GetAttackOrRevengeTarget(entity.entityId) as EntityAlive;

        var r = AIConstants.DefaultNearbySearchRadius;
        SearchBounds = new Bounds(entity.position, new Vector3(r, r, r));
    }
}
