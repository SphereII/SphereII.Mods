using System;
using HarmonyLib;
using UAI;
namespace Harmony.EAI
{
    [HarmonyPatch(typeof(EAITarget))]
    [HarmonyPatch("check")]
    public class EAITarget_check
    {
        public static void Postfix(ref bool __result, EAITarget __instance, global::EntityAlive _e)
        {
            if (!__result) return;

            // Checks if we are allies, either share a leader, or is our leader.
            if (EntityTargetingUtilities.IsAlly(__instance.theEntity, _e))
            {
                __result = false;
                return;
            }

            // Do we have a revenge target? Are they the ones attacking us?
            if (EntityTargetingUtilities.IsCurrentRevengeTarget(__instance.theEntity, _e))
            {
                __result = true;
                return;
            }

            // If the target entity is attacking our ally, target them too.
            var leader = EntityUtilities.GetLeaderOrOwner(__instance.theEntity.entityId);
            if (EntityTargetingUtilities.IsFightingFollowers(leader, _e))
            {
                __result = true;
                return;
            }

            var myRelationship = FactionManager.Instance.GetRelationshipTier(__instance.theEntity, _e);
            switch (myRelationship)
            {
                case FactionManager.Relationship.Hate:
                    __result = true;
                    break;
                case FactionManager.Relationship.Dislike:  
                    __result = true;
                    break;
                case FactionManager.Relationship.Neutral:
                case FactionManager.Relationship.Like:
                case FactionManager.Relationship.Love:
                case FactionManager.Relationship.Leader:
                default:
                    __result = false;
                    break;
            }

            return;
        }
    }
}
