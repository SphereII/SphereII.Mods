using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class SphereII__EntityAlivePatcher
{
    private static string AdvFeatureClass = "AdvancedNPCFeatures";
    private static string Feature = "EnhancedFeatures";


    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Attack")]
    public class SphereII_EntityAlive_Attack
    {
        static bool Prefix(EntityAlive __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            // Check if there's a door in our way, then open it.
            if (__instance.GetAttackTarget() == null)
            {
                // If it's an animal, don't let them attack blocks
                EntityAliveFarmingAnimalSDX animal = __instance as EntityAliveFarmingAnimalSDX;
                if (animal)
                {
                    if (__instance.GetAttackTarget() == null)
                        return false;
                }
                // If a door is found, try to open it. If it returns false, start attacking it.
                EntityAliveSDX myEntity = __instance as EntityAliveSDX;
                if (myEntity)
                {
                    if (myEntity.OpenDoor())
                        return true;
                }
            }

            if (__instance.GetAttackTarget() != null)
                __instance.RotateTo(__instance.GetAttackTarget(), 30f, 30f);

            return true;
        }
    }


    // Disables the friendly fire of allies
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("DamageEntity")]
    public class SphereII_EntityAlive_DamageEntity
    {
        static bool Prefix(EntityAlive __instance, DamageSource _damageSource)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (EntityUtilities.IsAnAlly(__instance.entityId, _damageSource.getEntityId()))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("SetAttackTarget")]
    public class SphereII_EntityAlive_SetAttackTarget
    {
        static bool Prefix(EntityAlive __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            // If a door is found, try to open it. If it returns false, start attacking it.
            EntityAliveSDX myEntity = __instance as EntityAliveSDX;
            if (myEntity)
                myEntity.RestoreSpeed();
            return true;
        }
    }


    // NPCs cause an exception if they have the bleeding buff. Since the entity is dying and not respawning, let's just avoid removing the buffs
    [HarmonyPatch(typeof(EntityBuffs))]
    [HarmonyPatch("RemoveBuff")]
    public class SphereII_EntityBuffs_RemoveBuffs
    {
        static bool Prefix(EntityBuffs __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (__instance == null || __instance.parent == null)
                return false;

            if(__instance.parent is EntityAliveSDX)
                if ( __instance.parent.IsDead())
                    return false;

            return true;
         
        }
    }

    
}