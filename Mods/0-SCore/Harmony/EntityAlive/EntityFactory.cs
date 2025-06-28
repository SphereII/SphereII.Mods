using System;
using HarmonyLib;
using UnityEngine;

namespace Harmony.EntityAlive {
    public class EntityFactoryPatch {
        
        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch(nameof(Animator.Play))]
        [HarmonyPatch(typeof(EntityFactory))]
        [HarmonyPatch("GetEntityType")]
        public class EntityFactoryGetEntityType {
            public static bool Prefix(ref Type __result, string _className) {
                if (_className == "EntityAliveSDX")
                {
                    __result =  typeof(EntityAliveSDX);
                    return false;
                }
                if (_className == "EntitySurvivor")
                {
                    __result =  typeof(EntitySurvivor);
                    return false;
                }
                if (_className == "EntityNPCBandit")
                {
                    __result =  typeof(EntityNPCBandit);
                    return false;
                }
                
                if (_className == "EntityBanditSDX")
                {
                    __result =  typeof(EntityBanditSDX);
                    return false;
                }
                if (_className == "EntitySwimmingSDX")
                {
                    __result =  typeof(EntitySwimmingSDX);
                    return false;
                }
                
                if (_className == "EntityZombieFlyingSDX")
                {
                    __result =  typeof(EntityZombieFlyingSDX);
                    return false;
                }
               
                return true;            
            }
        }
    }
}