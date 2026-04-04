using System;
using UnityEngine;

namespace UAI
{
    /// <summary>
    /// V4 combat and interaction utilities: debug display, item actions, weapon management,
    /// look/rotate helpers, and miscellaneous entity queries.
    /// <para>
    /// Differences from <see cref="SCoreUtils"/>:
    /// <list type="bullet">
    ///   <item><see cref="SetWeapon"/> no longer has an unconditional early <c>return</c>;
    ///         the dead code path is removed and the method is properly implemented.</item>
    ///   <item><c>SetRandomLook</c> is omitted — it was disabled (early <c>return</c>) in the
    ///         original and has no callers.</item>
    ///   <item><see cref="TurnToFaceEntity"/> uses <see cref="NPCFrameCache.EntityBuffer"/>
    ///         to avoid a per-call list allocation.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class CombatUtils
    {
        // ── Debug Display ─────────────────────────────────────────────────────────

        /// <summary>
        /// Writes the entity's active action, task, and target to its
        /// <c>DebugNameInfo</c> string (visible in the debug overlay).
        /// Does nothing on a dedicated server or when the debug-task overlay is disabled.
        /// </summary>
        public static void DisplayDebugInformation(Context context, string prefix = "", string postfix = "")
        {
            if (!GameManager.IsDedicatedServer)
            {
                if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks) || context.Self.IsDead())
                {
                    if (EntityUtilities.GetLeaderOrOwner(context.Self.entityId) == null)
                        context.Self.DebugNameInfo = String.Empty;
                    else if (string.IsNullOrEmpty(context.Self.DebugNameInfo))
                        context.Self.DebugNameInfo = context.Self.EntityName;

                    return;
                }
            }

            var msg = $" ( {context.Self.entityId} ) {prefix}\n";
            msg += $" Active Action: {context.ActionData.Action?.Name}\n";

            var taskIndex = context.ActionData.TaskIndex;
            var tasks     = context.ActionData.Action?.GetTasks();
            if (tasks == null)
            {
                msg += " Active Task: None";
                context.Self.DebugNameInfo = msg;
                return;
            }

            msg += $" Active Task: {tasks[taskIndex]}\n";
            msg += $" Active Target: {context.ActionData.Target}\n";
            msg += $" {postfix}";

            context.Self.DebugNameInfo = msg;
        }

        // ── Item Actions ──────────────────────────────────────────────────────────

        /// <summary>
        /// Fires the start/end min-events for item action <paramref name="actionIdx"/>,
        /// executes the action instantly, and stops the current task.
        /// </summary>
        public static void SimulateActionInstantExecution(Context context, int actionIdx, ItemStack itemStack)
        {
            if (!Equals(itemStack, ItemStack.Empty))
            {
                context.Self.MinEventContext.ItemValue = itemStack.itemValue;

                var startEvent = actionIdx == 0
                    ? MinEventTypes.onSelfPrimaryActionStart
                    : MinEventTypes.onSelfSecondaryActionStart;
                var endEvent = actionIdx == 0
                    ? MinEventTypes.onSelfPrimaryActionEnd
                    : MinEventTypes.onSelfSecondaryActionEnd;

                itemStack.itemValue.FireEvent(startEvent, context.Self.MinEventContext);
                context.Self.FireEvent(startEvent, false);

                itemStack.itemValue.ItemClass.Actions[actionIdx]
                    .ExecuteInstantAction(context.Self, itemStack, false, null);

                itemStack.itemValue.FireEvent(endEvent, context.Self.MinEventContext);
                context.Self.FireEvent(endEvent, false);
            }

            context.ActionData.CurrentTask.Stop(context);
        }

        // ── Weapon Management ─────────────────────────────────────────────────────

        /// <summary>
        /// Switches the entity's held item to inventory slot 0 (primary weapon slot).
        /// No-ops when slot 0 is already active.
        /// </summary>
        public static void SetWeapon(Context context)
        {
            if (context.Self.inventory.holdingItemIdx != 0)
            {
                context.Self.inventory.SetHoldingItemIdx(0);
                context.Self.inventory.OnUpdate();
            }
        }

        /// <summary>
        /// Switches the entity's held item to the dummy (hidden) slot.
        /// No-ops when the dummy slot is already active.
        /// </summary>
        public static void HideWeapon(Context context)
        {
            if (context.Self.inventory.holdingItemIdx != context.Self.inventory.DUMMY_SLOT_IDX)
            {
                context.Self.inventory.SetHoldingItemIdx(context.Self.inventory.DUMMY_SLOT_IDX);
                context.Self.inventory.OnUpdate();
            }
        }

        // ── Look / Rotate ─────────────────────────────────────────────────────────

        /// <summary>
        /// Rotates the entity to face the nearest non-enemy player (or
        /// <paramref name="priorityEntity"/> if provided) within a 5-unit radius.
        /// Skips sleeping entities. Does nothing when the entity itself is sleeping.
        /// Uses <see cref="NPCFrameCache.EntityBuffer"/> to avoid a per-call list allocation.
        /// </summary>
        public static void TurnToFaceEntity(Context context, EntityAlive priorityEntity = null)
        {
            if (context.Self.IsSleeping)
                return;

            var buffer = NPCFrameCache.EntityBuffer;
            buffer.Clear();

            context.Self.world.GetEntitiesInBounds(
                typeof(EntityAlive),
                new Bounds(context.Self.position, Vector3.one * 5f),
                buffer);

            if (buffer.Count == 0)
                return;

            Entity lookTarget = null;

            foreach (var entity in buffer)
            {
                if (entity.IsDead()) continue;

                if (priorityEntity != null && entity.entityId == priorityEntity.entityId)
                {
                    lookTarget = entity;
                    break;
                }

                if (entity is not EntityPlayerLocal && entity is not EntityPlayer) continue;
                if (EntityTargetingUtilities.IsEnemy(context.Self, entity)) continue;

                if (context.Self
                    .GetActivationCommands(new Vector3i(context.Self.position), lookTarget as EntityAlive)
                    .Length > 0)
                {
                    lookTarget = entity;
                }
            }

            if (lookTarget == null)
                return;

            context.Self.SetLookPosition(lookTarget.getHeadPosition());
            context.Self.RotateTo(lookTarget.position.x, lookTarget.position.y, lookTarget.position.z, 8f, 8f);
        }

        /// <summary>
        /// Sets the entity's look position and rotation toward <paramref name="target"/>.
        /// When <paramref name="target"/> is an entity alive, a small random yaw/pitch offset
        /// is applied. When it is a <see cref="Vector3"/>, the entity rotates directly to it.
        /// </summary>
        public static void SetLookPosition(Context context, object target)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(context.ActionData.Target);
            if (entityAlive != null)
            {
                var headPos     = entityAlive.getHeadPosition();
                var forwardVec  = context.Self.GetForwardVector();
                forwardVec = Quaternion.Euler(
                    context.Self.rand.RandomFloat * 60f - 30f,
                    context.Self.rand.RandomFloat * 120f - 60f,
                    0f) * forwardVec;
                context.Self.SetLookPosition(headPos + forwardVec);
            }

            if (target is not Vector3 vec)
                return;

            context.Self.RotateTo(vec.x, vec.y, vec.z, 8f, 8f);
            context.Self.SetLookPosition(vec);
        }

        // ── Stance ────────────────────────────────────────────────────────────────

        /// <summary>Sets the entity's crouching state.</summary>
        public static void SetCrouching(Context context, bool crouch = false)
        {
            context.Self.Crouching = crouch;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when the entity has the named buff active.
        /// Returns <c>false</c> immediately when <paramref name="buff"/> is null or empty.
        /// </summary>
        public static bool HasBuff(Context context, string buff)
        {
            return !string.IsNullOrEmpty(buff) && context.Self.Buffs.HasBuff(buff);
        }

        /// <summary>
        /// Returns a random position near the entity's home position,
        /// or <see cref="Vector3.zero"/> when no home is set or no valid
        /// spawn point could be calculated.
        /// </summary>
        public static Vector3 HasHomePosition(Context context)
        {
            if (!context.Self.hasHome())
                return Vector3.zero;

            var homePos  = context.Self.getHomePosition();
            var position = RandomPositionGenerator.CalcTowards(
                context.Self, 5, 100, 10, homePos.position.ToVector3());

            return position == Vector3.zero ? Vector3.zero : position;
        }
    }
}
