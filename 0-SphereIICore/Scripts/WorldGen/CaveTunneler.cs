using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public static class SphereII_CaveTunneler
{

    private static string AdvFeatureClass = "CaveConfiguration";

    // Special air that has stability
    static BlockValue caveAir = new BlockValue((uint)Block.GetBlockByName("caveBlock02", false).blockID);

    static BlockValue bottomCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntCaveFloorRandomLootHelper", false).blockID);
    static BlockValue bottomDeepCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntDeepCaveFloorRandomLootHelper", false).blockID);
    static BlockValue topCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntCaveCeilingRandomLootHelper", false).blockID);

    public static void AddLevel(Chunk chunk, FastNoise fastNoise, int DepthFromTerrain = 10)
    {
        var chunkPos = chunk.GetWorldPos();

        float caveThresholdXZ = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdXZ"));
        float caveThresholdY = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdY"));
        for (int chunkX = 0; chunkX < 16; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                var tHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                int targetDepth = tHeight - DepthFromTerrain;

                // If the depth isn't deep enough, don't try
                if (targetDepth < 5)
                    continue;

                // used for x and z block checks
                var noise = fastNoise.GetNoise(chunkX, chunkZ);

                // used for depth
                var noise2 = fastNoise.GetPerlin(targetDepth, chunkZ);

                String display = "Noise: " + Math.Abs(noise) + " noise2: " + Math.Abs(noise2) + " Chunk Position: " + chunkX + "." + targetDepth + "." + chunkZ + " World Pos: " + worldX + "." + worldZ + " Terrain Heigth: " + tHeight + " Depth From Terrain: " + DepthFromTerrain;
                AdvLogging.DisplayLog(AdvFeatureClass, display);
                Vector3i targetPos = Vector3i.zero;
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
                    chunk.SetBlock(GameManager.Instance.World, chunkX, targetDepth, chunkZ, caveAir);
                    chunk.SetDensity(chunkX, targetDepth, chunkZ, MarchingCubes.DensityAir);

                    // Make each cave height 5 blocks.
                    for (int caveHeight = 0; caveHeight < 5; caveHeight++)
                    {
                        chunk.SetBlock(GameManager.Instance.World, chunkX, targetDepth + caveHeight, chunkZ, caveAir);
                        chunk.SetDensity(chunkX, targetDepth + caveHeight, chunkZ, MarchingCubes.DensityAir);
                    }
                }
            }
        }

    }
    public static void AddCaveToChunk(Chunk chunk)
    {
        if (chunk == null)
            return;

        var chunkPos = chunk.GetWorldPos();

        // Find middle of chunk for its height
        int tHeight = chunk.GetTerrainHeight(8,8);

        int MaxLevels = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxCaveLevels"));

        String caveType = Configuration.GetPropertyValue(AdvFeatureClass, "CaveType");
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

        GameRandom _random = GameManager.Instance.World.GetGameRandom();

        FastNoise fastNoise = GetFastNoise(chunk);

        int DepthFromTerrain = 10;
        int currentLevel = 0;
        while (DepthFromTerrain < tHeight || MaxLevels > currentLevel)
        {
            AddLevel(chunk, fastNoise, DepthFromTerrain);
            DepthFromTerrain += 10;
            currentLevel++;
            
        }
    }

    public static FastNoise GetFastNoise(Chunk chunk)
    {
        int Octaves = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Octaves"));
        float Lacunarity = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Lacunarity"));
        float Gain = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Gain"));
        float Frequency = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Frequency"));


        FastNoise.FractalType fractalType = EnumUtils.Parse<FastNoise.FractalType>(Configuration.GetPropertyValue(AdvFeatureClass, "FractalType"), false);
        FastNoise.NoiseType noiseType = EnumUtils.Parse<FastNoise.NoiseType>(Configuration.GetPropertyValue(AdvFeatureClass, "NoiseType"), false);

        FastNoise fastNoise = new FastNoise();
        fastNoise.SetFractalType(fractalType);
        fastNoise.SetNoiseType(noiseType);
        fastNoise.SetFractalOctaves(Octaves);
        fastNoise.SetFractalLacunarity(Lacunarity);
        fastNoise.SetFractalGain(Gain);
        fastNoise.SetFrequency(Frequency);
        fastNoise.SetSeed(chunk.GetHashCode());

        var chunkPos = chunk.GetWorldPos();

        String display = "Chunk and Seed: " + chunk.GetHashCode() + " Fractal Type: " + fractalType.ToString() + " Noise Type: " + noiseType.ToString() + " Position: " + chunkPos + " Octaves: " + Octaves + " Lacunarity: " + Lacunarity + " Gain: " + Gain + " Frequency: " + Frequency;
        AdvLogging.DisplayLog(AdvFeatureClass, display);

        return fastNoise;
    }

    public static void AddDecorationsToCave(Chunk chunk)
    {

        if (chunk == null)
            return;

        var chunkPos = chunk.GetWorldPos();
        Vector3i caveEntrance = Vector3i.zero;
        for (int x = 0; x < SphereCache.caveEntrances.Count; x++)
        {
            var Entrance = SphereCache.caveEntrances[x];
            if (chunkPos.x < Entrance.x && chunkPos.x + 16 > Entrance.x)
            {
                if (chunkPos.z < Entrance.z && chunkPos.z + 16 > Entrance.z)
                {
                    caveEntrance = Entrance;
                    break;
                }
            }
        }

        //
        List<String> POIs = new List<string>();
        foreach (String poi in Configuration.GetPropertyValue(AdvFeatureClass, "CavePOIs").Split(','))
            POIs.Add(poi);

        List<String> CaveSpawners = new List<string>();
        foreach (String poi in Configuration.GetPropertyValue(AdvFeatureClass, "CaveSpawners").Split(','))
            CaveSpawners.Add(poi);

        FastNoise fastNoise = GetFastNoise(chunk);

        AdvLogging.DisplayLog(AdvFeatureClass, "Decorating new Cave System...");
        GameRandom _random = GameManager.Instance.World.GetGameRandom();

        int MaxPrefab = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxPrefabPerChunk"));
        int currentPrefabCount = 0;

        // Decorate decorate the cave spots with blocks. Shrink the chunk loop by 1 on its edges so we can safely check surrounding blocks.
        for (int chunkX = 1; chunkX < 15; chunkX++)
        {
            for (int chunkZ = 1; chunkZ < 15; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                var tHeight = chunk.GetTerrainHeight(chunkX, chunkZ);

                // Move from the bottom up, leaving the last few blocks untouched.
                for (int y = tHeight; y > 5; y--)
                {
                    var b = chunk.GetBlock(chunkX, y, chunkZ);

                    if (b.type != caveAir.type)
                        continue;

                    var under = chunk.GetBlock(chunkX, y - 1, chunkZ);
                    var above = chunk.GetBlock(chunkX, y + 1, chunkZ);
                    var side = chunk.GetBlock(chunkX + 1, y, chunkZ);

                    float noise = fastNoise.GetNoise(chunkX, chunkZ);
                    _random.SetSeed(chunkX * chunkZ * y);
                    if (IsIsolatedBlock(chunk, new Vector3i(chunkX, y, chunkZ)) && _random.RandomRange(0, 10) < 1)
                    {
                        String FindPrefab = POIs[_random.RandomRange(0, POIs.Count)];
                        Prefab prefab = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(FindPrefab);
                        if (prefab != null)
                        {
                            Vector3i destination = chunk.ToWorldPos(new Vector3i(chunkX, y + prefab.yOffset, chunkZ));
                            AdvLogging.DisplayLog(AdvFeatureClass, "Placing Prefab " + FindPrefab + " at " + destination);
                            prefab.CopyIntoLocal(GameManager.Instance.World.ChunkClusters[0], destination, true, true);
                            prefab.SnapTerrainToArea(GameManager.Instance.World.ChunkClusters[0], destination);
                            currentPrefabCount++;
                        }
                        continue;
                    }

                    if (IsIsolatedBlock(chunk, new Vector3i(chunkX, y, chunkZ)) && _random.RandomRange(0, 10) < 1)
                    {
                        String FindPrefab = CaveSpawners[_random.RandomRange(0, CaveSpawners.Count)];
                        Prefab prefab = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(FindPrefab);
                        if (prefab != null)
                        {
                            Vector3i destination = chunk.ToWorldPos(new Vector3i(chunkX, y + prefab.yOffset, chunkZ));
                            AdvLogging.DisplayLog(AdvFeatureClass, "Placing Spawner " + FindPrefab + " at " + destination);
                            prefab.CopyIntoLocal(GameManager.Instance.World.ChunkClusters[0], destination, true, true);
                            prefab.SnapTerrainToArea(GameManager.Instance.World.ChunkClusters[0], destination);
                        }
                        continue;
                    }
                    // If there's a cave entrance on this chunk, find the top air block, and build the cave entrance from it.
                    if (caveEntrance != Vector3i.zero)
                    {
                        if (caveEntrance.x == worldX && caveEntrance.z == worldZ)
                        {
                            Vector3i destination = caveEntrance;
                            Prefab prefab = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab("caveOpening02");
                            if (prefab != null)
                            {
                                destination.y = tHeight;
                                AdvLogging.DisplayLog(AdvFeatureClass, "Placing Cave Entrance: " + caveEntrance);

                                for (int x = 0; x < 10; x++)
                                {
                                    prefab = prefab.Clone();
                                    prefab.CopyIntoLocal(GameManager.Instance.World.ChunkClusters[0], destination, false, false);
                                    prefab.SnapTerrainToArea(GameManager.Instance.World.ChunkClusters[0], destination);
                                    destination.y--;

                                    // Use noise to give the tunnel a bit of a natural look
                                    float noise2 = fastNoise.GetNoise(destination.x, destination.z);
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
                        }
                    }
                    // Check the floor for possible decoration
                    if (under.Block.shape.IsTerrain())
                    {
                        BlockValue blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomCaveDecoration, _random, chunk, worldX, worldZ, false, QuestTags.none);

                        // Place alternative blocks down deeper
                        if (y < 45)
                            blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomDeepCaveDecoration, _random, chunk, worldX, worldZ, false, QuestTags.none);

                        chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                        continue;
                    }


                    // Check the ceiling to see if its a ceiling decoration
                    if (above.Block.shape.IsTerrain())
                    {
                        BlockValue blockValue2 = BlockPlaceholderMap.Instance.Replace(topCaveDecoration, _random, chunk, worldX, worldZ, false, QuestTags.none);
                        chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                        continue;
                    }


                }
            }
        }
    }



    static public bool IsIsolatedBlock(Chunk chunk, Vector3i center)
    {
        int counter = 0;
        foreach (var side in Vector3i.AllDirections)
        {
            if (chunk.GetBlock(center + side).Block.shape.IsTerrain())
                counter++;
        }
        //  Debug.Log("Isolated Block: " + center + ": Counter: " + counter + " Total Possible: " + Vector3i.AllDirections.Length );
        if (counter == 5)
            return true;
        return false;

    }
}