using HarmonyLib;
using UnityEngine;

namespace Harmony.ZombieFeatures
{
    /**
     * 
     * This class includes a Harmony patch that freezes an entity until it has an attack target.
     * 
     *  
     * Usage XML:
     * <property name="SleepUntilAttacked" value="true" />
     
     */
    public class IdleZombies
    {
        public static bool IsInert(global::EntityAlive alive)
        {
            if (alive == null) return false;
            if (alive is EntitySupplyCrate) return false;
            if (alive is not EntityZombie entityZombie) return false;
            var entityClass = EntityClass.list[alive.entityClass];
            if ( entityClass == null) return false;
            var result = entityClass.Properties.Values.ContainsKey("SleepUntilAttacked");
            if (result == false) return false;
            if (EntityUtilities.GetAttackOrRevengeTarget(alive.entityId) != null) return false;
            if (entityZombie.IsSleeping) return true;
            entityZombie.TriggerSleeperPose(5, false);
            return true;
        }


        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("Update")]
        public class EntityAliveUpdate
        {
            public static bool Prefix(global::EntityAlive __instance)
            {
                // null safety check
                if (__instance.emodel == null || __instance.emodel.avatarController == null ||
                    __instance.emodel.avatarController.GetAnimator() == null) return true;

                if (__instance.WorldTimeBorn + 15 > __instance.world.worldTime) return true;
                return !IsInert(__instance);
            }
        }

    
    }
}