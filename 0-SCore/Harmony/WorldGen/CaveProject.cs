using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Harmony.WorldGen
{
    public class SCoreCaveProject
    {
        private static readonly string AdvFeatureClass = "CaveConfiguration";
        private static readonly string Feature = "CaveEnabled";

        private static readonly float depth = 30;


        // Make the world darker underground
        [HarmonyPatch(typeof(SkyManager))]
        [HarmonyPatch("Update")]
        public class CaveProjectSkyManager
        {
            public static bool Prefix(float ___sunIntensity, float ___sMaxSunIntensity)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty" || GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting")
                    return true;


                if (GameManager.Instance.World.GetPrimaryPlayer() == null)
                    return true;

                if (GameManager.Instance.World.GetPrimaryPlayer().position.y < depth) SkyManager.SetSunIntensity(0.1f);
                return true;
            }

            public static void Postfix(float ___sunIntensity, float ___sMaxSunIntensity)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                if (GameManager.Instance.World.GetPrimaryPlayer() == null)
                    return;

                if (GameManager.Instance.World.GetPrimaryPlayer().position.y < depth) SkyManager.SetSunIntensity(0.1f);
            }
        }

        [HarmonyPatch(typeof(SpawnManagerBiomes))]
        [HarmonyPatch("Update")]
        public class CaveProjectSpawnmanagerBiomes
        {
            // We want to run our cave spawning class right under the main biome spawner.
            public static bool Prefix(SpawnManagerBiomes __instance, string _spawnerName, bool _bSpawnEnemyEntities, object _userData, ref List<Entity> ___spawnNearList, ref int ___lastClassId)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;


                if (!GameUtils.IsPlaytesting())
                {

                    SpawnUpdate(_spawnerName, _bSpawnEnemyEntities, _userData as ChunkAreaBiomeSpawnData,
                        ref ___spawnNearList, ref ___lastClassId);
                }

                return true;
            }

            private static bool isPositionMinDistanceAwayFromAllPlayers(Vector3 _position, int _minDistance)
            {
                var num = _minDistance * _minDistance;
                foreach (var t in GameManager.Instance.World.Players.list)
                    if (t.GetDistanceSq(_position) < num)
                        return false;

                return true;
            }

            // This is a slightly modified version of the underground code from vanilla. The range of y is a bit throttled, as we want to spawn creatures near the player, and the original
            // underground code did not check if they were within the view spawn code, so you could see them spawn in front of you.
            // public static bool FindRandomSpawnPointNearPositionUnderground(Rect _area, int _minDistance, int _maxDistance, bool _bConsiderBedrolls, out Vector3 _position, Vector3i PlayerPosition)
            // {
            //     _position = Vector3.zero;
            //     if (GameManager.Instance.World.Players.list.Count == 0) return false;
            //
            //     // Since the cave system can be eratic in its location, we want to try 20 times to find a random spot where they can spawn at.
            //     for (var i = 0; i < 40; i++)
            //     {
            //         var rangeY = new Vector2(PlayerPosition.y - 10, PlayerPosition.y + 10);
            //         if (rangeY.x < 1)
            //             rangeY.x = 2;
            //
            //         _position = new Vector3(_area.x + GameManager.Instance.World.RandomRange(0f, _area.width - 1f), GameManager.Instance.World.RandomRange(rangeY.x, rangeY.y),
            //             _area.y + GameManager.Instance.World.RandomRange(0f, _area.height - 1f));
            //         if (_position.y < 1)
            //             _position.y = 2;
            //         var vector3i = World.worldToBlockPos(_position);
            //         var chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(vector3i);
            //         if (chunk == null) continue;
            //
            //         var x = World.toBlockXZ(vector3i.x);
            //         var z = World.toBlockXZ(vector3i.z);
            //
            //         // Grab the terrian height. If it's above the terrain level, ignore it.
            //         float terrainLevel = chunk.GetHeight(x, z) + 1;
            //         float maxLevel = PlayerPosition.y + 10;
            //         vector3i.y = (int)GameManager.Instance.World.RandomRange((float)PlayerPosition.y - 10, maxLevel);
            //         if (vector3i.y < 1)
            //             vector3i.y = PlayerPosition.y;
            //
            //         if (maxLevel >= terrainLevel)
            //             vector3i.y = PlayerPosition.y;
            //
            //         if (!chunk.CanMobsSpawnAtPos(x, vector3i.y, z)) continue;
            //
            //         var flag = isPositionMinDistanceAwayFromAllPlayers(_position, _minDistance);
            //         var num = 0;
            //         while (flag && num < GameManager.Instance.World.Players.list.Count)
            //         {
            //             if ((_position - GameManager.Instance.World.Players.list[num].GetPosition()).sqrMagnitude < 2500f &&
            //                 GameManager.Instance.World.Players.list[num].IsInViewCone(_position)) flag = false;
            //             num++;
            //         }
            //
            //         if (!flag) continue;
            //
            //         // Set the y position correctly.
            //         _position.y = vector3i.y;
            //         return true;
            //     }
            //
            //     _position = Vector3.zero;
            //     return false;
            // }


            // This method is a modified version of vanilla, doing the same checks and balances. However, we do use the player position a bit more, and we change which biome spawning group we
            // will use, when below the terrain. 
            public static void SpawnUpdate(string _spawnerName, bool _bSpawnEnemyEntities, ChunkAreaBiomeSpawnData _chunkBiomeSpawnData, ref List<Entity> spawnNearList, ref int lastClassId)
            {
                var deepCaveThreshold = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "DeepCaveThreshold"));

                if (_chunkBiomeSpawnData == null) return;
                if (_bSpawnEnemyEntities)
                {
                    if (GameStats.GetInt(EnumGameStats.EnemyCount) >= GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies))
                        _bSpawnEnemyEntities = false;
                    else if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive) _bSpawnEnemyEntities = false;
                }

                if (!_bSpawnEnemyEntities && GameStats.GetInt(EnumGameStats.AnimalCount) >= GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals)) return;
                var flag = false;
                var players = GameManager.Instance.World.GetPlayers();

                // Player Position.
                var position = Vector3.zero;
                var rect = new Rect(1, 1, 1, 1);
                foreach (var t in players)
                {
                    if (!t.Spawned) continue;

                    position = t.GetPosition();
                    rect = new Rect(position.x - 40f, position.z - 40f, 80f, 20f);
                    if (!rect.Overlaps(_chunkBiomeSpawnData.area)) continue;

                    flag = true;
                    break;
                }

                // No valid player position.
                if (position == Vector3.zero)
                    return;

                // Don't allow above ground spawning.
                var playerPosition = new Vector3i(position);
                float offSet = GameManager.Instance.World.GetTerrainHeight(playerPosition.x, playerPosition.z);
                if (offSet <= playerPosition.y)
                    return;

                var minDistance = _bSpawnEnemyEntities ? 28 : 48;
                var maxDistance = _bSpawnEnemyEntities ? 54 : 70;
                //if (!flag || !FindRandomSpawnPointNearPositionUnderground(rect, minDistance, maxDistance, false, out var vector, playerPosition))
                var size = new Vector3(60, 10, 60);
                if (!flag || !GameManager.Instance.World.FindRandomSpawnPointNearPositionUnderground(playerPosition, 16, out int x, out int y, out int z, size ))
                    return;

                var vector = new Vector3();
                vector.x = x;
                vector.z = z;
                vector.y = y;
                // Mob is above terrain; ignore.
                if (vector.y > offSet)
                    return;

            
                var biome = GameManager.Instance.World.Biomes.GetBiome(_chunkBiomeSpawnData.biomeId);
                if (biome == null) return;

                // Customize which spawning.xml entry to we want to use for spawns.
                var caveType = "Cave";
                // Search for the biome_Cave spawn group. If not found, load the generic Cave one.
                var biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName + "_Cave"];
                if (biomeSpawnEntityGroupList == null) biomeSpawnEntityGroupList = BiomeSpawningClass.list["Cave"];
                // if we are below 30, look for the biome specific deep cave, then deep cave if its not set.
                if (vector.y < deepCaveThreshold)
                {
                    caveType = "DeepCave";
                    biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName + "_DeepCave"];
                    if (biomeSpawnEntityGroupList == null) biomeSpawnEntityGroupList = BiomeSpawningClass.list["DeepCave"];
                }

                if (biomeSpawnEntityGroupList == null)
                    return;

                var edaytime = GameManager.Instance.World.IsDaytime() ? EDaytime.Day : EDaytime.Night;
                var gameRandom = GameManager.Instance.World.GetGameRandom();
                string entityGroupName = null;
                var num = -1;
                var num2 = gameRandom.RandomRange(biomeSpawnEntityGroupList.list.Count);
                var j = 0;
                
                while (j < 5)
                {
                    BiomeSpawnEntityGroupData biomeSpawnEntityGroupData2 = biomeSpawnEntityGroupList.list[num2];
                    if (biomeSpawnEntityGroupData2.daytime == EDaytime.Any || biomeSpawnEntityGroupData2.daytime == edaytime)
                    {
                        bool flag2 = EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData2.entityGroupRefName);
                        if (!flag2 || _bSpawnEnemyEntities)
                        {
                            int num4 = biomeSpawnEntityGroupData2.maxCount;
                            if (flag2)
                            {
                                num4 = EntitySpawner.ModifySpawnCountByGameDifficulty(num4);
                            }
                            
                            entityGroupName = biomeSpawnEntityGroupData2.entityGroupRefName + "_" + biomeSpawnEntityGroupData2.daytime.ToStringCached<EDaytime>();
                            ulong respawnDelayWorldTime = _chunkBiomeSpawnData.GetRespawnDelayWorldTime(entityGroupName);
                            if (respawnDelayWorldTime > 0UL)
                            {
                                if (GameManager.Instance.World.worldTime < respawnDelayWorldTime)
                                {
                                    break;
                                }
                                _chunkBiomeSpawnData.ClearRespawn(entityGroupName);
                            }
                            if (_chunkBiomeSpawnData.GetEntitiesSpawned(entityGroupName) < num4)
                            {
                                
                                num = num2;
                                break;
                            }
                        }
                    }
                    j++;
                    num2 = (num2 + 1) % biomeSpawnEntityGroupList.list.Count;
                }

                if (num < 0)
                    return;
                var bb = new Bounds(vector, new Vector3(4f, 2.5f, 4f));
                GameManager.Instance.World.GetEntitiesInBounds(typeof(Entity), bb, spawnNearList);
                var count = spawnNearList.Count;
                spawnNearList.Clear();
                if (count > 0)
                    return;
                var biomeSpawnEntityGroupData3 = biomeSpawnEntityGroupList.list[num];
                var randomFromGroup = EntityGroups.GetRandomFromGroup(biomeSpawnEntityGroupData3.entityGroupRefName, ref lastClassId);
                var spawnDeadChance = biomeSpawnEntityGroupData3.spawnDeadChance;
                _chunkBiomeSpawnData.IncEntitiesSpawned(entityGroupName);
                var entity = EntityFactory.CreateEntity(randomFromGroup, vector);
                entity.SetSpawnerSource(EnumSpawnerSource.Dynamic, _chunkBiomeSpawnData.chunk.Key, entityGroupName);
                var myEntity = entity as global::EntityAlive;
                if (myEntity) myEntity.SetSleeper();

                // Debug.Log("Spawning: " + myEntity.entityId + " " + vector );
                GameManager.Instance.World.SpawnEntityInWorld(entity);


                if (spawnDeadChance > 0f && gameRandom.RandomFloat < spawnDeadChance) entity.Kill(DamageResponse.New(true));
                GameManager.Instance.World.DebugAddSpawnedEntity(entity);
            }
        }

      

        [HarmonyPatch(typeof(TerrainGeneratorWithBiomeResource))]
        [HarmonyPatch("GenerateTerrain")]
        [HarmonyPatch(new[] { typeof(World), typeof(Chunk), typeof(GameRandom), typeof(Vector3i), typeof(Vector3i), typeof(bool), typeof(bool) })]
        public class CaveProjectTerrainGeneratorWithBiomeResource
        {
            public static void Postfix(Chunk _chunk)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                SphereCache.GenerateCaveChunks();
                var configurationType = Configuration.GetPropertyValue(AdvFeatureClass, "GenerationType");
                switch (configurationType)
                {
                    case "Legacy":
                        LegacyCaveSystem.AddCaveToChunk(_chunk);
                        //CaveSystemV2.AddCaveToChunk(_chunk);
                        break;
                    case "Sebastian":
                        Sebastian.AddCaveToChunk(_chunk);
                        break;

                    case "HeightMap":
                        HeightMapTunneler.AddCaveToChunk(_chunk);
                        break;
                    case "PathingWorm":
                        PathingWormTunneler.AddCaveToChunk(_chunk);
                        break;
                    //case "FastNosieSIMD":
                    //    TerrainGeneratorSIMD_Caves.GenerateChunk(_chunk);
                    //    break;
                    //case "SIMDCaveTunnler":
                    //    SIMDCaveTunnler.AddCaveToChunk(_chunk);
                    //    break;
                    default:
                        break;
                }
            }
        }


        [HarmonyPatch(typeof(DynamicPrefabDecorator))]
        [HarmonyPatch("DecorateChunk")]
        [HarmonyPatch(new[] { typeof(World), typeof(Chunk), typeof(bool) })]
        public class CaveProjectDynamicPrefabDecorator
        {
            public static void Postfix(Chunk _chunk)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                SphereCache.GenerateCaveChunks();
                var configurationType = Configuration.GetPropertyValue(AdvFeatureClass, "GenerationType");
                switch (configurationType)
                {
                    case "Legacy":
                        LegacyCaveSystem.AddDecorationsToCave(_chunk);
                        break;
                    case "Sebastian":
                        Sebastian.AddDecorationsToCave(_chunk);
                        break;
                    case "HeightMap":
                        HeightMapTunneler.AddDecorationsToCave(_chunk);
                        break;
                    default:
                        break;
                }
            }
        }


        [HarmonyPatch(typeof(EntityPlayerLocal))]
        [HarmonyPatch("Init")]
        public class EntityPlayerLocalInit
        {
            private static void Postfix(EntityPlayerLocal __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                var configurationType = Configuration.GetPropertyValue(AdvFeatureClass, "GenerationType");
                if (configurationType != "Sebastian") return;

                Log.Out("Initializing Sebastian Cave System...");
                var counter = 0;
                var prefabs = GameManager.Instance.GetDynamicPrefabDecorator().allPrefabs;
                // garage_02,remnant_oldwest_06,cemetery_01,abandoned_house_01,house_burnt_06,vacant_lot_01,mp_waste_bldg_05_grey,oldwest_coal_factory,diner_03
                var prefabFilter = Configuration.GetPropertyValue(AdvFeatureClass, "PrefabSister").Split(',').ToList();
                foreach (var sister in prefabFilter)
                {
                    foreach (var individualInstance in prefabs.FindAll(instance => instance.name.Contains(sister)))
                    {
                        var pos = individualInstance.boundingBoxPosition;
                        var size = 400;
                        counter++;
                        Log.Out($"Generating Cave at {pos} of size {size}...");
                        Sebastian.GenerateCave(pos, size, size);
                    }
                }
                Log.Out($"Cave System Generation Complete: {counter} Caves Generated.");
            }
        }
    }


}