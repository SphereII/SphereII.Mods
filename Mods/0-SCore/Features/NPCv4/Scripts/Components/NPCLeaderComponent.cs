using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns all leader-following concerns: collision passthrough near allies, companion-list
/// management, leader-cache expiry, and teleport-to-leader decisions.
///
/// Key improvements over EntityAliveSDX:
///   - Proximity check is throttled (ProximityCheckInterval) instead of running every frame.
///   - Entity query uses a pre-allocated buffer list — no per-tick heap allocation.
///   - Entity query filters to EntityPlayer at the World layer, skipping the manual cast loop.
///   - Companion IndexOf is called once; Count-1 is used after Add instead of a second IndexOf.
///   - Leader-cache expiry countdown lives here instead of as a raw int on the entity.
///   - LargeEntityBlocker transform is found once and cached.
/// </summary>
public class NPCLeaderComponent : INPCComponent
{
    // -----------------------------------------------------------------------
    // Configuration
    // -----------------------------------------------------------------------

    /// <summary>Seconds between collision/proximity recalculations.</summary>
    private const float ProximityCheckInterval = 0.5f;

    /// <summary>Ticks between forced leader-cache invalidations.</summary>
    private const int LeaderCacheExpiry = 30;

    /// <summary>Distance (units) at which a following NPC is teleported to the leader.</summary>
    private const float TeleportThreshold   = 60f;
    private const float TeleportThresholdSq = TeleportThreshold * TeleportThreshold;

    /// <summary>Distance (units) at which a vehicle-attached NPC is snapped to the leader.</summary>
    private const float VehicleSnapThreshold   = 10f;
    private const float VehicleSnapThresholdSq  = VehicleSnapThreshold * VehicleSnapThreshold;

    // -----------------------------------------------------------------------
    // State
    // -----------------------------------------------------------------------

    private EntityAliveSDXV4 _entity;
    private int   _leaderCacheCountdown = LeaderCacheExpiry;
    private float _lastProximityCheckTime = -999f;
    private Transform _largeEntityBlocker;

    /// <summary>Pre-allocated list reused on every proximity query — never re-allocated.</summary>
    private readonly List<Entity> _proximityBuffer = new List<Entity>();

    // -----------------------------------------------------------------------
    // INPCComponent
    // -----------------------------------------------------------------------

    public void Initialize(EntityAliveSDXV4 entity) => _entity = entity;

    public void OnUpdateLive(ref NPCFrameCache cache)
    {
        if (_entity.IsDead()) return;
        if (_entity.Buffs.HasBuff("buffOrderDismiss")) return;

        if (!cache.HasLeader)
        {
            _entity.Owner = null;
            _entity.bWillRespawn = false;
            return;
        }

        // Throttled: only re-evaluate ally collision every ProximityCheckInterval seconds.
        if (Time.time - _lastProximityCheckTime >= ProximityCheckInterval)
        {
            _lastProximityCheckTime = Time.time;
            CheckLeaderProximity();
        }

        // Set Owner reference and nav object on first contact.
        if (_entity.Owner == null)
        {
            _entity.Owner = cache.Leader;
            if (cache.IsLeaderLocalPlayer)
                _entity.HandleNavObject();
        }

        // Periodically force-clear the leader cache to pick up ownership changes.
        if (--_leaderCacheCountdown < 0)
        {
            _leaderCacheCountdown = LeaderCacheExpiry;
            SphereCache.LeaderCache.Remove(_entity.entityId);
        }

        // Keep the "hired_<id>" cvar on the leader so the party system tracks us.
        if (cache.LeaderAsPlayer != null)
            cache.Leader.Buffs.SetCustomVar($"hired_{_entity.entityId}", (float)_entity.entityId);

        switch (EntityUtilities.GetCurrentOrder(_entity.entityId))
        {
            case EntityUtilities.Orders.Loot:
            case EntityUtilities.Orders.Follow:
                HandleFollowOrder(ref cache);
                break;

            default:
                _entity.bWillRespawn = false;
                cache.LeaderAsPlayer?.Companions.Remove(_entity);
                break;
        }
    }

    public void OnDead()   { }
    public void OnUnload() { }

    // -----------------------------------------------------------------------
    // Proximity / collision
    // -----------------------------------------------------------------------

    /// <summary>
    /// Disables collision with the NPC when an allied player is within 2 blocks.
    /// Uses a pre-allocated buffer and a typed world query to avoid allocations.
    /// </summary>
    private void CheckLeaderProximity()
    {
        _proximityBuffer.Clear();
        GameManager.Instance.World.GetEntitiesInBounds(
            typeof(EntityPlayer),
            new Bounds(_entity.position, Vector3.one * 2f),
            _proximityBuffer);

        foreach (var e in _proximityBuffer)
        {
            if (e is EntityPlayer player && EntityUtilities.IsAnAlly(_entity.entityId, player.entityId))
            {
                SetCollisions(false);
                return;
            }
        }

        SetCollisions(true);
    }

    private void SetCollisions(bool enabled)
    {
        // Cache the blocker transform on first use.
        if (_largeEntityBlocker == null)
            _largeEntityBlocker = GameUtils.FindTagInChilds(_entity.RootTransform, "LargeEntityBlocker");

        if (_largeEntityBlocker != null)
            _largeEntityBlocker.gameObject.SetActive(enabled);

        _entity.PhysicsTransform.gameObject.SetActive(enabled);
        _entity.IsNoCollisionMode.Value = !enabled;
    }

    // -----------------------------------------------------------------------
    // Follow / teleport
    // -----------------------------------------------------------------------

    private void HandleFollowOrder(ref NPCFrameCache cache)
    {
        if (cache.Leader.AttachedToEntity != null)
        {
            // Leader is in a vehicle — hide the NPC and keep it near the vehicle.
            _entity.SendOnMission(true);
            if (cache.DistanceSqToLeader > VehicleSnapThresholdSq)
            {
                var snapPos = cache.Leader.GetPosition();
                snapPos.y += 2f;
                _entity.SetPosition(snapPos);
            }
        }
        else
        {
            if (_entity.Buffs.HasCustomVar("onMission"))
                _entity.SendOnMission(false);
        }

        _entity.bWillRespawn = true;

        if (cache.DistanceSqToLeader > TeleportThresholdSq)
            _entity.TeleportToPlayer(cache.Leader);

        // Companion-list management.  Single IndexOf call; uses Count-1 after Add.
        if (cache.LeaderAsPlayer != null && _entity.AddNPCToCompanion && _entity.IsAlive())
        {
            var companions = cache.LeaderAsPlayer.Companions;
            var idx = companions.IndexOf(_entity);
            if (idx < 0)
            {
                companions.Add(_entity);
                idx = companions.Count - 1;
                var color = Constants.TrackedFriendColors[idx % Constants.TrackedFriendColors.Length];
                if (_entity.NavObject != null)
                {
                    _entity.NavObject.UseOverrideColor = true;
                    _entity.NavObject.OverrideColor = color;
                }
            }
        }
    }
}
