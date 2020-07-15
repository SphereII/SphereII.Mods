using HarmonyLib;
using System.Reflection;
using UnityEngine;
using DMT;
using System;
using System.Runtime.CompilerServices;

/**
 * SphereII_EmodelBase_InitCommon
 * 
 * This class includes a Harmony patch to allow MecanimSDX to work properly on dedicated servers.
 * 
 *   On dedicated servers, when using an external animation class (MecanimSDX), the server incorrectly assigns a Dummy avatar, rather than our own.
 *   We can get around this by disabling ragdolling on external entities, but it also generates warnings in the log file. This should jump in right before we do the 
 *   switchandmodel.
 */
[HarmonyPatch(typeof(EModelBase))]
[HarmonyPatch("InitCommon")]
public class SphereII_EModelBase_InitCommon
{
    static bool Prefix(ref EModelBase __instance, Entity ___entity)
    {
        EntityClass entityClass = EntityClass.list[___entity.entityClass];
        if (entityClass.Properties.Values.ContainsKey(EntityClass.PropAvatarController))
        {
            if (entityClass.Properties.Values[EntityClass.PropAvatarController].Contains("MecanimSDX") && __instance.avatarController is AvatarControllerDummy)
            {
                __instance.avatarController = (__instance.transform.gameObject.AddComponent(Type.GetType(entityClass.Properties.Values[EntityClass.PropAvatarController])) as AvatarController);
                __instance.avatarController.SetVisible(true);
            }
        }
        return true;
    }
}


// Fix for NPCs turning their heads at the wrong angle. The only thing different in this method, than the base method, is a 90f turn if the class 
// is EntityNPC. We use that class for EntityAliveSDX, but we want them to behave more like non-NPCs for this method.
[HarmonyPatch(typeof(EModelBase))]
[HarmonyPatch("LookAtUpdate")]
public class SphereII_EmodelBase_LookAtUpdate
{
    public static bool Prefix(EModelBase __instance, Entity ___entity, Transform ___neckParentTransform, Transform ___headTransform, ref Vector3 ___lookAtPos, float ___lookAtMaxAngle,
        ref Quaternion ___lookAtRot, Transform ___neckTransform, ref float ___lookAtBlendPer, ref float ___lookAtBlendPerTarget, bool ___lookAtIsPos)
    {
        EntityAliveSDX entityAlive = ___entity as EntityAliveSDX;
        if (!entityAlive)
            return true;

        EnumEntityStunType currentStun = entityAlive.bodyDamage.CurrentStun;
        float deltaTime = Time.deltaTime;
        if (entityAlive.IsDead() || (currentStun != EnumEntityStunType.None && currentStun != EnumEntityStunType.Getup))
        {
            ___lookAtBlendPerTarget = 0f;
        }

        // If the entity has an attack target, look at them, instead of hte player.
        Entity target = EntityUtilities.GetAttackOrReventTarget(___entity.entityId);
        if (target != null)
        {
            ___lookAtBlendPerTarget -= deltaTime;
            if (target && entityAlive.CanSee(target as EntityAlive))
            {
                ___lookAtPos = target.getHeadPosition();
                ___lookAtBlendPerTarget = 1f;
            }
        }
        if (___lookAtBlendPer <= 0f && ___lookAtBlendPerTarget <= 0f)
        {
            return true;
        }
        ___lookAtBlendPer = Mathf.MoveTowards(___lookAtBlendPer, ___lookAtBlendPerTarget, deltaTime * 2f);

        Quaternion rotation3 = ___neckParentTransform.rotation;
        Transform transform = ___headTransform;
        Vector3 upwards = rotation3 * Vector3.up;
        Quaternion quaternion;
        quaternion = Quaternion.LookRotation(___lookAtPos - Origin.position - transform.position);
        quaternion *= Quaternion.Slerp(Quaternion.identity, transform.localRotation, 0.5f);

        Quaternion b = Quaternion.RotateTowards(rotation3, quaternion, ___lookAtMaxAngle);
        ___lookAtRot = Quaternion.Slerp(___lookAtRot, b, 0.16f);
        float num = ___lookAtBlendPer;
        ___neckTransform.rotation = Quaternion.Slerp(___neckTransform.rotation, ___lookAtRot, num * 0.4f);
        Quaternion rotation2 = transform.rotation;
        transform.rotation = Quaternion.Slerp(rotation2, ___lookAtRot, num);
        return false;
    }
}


