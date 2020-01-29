//using DMT;
//using Harmony;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;
//class SphereII_NPCFeatures_EntityAttention
//{

//    [HarmonyPatch(typeof(EntityAlive))]
//    [HarmonyPatch("OnUpdateLive")]
//    public class SphereII_NPCFeatures_EntityAttention_OnUpdateLive
//    {

//        public static void Postfix(EntityAlive __instance)
//        {
//            // If it's a zombie, don't do anything extra
//            if (__instance.HasAnyTags(FastTags.Parse("zombie")))
//                return;

//            // If they have attack targets, don't interrupt them.
//            if (__instance.GetAttackTarget() != null || __instance.GetRevengeTarget() != null)
//                return;

//            List<global::Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(__instance, new Bounds(__instance.position, Vector3.one * 4f));
//            if (entitiesInBounds.Count > 0)
//            {
//                for (int i = 0; i < entitiesInBounds.Count; i++)
//                {
//                    if (entitiesInBounds[i] is EntityPlayer)
//                    {
//                        if (__instance.GetDistance(entitiesInBounds[i]) > 1)
//                        {
//                            __instance.SetLookPosition(entitiesInBounds[i].getHeadPosition());
//                            __instance.RotateTo(entitiesInBounds[i], 30f, 30f);
//                            break;
//                        }
//                        else
//                        {
//                            // Move to where the player is looking.
//                            __instance.moveHelper.SetMoveTo((entitiesInBounds[i] as EntityPlayer).GetLookVector(), false);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}

