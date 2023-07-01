using HarmonyLib;
using UnityEngine;

namespace Harmony.ZombieFeatures
{
    /**
     * SCoreHeadshotOnly
     * 
     * This class includes a Harmony patch that freezes an entity if its day or night.
     * 
     * Used in Winter Project 2019 for the Snowman effect.
     *  
     * Usage XML:
     * <property name="EntityActiveWhen" value="night" />
     * <property name="EntityActiveWhen" value="day" />
     */
    public class InertEntity
    {
        public static bool IsInert(global::EntityAlive alive)
        {
            if (alive == null)
                return false;

            if (alive is EntitySupplyCrate) return false;

            var entityClass = EntityClass.list[alive.entityClass];
            if ( entityClass == null) return false;
            if (!entityClass.Properties.Values.ContainsKey("EntityActiveWhen")) return false;

            var strActive = entityClass.Properties.Values["EntityActiveWhen"];

            if (strActive.ToLower() == "never")
                return true;

            return strActive.ToLower() == "night" && alive.world.IsDaytime();
        }

        [HarmonyPatch(typeof(EAIManager))]
        [HarmonyPatch("Update")]
        public class InertEAIManager
        {
            public static bool Prefix(EAIManager __instance, global::EntityAlive ___entity)
            {
                return !IsInert(___entity);
            }
        }

        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("Update")]
        public class EntityAliveUpdate
        {
            public static bool Prefix(global::EntityAlive __instance)
            {
                if (!IsInert(__instance)) return true;

                // null safety check
                if (__instance.emodel == null || __instance.emodel.avatarController == null || __instance.emodel.avatarController.GetAnimator() == null) return false;
                if (__instance.WorldTimeBorn + 15 > __instance.world.worldTime)
                {
                    __instance.emodel.avatarController.GetAnimator().enabled = true;
                    return true;
                }

                __instance.emodel.avatarController.GetAnimator().enabled = false;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("OnUpdateEntity")]
        public class EntityAliveOnUpdateLive
        {
            public static bool Prefix(global::EntityAlive __instance)
            {
                return !IsInert(__instance);
            }
        }

        [HarmonyPatch(typeof(Entity))]
        [HarmonyPatch("CanDamageEntity")]
        public class EntityCanUpdateEntity
        {
            public static bool Prefix(global::EntityAlive __instance, ref bool __result)
            {
                if (!IsInert(__instance)) return true;

                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(XUiC_TargetBar))]
        [HarmonyPatch("Update")]
        public class TargetBar
        {
            public static bool Prefix(XUiC_TargetBar __instance)
            {
                global::EntityAlive entityAlive = null;
                var hitInfo = __instance.xui.playerUI.entityPlayer.HitInfo;
                if (!hitInfo.bHitValid || !hitInfo.transform) return true;
                if (!hitInfo.tag.StartsWith("E_")) return true;

                Transform hitRootTransform;
                if ((hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform)) != null) 
                    entityAlive = hitRootTransform.GetComponent<global::EntityAlive>();
                if (entityAlive == null || !entityAlive.IsAlive()) return true;
                return !IsInert(entityAlive);
            }
        }
    }
}