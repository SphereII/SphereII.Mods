using System.Collections;
using UnityEngine;

public partial class EntityAliveSDXV4
{
    // =========================================================================
    // Position / movement
    // =========================================================================

    public override void OnUpdatePosition(float _partialTicks)
    {
        if (isHirable && position.y <= 0)
        {
            // Use frame cache leader if available; otherwise fall back to a direct lookup.
            var leader = _frameCache.HasLeader
                ? _frameCache.Leader
                : EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
            if (leader != null)
            {
                TeleportToPlayer(leader);
                return;
            }
        }
        base.OnUpdatePosition(_partialTicks);
    }

    // =========================================================================
    // Movement
    // =========================================================================

    /// <summary>
    /// EntityTrader overrides MoveEntityHeaded and does NOT call base, which leaves the NPC
    /// standing in a T-pose with no physics response.  This override restores the full
    /// EntityAlive movement pipeline, including root-motion blending, jump handling, and
    /// animation speed state.
    /// </summary>
    public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
    {
        var isBusy = false;
        if (emodel != null && emodel.avatarController != null)
            emodel.avatarController.TryGetBool("IsBusy", out isBusy);
        if (isBusy) return;

        if (walkType == 21 && Jumping)
        {
            motion = accumulatedRootMotion;
            accumulatedRootMotion = Vector3.zero;
            IsRotateToGroundFlat = true;
            if (moveHelper != null)
            {
                var delta = moveHelper.JumpToPos - position;
                if (Utils.FastAbs(delta.y) < 0.2f) motion.y = delta.y * 0.2f;
                if (Utils.FastAbs(delta.x) < 0.3f) motion.x = delta.x * 0.2f;
                if (Utils.FastAbs(delta.z) < 0.3f) motion.z = delta.z * 0.2f;
                if (delta.sqrMagnitude < 0.010000001f)
                {
                    if (emodel && emodel.avatarController)
                        emodel.avatarController.StartAnimationJump(AnimJumpMode.Land);
                    Jumping = false;
                }
            }
            entityCollision(motion);
            return;
        }

        // EntityTrader does not call base here — we replicate EntityAlive's logic directly.
        if (AttachedToEntity != null) return;
        if (jumpIsMoving) { JumpMove(); return; }

        if (RootMotion)
        {
            if (isEntityRemote && bodyDamage.CurrentStun == EnumEntityStunType.None && !IsDead() &&
                (emodel == null || emodel.avatarController == null ||
                 !emodel.avatarController.IsAnimationHitRunning()))
            {
                accumulatedRootMotion = Vector3.zero;
                return;
            }

            bool ragdoll = emodel && emodel.IsRagdollActive;
            if (isSwimming && !ragdoll)
            {
                motion += accumulatedRootMotion * 0.001f;
            }
            else if (onGround || jumpTicks > 0)
            {
                if (ragdoll) { motion.x = 0f; motion.z = 0f; }
                else { float y = motion.y; motion = accumulatedRootMotion; motion.y += y; }
            }
            accumulatedRootMotion = Vector3.zero;
        }

        if (IsFlyMode.Value)
        {
            EntityPlayerLocal primary = GameManager.Instance.World.GetPrimaryPlayer();
            float godMod = primary != null ? primary.GodModeSpeedModifier : 1f;
            float speed = 2f * (MovementRunning ? 0.35f : 0.12f) * godMod;
            if (!RootMotion)
                Move(_direction, _isDirAbsolute, GetPassiveEffectSpeedModifier() * speed,
                                                 GetPassiveEffectSpeedModifier() * speed);
            if (!IsNoCollisionMode.Value)
            {
                entityCollision(motion);
                motion *= ConditionalScalePhysicsMulConstant(0.546f);
            }
            else
            {
                SetPosition(position + motion, true);
                motion = Vector3.zero;
            }
        }
        else
        {
            DefaultMoveEntity(_direction, _isDirAbsolute);
        }

        if (!isEntityRemote && RootMotion)
        {
            float factor = landMovementFactor * 2.5f;
            if (inWaterPercent > 0.3f)
            {
                if (factor > 0.01f)
                {
                    float t = (inWaterPercent - 0.3f) * 1.4285715f;
                    factor = Mathf.Lerp(factor, 0.01f + (factor - 0.01f) * 0.1f, t);
                }
                if (isSwimming) factor = landMovementFactor * 5f;
            }

            float mag = _direction.magnitude;
            if (mag > 1f) factor /= mag;

            float fwd = _direction.z * factor;
            if (lerpForwardSpeed)
            {
                if (Utils.FastAbs(speedForwardTarget - fwd) > 0.05f)
                    speedForwardTargetStep = Utils.FastAbs(fwd - speedForward) / 0.18f;
                speedForwardTarget = fwd;
            }
            else
            {
                speedForward = fwd;
            }

            speedStrafe = _direction.x * factor;
            SetMovementState();
            ReplicateSpeeds();
        }
    }

    /// <summary>
    /// Returns the Y coordinate one block above the highest solid block at (x, z),
    /// scanning upward from terrain height through any player-placed blocks (e.g. farm plots).
    /// Unlike GetHeightAt, this accounts for structures built on top of terrain.
    /// </summary>
    private static int GetSurfaceY(float x, float z)
    {
        var world = GameManager.Instance.World;
        int y = (int)world.GetHeightAt(x, z);
        int bx = (int)x;
        int bz = (int)z;
        int cap = y + 20;
        while (y < cap && world.GetBlock(bx, y + 1, bz).Block.shape.IsSolidSpace)
            y++;
        return y + 1;
    }

    public void TeleportToPlayer(EntityAlive target, bool randomPosition = false)
    {
        if (target == null) return;
        if (EntityUtilities.GetCurrentOrder(entityId) == EntityUtilities.Orders.Stay)  return;
        if (EntityUtilities.GetCurrentOrder(entityId) == EntityUtilities.Orders.Guard) return;

        var dist2D = Vector2.Distance(
            new Vector2(target.position.x, target.position.z),
            new Vector2(position.x, position.z));
        if (dist2D < 20f) return;
        if (isTeleporting) return;

        var myPosition = target.position + Vector3.back;
        var player     = target as EntityPlayer;
        if (player != null)
        {
            myPosition = player.GetBreadcrumbPos(3f * rand.RandomFloat);
            if (Vector3.Distance(myPosition, player.position) > 40f) randomPosition = true;

            if (randomPosition)
            {
                Vector3 dir = target.position - position;
                myPosition  = RandomPositionGenerator.CalcPositionInDirection(target, target.position, dir, 5, 80f);
            }

            // Find the actual surface, including player-placed blocks (e.g. farm plots).
            myPosition.y = GetSurfaceY(myPosition.x, myPosition.z);
        }

        motion = Vector3.zero;
        navigator?.clearPath();
        SphereCache.RemovePaths(entityId);
        SetPosition(myPosition, true);
        StartCoroutine(ValidateTeleport(target, randomPosition));
    }

    private IEnumerator ValidateTeleport(EntityAlive target, bool randomPosition = false)
    {
        yield return new WaitForSeconds(1f);
        var y = GetSurfaceY(position.x, position.z);
        if (position.y < y)
        {
            var myPosition = position;
            var player = target as EntityPlayer;
            if (player != null)
                myPosition = player.GetBreadcrumbPos(3f * rand.RandomFloat);

            if (randomPosition)
            {
                Vector3 dir = target.position - position;
                myPosition  = RandomPositionGenerator.CalcPositionInDirection(target, target.position, dir, 5, 80f);
            }

            // Find the actual surface, including player-placed blocks (e.g. farm plots).
            myPosition.y = GetSurfaceY(myPosition.x, myPosition.z) + 1;
            motion = Vector3.zero;
            navigator?.clearPath();
            SetPosition(myPosition, true);
        }

        isTeleporting = false;
    }

    public override void updateStepSound(float distX, float distZ, float rotDelta)
    {
        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
        if (leader == null)                            { base.updateStepSound(distX, distZ, rotDelta); return; }
        if (leader.Buffs.GetCustomVar("quietNPC") > 0) return;
        if (IsCrouching)                               return;
        base.updateStepSound(distX, distZ, rotDelta);
    }

    public override float GetMoveSpeed()
    {
        var speed = EffectManager.GetValue(PassiveEffects.WalkSpeed, null, moveSpeed);
        if (IsCrouching)
            speed = EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, moveSpeed);
        return speed;
    }

    public override float GetMoveSpeedAggro()
        => EffectManager.GetValue(PassiveEffects.RunSpeed, null, moveSpeedPanic);

    public new float GetMoveSpeedPanic()
        => EffectManager.GetValue(PassiveEffects.RunSpeed, null, moveSpeedPanic);

    public override float GetEyeHeight()
    {
        if (walkType == 21) return 0.15f;
        if (walkType == 22) return 0.6f;
        return IsCrouching ? base.height * 0.5f : base.height * 0.8f;
    }

    public override Ray GetLookRay()
        => new Ray(position + new Vector3(0f, GetEyeHeight() * eyeHeightHackMod, 0f), GetLookVector());

    public override bool CanBePushed() => true;

    // =========================================================================
    // Boundary box
    // =========================================================================

    public void ConfigureBoundaryBox(Vector3 newSize, Vector3 center)
    {
        var col = gameObject.GetComponent<BoxCollider>();
        if (!col) return;
        col.size     = newSize;
        scaledExtent = new Vector3(col.size.x / 2f * transform.localScale.x,
                                   col.size.y / 2f * transform.localScale.y,
                                   col.size.z / 2f * transform.localScale.z);
        var offset   = new Vector3(col.center.x * transform.localScale.x,
                                   col.center.y * transform.localScale.y,
                                   col.center.z * transform.localScale.z);
        boundingBox  = BoundsUtils.BoundsForMinMax(-scaledExtent.x, -scaledExtent.y, -scaledExtent.z,
                                                    scaledExtent.x,  scaledExtent.y,  scaledExtent.z);
        boundingBox.center += offset;
        if (center != Vector3.zero) boundingBox.center = center;
    }

    public override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (isEntityRemote && _partialTicks > 1f) _dist /= _partialTicks;
        speedForward *= 0.5f; speedStrafe *= 0.5f; speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            float s = Mathf.Sin(-rotation.y * Mathf.PI / 180f);
            float c = Mathf.Cos(-rotation.y * Mathf.PI / 180f);
            speedForward += c * _dist.z - s * _dist.x;
            speedStrafe  += c * _dist.x + s * _dist.z;
        }
        if (Mathf.Abs(_dist.y) > 0.001f) speedVertical += _dist.y;
        SetMovementState();
    }

    public void RestoreSpeed()
    {
        moveSpeed = EntityUtilities.GetFloatValue(entityId, "MoveSpeed");
        var v = new Vector2(moveSpeed, moveSpeed);
        EntityClass.list[entityClass].Properties.ParseVec(EntityClass.PropMoveSpeedAggro, ref v);
        moveSpeedAggro    = v.x;
        moveSpeedAggroMax = v.y;
    }

    // =========================================================================
    // Stuck resolution (unchanged from EntityAliveSDX)
    // =========================================================================

    public virtual void CheckStuck()
    {
        IsStuck = false;
        if (IsFlyMode.Value) return;

        // Throttle block lookups to every 5 ticks (~83 ms). Responsiveness is sufficient
        // since the push applied when stuck persists in `motion` across skipped frames.
        if (++_stuckCheckCounter % 5 != 0) return;

        // Skip when airborne or jumping — horizontal block stuck cannot occur mid-air.
        if (!onGround && !isSwimming) return;

        // Skip when already moving — a moving NPC is not stuck.
        // sqrMagnitude avoids the sqrt; threshold ~0.2 units/tick.
        if (motion.sqrMagnitude > 0.04f) return;

        // Cache position/dimension locals — avoids repeated property access across the four corner calls.
        var px   = position.x;
        var pz   = position.z;
        var yMin = boundingBox.min.y + 0.5f;
        var hw   = width * 0.3f;
        var hd   = depth * 0.3f;

        // Short-circuit after the first stuck corner — once a push direction is set
        // there is no benefit in checking the remaining corners.
        IsStuck = PushOutOfBlocks(px - hw, yMin, pz + hd)
               || PushOutOfBlocks(px - hw, yMin, pz - hd)
               || PushOutOfBlocks(px + hw, yMin, pz - hd)
               || PushOutOfBlocks(px + hw, yMin, pz + hd);
        if (IsStuck) return;

        var bx = Utils.Fastfloor(px);
        var by = Utils.Fastfloor(yMin);
        var bz = Utils.Fastfloor(pz);
        if (!ShouldPushOutOfBlock(bx, by, bz, true) ||
            !CheckNonSolidVertical(new Vector3i(bx, by + 1, bz), 4, 2)) return;
        IsStuck = true;
        motion  = new Vector3(0f, 1.6f, 0f);
        Log.Warning($"{EntityName} ({entityId}) is stuck. Unsticking.");
    }

    private bool PushOutOfBlocks(float _x, float _y, float _z)
    {
        var nx = Utils.Fastfloor(_x); var ny = Utils.Fastfloor(_y); var nz = Utils.Fastfloor(_z);
        var fx = _x - nx;             var fz = _z - nz;
        if (!ShouldPushOutOfBlock(nx, ny, nz, false) && !ShouldPushOutOfBlock(nx, ny + 1, nz, false)) return false;

        bool l = !ShouldPushOutOfBlock(nx-1,ny,nz,true)&&!ShouldPushOutOfBlock(nx-1,ny+1,nz,true);
        bool r = !ShouldPushOutOfBlock(nx+1,ny,nz,true)&&!ShouldPushOutOfBlock(nx+1,ny+1,nz,true);
        bool b = !ShouldPushOutOfBlock(nx,ny,nz-1,true)&&!ShouldPushOutOfBlock(nx,ny+1,nz-1,true);
        bool f = !ShouldPushOutOfBlock(nx,ny,nz+1,true)&&!ShouldPushOutOfBlock(nx,ny+1,nz+1,true);

        byte dir = 255; float best = 9999f;
        if (l && fx              < best) { best = fx;       dir = 0; }
        if (r && 1f - fx         < best) { best = 1f - fx;  dir = 1; }
        if (b && fz              < best) { best = fz;       dir = 4; }
        if (f && 1f - fz         < best) {                  dir = 5; }

        const float push = 0.1f;
        if (dir == 0) motion.x = -push;
        else if (dir == 1) motion.x = push;
        else if (dir == 4) motion.z = -push;
        else if (dir == 5) motion.z = push;
        return dir != 255;
    }

    private bool ShouldPushOutOfBlock(int x, int y, int z, bool checkTerrain)
    {
        var shape = world.GetBlock(x, y, z).Block.shape;
        if (shape.IsSolidSpace && !shape.IsTerrain()) return true;
        if (!checkTerrain || !shape.IsSolidSpace || !shape.IsTerrain()) return false;
        var above = world.GetBlock(x, y + 1, z).Block.shape;
        return above.IsSolidSpace && above.IsTerrain();
    }

    private bool CheckNonSolidVertical(Vector3i pos, int maxY, int space)
    {
        for (int i = 0; i < maxY; i++)
        {
            if (world.GetBlock(pos.x, pos.y + i + 1, pos.z).Block.shape.IsSolidSpace) continue;
            bool clear = true;
            for (int j = 1; j < space; j++)
            {
                if (world.GetBlock(pos.x, pos.y + i + 1 + j, pos.z).Block.shape.IsSolidSpace) { clear = false; break; }
            }
            if (clear) return true;
        }
        return false;
    }
}
