using Harmony.XUiC;
using HarmonyLib;
using UnityEngine;

namespace Harmony.Animation
{
/// <summary>
/// Fixes the head twist that NPCs have that are not code-corrected in vanilla.
/// </summary>
   
    [HarmonyPatch(typeof(EModelBase))]
    [HarmonyPatch("LookAtUpdate")]
    public class EModelBaseLookAtUpdate
    {
        public static bool Prefix(EModelBase __instance, Entity ___entity, Transform ___neckParentTransform, Transform ___headTransform, ref Vector3 ___lookAtPos, float ___lookAtMaxAngle,
            ref Quaternion ___lookAtRot, Transform ___neckTransform, ref float ___lookAtBlendPer, ref float ___lookAtBlendPerTarget, bool ___lookAtIsPos)
        {
            var entityAlive = ___entity as EntityAliveSDX;
            if (entityAlive == null)
                return true;

            // Disable this check for any entity that has UMA2 as a tag
            if (entityAlive.HasAnyTags(FastTags.Parse("UMA2")))
                return true;

            var currentStun = entityAlive.bodyDamage.CurrentStun;
            var deltaTime = Time.deltaTime;
            if (entityAlive.IsDead() || currentStun != EnumEntityStunType.None && currentStun != EnumEntityStunType.Getup) ___lookAtBlendPerTarget = 0f;

            // If the entity has an attack target, look at them, instead of hte player.
            var target = EntityUtilities.GetAttackOrRevengeTarget(entityAlive.entityId);
            if (target != null)
            {
                ___lookAtBlendPerTarget -= deltaTime;
                if (target.IsAlive() && entityAlive.CanSee(target as global::EntityAlive))
                {
                    ___lookAtPos = target.getHeadPosition();
                    ___lookAtBlendPerTarget = 1f;
                }
                else
                {
                    ___lookAtPos = entityAlive.getHeadPosition() + Vector3.forward;    
                }
            }

            if (___lookAtBlendPer <= 0f && ___lookAtBlendPerTarget <= 0f) return true;
            ___lookAtBlendPer = Mathf.MoveTowards(___lookAtBlendPer, ___lookAtBlendPerTarget, deltaTime * 2f);

            if (___neckParentTransform == null)
                return true;

            var rotation3 = ___neckParentTransform.rotation;
            var transform = ___headTransform;
            var quaternion = Quaternion.LookRotation(___lookAtPos - Origin.position - transform.position);
            quaternion *= Quaternion.Slerp(Quaternion.identity, transform.localRotation, 0.5f);

            var b = Quaternion.RotateTowards(rotation3, quaternion, ___lookAtMaxAngle);
            ___lookAtRot = Quaternion.Slerp(___lookAtRot, b, 0.16f);
            var num = ___lookAtBlendPer;
            ___neckTransform.rotation = Quaternion.Slerp(___neckTransform.rotation, ___lookAtRot, num * 0.4f);
            var rotation2 = transform.rotation;
            transform.rotation = Quaternion.Slerp(rotation2, ___lookAtRot, num);
            return false;
        }
    }
}