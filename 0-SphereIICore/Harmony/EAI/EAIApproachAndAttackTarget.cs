//using DMT;
//using Harmony;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;
//class SphereII_EAIApproachAndAttackTarget
//{
//    public class SphereII_EAIApproachAndAttackTarget_Helper
//    {
//        public static bool CanContinue(EAIApproachAndAttackTarget __instance)
//        {
//            bool result = true;
//            if (__instance.entityTarget == null)
//                return result;

//            // If it's a zombie, don't do anything extra
//            if (__instance.theEntity.HasAnyTags(FastTags.Parse("zombie")))
//                return result;

//            // Non zombies should continue to attack
//            if (__instance.entityTarget.IsDead())
//            {
//                Debug.Log("Entity is dead. Leaving it");
//                __instance.theEntity.IsEating = false;
//                __instance.theEntity.SetAttackTarget(null, 0);
//                return false;
//            }

//            // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
//            if (EntityUtilities.HasTask(__instance.theEntity.entityId, "Ranged"))
//            {
//                float distanceSq = __instance.entityTarget.GetDistanceSq(__instance.theEntity);
//                Debug.Log(" Ranged Entity: Distance between " + __instance.entityTarget.EntityName + " : " + distanceSq);
//                if (distanceSq > 40 && distanceSq < 60)
//                    return result;

//                if (distanceSq > 4f && distanceSq < 60)
//                {
//                    Debug.Log("I am ranged, so I will not move forward.");
//                    __instance.theEntity.navigator.clearPath();
//                    __instance.theEntity.moveHelper.Stop();
//                    __instance.theEntity.SetLookPosition(__instance.entityTarget.position);
//                    __instance.theEntity.RotateTo(__instance.entityTarget, 45, 45);
//                    return false;
//                }
//            }

//            return result;
//        }
//    }
//    // If the entity is dead, don't hover over it.
//    [HarmonyPatch(typeof(EAIApproachAndAttackTarget))]
//    [HarmonyPatch("Continue")]
//    public class SphereII_EAIApproachAndAttackTarget_Continue
//    {
//        public static bool Postfix(bool __result, EAIApproachAndAttackTarget __instance, ref bool ___isTargetToEat)
//        {
//            if (__result)
//                return SphereII_EAIApproachAndAttackTarget_Helper.CanContinue(__instance);
//            return __result;
//        }
//    }

//    // If the entity is dead, don't hover over it.
//    [HarmonyPatch(typeof(EAIApproachAndAttackTarget))]
//    [HarmonyPatch("CanExecute")]
//    public class SphereII_EAIApproachAndAttackTarget_CanExecute
//    {
//        public static bool Postfix(bool __result, EAIApproachAndAttackTarget __instance, ref bool ___isTargetToEat)
//        {
//            if (__result)
//                return SphereII_EAIApproachAndAttackTarget_Helper.CanContinue(__instance);

//            return __result;
//        }
//    }



//}

