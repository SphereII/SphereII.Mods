// using System;
// using System.Collections.Generic;
// using System.Threading;
// using HarmonyLib;
// using SCore.Features.EntityPooling.Scripts;
// using SCore.Features.RemoteCrafting.Scripts;
// using UnityEngine;
//
// namespace SCore.Features.EntityPooling.Harmony {
//     public class EntityFactoryPatch {
//         private const string AdvFeatureClass = "AdvancedZombieFeatures";
//         private const string Feature = "EntityPooling";
//
//
//         [HarmonyPatch(typeof(EntityFactory))]
//         [HarmonyPatch("CreateEntity")]
//         [HarmonyPatch(new[] { typeof(EntityCreationData) })]
//         public class EntityFactoryCreateEntity {
//             public static bool Prefix(ref Entity __result, EntityFactory __instance, EntityCreationData _ecd) {
//                 if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
//                     return true;
//                 // Factory pool offline?
//                 if (EntityFactoryPool.Instance == null) return true;
//
//                 // Skip the player.
//                 var isPlayer = _ecd.entityClass == EntityClass.playerMaleClass ||
//                                _ecd.entityClass == EntityClass.playerFemaleClass;
//                 if (isPlayer) return true;
//
//                 if (EntityFactoryPool.Instance.isRunning) return true;
//                 
//                 // If the pool is empty, skip it.
//                 __result= EntityFactoryPool.Instance.GetEntityFromPool(_ecd.entityClass);
//                 if (__result == null) return true;
//                 if (_ecd.id == -1)
//                 {
//                     _ecd.id = EntityFactory.nextEntityID++;
//                 }
//                 else
//                 {
//                     EntityFactory.nextEntityID = Math.Max(_ecd.id + 1, EntityFactory.nextEntityID);
//                 }
//                 
//                 Debug.Log($"Creating from the pool entry: {_ecd.id} ");
//                 EntityFactoryPool.Instance.ConfigurePoolEntity(ref __result, _ecd);
//                 return false;
//             }
//
//          
//         }
//     }
// }