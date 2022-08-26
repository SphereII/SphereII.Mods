using System;
using System.Collections.Generic;
using UnityEngine;

public static class Sebastian 
{
    private static readonly string AdvFeatureClass = "CaveConfiguration";
    // private static FastNoise fastNoise;

    // Special air that has stability
    private static BlockValue caveAir = new BlockValue((uint)Block.GetBlockByName("air").blockID);

    private static readonly BlockValue bottomCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntCaveFloorRandomLootHelper").blockID);
    private static readonly BlockValue bottomDeepCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntDeepCaveFloorRandomLootHelper").blockID);
    private static readonly BlockValue topCaveDecoration = new BlockValue((uint)Block.GetBlockByName("cntCaveCeilingRandomLootHelper").blockID);

    public enum CaveType
    {
        Normal,
        Chaotic,
        Worm
    }

    public static void GenerateCave(Vector3i startPos, int width = 100, int height = 100, CaveType caveType = CaveType.Normal)
    {
        var mapGenerator = new MapGenerator();
        var map = mapGenerator.GenerateMap(startPos, width, height, startPos.ToString());

        // Add the new map to the cache.
        for (var x = 0; x < width; x++)
        {
            for (var z = 0; z < height; z++)
            {
                if (map[x, z] == 0)
                    SphereCache.caveMap.Add(new Vector2(startPos.x + x, startPos.z + z));
            }
        }
    }

    public static void AddLevel(Chunk chunk, int depth, bool reverse = false)
    {
        var chunkPos = chunk.GetWorldPos();

        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = (chunkPos.x + chunkX);
                var worldZ = (chunkPos.z + chunkZ);

                var vector = new Vector2(worldX, worldZ);
                if ( reverse)
                    vector = new Vector2(worldZ, worldX);

                if (!SphereCache.caveMap.Contains(vector)) continue;

                var y = chunk.GetTerrainHeight(chunkX, chunkZ) - depth;
                if (y < 5)
                    return;
                for (var caveHeight = 0; caveHeight < 4; caveHeight++)
                {
                    chunk.SetBlockRaw(chunkX, y + caveHeight, chunkZ, BlockValue.Air);
                    chunk.SetDensity(chunkX, y + caveHeight, chunkZ, MarchingCubes.DensityAir);
                }
            }
        }
    }

    public static void AddCaveToChunk(Chunk chunk)
    {
        // Populates the pois
        var noise = SphereCache.GetFastNoise(chunk);

        AddLevel(chunk, 5);
        return;
        var MaxLevels = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxCaveLevels"));
        MaxLevels = 1;
        var depth = 10;
        for (var x = 0; x <= MaxLevels; x++)
        {
            AddLevel(chunk, depth, x%2 == 0);
            depth += 8;
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

        AdvLogging.DisplayLog(AdvFeatureClass, "Decorating new Cave System...");
        var random = GameManager.Instance.World.GetGameRandom();
        // Decorate decorate the cave spots with blocks. Shrink the chunk loop by 1 on its edges so we can safely check surrounding blocks.

        var MaxPrefab = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxPrefabPerChunk"));
        var prefabCounter = 0;
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
                for (var y = 5; y < tHeight - 10; y++)
                {
                    var b = chunk.GetBlock(chunkX, y, chunkZ);

                    if (b.type != caveAir.type)
                        continue;

                    var under = chunk.GetBlock(chunkX, y - 1, chunkZ);
                    var above = chunk.GetBlock(chunkX, y + 1, chunkZ);

                    // Check the floor for possible decoration
                    if (under.Block.shape.IsTerrain())
                    {
                        var blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomCaveDecoration, random, worldX, worldZ);

                        // Place alternative blocks down deeper
                        if (y < 30)
                            blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomDeepCaveDecoration, random, worldX, worldZ);

                        chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);

                        // Check to see if we can place a prefab here.
                        if (prefabCounter >= MaxPrefab) continue;

                        if (random.RandomRange(0, 10) > 1) continue;

                        //string strPOI;
                        //if (y < 30)
                        //    strPOI = SphereCache.DeepCavePrefabs[random.RandomRange(0, SphereCache.DeepCavePrefabs.Count)];
                        //else
                        //    strPOI = SphereCache.POIs[random.RandomRange(0, SphereCache.POIs.Count)];

                        //var newPrefab = FindOrCreatePrefab(strPOI);
                        //if (newPrefab != null)
                        //{
                        //    var prefab = newPrefab.Clone();
                        //    prefab.RotateY(true, random.RandomRange(4));
                        //    var destination = chunk.ToWorldPos(new Vector3i(chunkX, y , chunkZ));
                        //    try
                        //    {
                        //        // Winter Project counter-sinks all prefabs -8 into the ground. However, for underground spawning, we want to avoid this, as they are already deep enough
                        //        // Instead, temporarily replace the tag with a custom one, so that the Harmony patch for the CopyIntoLocal of the winter project won't execute.
                        //        var temp = prefab.Tags;
                        //        prefab.Tags = POITags.Parse("SKIP_HARMONY_COPY_INTO_LOCAL");
                        //        prefab.yOffset = 0;
                        //        prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, destination, true);
                        //        var entityInstanceIds = new List<int>();
                        //        prefab.CopyEntitiesIntoChunkStub(chunk, destination, entityInstanceIds, true);

                        //        // Restore any of the tags that might have existed before.
                        //        prefab.Tags = temp;
                        //        prefabCounter++;

                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Debug.Log("Warning: Could not copy over prefab: " + strPOI + " " + ex);
                        //    }
                        //}
                        continue;
                    }

                    // Check the ceiling to see if its a ceiling decoration
                    if (above.Block.shape.IsTerrain())
                    {
                        var blockValue2 = BlockPlaceholderMap.Instance.Replace(topCaveDecoration, random, worldX, worldZ);
                        chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                    }
                }
            }
        }
        // chunk.NeedsRegeneration = true;
    }
}