// using HarmonyLib;
// using Platform;
//
// namespace SCore.Features.Drones.Harmony
// {
//     [HarmonyPatch(typeof(EntityDrone))]
//     [HarmonyPatch("EntityDrone")]
//     public class EntityDroneEntityDrone
//     {
//         public static bool Prefix(Entity _entity)
//         {
//             var instance = GameManager.Instance;
//             if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
//             {
//                 var entityDrone = _entity as EntityDrone;
//                 if (entityDrone)
//                 {
//                     entityDrone.OwnerID = PlatformManager.InternalLocalUserIdentifier;
//                     var playerData = instance.GetPersistentPlayerList().GetPlayerData(entityDrone.OwnerID);
//                     if (playerData != null)
//                     {
//                         entityDrone.belongsPlayerId = playerData.EntityId;
//                         (instance.World.GetEntity(playerData.EntityId) as EntityAlive)?.AddOwnedEntity(_entity);
//                     }
//                 }
//             }
//
//             instance.World.EntityLoadedDelegates -= EntityDrone.OnClientSpawnRemote;
//             return false;
//         }
//     }
// }