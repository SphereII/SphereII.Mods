using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class LegacyCaveSystem
{
    private static readonly string AdvFeatureClass = "CaveConfiguration";
    private static FastNoise fastNoise;

    // Special air that has stability
    private static BlockValue caveAir = new BlockValue((uint) Block.GetBlockByName("air").blockID);
    private static BlockValue pillarBlock = new BlockValue((uint) Block.GetBlockByName("terrDesertGround").blockID);

    private static readonly BlockValue bottomCaveDecoration = new BlockValue((uint) Block.GetBlockByName("cntCaveFloorRandomLootHelper").blockID);

    private static readonly BlockValue bottomDeepCaveDecoration = new BlockValue((uint) Block.GetBlockByName("cntDeepCaveFloorRandomLootHelper").blockID);

    private static readonly BlockValue topCaveDecoration = new BlockValue((uint) Block.GetBlockByName("cntCaveCeilingRandomLootHelper").blockID);
    private static int _deepCaveThreshold = 30;

    private static List<int> _skipRanges = new List<int>() {7, 8};

    public static void AddCaveToChunk(Chunk chunk)
    {
        if (chunk == null)
            return;
        var chunkPos = chunk.GetWorldPos();

        // Find middle of chunk for its height
        int tHeight = chunk.GetTerrainHeight(8, 8);

        var maxLevels = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxCaveLevels"));
        _deepCaveThreshold = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "DeepCaveThreshold"));
        var startCaveThreshold = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "StartCaveThreshold"));
        
        // Don't generate a cave system if the threshold is higher than the terrain height.
        var minStartCaveThreshold = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MinStartCaveThreshold"));
        if (minStartCaveThreshold != -1)
        {
            if ( minStartCaveThreshold > tHeight)
                return;
        }

        var caveType = Configuration.GetPropertyValue(AdvFeatureClass, "CaveType");
        switch (caveType)
        {
            case "DeepMountains":
                // If the chunk is lower than 100 at the terrain, don't generate a cave here.
                if (tHeight < 80)
                {
                    Debug.Log("Terrain Height not high enough.");
                    return;
                }

                maxLevels = 100;
                break;
            case "Mountains":
                // If the chunk is lower than 100 at the terrain, don't generate a cave here.
                if (tHeight < 80)
                {
                    return;
                }

                break;
            case "All":
                // Generate caves on every single chunk in the world.
                break;
            case "Random":
            default:
                // If the chunk isn't in the cache, don't generate.
                if (!SphereCache.caveChunks.Contains(chunkPos))
                {
                    return;
                }

                break;
        }


        fastNoise = SphereCache.GetFastNoise(chunk);

        var depthFromTerrain = startCaveThreshold;
        var currentLevel = 0;

        while (depthFromTerrain < tHeight )
        {
            AddLevel(chunk, fastNoise, depthFromTerrain);
            depthFromTerrain += 10;
            currentLevel++;
            if (maxLevels == currentLevel)
                break;
        }

        // Decorate is done via another patch in Caves.cs
    }

    private static void PlaceCaveEntrance(Chunk chunk)
    {
        var chunkPos = chunk.GetWorldPos();

        var caveEntrance = Vector3i.zero;
        foreach (var entrance in SphereCache.caveEntrances)
        {
            for (var chunkX = 0; chunkX < 16; chunkX++)
            {
                for (var chunkZ = 0; chunkZ < 16; chunkZ++)
                {
                    var worldX = chunkPos.x + chunkX;
                    var worldZ = chunkPos.z + chunkZ;
                    if (entrance.x != worldX || worldZ != entrance.z) continue;
                    caveEntrance = entrance;
                    break;
                }
            }
        }

        // No cave entrance on this chunk.
        if (caveEntrance == Vector3i.zero) return;

        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;
                if (caveEntrance.x == worldX && caveEntrance.z == worldZ)
                {
                    int terrainHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                    var destination = new Vector3i(2, terrainHeight, 2);
                    // Move it around for a bit of variety
                    for (var travel = 0; travel < 10; travel++)
                    {
                        PlaceAround(chunk, destination);
                        destination.y--;
                        destination.x++;
                    }

                    for (var travel = 0; travel < 10; travel++)
                    {
                        PlaceAround(chunk, destination);
                        destination.y--;
                        destination.z++;
                    }

                    return;
                }
            }
        }
    }

    private static void PlaceBlock(Chunk chunk, Vector3i position, bool isPillar = false)
    {
        // Make sure the position is in bounds, and not currently air. no sense in changing it
        if (position.x > 15 || position.z > 15) return;
        if (position.x < 0 || position.z < 0) return;
        if (chunk.GetBlock(position.x, position.y, position.z).isair) return;

        if (!isPillar)
        {
            chunk.SetBlockRaw(position.x, position.y, position.z, caveAir);
            chunk.SetDensity(position.x, position.y, position.z, MarchingCubes.DensityAir);
            return;
        }

        chunk.SetBlockRaw(position.x, position.y, position.z, pillarBlock);
        chunk.SetDensity(position.x, position.y, position.z, MarchingCubes.DensityTerrain);
    }

    // Generate a prefab to push around.
    private static void CreateEmptyPrefab(Chunk chunk, Vector3i position)
    {
        var random = GameManager.Instance.World.GetGameRandom();
        var x = random.RandomRange(2, 4);
        var z = random.RandomRange(2, 4);
        var y = random.RandomRange(3, 5);
        //var prefab = new Prefab(new Vector3i(4, 3, 4));
        var prefab = new Prefab(new Vector3i(x, y, z));
        prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, position, true);
    }


    private static void PlacePillar(Chunk chunk, Vector3i position)
    {
        int tHeight = chunk.GetTerrainHeight(position.x, position.z);
        for (var x = 1; x < tHeight; x++)
        {
            position.y = x;
            PlaceAround(chunk, position, true);
        }
    }

// Changes the blocks. This works better when doing the cave openings, vs the prefab
    private static void PlaceAround(Chunk chunk, Vector3i position, bool isPillar = false)
    {
        PlaceBlock(chunk, position, isPillar);
        PlaceBlock(chunk, position + Vector3i.right, isPillar);
        PlaceBlock(chunk, position + Vector3i.left, isPillar);

        PlaceBlock(chunk, position + Vector3i.forward, isPillar);
        PlaceBlock(chunk, position + Vector3i.forward + Vector3i.right, isPillar);
        PlaceBlock(chunk, position + Vector3i.forward + Vector3i.left, isPillar);

        PlaceBlock(chunk, position + Vector3i.back, isPillar);
        PlaceBlock(chunk, position + Vector3i.back + Vector3i.right, isPillar);
        PlaceBlock(chunk, position + Vector3i.back + Vector3i.left, isPillar);
    }

// Builds a cave area section
    private static void AddLevel(Chunk chunk, FastNoise fastNoise, int DepthFromTerrain = 10)
    {
        var chunkPos = chunk.GetWorldPos();

        var biomeRestriction = Configuration.GetPropertyValue(AdvFeatureClass, "AllowedBiomes");

        var caveThresholdXZ = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdXZ"));
        var caveThresholdY = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdY"));
        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                if (_skipRanges.Contains(chunkX)) continue;
                if (_skipRanges.Contains(chunkZ)) continue;
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                var biomeId = chunk.GetBiomeId(chunkX, chunkZ);
                if (WorldBiomes.Instance.TryGetBiome(biomeId, out var biomeDefinition))
                {
                    if (biomeRestriction != "All")
                    {
                        if (!biomeRestriction.ToLower().Contains(biomeDefinition.m_sBiomeName.ToLower()))
                            continue;
                    }
                }


                //  var tHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                var tHeight = chunk.GetTerrainHeight(8, 8);

                // Place the top of this cave system higher up, if its a trap block.
                var surfaceBlock = chunk.GetBlock(chunkX, tHeight, chunkZ);
                if (surfaceBlock.Block.GetBlockName() == "terrSnowTrap" && DepthFromTerrain == 8)
                    DepthFromTerrain = 5;


                var targetDepth = tHeight - DepthFromTerrain;

                // If the depth isn't deep enough, don't try
                if (targetDepth < 5)
                    continue;

                // used for x and z block checks
                var noise = fastNoise.GetNoise(chunkX, chunkZ);

                // used for depth
                var noise2 = fastNoise.GetPerlin(targetDepth, chunkZ);

                var display = "Noise: " + Math.Abs(noise) + " noise2: " + Math.Abs(noise2) + " Chunk Position: " +
                              chunkX + "." + targetDepth + "." + chunkZ + " World Pos: " + worldX + "." + worldZ +
                              " Terrain Heigth: " + tHeight + " Depth From Terrain: " + DepthFromTerrain;
                AdvLogging.DisplayLog(AdvFeatureClass, display);
                if (!(Math.Abs(noise) < caveThresholdXZ)) continue;
                
                // Drop a level
                if (Math.Abs(noise2) < caveThresholdY)
                    DepthFromTerrain++;

                targetDepth = tHeight - DepthFromTerrain;

                //Work your way back upwards
                if (targetDepth < 5)
                    targetDepth--;

                // if its already air, don't bother creating a prefab
                if (chunk.GetBlock(chunkX, targetDepth, chunkZ).isair) continue;

                var position = new Vector3i(worldX, targetDepth, worldZ);
                CreateEmptyPrefab(chunk, position);
            }
        }
    }

// Helper method is check the prefab decorator first to see if its there, then create it if it does not exist.
    public static Prefab FindOrCreatePrefab(string strPOIname)
    {
        // Check if the prefab already exists.
        var prefab = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(strPOIname, true, true, true);
        if (prefab != null)
            return prefab;

        // If it's not in the prefab decorator, load it up.
        prefab = new Prefab();
        prefab.Load(strPOIname, true, true, true);
        var location = PathAbstractions.PrefabsSearchPaths.GetLocation(strPOIname);
        prefab.LoadXMLData(location);

        if (string.IsNullOrEmpty(prefab.PrefabName))
            prefab.PrefabName = strPOIname;

        return prefab;
    }

    public static void AddDecorationsToCave(Chunk chunk)
    {
        if (chunk == null)
            return;

        var chunkPos = chunk.GetWorldPos();
        int terrainHeight = chunk.GetTerrainHeight(8, 8);

        fastNoise = SphereCache.GetFastNoise(chunk);
        //  var noise = fastNoise.GetNoise(chunkPos.x,terrainHeight, chunkPos.z);
        var random = GameManager.Instance.World.GetGameRandom();


        var caveType = Configuration.GetPropertyValue(AdvFeatureClass, "CaveType");
        switch (caveType)
        {
            case "DeepMountains":
                // If the chunk is lower than 100 at the terrain, don't generate a cave here.
                if (terrainHeight < 80)
                    return;
                break;
            case "Mountains":
                // If the chunk is lower than 100 at the terrain, don't generate a cave here.
                if (terrainHeight < 80)
                    return;
                break;
            case "All":
                // Generate caves on every single chunk in the world.
                break;
            case "Random":
            default:
                // If the chunk isn't in the cache, don't generate.
                if (!SphereCache.caveChunks.Contains(chunkPos))
                    return;
                break;
        }

        PlaceCaveEntrance(chunk);
        GeneratePrefabs(chunk);

        AdvLogging.DisplayLog(AdvFeatureClass, "Decorating new Cave System...");
        // Decorate decorate the cave spots with blocks. Shrink the chunk loop by 1 on its edges so we can safely check surrounding blocks.
        for (var chunkX = 1; chunkX < 15; chunkX++)
        {
            for (var chunkZ = 1; chunkZ < 15; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                var tHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                //      tHeight -= 10;

                // One test world, we blew through a threshold.
                if (tHeight > 250)
                    tHeight = 240;

                // Move from the bottom up, leaving the last few blocks untouched.
                //for (int y = tHeight; y > 5; y--)
                for (var y = 5; y < tHeight - 2; y++)
                {
                    var b = chunk.GetBlock(chunkX, y, chunkZ);

                    if (b.type != caveAir.type)
                        continue;

                    var under = chunk.GetBlock(chunkX, y - 1, chunkZ);
                    var above = chunk.GetBlock(chunkX, y + 1, chunkZ);

                    BlockValue blockValue2;
                    // Check the floor for possible decoration
                    if (under.Block.shape.IsTerrain())
                    {
                        blockValue2 =
                            BlockPlaceholderMap.Instance.Replace(bottomCaveDecoration, random, worldX, worldZ);

                        // Place alternative blocks down deeper
                        if (y < _deepCaveThreshold)
                            blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomDeepCaveDecoration, random, worldX, worldZ);

                        chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                        continue;
                    }

                    // Check the ceiling to see if its a ceiling decoration
                    if (!above.Block.shape.IsTerrain()) continue;

                    blockValue2 = BlockPlaceholderMap.Instance.Replace(topCaveDecoration, random, worldX, worldZ);
                    chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                }
            }
        }
    }

    private static void GeneratePrefabs(Chunk chunk)
    {
        var random = GameManager.Instance.World.GetGameRandom();
        // Place Prefabs
        var maxPrefab = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxPrefabPerChunk"));
        for (var a = 0; a < maxPrefab; a++)
        {
            // Random chance to place a prefab to try to sparse them out.
            random.SetSeed(chunk.GetHashCode());
            if (maxPrefab < 2)
            {
                if (random.RandomRange(0, 10) > 1)
                    continue;
            }

            // Grab a random range slightly smaller than the chunk. This is to help pad them away from each other.
            var x = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
            var z = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
            var height = (int) chunk.GetHeight(x, z);
            if (height < 20)
                // Chunk is too shallow here.
                continue;

            var Max = height - 30;
            if (Max < 1)
                Max = 20;

            var y = random.RandomRange(0, Max);

            if (y < 10)
                y = 5;

            var prefabDestination = Vector3i.zero;
            for (var checkLocation = 0; checkLocation < 10; checkLocation++)
            {
                var checkX = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
                var checkZ = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
                //      if (Max <= 30)
                //           Max = height;


                if (Max <= _deepCaveThreshold)
                    continue;

                var checkY = GameManager.Instance.World.GetGameRandom().RandomRange(_deepCaveThreshold, Max);
                if (y < _deepCaveThreshold)
                    checkY = GameManager.Instance.World.GetGameRandom().RandomRange(2, _deepCaveThreshold);

                var b = chunk.GetBlock(checkX, checkY, checkZ);

                if (!b.isair) continue;

                prefabDestination = chunk.ToWorldPos(new Vector3i(checkX, checkY, checkZ));
                y = checkY;
                break;
            }

            // Decide what kind of prefab to spawn in.
            string strPOI;
            if (y < _deepCaveThreshold)
                strPOI = SphereCache.DeepCavePrefabs[random.RandomRange(0, SphereCache.DeepCavePrefabs.Count)];
            else
                strPOI = SphereCache.POIs[random.RandomRange(0, SphereCache.POIs.Count)];

            var newPrefab = FindOrCreatePrefab(strPOI);
            if (newPrefab == null) continue;
            if (prefabDestination == Vector3i.zero) continue;
            var prefab = newPrefab.Clone();
            prefab.RotateY(true, random.RandomRange(4));

            AdvLogging.DisplayLog(AdvFeatureClass, "Placing Prefab " + strPOI + " at " + prefabDestination);

            try
            {
                // Winter Project counter-sinks all prefabs -8 into the ground. However, for underground spawning, we want to avoid this, as they are already deep enough
                // Instead, temporarily replace the tag with a custom one, so that the Harmony patch for the CopyIntoLocal of the winter project won't execute.
                var temp = prefab.Tags;
                prefab.Tags = POITags.Parse("SKIP_HARMONY_COPY_INTO_LOCAL");
                prefab.yOffset = 0;
                prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, prefabDestination,
                    true);
                var entityInstanceIds = new List<int>();
                prefab.CopyEntitiesIntoChunkStub(chunk, prefabDestination, entityInstanceIds, true);

                // Trying to track a crash in something.
                //prefab.CopyIntoLocal(GameManager.Instance.World.ChunkClusters[0], destination, true, true);
                // Restore any of the tags that might have existed before.
                prefab.Tags = temp;
                //  prefab.SnapTerrainToArea(GameManager.Instance.World.ChunkClusters[0], destination);
            }
            catch (Exception ex)
            {
                Debug.Log("Warning: Could not copy over prefab: " + strPOI + " " + ex);
            }
        }
    }
}