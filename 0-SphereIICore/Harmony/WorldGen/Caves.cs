using DMT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SphereII_CaveProject
{
    private static readonly string AdvFeatureClass = "CaveConfiguration";
    private static readonly string Feature = "CaveEnabled";
    public class SphereII_CaveProject_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());

            // Reduce extra logging stuff
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

            var harmony = new Harmony(GetType().ToString());

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // Make the world darker underground
    [HarmonyPatch(typeof(SkyManager))]
    [HarmonyPatch("Update")]
    public class SphereII_CaveProject_SkyManager
    {
        public static bool Prefix(float ___sunIntensity, float ___sMaxSunIntensity)
        {

            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty" || GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting")
                return true;


            if (GameManager.Instance.World.GetPrimaryPlayer() == null)
                return true;

            if (GameManager.Instance.World.GetPrimaryPlayer().position.y < 30)
            {
                SkyManager.SetSunIntensity(0.1f);
            }
            return true;
        }

        public static void Postfix(float ___sunIntensity, float ___sMaxSunIntensity)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;
            if (GameManager.Instance.World.GetPrimaryPlayer() == null)
                return;

            if (GameManager.Instance.World.GetPrimaryPlayer().position.y < 30)
            {
                SkyManager.SetSunIntensity(0.1f);
            }

        }
    }

    [HarmonyPatch(typeof(SpawnManagerBiomes))]
    [HarmonyPatch("Update")]
    public class SphereII_CaveProject_Spawnmanager_Biomes
    {
        // We want to run our cave spawning class right under the main biome spawner.
        public static void Postfix(SpawnManagerBiomes __instance, string _spawnerName, bool _bSpawnEnemyEntities, object _userData, ref List<Entity> ___spawnNearList, ref int ___lastClassId)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            if (!GameUtils.IsPlaytesting())
            {
                SpawnUpdate(_spawnerName, _bSpawnEnemyEntities, _userData as ChunkAreaBiomeSpawnData, ref ___spawnNearList, ref ___lastClassId);
            }
        }
        private static bool isPositionMinDistanceAwayFromAllPlayers(Vector3 _position, int _minDistance)
        {
            int num = _minDistance * _minDistance;
            for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
            {
                if (GameManager.Instance.World.Players.list[i].GetDistanceSq(_position) < (float)num)
                {
                    return false;
                }
            }
            return true;
        }

        // This is a slightly modified version of the underground code from vanilla. The range of y is a bit throttled, as we want to spawn creatures near the player, and the original
        // underground code did not check if they were within the view spawn code, so you could see them spawn in front of you.
        public static bool FindRandomSpawnPointNearPositionUnderground(Rect _area, int _minDistance, int _maxDistance, bool _bConsiderBedrolls, out Vector3 _position, Vector3i PlayerPosition)
        {
            _position = Vector3.zero;
            if (GameManager.Instance.World.Players.list.Count == 0)
            {
                return false;
            }

            // Since the cave system can be eratic in its location, we want to try 20 times to find a random spot where they can spawn at.
            for (int i = 0; i < 40; i++)
            {
                Vector2 rangeY = new Vector2(PlayerPosition.y - 10, PlayerPosition.y + 10);
                if (rangeY.x < 1)
                    rangeY.x = 2;

                _position = new Vector3(_area.x + GameManager.Instance.World.RandomRange(0f, _area.width - 1f), GameManager.Instance.World.RandomRange(rangeY.x, rangeY.y), _area.y + GameManager.Instance.World.RandomRange(0f, _area.height - 1f));
                if (_position.y < 1)
                    _position.y = 2;
                Vector3i vector3i = World.worldToBlockPos(_position);
                Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(vector3i);
                if (chunk != null)
                {
                    int x = World.toBlockXZ(vector3i.x);
                    int z = World.toBlockXZ(vector3i.z);

                    // Grab the terrian height. If it's above the terrain level, ignore it.
                    float terrainLevel = (float)(chunk.GetHeight(x, z) + 1);
                    float maxLevel = PlayerPosition.y + 10;
                    vector3i.y = (int)GameManager.Instance.World.RandomRange((float)PlayerPosition.y - 10, maxLevel);
                    if (vector3i.y < 1)
                        vector3i.y = PlayerPosition.y;

                    if (maxLevel >= terrainLevel)
                        vector3i.y = PlayerPosition.y;
                    if (chunk.CanMobsSpawnAtPos(x, vector3i.y, z, false))
                    {
                        bool flag = isPositionMinDistanceAwayFromAllPlayers(_position, _minDistance);
                        int num = 0;
                        while (flag && num < GameManager.Instance.World.Players.list.Count)
                        {
                            if ((_position - GameManager.Instance.World.Players.list[num].GetPosition()).sqrMagnitude < 2500f && GameManager.Instance.World.Players.list[num].IsInViewCone(_position))
                            {
                                flag = false;
                            }
                            num++;
                        }
                        if (flag)
                        {
                            // Set the y position correctly.
                            _position.y = vector3i.y;
                            return true;
                        }
                    }
                }
            }
            _position = Vector3.zero;
            return false;
        }


        // This method is a modified version of vanilla, doing the same checks and balances. However, we do use the player position a bit more, and we change which biome spawning group we
        // will use, when below the terrain. 
        public static void SpawnUpdate(string _spawnerName, bool _bSpawnEnemyEntities, ChunkAreaBiomeSpawnData _chunkBiomeSpawnData, ref List<Entity> spawnNearList, ref int lastClassId)
        {
            if (_chunkBiomeSpawnData == null)
            {
                return;
            }
            if (_bSpawnEnemyEntities)
            {
                if (GameStats.GetInt(EnumGameStats.EnemyCount) >= GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies))
                {
                    _bSpawnEnemyEntities = false;
                }
                else if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
                {
                    _bSpawnEnemyEntities = false;
                }
            }
            if (!_bSpawnEnemyEntities && GameStats.GetInt(EnumGameStats.AnimalCount) >= GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals))
            {
                return;
            }
            bool flag = false;
            List<EntityPlayer> players = GameManager.Instance.World.GetPlayers();

            // Player Position.
            Vector3 position = Vector3.zero;
            Rect rect = new Rect(1, 1, 1, 1);
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Spawned)
                {
                    position = players[i].GetPosition();
                    rect = new Rect(position.x - 40f, position.z - 40f, 80f, 20f);
                    if (rect.Overlaps(_chunkBiomeSpawnData.area))
                    {
                        flag = true;
                        break;
                    }
                }
            }

            // No valid player position.
            if (position == Vector3.zero)
                return;

            // Don't allow above ground spawning.
            Vector3i playerPosition = new Vector3i(position);
            float offSet = GameManager.Instance.World.GetTerrainHeight(playerPosition.x, playerPosition.z);
            if (offSet <= playerPosition.y)
                return;

            int minDistance = _bSpawnEnemyEntities ? 28 : 48;
            int maxDistance = _bSpawnEnemyEntities ? 54 : 70;
            Vector3 vector;
            if (!flag || !FindRandomSpawnPointNearPositionUnderground(rect, minDistance, maxDistance, false, out vector, playerPosition))
                return;

            // Mob is above terrain; ignore.
            if (vector.y > offSet)
                return;


            BiomeDefinition biome = GameManager.Instance.World.Biomes.GetBiome(_chunkBiomeSpawnData.biomeId);
            if (biome == null)
            {
                return;
            }

            // Customize which spawning.xml entry to we want to use for spawns.
            String CaveType = "Cave";
            // Search for the biome_Cave spawn group. If not found, load the generic Cave one.
            BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName + "_Cave"];
            if (biomeSpawnEntityGroupList == null)
            {
                biomeSpawnEntityGroupList = BiomeSpawningClass.list["Cave"];
            }
            // if we are below 30, look for the biome specific deep cave, then deep cave if its not set.
            if (vector.y < 30)
            {
                CaveType = "DeepCave";
                biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName + "_DeepCave"];
                if (biomeSpawnEntityGroupList == null)
                {
                    biomeSpawnEntityGroupList = BiomeSpawningClass.list["DeepCave"];
                }
            }
            if (biomeSpawnEntityGroupList == null)
                return;

            EDaytime edaytime = GameManager.Instance.World.IsDaytime() ? EDaytime.Day : EDaytime.Night;
            GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
            string entityGroupName = null;
            int num = -1;
            int num2 = gameRandom.RandomRange(biomeSpawnEntityGroupList.list.Count);
            int j = 0;
            while (j < 5)
            {
                BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.list[num2];
                if (biomeSpawnEntityGroupData.daytime == EDaytime.Any || biomeSpawnEntityGroupData.daytime == edaytime)
                {
                    bool flag2 = EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData.entityGroupRefName);
                    if (!flag2 || _bSpawnEnemyEntities)
                    {
                        int num3 = biomeSpawnEntityGroupData.maxCount;
                        if (flag2)
                        {
                            num3 = EntitySpawner.ModifySpawnCountByGameDifficulty(num3);
                        }
                        entityGroupName = biomeSpawnEntityGroupData.entityGroupRefName + "_" + biomeSpawnEntityGroupData.daytime.ToStringCached<EDaytime>() + "_" + CaveType;
                        ulong respawnLockedUntilWorldTime = _chunkBiomeSpawnData.GetRespawnLockedUntilWorldTime(entityGroupName);
                        if (respawnLockedUntilWorldTime <= 0UL || GameManager.Instance.World.worldTime >= respawnLockedUntilWorldTime)
                        {
                            if (respawnLockedUntilWorldTime > 0UL)
                            {
                                _chunkBiomeSpawnData.ClearRespawnLocked(entityGroupName);
                            }
                            if (_chunkBiomeSpawnData.GetEntitiesSpawned(entityGroupName) < num3)
                            {
                                num = num2;
                                break;
                            }
                        }
                    }
                }
                j++;
                num2 = (num2 + 1) % biomeSpawnEntityGroupList.list.Count;
            }
            if (num < 0)
            {
                //Debug.Log("max spawn reached: " + entityGroupName);
                return;
            }
            Bounds bb = new Bounds(vector, new Vector3(4f, 2.5f, 4f));
            GameManager.Instance.World.GetEntitiesInBounds(typeof(Entity), bb, spawnNearList);
            int count = spawnNearList.Count;
            spawnNearList.Clear();
            if (count > 0)
            {
                //Debug.Log("Spawn Count is maxed for ");
                return;
            }
            BiomeSpawnEntityGroupData biomeSpawnEntityGroupData2 = biomeSpawnEntityGroupList.list[num];
            int randomFromGroup = EntityGroups.GetRandomFromGroup(biomeSpawnEntityGroupData2.entityGroupRefName, ref lastClassId, null);
            float spawnDeadChance = biomeSpawnEntityGroupData2.spawnDeadChance;
            _chunkBiomeSpawnData.IncEntitiesSpawned(entityGroupName);
            Entity entity = EntityFactory.CreateEntity(randomFromGroup, vector);
            entity.SetSpawnerSource(EnumSpawnerSource.Dynamic, _chunkBiomeSpawnData.chunk.Key, entityGroupName);
            EntityAlive myEntity = entity as EntityAlive;
            if (myEntity)
            {
                myEntity.SetSleeper();
            }

            // Debug.Log("Spawning: " + myEntity.entityId + " " + vector );
            GameManager.Instance.World.SpawnEntityInWorld(entity);


            if (spawnDeadChance > 0f && gameRandom.RandomFloat < spawnDeadChance)
            {
                entity.Kill(DamageResponse.New(true));
            }
            GameManager.Instance.World.DebugAddSpawnedEntity(entity);

        }
    }



    [HarmonyPatch(typeof(TerrainGeneratorWithBiomeResource))]
    [HarmonyPatch("GenerateTerrain")]
    [HarmonyPatch(new Type[] { typeof(World), typeof(Chunk), typeof(GameRandom), typeof(Vector3i), typeof(Vector3i), typeof(bool) })]
    public class SphereII_CaveProject_TerrainGeneratorWithBiomoeResource
    {
        public static void Postfix(Chunk _chunk)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            // Allow us to throttle which chunks get caves or not by creating a list of world positions.
            SphereCache.GenerateCaveChunks();
            SphereII_CaveTunneler.AddCaveToChunk(_chunk);

        }
    }


    [HarmonyPatch(typeof(DynamicPrefabDecorator))]
    [HarmonyPatch("DecorateChunk")]
    [HarmonyPatch(new Type[] { typeof(World), typeof(Chunk), typeof(bool) })]
    public class SphereII_CaveProject_DynamicPrefabDecorator
    {
        public static void Postfix(Chunk _chunk)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            //// Allow us to throttle which chunks get caves or not by creating a list of world positions.
            SphereCache.GenerateCaveChunks();
            SphereII_CaveTunneler.AddDecorationsToCave(_chunk);
        }
    }


}
