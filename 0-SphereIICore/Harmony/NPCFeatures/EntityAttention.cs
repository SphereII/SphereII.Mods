//using Harmony;
//using System.Collections.Generic;
//using UnityEngine;
//class SphereII_NPCFeatures_EntityAttention
//{

//    [HarmonyPatch(typeof(EntityAlive))]
//    [HarmonyPatch("OnUpdateLive")]
//    public class SphereII_NPCFeatures_EntityAttention_OnUpdateLive
//    {

//        public static void Postfix(EntityAlive __instance)
//        {
//            if (__instance is EntityPlayerLocal)
//                return;

//            // If they are non-human, then don't do anything fancy
//            if (!EntityUtilities.IsHuman(__instance.entityId))
//                return;

//               // If they have attack targets, don't interrupt them.
//            if (__instance.GetAttackTarget() != null || __instance.GetRevengeTarget() != null)
//                return;

//            // Scan the area around the Entity
//            List<global::Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(__instance, new Bounds(__instance.position, Vector3.one * 4f));
//            if (entitiesInBounds.Count > 0)
//            {
//                for (int i = 0; i < entitiesInBounds.Count; i++)
//                {
//                    if (entitiesInBounds[i] is EntityPlayerLocal)
//                    {
//                        // Check your faction relation. If you hate each other, don't stop and talk.
//                        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(__instance, entitiesInBounds[i] as EntityPlayerLocal);
//                        if (myRelationship == FactionManager.Relationship.Hate)
//                            break;

//                        // Give the player some space if NPC is too close.
//                        if (__instance.GetDistance(entitiesInBounds[i]) < 1)
//                        {
//                            // Give the NPC a chance to move into position before facing the player.
//                            __instance.moveHelper.SetMoveTo((entitiesInBounds[i] as EntityPlayerLocal).GetLookVector(), false);
//                            break;
//                        }

//                        // Turn to face the player, and stop the movement.
//                        __instance.SetLookPosition(entitiesInBounds[i].getHeadPosition());
//                        __instance.RotateTo(entitiesInBounds[i], 30f, 30f);
//                        __instance.navigator.clearPath();
//                        __instance.moveHelper.Stop();
//                        break;
//                    }
//                }
//            }
//        }
//    }
//}

