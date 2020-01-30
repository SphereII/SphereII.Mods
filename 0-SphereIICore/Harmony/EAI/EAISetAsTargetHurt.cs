using Harmony;
class SphereII_EAISetAsTargetHurt_Tweaks
{

    // If this entity has a CVAr for an owner or leader, and they accidentally hurt this entity, don't set it as a target.
    [HarmonyPatch(typeof(EAISetAsTargetIfHurt))]
    [HarmonyPatch("CanExecute")]
    public class SphereII_EAISetAsTargetIfHurt_CanExecute
    {
        public static bool Postfix(bool __result, EAISetAsTargetIfHurt __instance)
        {
            EntityAlive targetEntity = null;
            if(__result)
            {
                // Do extra processing to see if your revenge target is your leader, or even part of the same faction as you are.
                if(__instance.theEntity.GetRevengeTarget() != null)
                {
                    targetEntity = __instance.theEntity.GetRevengeTarget();
                    Entity myLeader = EntityUtilities.GetLeaderOrOwner(__instance.theEntity.entityId);
                    if(myLeader)
                    {
                        if(targetEntity.entityId == myLeader.entityId)
                            __result = false;
                    }

                    // If the relationship is really positive, then forgive them for the attack.
                    FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(__instance.theEntity, targetEntity);
                    if( myRelationship == FactionManager.Relationship.Love)
                        __result = false;

                }
            }

            return __result;
        }
    }



}

