using Harmony;
using UnityEngine;
class SphereII_EAITarget_Tweaks
{

    // If this entity has a CVAr for an owner or leader, and they accidentally hurt this entity, don't set it as a target.
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

          //  Debug.Log("Entity: " + __instance.theEntity.entityId + " Target: " + _e.entityId +  " + Result: " + __result);
            // The base class sees this as a valid target.
            if (__result)
            {
                // same faction
                if (__instance.theEntity.factionId == _e.factionId)
                    return false;

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
                    default:
                        return false;
                }
            }
            return __result;
        }
    }



}

