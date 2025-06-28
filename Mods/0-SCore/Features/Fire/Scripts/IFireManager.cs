// using System.Collections;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.IO;
//
// public interface IFireManager
// {
//     event FireManager.OnBlockDestroyedByFire OnDestroyed;
//     event FireManager.OnFireStart OnStartFire;
//     event FireManager.OnFireRefresh OnFireUpdate;
//     event FireManager.OnExtinguishFire OnExtinguish;
//     bool Enabled { get; }
//
//     void ReadConfig();
//     void LightsUpdate();
//     void CheckExtinguishedPosition();
//     ConcurrentDictionary<Vector3i, BlockValue> GetFireMap();
//     int CloseFires(Vector3i position, int range = 5);
//     bool IsPositionCloseToFire(Vector3i position, int range = 5);
//     void CheckFireSystems();
//     void CheckSmokeSystems();
//     void CheckPlayerSystems();
//     void CheckForPlayer();
//     IEnumerator CheckBlocks();
//     IEnumerator ProcessNeighbourBlocks(List<Vector3i> neighbors);
//     void SyncFireMap();
//     void ToggleSound(Vector3i blockPos, bool turnOn);
//     void ToggleParticle(Vector3i blockPos, bool turnOn);
//     string GetFireSound(Vector3i blockPos);
//     string GetRandomFireParticle(Vector3i blockPos);
//     string GetRandomSmokeParticle(Vector3i blockPos);
//     void Read(BinaryReader br);
//     void ClearPosOnly(Vector3i blockPos);
//     void ClearPos(Vector3i blockPos);
//     void Add(Vector3i blockPos, int entityID = -1, bool net = true);
//     void Extinguish(Vector3i blockPos, int entityID = -1);
//     void Remove(Vector3i blockPos, int entityID = -1);
//     void RemoveFire(Vector3i blockPos);
//     void ExtinguishBlock(Vector3i blockPos, int entityId);
//     void AddBlock(Vector3i blockPos);
//     int SaveDataThreaded(ThreadManager.ThreadInfo threadInfo);
//     void Save();
//     void Load();
//     void Reset();
// }