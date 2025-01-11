using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SCore.Features.Caves.Scripts;
using UnityEngine;

namespace Harmony.WorldGen {
    public class SCoreCaveProject {
        private static readonly string AdvFeatureClass = "CaveConfiguration";
        private static readonly string Feature = "CaveEnabled";

        private static readonly float depth = 30;


        // Make the world darker underground
        [HarmonyPatch(typeof(SkyManager))]
        [HarmonyPatch("Update")]
        public class CaveProjectSkyManager {
            public static bool Prefix(float ___sunIntensity, float ___sMaxSunIntensity) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty" ||
                    GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting")
                    return true;


                if (GameManager.Instance.World.GetPrimaryPlayer() == null)
                    return true;

                if (GameManager.Instance.World.GetPrimaryPlayer().position.y < depth) SkyManager.SetSunIntensity(0.1f);
                return true;
            }

            public static void Postfix(float ___sunIntensity, float ___sMaxSunIntensity) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                if (GameManager.Instance.World.GetPrimaryPlayer() == null)
                    return;

                if (GameManager.Instance.World.GetPrimaryPlayer().position.y < depth) SkyManager.SetSunIntensity(0.1f);
            }
        }

        [HarmonyPatch(typeof(SpawnManagerBiomes))]
        [HarmonyPatch("SpawnUpdate")]
        public class CaveProjectSpawnmanagerBiomes {
            // We want to run our cave spawning class right under the main biome spawner.
            public static bool Prefix(SpawnManagerBiomes __instance, string _spawnerName, bool _isSpawnEnemy,
                ChunkAreaBiomeSpawnData _spawnData) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;


                if (GameUtils.IsPlaytesting()) return true;
                
                SpawnUpdate(ref __instance, _isSpawnEnemy, ref _spawnData);

                return true;
            }

            private static bool isPositionMinDistanceAwayFromAllPlayers(Vector3 _position, int _minDistance) {
                var num = _minDistance * _minDistance;
                foreach (var t in GameManager.Instance.World.Players.list)
                    if (t.GetDistanceSq(_position) < num)
                        return false;

                return true;
            }


            private static ChunkAreaBiomeSpawnData ResetRespawn(ChunkAreaBiomeSpawnData spawnData,
                BiomeSpawnEntityGroupData biomeSpawnEntityGroupData, int idHash, World world, int maxCount) {
                spawnData.entitesSpawned.TryGetValue(idHash, out var countsAndTime);
                countsAndTime.delayWorldTime = world.worldTime +
                                               (ulong)((float)biomeSpawnEntityGroupData.respawnDelayInWorldTime *
                                                       world.RandomRange(0.9f, 1.1f));
               // Debug.Log($"Reset respawn for {idHash}: {countsAndTime.count}");
                countsAndTime.maxCount = maxCount;
                spawnData.entitesSpawned[idHash] = countsAndTime;
                spawnData.chunk.isModified = true;
                return spawnData;
            }

            // This method is a modified version of vanilla, doing the same checks and balances. However, we do use the player position a bit more, and we change which biome spawning group we
            // will use, when below the terrain. 
            private static void SpawnUpdate(ref SpawnManagerBiomes spawnManagerBiomes, bool isSpawnEnemy,
                ref ChunkAreaBiomeSpawnData spawnData) {
                // Check for a valid player
                var position = GetNearestPlayerPosition(isSpawnEnemy, spawnData);
                if (position == Vector3.zero) return;

                var biomeSpawnEntityGroupList = GetBiomeSpawnGroupList(spawnData, position);
                if (biomeSpawnEntityGroupList == null) return;

                CheckPOITags(spawnManagerBiomes, ref spawnData, biomeSpawnEntityGroupList);

                var edaytime = GameManager.Instance.World.IsDaytime() ? EDaytime.Day : EDaytime.Night;
                var gameRandom = GameManager.Instance.World.GetGameRandom();
                var spawnGroupIndex = -1;
                var randomGroupIndex = gameRandom.RandomRange(biomeSpawnEntityGroupList.list.Count);
                var idHash = 0;
                var spawnLoop = Utils.FastMin(5, biomeSpawnEntityGroupList.list.Count);

                var j = 0;
                while (j < spawnLoop)
                {
                    var biomeSpawnEntityGroupData2 = biomeSpawnEntityGroupList.list[randomGroupIndex];
                    if ((spawnData.groupsEnabledFlags & (1 << randomGroupIndex)) != 0 &&
                        biomeSpawnEntityGroupData2.daytime == EDaytime.Any ||
                        biomeSpawnEntityGroupData2.daytime == edaytime)
                    {
                        var isEnemyGroup = EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData2.entityGroupName);
                        if (!isEnemyGroup || isSpawnEnemy)
                        {
                            idHash = biomeSpawnEntityGroupData2.idHash;
                            spawnData.entitesSpawned.TryGetValue(idHash, out var countsAndTime);

                            var respawnDelayWorldTime = spawnData.GetDelayWorldTime(idHash);
                            if (GameManager.Instance.World.worldTime > respawnDelayWorldTime)
                            {
                                var maxCount = biomeSpawnEntityGroupData2.maxCount;
                                if (isEnemyGroup)
                                    maxCount = EntitySpawner.ModifySpawnCountByGameDifficulty(maxCount);

                                spawnData = ResetRespawn(spawnData, biomeSpawnEntityGroupData2, idHash,
                                    GameManager.Instance.World, maxCount);
                            }

                            if (spawnData.CanSpawn(idHash))
                            {
                                spawnGroupIndex = randomGroupIndex;
                                break;
                            }
                        }
                    }

                    j++;
                    randomGroupIndex = (randomGroupIndex + 1) % biomeSpawnEntityGroupList.list.Count;
                }

                if (spawnGroupIndex < 0)
                {
                    return;
                }

                var bb = new Bounds(position, new Vector3(4f, 2.5f, 4f));
                GameManager.Instance.World.GetEntitiesInBounds(typeof(Entity), bb, spawnManagerBiomes.spawnNearList);
                var count = spawnManagerBiomes.spawnNearList.Count;
                spawnManagerBiomes.spawnNearList.Clear();
                if (count > 0)
                    return;
                var biomeSpawnEntityGroupData3 = biomeSpawnEntityGroupList.list[spawnGroupIndex];
                var randomFromGroup =
                    EntityGroups.GetRandomFromGroup(biomeSpawnEntityGroupData3.entityGroupName, ref spawnManagerBiomes.lastClassId);
                if (randomFromGroup == 0)
                {
                    spawnData.DecMaxCount(idHash);
                    return;
                }

                spawnData.IncCount(idHash);
                var entity = EntityFactory.CreateEntity(randomFromGroup, position);
                entity.SetSpawnerSource(EnumSpawnerSource.Dynamic, spawnData.chunk.Key, idHash);
                var myEntity = entity as global::EntityAlive;
                if (myEntity) myEntity.SetSleeper();

                GameManager.Instance.World.SpawnEntityInWorld(entity);
                GameManager.Instance.World.DebugAddSpawnedEntity(entity);
            }

            private static void CheckPOITags(SpawnManagerBiomes _spawnManagerBiomes,
                ref ChunkAreaBiomeSpawnData _spawnData,
                BiomeSpawnEntityGroupList biomeSpawnEntityGroupList) {
                if (_spawnData.checkedPOITags) return;
                _spawnData.checkedPOITags = true;
                var fastTags = FastTags<TagGroup.Poi>.none;
                var worldPos = _spawnData.chunk.GetWorldPos();
                _spawnManagerBiomes.world.GetPOIsAtXZ(worldPos.x + 16, worldPos.x + 80 - 16, worldPos.z + 16,
                    worldPos.z + 80 - 16, _spawnManagerBiomes.spawnPIs);
                for (var i = 0; i < _spawnManagerBiomes.spawnPIs.Count; i++)
                {
                    var prefabInstance = _spawnManagerBiomes.spawnPIs[i];
                    fastTags |= prefabInstance.prefab.Tags;
                }

                _spawnData.poiTags = fastTags;
                var isEmpty = fastTags.IsEmpty;
                for (var k = 0; k < biomeSpawnEntityGroupList.list.Count; k++)
                {
                    var biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.list[k];
                    if ((biomeSpawnEntityGroupData.POITags.IsEmpty ||
                         biomeSpawnEntityGroupData.POITags.Test_AnySet(fastTags)) && (isEmpty ||
                            biomeSpawnEntityGroupData.noPOITags.IsEmpty ||
                            !biomeSpawnEntityGroupData.noPOITags.Test_AnySet(fastTags)))
                    {
                        _spawnData.groupsEnabledFlags |= 1 << k;
                    }
                }
            }

            private static BiomeSpawnEntityGroupList GetBiomeSpawnGroupList(
                ChunkAreaBiomeSpawnData _chunkBiomeSpawnData,
                Vector3 vector) {
                var deepCaveThreshold = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "DeepCaveThreshold"));
                var biome = GameManager.Instance.World.Biomes.GetBiome(_chunkBiomeSpawnData.biomeId);
                if (biome == null) return null;

                // Customize which spawning.xml entry to we want to use for spawns.
                var caveType = "Cave";
                // Search for the biome_Cave spawn group. If not found, load the generic Cave one.
                var biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName + $"_{caveType}"];
                if (biomeSpawnEntityGroupList == null)
                    biomeSpawnEntityGroupList = BiomeSpawningClass.list[$"{caveType}"];


                // if we are below 30, look for the biome specific deep cave, then deep cave if its not set.
                if (vector.y > deepCaveThreshold) return biomeSpawnEntityGroupList;
                caveType = "DeepCave";
                biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName + $"_{caveType}"];
                if (biomeSpawnEntityGroupList == null)
                    biomeSpawnEntityGroupList = BiomeSpawningClass.list[$"{caveType}"];

                return biomeSpawnEntityGroupList;
            }

            private static Vector3 GetNearestPlayerPosition(bool _isSpawnEnemy, ChunkAreaBiomeSpawnData _spawnData) {
                {
                    if (_spawnData == null)
                    {
                        return Vector3.zero;
                    }

                    if (_isSpawnEnemy)
                    {
                        if (!AIDirector.CanSpawn(1f))
                        {
                            _isSpawnEnemy = false;
                        }
                        else if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
                        {
                            _isSpawnEnemy = false;
                        }
                    }

                    if (!_isSpawnEnemy && GameStats.GetInt(EnumGameStats.AnimalCount) >=
                        GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals))
                    {
                        return Vector3.zero;
                    }

                    var flag = false;
                    var playerPosition = new Vector3();
                    var players = GameManager.Instance.World.GetPlayers();
                    for (var i = 0; i < players.Count; i++)
                    {
                        var entityPlayer = players[i];
                        if (!entityPlayer.Spawned) continue;
                        var rect = new Rect(entityPlayer.position.x - 40f, entityPlayer.position.z - 40f, 80f,
                            20f);
                        if (!rect.Overlaps(_spawnData.area)) continue;
                        playerPosition = entityPlayer.position;
                        flag = true;
                        break;
                    }

                    // If the player is above ground, do not run this spawner.
                    var position = new Vector3i(playerPosition);
                    float offSet = GameManager.Instance.World.GetTerrainHeight(position.x, position.z);
                    if (offSet - 10 <= playerPosition.y)
                    {
                        return Vector3.zero;
                    }

                    if (!flag)
                    {
                        return Vector3.zero;
                    }
                    var size = new Vector3(60, 10, 60);
                    if (GameManager.Instance.World.FindRandomSpawnPointNearPositionUnderground(playerPosition, 16,
                            out int x, out int y, out int z, size))
                    {
                        return new Vector3(x, y, z);
                    }

                    return Vector3.zero;
                }
            }


            [HarmonyPatch(typeof(TerrainGeneratorWithBiomeResource))]
            [HarmonyPatch("GenerateTerrain")]
            [HarmonyPatch(new[] {
                typeof(World), typeof(Chunk), typeof(GameRandom), typeof(Vector3i), typeof(Vector3i), typeof(bool),
                typeof(bool)
            })]
            public class CaveProjectTerrainGeneratorWithBiomeResource {
                public static void Postfix(TerrainGeneratorWithBiomeResource __instance, Chunk _chunk) {
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
                        case "Texture2D":
                            SCoreCavesUtils.SetTexture();
                            SCoreCavesTexture.AddCaveToChunk(_chunk);
                            break;
                        case "SCoreCavesSplat3":
                            SCoreCavesSplat3.AddCaveToChunk(_chunk);
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
            public class CaveProjectDynamicPrefabDecorator {
                public static void Postfix(Chunk _chunk) {
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
                        case "Texture2D":
                            SCoreCavesTexture.AddDecorationsToCave(_chunk);
                            break;
                        case "SCoreCavesSplat3":
                            SCoreCavesSplat3.AddDecorationsToCave(_chunk);
                            break;

                        default:
                            break;
                    }
                }
            }


            [HarmonyPatch(typeof(EntityPlayerLocal))]
            [HarmonyPatch("Init")]
            public class EntityPlayerLocalInit {
                private static void Postfix(EntityPlayerLocal __instance) {
                    if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                        return;

                    var configurationType = Configuration.GetPropertyValue(AdvFeatureClass, "GenerationType");
                    if (configurationType != "Sebastian") return;

                    Log.Out("Initializing Sebastian Cave System...");
                    var counter = 0;
                    var prefabs = GameManager.Instance.GetDynamicPrefabDecorator().allPrefabs;
                    // garage_02,remnant_oldwest_06,cemetery_01,abandoned_house_01,house_burnt_06,vacant_lot_01,mp_waste_bldg_05_grey,oldwest_coal_factory,diner_03
                    var prefabFilter = Configuration.GetPropertyValue(AdvFeatureClass, "PrefabSister").Split(',')
                        .ToList();
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
}