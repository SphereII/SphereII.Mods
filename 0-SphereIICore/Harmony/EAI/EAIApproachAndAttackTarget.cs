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
            if(__instance.entityTarget == null)
                return result;

            // If it's a zombie, don't do anything extra
            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
                return result;
    

            // Non zombies should continue to attack
            if(__instance.entityTarget.IsDead())
            {
                DisplayLog("Target is Dead. Leaving the Body.", __instance.theEntity);
                __instance.theEntity.IsEating = false;
                __instance.theEntity.SetAttackTarget(null, 0);
                return false;
            }

            // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
            if(EntityUtilities.HasTask(__instance.theEntity.entityId, "Ranged"))
            {
                float distanceSq = __instance.entityTarget.GetDistanceSq(__instance.theEntity);
                DisplayLog(" Ranged Entity: Distance between " + __instance.entityTarget.EntityName + " : " + distanceSq, __instance.theEntity);
                // Let the entity move closer, without walking a few steps and trying to fire, which can make the entity stutter as it tries to keep up with a retreating enemey.
                if(distanceSq > 50 && distanceSq < 60)
                    return result;

                // Hold your ground
                if (distanceSq > 10f && distanceSq < 60)
                {
                    DisplayLog("I am ranged, so I will not move forward.", __instance.theEntity);
                    __instance.theEntity.SetLookPosition(__instance.entityTarget.position);
                    __instance.theEntity.RotateTo(__instance.entityTarget, 45, 45);
                    __instance.theEntity.navigator.clearPath();
                    __instance.theEntity.moveHelper.Stop();
              
                    return false;
                }

                // Back away!
                if (distanceSq > 4 && distanceSq < 10)
                {
                    DisplayLog(" Ranged Entity: They are coming too close to me! I am backing away", __instance.theEntity);

                    Vector3 dirV = __instance.theEntity.position - __instance.entityTarget.position;
                    Vector3 vector = RandomPositionGenerator.CalcPositionInDirection(__instance.theEntity, __instance.theEntity.position, dirV, 40f, 80f);
                    __instance.theEntity.moveHelper.SetMoveTo(vector, false);
                    __instance.theEntity.SetLookPosition(__instance.entityTarget.position);
                    __instance.theEntity.RotateTo(__instance.entityTarget, 45, 45);
                    return false;
                }
                if (distanceSq < 5)
                {
                    DisplayLog("They are too close. Let's fight!", __instance.theEntity);
                    return true;
                }

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
            if(__result)
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
            if(__result)
                return SphereII_EAIApproachAndAttackTarget_Helper.CanContinue(__instance);

            return __result;
        }
    }



}

