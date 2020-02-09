using Harmony;
using System;
using UnityEngine;
class SphereII_EAISetNearestEntityAsTarget_Tweaks
{

    [HarmonyPatch(typeof(EAISetNearestEntityAsTarget))]
    [HarmonyPatch("CanExecute")]
    public class SphereII_EAISetNearestEntityAsTarget_CanExecute
    {
        public static bool blDisplayLog = false;
        public static void DisplayLog(String strMessage, EntityAlive theEntity)
        {
            if (blDisplayLog)
                Debug.Log(theEntity.EntityName + ": " + strMessage);
        }


        public static bool Postfix(bool __result, EAISetNearestEntityAsTarget __instance)
        {
            // If it's a zombie, don't do anything extra
            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
                return __result;


            // Check if we have any target in mind.
            EntityAlive targetEntity = __instance.targetEntity;
            if (targetEntity == null)
            {
                DisplayLog("No Target Entity", __instance.theEntity);
                return __result;
            }

            DisplayLog("Postfix for CanExecute()", __instance.theEntity);
            // If we have a target, check if they are our leader, so we can forgive them.
            if (__result)
            {
                DisplayLog("Checking for Leader", __instance.theEntity);
                // If the Revenge Target is your leader, then forgive them?
                Entity myLeader = EntityUtilities.GetLeaderOrOwner(__instance.theEntity.entityId);
                if (myLeader)
                {
                    DisplayLog("Leader Found " + myLeader.entityId + ", checking ID", __instance.theEntity);
                    if (targetEntity.entityId == myLeader.entityId)
                        __result = false;
                }
            }

            // If we are still intent on attacking this target, check its faction, and see if we can forgive them.
            // If they don't really like them, it doesn't mean they want to kill them.
            if (__result)
            {
                DisplayLog("Checking Relationship with " + targetEntity.entityId, __instance.theEntity);
                FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(__instance.theEntity, targetEntity);
                if (myRelationship != FactionManager.Relationship.Hate)
                    __result = false;
                DisplayLog("\tMRelationship with " + targetEntity.entityId + " is " + myRelationship.ToString(), __instance.theEntity);
            }

            DisplayLog("CanExecute(): " + __result, __instance.theEntity);
            return __result;
        }
    }



}

