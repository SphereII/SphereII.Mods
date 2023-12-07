// using System.Collections.Generic;
// using HarmonyLib;
// using Platform;
// using UnityEngine;
//
// namespace SCore.Features.Drones.Harmony
// {
//     [HarmonyPatch(typeof(EntityDrone))]
//     [HarmonyPatch("EntityDrone")]
//     public class EntityDroneEntityDrone
//     {
//         // public static bool Prefix(Entity _entity)
//         // {
//         //     var instance = GameManager.Instance;
//         //     if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
//         //     {
//         //         var entityDrone = _entity as EntityDrone;
//         //         if (entityDrone)
//         //         {
//         //             entityDrone.OwnerID = PlatformManager.InternalLocalUserIdentifier;
//         //             var playerData = instance.GetPersistentPlayerList().GetPlayerData(entityDrone.OwnerID);
//         //             if (playerData != null)
//         //             {
//         //                 entityDrone.belongsPlayerId = playerData.EntityId;
//         //                 (instance.World.GetEntity(playerData.EntityId) as EntityAlive)?.AddOwnedEntity(_entity);
//         //             }
//         //         }
//         //     }
//         //
//         //     instance.World.EntityLoadedDelegates -= EntityDrone.OnClientSpawnRemote;
//         //     return false;
//         // }
//
//
//         [HarmonyPatch(typeof(ItemActionSpawnTurret))]
//         [HarmonyPatch("updatePreview")]
//         public class ItemActionSpawnTurretUpdatePreview
//         {
//             public class ItemActionDataSpawnTurret : ItemActionAttackData
//             {
//                 // Token: 0x0600968D RID: 38541 RVA: 0x003DED4F File Offset: 0x003DCF4F
//                 public ItemActionDataSpawnTurret(ItemInventoryData _invData, int _indexInEntityOfAction) : base(
//                     _invData, _indexInEntityOfAction)
//                 {
//                 }
//
//                 // Token: 0x04007061 RID: 28769
//                 public Transform TurretPreviewT;
//
//                 // Token: 0x04007062 RID: 28770
//                 public Renderer[] PreviewRenderers;
//
//                 // Token: 0x04007063 RID: 28771
//                 public bool ValidPosition;
//
//                 // Token: 0x04007064 RID: 28772
//                 public Vector3 Position;
//
//                 // Token: 0x04007065 RID: 28773
//                 public bool Placing;
//             }
//
//
//             public static bool CalcSpawnPosition(ItemActionDataSpawnTurret _actionData, ref Vector3 position,
//                 Vector3 turretSize)
//             {
//                 World world = _actionData.invData.world;
//                 Ray lookRay = _actionData.invData.holdingEntity.GetLookRay();
//                 if (Voxel.Raycast(world, lookRay, 4f + turretSize.x, 8454144, 69, 0f))
//                 {
//                     position = Voxel.voxelRayHitInfo.hit.pos;
//                 }
//                 else
//                 {
//                     position = lookRay.origin + lookRay.direction * (4f + turretSize.x);
//                 }
//
//                 Collider[] array = Physics.OverlapSphere(position - Origin.position + Vector3.up * 0.525f, 0.5f);
//                 Debug.Log($"Colliders: {array.Length}");
//                 for (int i = 0; i < array.Length; i++)
//                 {
//                     Debug.Log(
//                         $"\tItem: {_actionData.invData.item.GetItemName()} :: {array[i].gameObject.name} Tags: {array[i].gameObject.tag}");
//                     if (array[i].gameObject.layer != 18 &&
//                         !(array[i].gameObject == _actionData.TurretPreviewT.gameObject))
//                     {
//                         return false;
//                     }
//                 }
//
//                 return true;
//             }
//
//             public static void Postfix(ItemActionDataSpawnTurret data, ref Vector3i ___turretSize)
//             {
//                 Debug.Log(
//                     $"Update Preview: Data Position: {data.Position} Origin Position: {Origin.position} Location: {data.Position - Origin.position}");
//                 Debug.Log($"CalcSpawn Point: Turret Size: {___turretSize}");
//                 var flag = CalcSpawnPosition(data, ref data.Position, ___turretSize);
//                 Debug.Log($"Result: {flag}");
//             }
//         }
//
//         [HarmonyPatch(typeof(ItemAction))]
//         [HarmonyPatch("ExecuteBuffActions")]
//         public class ItemActionExecuteBuffActions
//         {
//             public static bool Prefix(List<string> actions, int instigatorId, EntityAlive target, bool isCritical,
//                 EnumBodyPartHit hitLocation, string context)
//             {
//                 Debug.Log($"ExecuteBuffActions for {instigatorId} ");
//                 if (target)
//                     Debug.Log($"Target is: {target.entityId}");
//                 Debug.Log($"Hit Location: {hitLocation}");
//                 Debug.Log($"Context: {context}");
//                 foreach (var action in actions)
//                     Debug.Log($"Action: {action}");
//                 Debug.Log($"Is Critical? {isCritical}");
//                 return true;
//             }
//         }
//
//         [HarmonyPatch(typeof(DroneWeapons.HealBeamWeapon))]
//         [HarmonyPatch("Fire")]
//         public class HealBeamWeaponFire
//         {
//             public static bool Prefix(EntityAlive _target, EntityAlive ___entity)
//             {
//                 var drone = ___entity as EntityDrone;
//                 if (drone == null) return true;
//
//                 Debug.Log($"Get Height Height: {drone.GetEyeHeight()}");
//                 Debug.Log($"Get Look Direction: {drone.GetLookVector()}");
//                 drone.SetModelLayer(2, false, null);
//                 if (___entity.GetAttackTarget() == null)
//                     Debug.Log("Attack Target is null");
//                 else
//                 {
//                     Debug.Log("Attack target is set.");
//                 }
//
//                 if (Voxel.Raycast(___entity.world, drone.GetLookRay(), 4f, -538750981, 128, 2f))
//                 {
//                     Debug.Log("Ray cast was successful.");
//                     var entityAlive = (ItemActionUseOther.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
//                     if (entityAlive == null)
//                         Debug.Log("Entity was null");
//                     else
//                     {
//                         Debug.Log($"Target is {entityAlive.entityId}");
//                     }
//                 }
//                 else
//                 {
//                     Debug.Log("Ray cast failed.");
//                 }
//
//                 return true;
//             }
//
//             public static void Postfix(EntityAlive _target, EntityAlive ___entity)
//             {
//                 Debug.Log("DroneWeapons:: Fire!");
//                 Debug.Log($"Target: {_target.entityId}");
//                 Debug.Log($"Source: {___entity.entityId}");
//                 ItemAction itemAction = ___entity.inventory.holdingItem.Actions[1];
//                 ItemActionData actionData = ___entity.inventory.holdingItemData.actionData[1];
//                 if (actionData == null)
//                     Debug.Log("Action Data is NULL!");
//                 else
//                 {
//                     Debug.Log($"ItemAction Data: {actionData.invData.item.GetItemName()}");
//                 }
//
//                 if (itemAction == null)
//                     Debug.Log("Item Action is null.");
//             }
//         }
//
//         [HarmonyPatch(typeof(ItemActionUseOther))]
//         [HarmonyPatch("ExecuteAction")]
//         public class ItemActionUseOtherExecuteAction
//         {
//             public static bool Prefix(ref ItemActionData _actionData)
//             {
//                 var feedInventoryData = (ItemActionUseOther.FeedInventoryData) _actionData;
//                 var holdingEntity = feedInventoryData.invData.holdingEntity as EntityDrone;
//                 if (holdingEntity == null) return true;
//
//                 var target = holdingEntity.GetAttackTarget();
//                 if (target != null)
//                 {
//                     feedInventoryData.TargetEntity = target;
//                 }
//
//                 return true;
//             }
//
//             public static void Postfix(ItemActionUseOther __instance, ItemActionData _actionData, bool _bReleased)
//             {
//                 Debug.Log("ItemActionUserOther");
//                 ItemActionUseOther.FeedInventoryData feedInventoryData =
//                     (ItemActionUseOther.FeedInventoryData) _actionData;
//                 if (feedInventoryData.TargetEntity == null)
//                     Debug.Log("Target Entity is null.");
//                 else
//                 {
//                     Debug.Log($"Target Entity is: {feedInventoryData.TargetEntity.entityId}");
//                 }
//
//                 Debug.Log($"Feeding Started? {feedInventoryData.bFeedingStarted}");
//                 float distance = 4f;
//                 EntityAlive entityAlive = null;
//                 if (Voxel.Raycast(feedInventoryData.invData.world, feedInventoryData.ray, distance, -538750981, 128,
//                         __instance.SphereRadius))
//                 {
//                     Debug.Log("Ray cast hit recorded. Checking for entity alive...");
//                     entityAlive = (ItemActionUseOther.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
//                 }
//
//                 if (entityAlive == null || !entityAlive.IsAlive() || !(entityAlive is EntityPlayer))
//                 {
//                     Debug.Log("Ray cast failed. Running another ray cast.");
//                     Voxel.Raycast(feedInventoryData.invData.world, feedInventoryData.ray, distance, -538488837, 128,
//                         __instance.SphereRadius);
//                 }
//
//                 if (entityAlive == null)
//                 {
//                     Debug.Log("Ray cast still failed. Extending distance.");
//                     distance = 6f;
//                     if (Voxel.Raycast(feedInventoryData.invData.world, feedInventoryData.ray, distance, -538750981, 128,
//                             __instance.SphereRadius))
//                     {
//                         Debug.Log("Ray cast was successful.. check for entity alive.");
//                         entityAlive = (ItemActionUseOther.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
//                     }
//                 }
//
//                 if (entityAlive == null)
//                     Debug.Log("Entity alive is still null");
//                 else
//                 {
//                     Debug.Log("Entity alive was found.s");
//                 }
//             }
//         }
//     }
// }