using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GamePath;
using HarmonyLib;
using UAI;
using UnityEngine;

/// <summary>
/// [NPC-TRACE] Movement trace for SCore NPCs (anything that takes orders: IEntityOrderReceiverSDX).
/// Answers "where is this NPC being sent, and by whom".
///
/// Off by default. Turn it on with either:
///   blocks.xml  ConfigFeatureBlock / AdvancedTroubleshootingFeatures / NPCMovementTrace = true
///   console     setcvar AdvancedTroubleshootingFeatures_NPCMovementTrace 1   (0 turns it back off)
///
/// What it logs (all lines start with [NPC-TRACE] name#id):
///   MOVETO  a new move-helper destination: target, distance, speed, and the calling code.
///   PATH    a new path request (EntityAlive.FindPath): target, distance, speed, calling code.
///   ACTION  the UAI action or task changed: old and new, order, guard spot.
///   ORDER   the order changed.
///   GUARD   the guard position changed.
///   FAST    the NPC covered ground faster than FastSpeed over the last sample.
///   JUMP    the NPC moved more than TeleportDistance in one sample (teleport or snap).
/// Each state line carries pos, order, action/task, the move helper target and the path end, so
/// a run in the wrong direction shows which of them it was following.
///
/// Only changes are logged, never every tick, so it is safe to leave on for a test session.
/// </summary>
public static class NpcMovementTrace
{
    private const string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
    private const string Feature = "NPCMovementTrace";

    private const float FlagRefreshSeconds = 2f;   // how often the on/off switch is re-read
    private const float SampleSeconds = 0.5f;      // position sample interval
    private const float FastSpeed = 3.5f;          // m/s; walking is about 1.5, panic 5.5
    private const float TeleportDistance = 10f;    // metres within one sample
    private const float NewTargetDistance = 1.5f;  // a destination this close to the last is the same one
    private const int CallerFrames = 3;

    private static bool _enabled;
    private static float _nextFlagCheck;

    private class TraceState
    {
        public float NextSample;
        public Vector3 LastPos;
        public bool HasPos;
        public string LastAction = "";
        public string LastOrder = "";
        public Vector3 LastGuard;
        public Vector3 LastMoveTo;
        public bool HasMoveTo;
        public Vector3 LastPathTarget;
        public bool HasPathTarget;
    }

    private static readonly Dictionary<int, TraceState> States = new Dictionary<int, TraceState>();

    public static bool Enabled
    {
        get
        {
            var now = Time.time;
            if (now < _nextFlagCheck) return _enabled;
            _nextFlagCheck = now + FlagRefreshSeconds;
            var was = _enabled;
            _enabled = Configuration.CheckFeatureStatus(AdvFeatureClass, Feature, true);
            if (was != _enabled)
            {
                Log.Out($"[NPC-TRACE] movement trace {(_enabled ? "ON" : "OFF")}");
                if (!_enabled) States.Clear();
            }
            return _enabled;
        }
    }

    private static bool IsTraced(EntityAlive entity)
    {
        return entity != null && entity is IEntityOrderReceiverSDX && !entity.IsDead() && Enabled;
    }

    private static TraceState GetState(EntityAlive entity)
    {
        if (!States.TryGetValue(entity.entityId, out var state))
        {
            state = new TraceState();
            States[entity.entityId] = state;
        }
        return state;
    }

    private static string V(Vector3 v) => $"({v.x:F1}, {v.y:F1}, {v.z:F1})";

    private static string Prefix(EntityAlive entity) => $"[NPC-TRACE] {entity.EntityName}#{entity.entityId}";

    private static string ActionName(EntityAlive entity)
    {
        var context = entity.utilityAIContext;
        if (context == null) return "none";
        var action = context.ActionData.Action?.Name ?? "none";
        var task = context.ActionData.CurrentTask?.GetType().Name ?? "none";
        return $"{action}/{task}";
    }

    private static string OrderName(EntityAlive entity)
    {
        return EntityUtilities.GetCurrentOrder(entity.entityId).ToString();
    }

    // The NPC's whole movement picture in one line.
    private static string Describe(EntityAlive entity)
    {
        var sb = new StringBuilder();
        sb.Append($"pos={V(entity.position)} order={OrderName(entity)} action={ActionName(entity)}");

        if (entity is IEntityOrderReceiverSDX receiver)
        {
            var guard = receiver.GuardPosition;
            sb.Append($" guard={V(guard)} guardDist={Vector3.Distance(entity.position, guard):F1}");
        }

        var helper = entity.moveHelper;
        if (helper != null)
        {
            sb.Append(helper.IsActive
                ? $" helper=ON to={V(helper.moveToPos)} dist={Vector3.Distance(entity.position, helper.moveToPos):F1} speed={helper.moveSpeed:F2} blocked={helper.BlockedTime:F2}"
                : " helper=off");
        }

        var path = entity.navigator?.getPath();
        if (path != null && path.HasPoints())
        {
            var end = path.GetEndPos();
            sb.Append($" pathEnd={V(end)} pathDist={Vector3.Distance(entity.position, end):F1} nodesLeft={path.NodeCountRemaining()}");
        }
        else if (entity.navigator != null && entity.navigator.isPlanningPath())
        {
            sb.Append(" path=planning");
        }

        // Body movement state. A move direction left on while the helper is off keeps the body
        // moving with no task steering it; abs=True means it is a world direction.
        var md = entity.moveDirection;
        sb.Append($" jumping={entity.Jumping} onGround={entity.onGround} moveDir=({md.x:F2}, {md.z:F2}) abs={entity.isMoveDirAbsolute} speedMod={entity.speedModifier:F2}");

        var leader = entity.Buffs.GetCustomVar("Leader");
        var owner = entity.Buffs.GetCustomVar("Owner");
        var farmOwner = entity.Buffs.GetCustomVar("FarmOwnerEntityId");
        sb.Append($" leader={leader:F0} owner={owner:F0} farmOwner={farmOwner:F0}");
        sb.Append($" home={entity.getHomePosition().position}");
        return sb.ToString();
    }

    // The first few frames above the move call that are not the trace itself, Harmony, or the
    // move helper plumbing. Names only; no file or line numbers in a release build.
    private static string Caller()
    {
        var frames = new StackTrace(2, false).GetFrames();
        if (frames == null) return "unknown";
        var sb = new StringBuilder();
        var count = 0;
        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            var type = method?.DeclaringType;
            if (type == null) continue;
            if (type == typeof(NpcMovementTrace)) continue;
            if (type.Namespace != null && type.Namespace.StartsWith("HarmonyLib")) continue;
            if (type.Name.Contains("NpcMovementTrace")) continue;

            if (count > 0) sb.Append(" < ");
            sb.Append(type.Name).Append('.').Append(method.Name);
            if (++count >= CallerFrames) break;
        }
        return count == 0 ? "unknown" : sb.ToString();
    }

    // Move helper destination. Called from the SetMoveTo prefixes.
    public static void OnMoveTo(EntityAlive entity, Vector3 target, float speed)
    {
        if (!IsTraced(entity)) return;
        var state = GetState(entity);
        if (state.HasMoveTo && Vector3.Distance(state.LastMoveTo, target) < NewTargetDistance) return;
        state.LastMoveTo = target;
        state.HasMoveTo = true;

        Log.Out($"{Prefix(entity)} MOVETO target={V(target)} dist={Vector3.Distance(entity.position, target):F1} speed={speed:F2} by {Caller()} | {Describe(entity)}");
    }

    // Path request. Called from the EntityAlive.FindPath prefix.
    public static void OnFindPath(EntityAlive entity, Vector3 target, float speed)
    {
        if (!IsTraced(entity)) return;
        var state = GetState(entity);
        if (state.HasPathTarget && Vector3.Distance(state.LastPathTarget, target) < NewTargetDistance) return;
        state.LastPathTarget = target;
        state.HasPathTarget = true;

        Log.Out($"{Prefix(entity)} PATH target={V(target)} dist={Vector3.Distance(entity.position, target):F1} speed={speed:F2} by {Caller()} | {Describe(entity)}");
    }

    // Periodic sample. Called from the EntityAliveSDXV4.OnUpdateLive postfix.
    public static void Sample(EntityAlive entity)
    {
        if (!IsTraced(entity)) return;
        var state = GetState(entity);
        var now = Time.time;
        if (now < state.NextSample) return;

        var elapsed = state.HasPos ? now - (state.NextSample - SampleSeconds) : 0f;
        state.NextSample = now + SampleSeconds;

        var action = ActionName(entity);
        if (action != state.LastAction)
        {
            Log.Out($"{Prefix(entity)} ACTION {state.LastAction} -> {action} | {Describe(entity)}");
            state.LastAction = action;
        }

        var order = OrderName(entity);
        if (order != state.LastOrder)
        {
            Log.Out($"{Prefix(entity)} ORDER {state.LastOrder} -> {order} | {Describe(entity)}");
            state.LastOrder = order;
        }

        if (entity is IEntityOrderReceiverSDX receiver && Vector3.Distance(receiver.GuardPosition, state.LastGuard) > 0.5f)
        {
            Log.Out($"{Prefix(entity)} GUARD {V(state.LastGuard)} -> {V(receiver.GuardPosition)} | {Describe(entity)}");
            state.LastGuard = receiver.GuardPosition;
        }

        if (state.HasPos && elapsed > 0.01f)
        {
            var moved = Vector3.Distance(entity.position, state.LastPos);
            if (moved > TeleportDistance)
                Log.Out($"{Prefix(entity)} JUMP moved={moved:F1} from={V(state.LastPos)} | {Describe(entity)}");
            else if (moved / elapsed > FastSpeed)
                Log.Out($"{Prefix(entity)} FAST speed={moved / elapsed:F1}m/s | {Describe(entity)}");
        }

        state.LastPos = entity.position;
        state.HasPos = true;
    }

    // ── Harmony hooks ──────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch(nameof(EntityMoveHelper.SetMoveTo))]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(bool) })]
    public class EntityMoveHelperSetMoveTo
    {
        public static void Prefix(EntityMoveHelper __instance, Vector3 __0)
        {
            if (!Enabled) return;
            var entity = __instance.entity;
            if (entity == null) return;
            OnMoveTo(entity, __0, entity.GetMoveSpeedAggro());
        }
    }

    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch(nameof(EntityMoveHelper.SetMoveTo))]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(bool), typeof(float) })]
    public class EntityMoveHelperSetMoveToSpeed
    {
        public static void Prefix(EntityMoveHelper __instance, Vector3 __0, float __2)
        {
            if (!Enabled) return;
            OnMoveTo(__instance.entity, __0, __2);
        }
    }

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch(nameof(EntityAlive.FindPath))]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(EAIBase) })]
    public class EntityAliveFindPath
    {
        public static void Prefix(EntityAlive __instance, Vector3 __0, float __1)
        {
            if (!Enabled) return;
            OnFindPath(__instance, __0, __1);
        }
    }

    [HarmonyPatch(typeof(EntityAliveSDXV4))]
    [HarmonyPatch(nameof(EntityAliveSDXV4.OnUpdateLive))]
    public class EntityAliveSDXV4OnUpdateLive
    {
        public static void Postfix(EntityAliveSDXV4 __instance)
        {
            if (!Enabled) return;
            Sample(__instance);
        }
    }
}
