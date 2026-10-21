using UnityEngine;

/// <summary>
/// Puts an NPC that fell below the world (y &lt;= 0) back on the surface. Shared by
/// EntityAliveSDX and EntityAliveSDXV4 so the two stay in step.
///
/// Why this exists: when an NPC drops below y 0, the game's Entity.OnUpdatePosition logs
/// "fell off the world" and unloads it. A hired NPC that falls through the terrain on game
/// re-entry (its chunk collider not built yet) is lost that way.
///
/// It deliberately does NOT go through TeleportToPlayer. That call refuses when the NPC is
/// within 20 m of the player horizontally, or on Stay or Guard, which is right for gathering
/// followers but wrong here: an NPC 15 m to the side and far below was never rescued, and the
/// old code also skipped base every tick, so the game's own check never ran either.
///
/// Where it puts the NPC, first match wins:
///   1. Stay or Guard with a guard spot: on the surface above the guard spot.
///   2. A leader or owner, or the farm owner (FarmHere zeroes leader and owner): beside them.
///   3. Otherwise: on the surface above where it fell.
///
/// At most one attempt a second and MaxAttempts in a WindowSeconds window. After that the
/// NPC is handed to the game's own check, so a spot with no ground cannot loop forever.
/// </summary>
public static class FallRescue
{
    private const int MaxAttempts = 5;
    private const float WindowSeconds = 30f;
    private const float RetrySeconds = 1f;
    private const float MaxBreadcrumbDistance = 40f;

    public struct State
    {
        public float WindowStart;
        public int Attempts;
        public float NextAttempt;
        public bool GaveUp;

        /// <summary>True while a further attempt is still coming; the caller should skip base
        /// so the game does not unload the NPC in the meantime.</summary>
        public bool Waiting;
    }

    /// <summary>
    /// Returns true with a destination when the caller should move the NPC there now.
    /// </summary>
    public static bool TryRescue(EntityAlive npc, EntityAlive leader, Vector3 guardPosition,
        ref State state, out Vector3 destination)
    {
        destination = Vector3.zero;
        var now = Time.time;

        if (now - state.WindowStart > WindowSeconds)
        {
            state.WindowStart = now;
            state.Attempts = 0;
            state.GaveUp = false;
        }

        if (state.Attempts >= MaxAttempts)
        {
            state.Waiting = false;
            if (!state.GaveUp)
            {
                state.GaveUp = true;
                Log.Warning($"[SCore] FallRescue: {npc.EntityName} ({npc.entityId}) is still below the world after {MaxAttempts} rescues at {npc.position}; leaving it to the game.");
            }
            return false;
        }

        if (now < state.NextAttempt)
        {
            state.Waiting = true;
            return false;
        }

        string reason;
        var order = EntityUtilities.GetCurrentOrder(npc.entityId);
        var target = leader;
        if (target == null && npc.Buffs.HasCustomVar("FarmOwnerEntityId"))
            target = npc.world.GetEntity((int)npc.Buffs.GetCustomVar("FarmOwnerEntityId")) as EntityAlive;

        if ((order == EntityUtilities.Orders.Stay || order == EntityUtilities.Orders.Guard) &&
            guardPosition != Vector3.zero)
        {
            destination = guardPosition;
            reason = $"guard spot ({order})";
        }
        else if (target != null)
        {
            destination = target.position + Vector3.back;
            if (target is EntityPlayer player)
            {
                var crumb = player.GetBreadcrumbPos(3f * npc.rand.RandomFloat);
                if (Vector3.Distance(crumb, player.position) <= MaxBreadcrumbDistance)
                    destination = crumb;
            }
            reason = leader != null ? $"beside leader {target.EntityName}" : $"beside farm owner {target.EntityName}";
        }
        else
        {
            destination = npc.position;
            reason = "surface above the fall";
        }

        var surface = EntityAliveSDXV4.GetSurfaceY(destination.x, destination.z);
        if (surface > 1)
            destination.y = surface;
        else if (target != null)
            destination = target.position; // no ground data at the spot: the player is standing on something
        else
            destination.y = Mathf.Max(destination.y, 1f);

        state.Attempts++;
        state.NextAttempt = now + RetrySeconds;
        state.Waiting = true;

        Log.Out($"[SCore] FallRescue: {npc.EntityName} ({npc.entityId}) fell below the world at {npc.position}; moving to {destination}, {reason} (attempt {state.Attempts}/{MaxAttempts}).");
        return true;
    }
}
