using System;
using System.Collections.Generic;
using UnityEngine;

public static class LegacyCaveSystem
{
    private static readonly string AdvFeatureClass = "CaveConfiguration";
    private static FastNoise fastNoise;

    // Special air that has stability
    private static BlockValue caveAir = new BlockValue((uint)Block.GetBlockByName("air").blockID);

    private static readonly BlockValue bottomCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntCaveFloorRandomLootHelper").blockID);
    private static readonly BlockValue bottomDeepCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntDeepCaveFloorRandomLootHelper").blockID);
    private static readonly BlockValue topCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntCaveCeilingRandomLootHelper").blockID);


    public static void AddCaveToChunk(Chunk chunk)
    {
        if (chunk == null)
            return;

        var chunkPos = chunk.GetWorldPos();

        // Find middle of chunk for its height
        int tHeight = chunk.GetTerrainHeight(8, 8);

        var MaxLevels = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxCaveLevels"));

        var caveType = Configuration.GetPropertyValue(AdvFeatureClass, "CaveType");
        switch (caveType)
        {
            case "DeepMountains":
                // If the chunk is lower than 100 at the terrain, don't generate a cave here.
                if (tHeight < 80)
                    return;
                MaxLevels = 100;
                break;
            case "Mountains":
                // If the chunk is lower than 100 at the terrain, don't generate a cave here.
                if (tHeight < 80)
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

        fastNoise = SphereCache.GetFastNoise(chunk);

        var DepthFromTerrain = 8;
        var currentLevel = 0;
        while (DepthFromTerrain < tHeight || MaxLevels > currentLevel)
        {
            AddLevel(chunk, fastNoise, DepthFromTerrain);
            DepthFromTerrain += 10;
            currentLevel++;
        }

        // Decorate is done via another patch in Caves.cs
    }


    // Builds a cave area section
    public static void AddLevel(Chunk chunk, FastNoise fastNoise, int DepthFromTerrain = 10)
    {
        var chunkPos = chunk.GetWorldPos();
        var caveThresholdXZ = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdXZ"));
        var caveThresholdY = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdY"));
        for (var chunkX = 0; chunkX < 16; chunkX++)
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                var tHeight = chunk.GetTerrainHeight(chunkX, chunkZ);

                // Place the top of this cave system higher up, if its a trap block.
                var SurfaceBlock = chunk.GetBlock(chunkX, tHeight, chunkZ);
                if (SurfaceBlock.Block.GetBlockName() == "terrSnowTrap" && DepthFromTerrain == 8)
                    DepthFromTerrain = 5;

                var targetDepth = tHeight - DepthFromTerrain;

                // If the depth isn't deep enough, don't try
                if (targetDepth < 5)
                    continue;

                // used for x and z block checks
                var noise = fastNoise.GetNoise(chunkX, chunkZ);

                // used for depth
                var noise2 = fastNoise.GetPerlin(targetDepth, chunkZ);

                var display = "Noise: " + Math.Abs(noise) + " noise2: " + Math.Abs(noise2) + " Chunk Position: " + chunkX + "." + targetDepth + "." + chunkZ + " World Pos: " + worldX + "." + worldZ +
                              " Terrain Heigth: " + tHeight + " Depth From Terrain: " + DepthFromTerrain;
                AdvLogging.DisplayLog(AdvFeatureClass, display);
                var targetPos = Vector3i.zero;
                if (Math.Abs(noise) < caveThresholdXZ)
                {
                    //// Drop a level
                    if (Math.Abs(noise2) < caveThresholdY)
                        DepthFromTerrain++;

                    targetDepth = tHeight - DepthFromTerrain;

                    //Work your way back upwards
                    if (targetDepth < 5)
                        targetDepth--;

                    display = "Noise Below ThresholdXZ: " + noise + " Threadhold: " + noise2 + " Target Depth: " + targetDepth;
                    AdvLogging.DisplayLog(AdvFeatureClass, display);

                    chunk.SetBlockRaw(chunkX, targetDepth, chunkZ, caveAir);
                    chunk.SetDensity(chunkX, targetDepth, chunkZ, MarchingCubes.DensityAir);

                    // Make each cave height 5 blocks.
                    for (var caveHeight = 0; caveHeight < 4; caveHeight++)
                    {
                        chunk.SetBlockRaw(chunkX, targetDepth + caveHeight, chunkZ, caveAir);
                        chunk.SetDensity(chunkX, targetDepth + caveHeight, chunkZ, MarchingCubes.DensityAir);
                    }
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

        // Check if a cave entrance exists in this chunk.
        var caveEntrance = Vector3i.zero;
        for (var x = 0; x < SphereCache.caveEntrances.Count; x++)
        {
            var Entrance = SphereCache.caveEntrances[x];
            if (chunkPos.x < Entrance.x && chunkPos.x + 16 > Entrance.x)
                if (chunkPos.z < Entrance.z && chunkPos.z + 16 > Entrance.z)
                {
                    caveEntrance = Entrance;
                    break;
                }
        }

        fastNoise = SphereCache.GetFastNoise(chunk);

        AdvLogging.DisplayLog(AdvFeatureClass, "Decorating new Cave System...");
        var _random = GameManager.Instance.World.GetGameRandom();

        // non-Random caves need an opening that isn't predefined in the cache.
        var caveType = Configuration.GetPropertyValue(AdvFeatureClass, "CaveType");
        if (caveType != "Random" && _random.RandomRange(0, 100) < 2)
        {
            caveEntrance.x = _random.RandomRange(1, 15);
            caveEntrance.z = _random.RandomRange(1, 15);
        }

        // Place Prefabs
        var MaxPrefab = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxPrefabPerChunk"));
        for (var a = 0; a < MaxPrefab; a++)
        {
            // Random chance to place a prefab to try to sparse them out.
            _random.SetSeed(chunk.GetHashCode());
            if (_random.RandomRange(0, 10) > 1) continue;
            // Grab a random range slightly smaller than the chunk. This is to help pad them away from each other.
            var x = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
            var z = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
            var height = (int)chunk.GetHeight(x, z);
            if (height < 20)
                // Chunk is too shallow here.
                continue;

            var Max = height - 15;
            if (Max < 1)
                Max = 20;

            var SanityCheck = Max - 10 + 10;
            if (SanityCheck <= 0)
            {
                Debug.Log("\t*************************");
                Debug.Log("\t Sanity Check Failed for Random Range");
                Debug.Log("\t Chunk: " + chunk.GetHashCode());
                Debug.Log("\t Chunk Height: " + Max);
            }

            var y = _random.RandomRange(0, Max);

            if (y < 10)
                y = 10;

            var destination = chunk.ToWorldPos(new Vector3i(x, y, z));
            //Debug.Log("Placing POI: " + chunk.GetHashCode() + " at " + destination + " MaxPrefab: " + MaxPrefab + " Current: " + a);


            // Decide what kind of prefab to spawn in.
            string strPOI;
            if (y < 30)
                strPOI = SphereCache.DeepCavePrefabs[_random.RandomRange(0, SphereCache.DeepCavePrefabs.Count)];
            else
                strPOI = SphereCache.POIs[_random.RandomRange(0, SphereCache.POIs.Count)];

            var newPrefab = FindOrCreatePrefab(strPOI);
            if (newPrefab != null)
            {
                var prefab = newPrefab.Clone();
                prefab.RotateY(true, _random.RandomRange(4));

                AdvLogging.DisplayLog(AdvFeatureClass, "Placing Prefab " + strPOI + " at " + destination);

                try
                {
                    // Winter Project counter-sinks all prefabs -8 into the ground. However, for underground spawning, we want to avoid this, as they are already deep enough
                    // Instead, temporarily replace the tag with a custom one, so that the Harmony patch for the CopyIntoLocal of the winter project won't execute.
                    var temp = prefab.Tags;
                    prefab.Tags = POITags.Parse("SKIP_HARMONY_COPY_INTO_LOCAL");
                    prefab.yOffset = 0;
                    prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, destination, true);
                    var entityInstanceIds = new List<int>();
                    prefab.CopyEntitiesIntoChunkStub(chunk, destination, entityInstanceIds, true);

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

        // Decorate decorate the cave spots with blocks. Shrink the chunk loop by 1 on its edges so we can safely check surrounding blocks.
        for (var chunkX = 1; chunkX < 15; chunkX++)
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
                for (var y = 5; y < tHeight - 10; y++)
                {
                    var b = chunk.GetBlock(chunkX, y, chunkZ);

                    if (b.type != caveAir.type)
                        continue;

                    var under = chunk.GetBlock(chunkX, y - 1, chunkZ);
                    var above = chunk.GetBlock(chunkX, y + 1, chunkZ);


                    // If there's a cave entrance on this chunk, find the top air block, and build the cave entrance from it.
                    if (caveEntrance != Vector3i.zero)
                        if (caveEntrance.x == worldX && caveEntrance.z == worldZ)
                        {
                            var destination = caveEntrance;
                            var prefab = FindOrCreatePrefab("caveOpening02");
                            if (prefab != null)
                            {
                                destination.y = tHeight;
                                AdvLogging.DisplayLog(AdvFeatureClass, "Placing Cave Entrance: " + caveEntrance);

                                for (var x = 0; x < 10; x++)
                                {
                                    prefab = prefab.Clone();
                                    prefab.CopyIntoLocal(GameManager.Instance.World.ChunkClusters[0], destination, false, false);
                                    prefab.SnapTerrainToArea(GameManager.Instance.World.ChunkClusters[0], destination);
                                    destination.y--;

                                    // Use noise to give the tunnel a bit of a natural look
                                    var noise2 = fastNoise.GetNoise(destination.x, destination.z);
                                    if (noise2 < 0.0)
                                        destination.x++;
                                    if (noise2 > 0.2)
                                        destination.x++;

                                    if (noise2 < 0.1)
                                        destination.z++;
                                    if (noise2 > 0.3)
                                        destination.z--;
                                }
                            }

                            caveEntrance = Vector3i.zero;
                        }

                    // Check the floor for possible decoration
                    if (under.Block.shape.IsTerrain())
                    {
                        var blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomCaveDecoration, _random, worldX, worldZ);

                        // Place alternative blocks down deeper
                        if (y < 30)
                            blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomDeepCaveDecoration, _random, worldX, worldZ);

                        chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                        continue;
                    }

                    // Check the ceiling to see if its a ceiling decoration
                    if (above.Block.shape.IsTerrain())
                    {
                        var blockValue2 = BlockPlaceholderMap.Instance.Replace(topCaveDecoration, _random, worldX, worldZ);
                        chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                    }
                }
            }


        // chunk.NeedsRegeneration = true;
    }

    // We want to place prefabs in isolated blocks, meaning that its just a single air block that is mostly covered with terrain. 
    // Since its not a usable space to the player, its a decent choice for spawning.
    public static bool IsIsolatedBlock(Chunk chunk, Vector3i center)
    {
        var counter = 0;
        foreach (var side in Vector3i.AllDirections)
            if (chunk.GetBlock(center + side).Block.shape.IsTerrain())
                counter++;
        //  Debug.Log("Isolated Block: " + center + ": Counter: " + counter + " Total Possible: " + Vector3i.AllDirections.Length );
        if (counter == 5)
            return true;
        return false;
    }
}