using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
class SphereII_EAISetAsTargetHurt_Leader_Protect
{
 
    // If this entity has a CVAr for an owner or leader, and they accidentally hurt this entity, don't set it as a target.
    [HarmonyPatch(typeof(EAISetAsTargetIfHurt))]
    [HarmonyPatch("CanExecute")]
    public class SphereII_EAIApproachAndAttackTarget_CanExecute
    {
        public static bool Postfix(bool __result, EAIApproachAndAttackTarget __instance)
        {
            if(__result)
            {
                // If the Revenge Target is your leader, then forgive them?
                if(__instance.theEntity.GetRevengeTarget() != null)
                {
                    Entity myLeader = EntityUtilities.GetLeaderOrOwner(__instance.theEntity.entityId);
                    if(myLeader)
                    {
                        if(__instance.theEntity.GetRevengeTarget().entityId == myLeader.entityId)
                            __result = false;
                    }
                }
            }

            return __result;
        }
    }



}

