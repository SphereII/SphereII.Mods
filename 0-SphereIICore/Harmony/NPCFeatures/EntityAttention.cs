//using Harmony;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using UnityEngine;
//class SphereII_NPCFeatures_EntityAttention
//{


//    [HarmonyPatch(typeof(EntityNPC))]
//    [HarmonyPatch("OnEntityActivated")]
//    public class SphereII_NPCFeatures_EntityAttention_OnEntityActivated
//    {
//        public static bool Prefix(EntityNPC __instance, EntityAlive _entityFocusing)
//        {
//            // Don't allow interaction with a Hated entity
//            FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(__instance, _entityFocusing);
//            if (myRelationship == FactionManager.Relationship.Hate)
//                return false;

//            // If they have attack targets, don't interrupt them.
//            if (__instance.GetAttackTarget() != null || __instance.GetRevengeTarget() != null)
//                return false;

//            return true;
//        }
//    }

//    [HarmonyPatch(typeof(EntityNPC))]
//    [HarmonyPatch("OnUpdateLive")]
//    public class SphereII_NPCFeatures_EntityAttention_OnUpdateLive
//    {

//        public static void Postfix(EntityNPC __instance)
//        {
//            if (__instance == null)
//                return;

//            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
//                return;

//            // If they are non-human, then don't do anything fancy
//            if (!EntityUtilities.IsHuman(__instance.entityId))
//                return;

//            // If they have attack targets, don't interrupt them.
//            if (__instance.GetAttackTarget() != null || __instance.GetRevengeTarget() != null)
//                return;

//            // No NPC info, don't continue
//            if (__instance.NPCInfo == null)
//                return;

//            // If the Tile Entity Trader isn't set, set it now. Sometimes this fails, and won't allow interaction.
//            if (__instance.TileEntityTrader == null)
//            {
//                __instance.TileEntityTrader = new TileEntityTrader(null);
//                __instance.TileEntityTrader.entityId = __instance.entityId;
//                __instance.TileEntityTrader.TraderData.TraderID = __instance.NPCInfo.TraderID;
//            }

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
//                        {
//                            //       XUiC_InteractionPrompt.SetText(LocalPlayerUI.GetUIForPlayer(entitiesInBounds[i] as EntityPlayerLocal), "");
//                            break;
//                        }
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


