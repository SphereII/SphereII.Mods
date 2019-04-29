using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class SphereII__EntityAlivePatcher
{
    public class SphereII_EntityAlivePatch : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Attack")]
    public class SphereII_EntityAlive_Attack
    {
        static bool Prefix(EntityAlive __instance)
        {
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

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("SetAttackTarget")]
    public class SphereII_EntityAlive_SetAttackTarget
    {
        static bool Prefix(EntityAlive __instance)
        {
            // If a door is found, try to open it. If it returns false, start attacking it.
            EntityAliveSDX myEntity = __instance as EntityAliveSDX;
            if (myEntity)
                myEntity.RestoreSpeed();
            return true;
        }
    }
}