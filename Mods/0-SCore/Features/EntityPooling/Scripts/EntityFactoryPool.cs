// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace SCore.Features.EntityPooling.Scripts {
//     public class EntityFactoryPool : MonoBehaviour {
//         private const string AdvFeatureClass = "AdvancedZombieFeatures";
//         private const string Feature = "EntityPooling";
//
//         private Dictionary<int, List<Entity>> _entityFactoryPool;
//
//         private int maxToPool = 100;
//         public static EntityFactoryPool Instance;
//         public bool isRunning;
//         private void Awake() {
//             if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
//             {
//                 Debug.Log("EntityPooling Disabled.");
//                 Destroy(this);
//             }
//
//             Debug.Log("EntityPooling Enabled.");
//             Instance = this;
//         }
//
//         private IEnumerator Start() {
//             yield return StartCoroutine(nameof(Init));
//         }
//
//         private IEnumerator Init() {
//             isRunning = true;
//             Debug.Log("Entity Factory Pool Initializing...");
//             _entityFactoryPool = new Dictionary<int, List<Entity>>();
//             foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
//             {
//                 // Don't pool non-spawnables.
//                 if (keyValuePair.Value.userSpawnType == EntityClass.UserSpawnType.None) continue;
//                 // Don't pool vehicles.
//                 if (keyValuePair.Value.Tags.Test_AnySet(FastTags.Parse("vehicle"))) continue;
//                 if ( !keyValuePair.Value.entityClassName.StartsWith("zombie")) continue;
//                 var entities = new List<Entity>();
//                 var clone = false;
//                 var baseEntity = EntityFactory.CreateEntity(keyValuePair.Key, Vector3.zero) as EntityAlive;
//                 for (var x = 0; x < maxToPool; x++)
//                 {
//                     Entity newEntity;
//                     if (clone)
//                     {
//                         newEntity = Instantiate(baseEntity);
//                     }
//                     else
//                     {
//                         newEntity = EntityFactory.CreateEntity(keyValuePair.Key, Vector3.zero) as EntityAlive;
//                     }
//
//                     if ( newEntity == null ) continue;
//                     newEntity.gameObject.SetActive(false);
//                     entities.Add(newEntity);
//                   
//                     yield return new WaitForEndOfFrame();
//                 }
//                 Debug.Log($"Created Entry Pool for {keyValuePair.Value.entityClassName}: {maxToPool}");
//                 _entityFactoryPool[keyValuePair.Key] = entities;
//                 break;
//             }
//
//             Debug.Log("Entity Factory Pool Complete.");
//             isRunning = false;
//
//         }
//
//         public Entity GetEntityFromPool(int entityClass) {
//             // Not pooled?
//             if (!_entityFactoryPool.ContainsKey(entityClass))
//                 return null;
//             foreach (var entity in _entityFactoryPool[entityClass])
//             {
//                 if ( entity == null ) continue;
//                 if (entity.gameObject.activeInHierarchy)
//                 {
//                     continue;
//                 }
//                 Debug.Log("Pulling entity from pool.");
//                 entity.gameObject.SetActive(true);
//                 return entity;
//             }
//
//             return null;
//         }
//
//         public void ConfigurePoolEntity(ref Entity targetEntity, EntityCreationData _ecd) {
//             _ecd.ApplyToEntity(targetEntity);
//             targetEntity.lifetime = _ecd.lifetime;
//             targetEntity.entityId = _ecd.id;
//             targetEntity.belongsPlayerId = _ecd.belongsPlayerId;
//             targetEntity.InitLocation(_ecd.pos, _ecd.rot);
//             targetEntity.onGround = _ecd.onGround;
//         }
//     }
// }