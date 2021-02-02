using HarmonyLib;
using System;
using UnityEngine;


/**
 * SphereII_HeadshotOnly
 *
 * This class includes a Harmony patch that freezes an entity if its day or night.
 * 
 *  Used in Winter Project 2019 for the Snowman effect.
 *  
 * Usage XML:
 * 
 *      <property name="EntityActiveWhen" value="night" />
 *      <property name="EntityActiveWhen" value="day" />
 *
 */
public class SphereII_InertEntity
{
    public static bool IsInert(EntityAlive alive)
    {
        if (alive == null)
            return false;

        EntityClass entityClass = EntityClass.list[alive.entityClass];
        if (entityClass.Properties.Values.ContainsKey("EntityActiveWhen"))
        {
            String strActive = entityClass.Properties.Values["EntityActiveWhen"];
            if (strActive.ToLower() == "night")
            {
                if (alive.world.IsDaytime())
                    return true;
            }
        }
        return false;
    }

    [HarmonyPatch(typeof(EAIManager))]
    [HarmonyPatch("Update")]
    public class SphereII_EAIManager
    {
        public static bool Prefix(EAIManager __instance, EntityAlive ___entity)
        {
            return !SphereII_InertEntity.IsInert(___entity);
        }

    }

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Update")]
    public class SphereII_EntityAlive_update
    {
        public static bool Prefix(EntityAlive __instance)
        {
            if (SphereII_InertEntity.IsInert(__instance))
            {
                if ((__instance.WorldTimeBorn + 15) > __instance.world.worldTime)
                {
                    __instance.emodel.avatarController.GetAnimator().enabled = true;
                    return true;
                }

                __instance.emodel.avatarController.GetAnimator().enabled = false;
                return false;
            }


            return true;
        }

    }

    [HarmonyPatch(typeof(Entity))]
    [HarmonyPatch("CanDamageEntity")]
    public class SphereII_Entity_CanUpdateEntity
    {
        public static bool Prefix(EntityAlive __instance, ref bool __result)
        {
            if (SphereII_InertEntity.IsInert(__instance))
            {
                __result = false;
                return false;
            }

            return true;
        }

    }

    [HarmonyPatch(typeof(XUiC_TargetBar))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_TargetBar
    {
        public static bool Prefix(XUiC_TargetBar __instance)
        {
            EntityAlive entityAlive = null;
            WorldRayHitInfo hitInfo = __instance.xui.playerUI.entityPlayer.HitInfo;
            if (hitInfo.bHitValid && hitInfo.transform && hitInfo.tag.StartsWith("E_"))
            {
                Transform hitRootTransform;
                if ((hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform)) != null)
                {
                    entityAlive = hitRootTransform.GetComponent<EntityAlive>();
                }
                if (entityAlive != null && entityAlive.IsAlive())
                {
                    if (SphereII_InertEntity.IsInert(entityAlive))
                        return false;

                }

            }

            return true;
        }

    }

}

