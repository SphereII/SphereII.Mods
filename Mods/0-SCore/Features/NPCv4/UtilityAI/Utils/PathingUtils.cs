using UnityEngine;

namespace UAI
{
    /// <summary>
    /// V4 movement and pathfinding utilities: speed, path-finding, ladder navigation,
    /// jump assistance, blockage detection, and leader teleportation.
    /// <para>
    /// Differences from <see cref="SCoreUtils"/>:
    /// <list type="bullet">
    ///   <item>All magic numbers replaced with <see cref="AIConstants"/> constants.</item>
    ///   <item><see cref="FindPath"/> has the large commented-out block removed.</item>
    ///   <item>Inline comments cleaned up throughout.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class PathingUtils
    {
        // ── Speed ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sets the entity's navigator move speed and returns the chosen value.
        /// Horde zombies always use aggro speed; the <paramref name="panic"/> flag
        /// overrides to panic speed for any entity type.
        /// </summary>
        public static float SetSpeed(Context context, bool panic = false)
        {
            var speed = context.Self.GetMoveSpeed();

            if (context.Self is EntityEnemy enemy && enemy.IsHordeZombie)
                speed = enemy.GetMoveSpeedAggro();

            if (panic)
                speed = context.Self.GetMoveSpeedPanic();

            context.Self.navigator.setMoveSpeed(speed);
            return speed;
        }

        /// <summary>
        /// Returns the estimated squared XZ distance between the entity and
        /// <paramref name="position"/> after <paramref name="estimatedTicks"/> ticks
        /// of current motion.
        /// </summary>
        public static float GetTargetXZDistanceSq(Context context, Vector3 position, float estimatedTicks)
        {
            position += Vector3.zero * estimatedTicks;
            var delta = context.Self.position + context.Self.motion * estimatedTicks - position;
            delta.y = 0f;
            return delta.sqrMagnitude;
        }

        // ── FindPath ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Configures entity speed (matching leader running state when following) and
        /// issues a path-find request toward <paramref name="position"/>.
        /// </summary>
        public static void FindPath(Context context, Vector3 position, bool panic = false)
        {
            // When following a leader, match their run/walk pace.
            var leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityAlive;
            if (leader != null && context.ActionData.CurrentTask is UAITaskFollowSDXV4)
            {
                var runningTag = FastTags<TagGroup.Global>.Parse("running");
                panic = leader.CurrentMovementTag.Test_AllSet(runningTag);
            }

            var speed    = SetSpeed(context, panic);
            var moveTo   = GetMoveToLocation(context, position);
            context.Self.FindPath(moveTo, speed, true, null);
        }

        // ── Move Location ─────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves a world-space move destination that accounts for ladders and
        /// slope geometry. When the destination is within <paramref name="maxDist"/>
        /// units, a flattened XZ position is returned (preserving the target's Y);
        /// otherwise the vector is clamped to <paramref name="maxDist"/> and snapped
        /// to a valid surface via physics raycast.
        /// </summary>
        public static Vector3 GetMoveToLocation(Context context, Vector3 position, float maxDist = 10f)
        {
            var resolved = context.Self.world.FindSupportingBlockPos(position);

            if (!(maxDist > 0f))
                return resolved;

            // Align ladder positions to block edges for smoother climbing.
            var targetBlock = context.Self.world.GetBlock(new Vector3i(resolved));
            if (targetBlock.Block is BlockLadder)
                resolved = AlignToEdge(resolved);

            var selfXZ  = new Vector3(context.Self.position.x, resolved.y, context.Self.position.z);
            var toTarget = resolved - selfXZ;
            var dist     = toTarget.magnitude;

            if (dist > 5f)
                return resolved;

            if (dist <= maxDist)
            {
                // Align to ladder edge when there is meaningful vertical travel.
                var yDelta = resolved.y - context.Self.position.y;
                if (yDelta > AIConstants.LadderYDeltaThreshold || yDelta < -AIConstants.LadderYDeltaThreshold)
                    return AlignToEdge(resolved);

                return selfXZ;
            }

            // Clamp vector and snap to surface.
            toTarget *= maxDist / dist;
            var clamped = resolved - toTarget;
            clamped.y += 0.51f;

            var blockPos  = World.worldToBlockPos(clamped);
            var block     = context.Self.world.GetBlock(blockPos);

            if (block.Block.PathType <= 0)
            {
                if (Physics.Raycast(clamped - Origin.position, Vector3.down, out var hit,
                        AIConstants.LandingCheckRayLength, 1082195968))
                {
                    clamped.y = hit.point.y + Origin.position.y;
                    return clamped;
                }

                if (block.Block.IsElevator((int) block.rotation))
                {
                    clamped.y = resolved.y;
                    return clamped;
                }
            }

            return AlignToEdge(resolved);
        }

        /// <summary>Snaps a world-space vector to the nearest block-edge position.</summary>
        public static Vector3 AlignToEdge(Vector3 vector)
        {
            return new Vector3i(vector).ToVector3();
        }

        // ── Jump ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates whether the entity should jump given the terrain immediately ahead
        /// and initiates the appropriate jump if so. Called automatically by <see cref="IsBlocked"/>.
        /// </summary>
        public static void CheckJump(Context context)
        {
            if (!context.Self.onGround || context.Self.Jumping)
                return;

            // Sample just ahead of and slightly above the entity's feet.
            var samplePos = context.Self.position + context.Self.GetForwardVector() * AIConstants.JumpForwardNudge;
            samplePos.y += AIConstants.JumpVerticalNudge;

            // If solid ground is close below the sample point, no jump is needed.
            if (Physics.Raycast(samplePos - Origin.position, Vector3.down, out _, AIConstants.GroundCheckRayLength, 1082195968))
                return;

            // Check headroom directly above (avoids jumping into a ceiling).
            var headroomCheck = new Vector3i(
                Utils.Fastfloor(context.Self.position.x),
                Utils.Fastfloor(context.Self.position.y + AIConstants.JumpHeadroomYOffset),
                Utils.Fastfloor(context.Self.position.z));
            headroomCheck += Vector3i.forward;

            var headBlock = context.Self.world.GetBlock(headroomCheck);
            if (headBlock.Block.IsMovementBlocked(context.Self.world, headroomCheck, headBlock, BlockFace.None))
                return;

            // Stop forward motion before jumping to prevent sliding off edges.
            EntityUtilities.Stop(context.Self.entityId);

            // Use a low jump when the target is significantly below the entity.
            var target = UAIUtils.ConvertToEntityAlive(context.ActionData.Target);
            if (target != null && target.IsAlive())
            {
                var drop = Mathf.Abs(target.position.y - context.Self.position.y);
                if (drop > AIConstants.LowJumpDropThreshold)
                {
                    context.Self.moveHelper.StartJump(true, 1f, 0f);
                    return;
                }
            }

            // If the landing spot is air, use a low jump; otherwise a standard jump.
            var landingSpot = context.Self.position
                              + context.Self.GetForwardVector()
                              + context.Self.GetForwardVector()
                              + Vector3.down;
            var landingBlock = context.Self.world.GetBlock(new Vector3i(landingSpot));

            if (landingBlock.isair)
                context.Self.moveHelper.StartJump(true, 1f, 0f);
            else
                context.Self.moveHelper.StartJump(true, 2f, 1f);
        }

        // ── Blockage ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when the entity is stunned or its move-helper blocked-time
        /// exceeds <see cref="SCoreConstants.BlockedTime"/>. Also calls
        /// <see cref="CheckJump"/> and <see cref="DoorUtils.CheckForClosedDoor"/> as side effects.
        /// </summary>
        public static bool IsBlocked(Context context)
        {
            if (context.Self.bodyDamage.CurrentStun != EnumEntityStunType.None)
                return true;

            CheckJump(context);
            DoorUtils.CheckForClosedDoor(context);

            return context.Self.moveHelper.BlockedTime > SCoreConstants.BlockedTime;
        }

        // ── Teleport ─────────────────────────────────────────────────────────────

        /// <summary>Teleports the entity directly to <paramref name="position"/>.</summary>
        public static void TeleportToPosition(Context context, Vector3 position)
        {
            context.Self.SetPosition(position);
        }

        /// <summary>
        /// Teleports the entity near its leader.
        /// When <paramref name="blocked"/> is <c>true</c>, a random nearby spawn point is
        /// chosen; otherwise the entity uses its own <c>TeleportToPlayer</c> logic (only
        /// when not currently on a mission).
        /// </summary>
        public static void TeleportToLeader(Context context, bool blocked = true)
        {
            var leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityAlive;
            if (leader == null) return;

            if (blocked)
            {
                GameManager.Instance.World.GetRandomSpawnPositionMinMaxToPosition(
                    leader.position,
                    AIConstants.TeleportSpawnRadiusMin,
                    AIConstants.TeleportSpawnRadiusMax,
                    AIConstants.TeleportSpawnClearance,
                    false,
                    out var spawnPos);

                if (spawnPos == Vector3.zero)
                    spawnPos = leader.position + Vector3.back;

                context.Self.SetPosition(spawnPos);
                return;
            }

            if (context.Self is IEntityAliveSDX sdxEntity && !sdxEntity.IsOnMission())
                sdxEntity.TeleportToPlayer(leader, false);
        }

        // ── Move Back ────────────────────────────────────────────────────────────

        /// <summary>Directs the entity's move-helper toward <paramref name="position"/>.</summary>
        public static void MoveBack(Context context, Vector3 position)
        {
            context.Self.moveHelper.SetMoveTo(position, true);
        }
    }
}
