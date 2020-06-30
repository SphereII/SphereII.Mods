using HarmonyLib;
using UnityEngine;

/**
 * SphereII_EAITarget_Tweaks
 * 
 * This class includes a Harmony patch for the EAITarget's check(). It adds additional rules for Human NPCs, such as if the NPC is sleeping,
 * they are the same faction, if you are their leader, your faction standing, etc.
 *
 */
class SphereII_EAITarget_Tweaks
{

    // Enables faction support in the targeting.
    [HarmonyPatch(typeof(EAITarget))]
    [HarmonyPatch("check")]
    public class SphereII_EAITarget_check
    {

        public static bool Postfix(bool __result, EAITarget __instance, EntityAlive _e)
        {
            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
                return __result;

            if (__instance.theEntity.IsSleeping)
                return __result;

            // The base class sees this as a valid target.
            if (__result)
            {
                // same faction
                if (__instance.theEntity.factionId == _e.factionId)
                    return false;

                // Check if its a leader.
                Entity myLeader = EntityUtilities.GetLeaderOrOwner(__instance.theEntity.entityId);
                if (myLeader)
                {
                    if (_e.entityId == myLeader.entityId)
                        return false;
                }

                // if the attack cool down is applied, don't set a new target for 30 seconds.
                if (__instance.theEntity.Buffs.HasBuff("buffAttackCoolDown"))
                    return false;

                // We have some complicated checks here, since this method gets called by 3 different target methods.
                FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(__instance.theEntity, _e);
               // Debug.Log("Checking Relationship: " + myRelationship.ToString());
                switch (myRelationship)
                {
                    case FactionManager.Relationship.Hate:
                        return true;
                    
                    case FactionManager.Relationship.Dislike:
                    case FactionManager.Relationship.Neutral:
                        // If you don't like them, or are more or less neutral to them, 
                        // know the difference between an aggressive hit, or just a regular scan of entities.
                        if (__instance is EAISetNearestEntityAsTarget)
                            return false; // they aren't an enemy to kill right off
                        if (__instance is EAISetAsTargetIfHurt)
                            return true;  // They suck! They hurt you. Get them!
                        return false;
                    case FactionManager.Relationship.Love:
                    case FactionManager.Relationship.Leader:
                        return false;
                    default:
                        return false;
                }
            }
            return __result;
        }
    }



}

