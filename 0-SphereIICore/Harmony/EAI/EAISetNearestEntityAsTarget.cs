using Harmony;
class SphereII_EAISetNearestEntityAsTarget_Tweaks
{

    [HarmonyPatch(typeof(EAISetNearestEntityAsTarget))]
    [HarmonyPatch("CanExecute")]
    public class SphereII_EAISetNearestEntityAsTarget_CanExecute
    {
        public static bool Postfix(bool __result, EAIApproachAndAttackTarget __instance)
        {
            // Check if we have any target in mind.
            EntityAlive targetEntity = __instance.theEntity.GetRevengeTarget();
            if(targetEntity == null)
                targetEntity = __instance.theEntity.GetAttackTarget();
            if(targetEntity == null)
                return __result;

            // If we have a target, check if they are our leader, so we can forgive them.
            if(__result)
            {
                // If the Revenge Target is your leader, then forgive them?
                Entity myLeader = EntityUtilities.GetLeaderOrOwner(__instance.theEntity.entityId);
                if(myLeader)
                {
                    if(targetEntity.entityId == myLeader.entityId)
                        __result = false;
                }
            }

            // If we are still intent on attacking this target, check its faction, and see if we can forgive them.
            // If they don't really like them, it doesn't mean they want to kill them.
            if(__result)
            {
                FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(__instance.theEntity, targetEntity);
                if(myRelationship != FactionManager.Relationship.Hate)
                    __result = false;
            }

            return __result;
        }
    }



}

