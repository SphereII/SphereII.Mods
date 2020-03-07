using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
class SphereII_EAIApproachAndAttackTarget
{
    public class SphereII_EAIApproachAndAttackTarget_Helper
    {

        public static bool blDisplayLog = false;
        public static void DisplayLog(String strMessage, EntityAlive theEntity)
        {
            if (blDisplayLog)
                Debug.Log(theEntity.EntityName + ": " + strMessage);
        }


        public static bool CanContinue(EAIApproachAndAttackTarget __instance)
        {
            bool result = true;
            if (__instance.entityTarget == null)
                return result;

            // If it's a zombie, don't do anything extra
            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
                return result;


            // Non zombies should continue to attack
            if (__instance.entityTarget.IsDead())
            {
                DisplayLog("Target is Dead. Leaving the Body.", __instance.theEntity);
                __instance.theEntity.IsEating = false;
                __instance.theEntity.SetAttackTarget(null, 0);
                __instance.theEntity.SetRevengeTarget(null);
                //EntityUtilities.ChangeHandholdItem(__instance.theEntity.entityId, EntityUtilities.Need.Melee, -1);

                return false;
            }

            if (!__instance.theEntity.CanSee(__instance.entityTarget))
            {
                __instance.theEntity.SetAttackTarget(null, 0);
                __instance.theEntity.SetRevengeTarget(null);
                return false;
            }


            float distanceSq = __instance.entityTarget.GetDistanceSq(__instance.theEntity);

            // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
            if (EntityUtilities.HasTask(__instance.theEntity.entityId, "Ranged"))
            {
                return EntityUtilities.CheckAIRange(__instance.theEntity.entityId, __instance.entityTarget.entityId);
                //DisplayLog(" Ranged Entity: Distance between " + __instance.entityTarget.EntityName + " : " + distanceSq, __instance.theEntity);
                //// Let the entity move closer, without walking a few steps and trying to fire, which can make the entity stutter as it tries to keep up with a retreating enemey.
                //if (distanceSq > 50 && distanceSq < 60)
                //    return result;

                //// Hold your ground
                //if (distanceSq > 20f && distanceSq < 60)
                //{
                //    DisplayLog("I am ranged, so I will not move forward.", __instance.theEntity);
                //    EntityUtilities.ChangeHandholdItem(__instance.theEntity.entityId, EntityUtilities.Need.Ranged);
                //    __instance.theEntity.navigator.clearPath();
                //    __instance.theEntity.moveHelper.Stop();
                //    return false;
                //}

                //// Back away!
                //if (distanceSq > 2 && distanceSq < 21)
                //{
                //    DisplayLog(" Ranged Entity: They are coming too close to me! I am backing away", __instance.theEntity);
                //    EntityUtilities.BackupHelper(__instance.theEntity.entityId, __instance.entityTarget.position, 40);
                //    EntityUtilities.ChangeHandholdItem(__instance.theEntity.entityId, EntityUtilities.Need.Ranged);
                //    return false;
                //}
                
                //if (distanceSq < 2)
                //{
                //    EntityUtilities.ChangeHandholdItem(__instance.theEntity.entityId, EntityUtilities.Need.Melee);
                //    return true;
                //}
            }

            return result;
        }
    }


    // If the entity is dead, don't hover over it.
    [HarmonyPatch(typeof(EAIApproachAndAttackTarget))]
    [HarmonyPatch("Continue")]
    public class SphereII_EAIApproachAndAttackTarget_Continue
    {
        public static bool Postfix(bool __result, EAIApproachAndAttackTarget __instance, ref bool ___isTargetToEat)
        {
            if (__result)
                return SphereII_EAIApproachAndAttackTarget_Helper.CanContinue(__instance);
            return __result;
        }
    }

    // If the entity is dead, don't hover over it.
    [HarmonyPatch(typeof(EAIApproachAndAttackTarget))]
    [HarmonyPatch("CanExecute")]
    public class SphereII_EAIApproachAndAttackTarget_CanExecute
    {
        public static bool Postfix(bool __result, EAIApproachAndAttackTarget __instance, ref bool ___isTargetToEat)
        {
            if (__result)
                return SphereII_EAIApproachAndAttackTarget_Helper.CanContinue(__instance);

            return __result;
        }
    }



}

