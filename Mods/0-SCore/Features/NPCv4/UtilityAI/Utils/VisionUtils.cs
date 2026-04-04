using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    /// <summary>
    /// V4 vision and sensing utilities: line-of-sight, shoot checks, see-cache access,
    /// nearby-entity queries, and facing tests.
    /// <para>
    /// Differences from <see cref="SCoreUtils"/>:
    /// <list type="bullet">
    ///   <item>All magic numbers replaced with <see cref="AIConstants"/> constants.</item>
    ///   <item><see cref="IsFacing"/> no longer emits unconditional <c>Debug.Log</c> calls;
    ///         the unreachable second code path is removed.</item>
    ///   <item><see cref="IsAnyoneNearby"/> and <see cref="IsEnemyNearby"/> use
    ///         <see cref="NPCFrameCache.EntityBuffer"/> to avoid per-call list allocation.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class VisionUtils
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        // ── CanSee ───────────────────────────────────────────────────────────────

        public static bool CanSee(EntityAlive sourceEntity, EntityAlive targetEntity, float maxDistance = -1f)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                $"CanSee: {sourceEntity.EntityName} Looking at {targetEntity.EntityName} ( {targetEntity.entityId} )");

            if (targetEntity.IsDead())
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, "\tTarget is Dead. False.");
                return false;
            }

            var distanceSq = sourceEntity.GetDistanceSq(targetEntity);

            // Skip view-cone for very close targets; apply it once they're further away.
            if (distanceSq > AIConstants.ViewConeSkipDistSq)
            {
                if (!sourceEntity.IsInViewCone(targetEntity.position))
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                        "\tTarget is far away, but not in my view cone. False.");
                    return false;
                }
            }

            // Positive see-cache hit → fast path.
            if (TryGetSeeCache(sourceEntity, targetEntity, out var isSeen) && isSeen)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tTarget is in the Source's CanSee cache. True");
                return true;
            }

            // Attack/revenge target that is very close counts as visible.
            var knownTarget = EntityUtilities.GetAttackOrRevengeTarget(sourceEntity.entityId);
            if (knownTarget != null && targetEntity.entityId == knownTarget.entityId)
            {
                if (distanceSq < AIConstants.CloseDistanceSq)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                        "\tTarget is the Source's attack/revenge target and is close. True");
                    return true;
                }
            }

            // Distance vs. see-distance check.
            float seeDistance = maxDistance > -1f ? maxDistance : sourceEntity.GetSeeDistance();
            var headPos   = sourceEntity.getHeadPosition();
            var headPos2  = targetEntity.getHeadPosition();
            var direction = headPos2 - headPos;

            if (direction.magnitude > seeDistance)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tTarget is out of range of see distance. False");
                return false;
            }

            // Raycast.
            var ray = new Ray(headPos, direction);
            ray.origin += direction.normalized * AIConstants.RaycastOriginNudge;

            var hitMask = GetHitMaskByWeaponBuff(sourceEntity);
            if (!Voxel.Raycast(sourceEntity.world, ray, seeDistance, hitMask, 0f))
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tSource's raycast failed to find anything. False");
                return false;
            }

            var hitRoot = GameUtils.GetHitRootTransform(
                Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);

            if (hitRoot == null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tSource sees something, but it's not an entity. False");
                return false;
            }

            var component = hitRoot.GetComponent<EntityAlive>();
            if (component == null || !component.IsAlive() || targetEntity != component)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tSource sees something but it's not the right alive entity. False");
                return false;
            }

            // Secondary see-cache check (belt-and-suspenders).
            if (TryGetSeeCache(sourceEntity, targetEntity, out isSeen) && isSeen)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tSource has target in its CanSee cache (double-check). True");
                return true;
            }

            // Crouching leader + sleeping target → don't break stealth.
            var leader = EntityUtilities.GetLeaderOrOwner(sourceEntity.entityId) as EntityAlive;
            if (leader != null && leader.IsCrouching && component.IsSleeping)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tLeader is crouching and target is sleeping. False");
                return false;
            }

            // Secondary view-cone re-check for mid-range targets.
            if (sourceEntity.GetDistanceSq(targetEntity) > AIConstants.ViewConeReCheckDistSq)
            {
                if (!sourceEntity.IsInViewCone(targetEntity.position))
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                        "\tTarget is mid-range but not in the view cone. False");
                    return false;
                }
            }

            // Player stealth check.
            var targetAsPlayer = knownTarget as EntityPlayer;
            if (targetAsPlayer != null)
            {
                var dist = sourceEntity.GetDistance(targetAsPlayer);
                if (!sourceEntity.CanSeeStealth(dist, targetAsPlayer.Stealth.lightLevel))
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                        "\tPlayer passes stealth check; source cannot see them. False");
                    return false;
                }
            }

            // Don't pull a Leroy Jenkins on the leader's stealth run.
            if (leader is EntityPlayer playerLeader &&
                !targetEntity.CanSeeStealth(
                    targetEntity.GetDistance(playerLeader),
                    playerLeader.Stealth.lightLevel))
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    "\tTarget cannot see the leader (stealth). False");
                return false;
            }

            sourceEntity.SetCanSee(targetEntity);
            AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                "\tSource passes all checks. Adding to see cache. True");
            return true;
        }

        // ── CanShoot ─────────────────────────────────────────────────────────────

        public static bool CanShoot(EntityAlive sourceEntity, EntityAlive targetEntity, float maxDistance = -1f)
        {
            var headPos   = sourceEntity.getHeadPosition();
            var headPos2  = targetEntity.getHeadPosition();
            var direction = headPos2 - headPos;

            float seeDistance = maxDistance > -1f ? maxDistance : sourceEntity.GetSeeDistance();

            if (direction.magnitude > seeDistance)
                return false;

            var ray = new Ray(headPos, direction);
            ray.origin += direction.normalized * AIConstants.RaycastOriginNudge;

            if (!Voxel.Raycast(sourceEntity.world, ray, seeDistance, true, true))
                return false;

            return !GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag);
        }

        // ── See Cache ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Checks the source entity's positive and negative see-caches for the target.
        /// Returns <c>true</c> and sets <paramref name="isSeen"/> when the target is found in
        /// either cache; returns <c>false</c> when neither entity is valid or the target is absent
        /// from both caches.
        /// </summary>
        public static bool TryGetSeeCache(EntityAlive sourceEntity, EntityAlive targetEntity, out bool isSeen)
        {
            isSeen = false;

            if (targetEntity == null)
                return false;

            if (sourceEntity?.seeCache?.positiveCache?.Contains(targetEntity.entityId) == true)
            {
                isSeen = true;
                return true;
            }

            return sourceEntity?.seeCache?.negativeCache?.Contains(targetEntity.entityId) == true;
        }

        // ── Nearby Entity Queries ─────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> if any live, non-self <see cref="EntityAlive"/> is within
        /// <paramref name="distance"/> units of the context entity.
        /// Uses <see cref="NPCFrameCache.EntityBuffer"/> to avoid a per-call list allocation.
        /// </summary>
        public static bool IsAnyoneNearby(Context context, float distance = AIConstants.DefaultNearbySearchRadius)
        {
            var buffer = NPCFrameCache.EntityBuffer;
            buffer.Clear();

            var bounds = new Bounds(context.Self.position, new Vector3(distance, distance, distance));
            context.Self.world.GetEntitiesInBounds(typeof(EntityAlive), bounds, buffer);

            for (var i = buffer.Count - 1; i >= 0; i--)
            {
                var other = buffer[i] as EntityAlive;
                if (other == null || other == context.Self || other.IsDead())
                    continue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if any living enemy <see cref="EntityAlive"/> is within
        /// <paramref name="distance"/> units of <paramref name="self"/> and can be seen.
        /// Uses <see cref="NPCFrameCache.EntityBuffer"/> to avoid a per-call list allocation.
        /// </summary>
        public static bool IsEnemyNearby(Context context, float distance = AIConstants.DefaultNearbySearchRadius)
            => IsEnemyNearby(context.Self, distance);

        public static bool IsEnemyNearby(EntityAlive self, float distance = AIConstants.DefaultNearbySearchRadius)
        {
            // A revenge target at any distance keeps the entity on alert.
            var revengeTarget = self.GetRevengeTarget();
            if (revengeTarget && !EntityTargetingUtilities.ShouldForgiveDamage(self, revengeTarget))
                return true;

            var buffer = NPCFrameCache.EntityBuffer;
            buffer.Clear();

            var bounds = new Bounds(self.position, new Vector3(distance, distance, distance));
            self.world.GetEntitiesInBounds(typeof(EntityAlive), bounds, buffer);

            for (var i = buffer.Count - 1; i >= 0; i--)
            {
                var other = buffer[i] as EntityAlive;
                if (other == null || other == self || other.IsDead())
                    continue;

                if (!EntityTargetingUtilities.IsEnemy(self, other))
                    continue;

                if (!CanSee(self, other, distance))
                    continue;

                return true;
            }

            return false;
        }

        // ── IsFacing ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when the angle between <paramref name="source"/>'s look vector and
        /// the direction to <paramref name="targetPos"/> is within <see cref="AIConstants.FacingAngleThreshold"/>.
        /// </summary>
        public static bool IsFacing(EntityAlive source, Vector3 targetPos)
        {
            var lookPos = source.GetLookVector();
            var angle   = Vector3.Angle(lookPos, targetPos - lookPos);
            return angle < AIConstants.FacingAngleThreshold;
        }

        // ── Hit Mask ─────────────────────────────────────────────────────────────

        public static int GetHitMaskByWeaponBuff(EntityAlive entity)
        {
            // Base value matches the bitmask used by the two-bool Voxel.Raycast overload.
            int baseMask =
                (int) HitMasks.CollideMovement |
                (int) HitMasks.Liquid;

            if (entity.Buffs.HasBuff("LBowUser") || entity.Buffs.HasBuff("XBowUser"))
                return baseMask | (int) HitMasks.CollideArrows;

            if (entity.Buffs.HasBuff("RocketLUser"))
                return baseMask | (int) HitMasks.CollideRockets;

            if (entity.HasAnyTags(FastTags<TagGroup.Global>.Parse("ranged")))
                return baseMask | (int) HitMasks.CollideBullets;

            return baseMask | (int) HitMasks.CollideMelee;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Raycast hit-mask values taken verbatim from <c>Voxel.raycastNew</c>.
        /// Each flag indicates whether a raycast should collide with that block type.
        /// </summary>
        private enum HitMasks : int
        {
            Transparent      = 1,
            Liquid           = 2,
            NotCollidable    = 4,
            CollideBullets   = 8,
            CollideRockets   = 0x10,
            CollideArrows    = 0x20,
            CollideMovement  = 0x40,
            CollideMelee     = 0x80,
        }
    }
}
