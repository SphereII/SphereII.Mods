using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WorldGenerationEngineFinal;

public static class HeightMapTunneler
{
    private static readonly string AdvFeatureClass = "CaveConfiguration";
    private static readonly string CavePath = "CavePath";
    private static readonly string Feature = "CaveEnabled";

    private static FastNoise fastNoise;

    // Special air that has stability
    private static BlockValue caveAir = new BlockValue((uint)Block.GetBlockByName("air").blockID);

    private static readonly BlockValue bottomCaveDecoration =
        new BlockValue((uint)Block.GetBlockByName("cntCaveFloorRandomLootHelper").blockID);

    private static readonly BlockValue bottomDeepCaveDecoration =
        new BlockValue((uint)Block.GetBlockByName("cntDeepCaveFloorRandomLootHelper").blockID);

    private static readonly BlockValue topCaveDecoration =
        new BlockValue((uint)Block.GetBlockByName("cntCaveCeilingRandomLootHelper").blockID);

    public static Color[,] caveMapColor;
    public static HeightMap heightMap;


    public static float GetPixel(int x, int z)
    {
        int numX = Mathf.Abs(x);
        int numZ = Mathf.Abs(z);
        if (numX >= caveMapColor.GetLength(0) || numZ >= caveMapColor.GetLength(1))
        {
            // Log.Out("Outside of bounds");
            return -1f;
        }

        var noise = caveMapColor[numX, numZ].grayscale;
        return noise;
    }


    public static Vector2 FindHighestHeight()
    {
        int tHeight = 0;
        int width = caveMapColor.GetLength(0);
        int height = caveMapColor.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var value = caveMapColor[x, z];
                if (value.g > 200)
                    return new Vector2(x, z);
            }
        }

        return Vector2.zero;
    }


    public static float GetTargetHeight(int x, int z, int tHeight)
    {
        var width = caveMapColor.GetLength(0);
        var height = caveMapColor.GetLength(1);
        if (x >= width || z >= height) return -1;
        if (x < 0 || z < 0) return -1;


        var result = caveMapColor[x, z].grayscale;
        Debug.Log($"Result: {result}");
        if (result == 0)
            return -1;


        return result;
        //
        //
        //   //        // 254 / 255 = 0.99
        //   //        result = result / 255;
        //
        //   //        // 0.99 * 100 = 99
        //   //        result *= 100;
        //
        //   //        //Log.Out($" Result 3: {Mathf.Abs(result - tHeight) - 3} ");
        //   //        result = Mathf.Abs(tHeight - result);
        //
        //   //        Log.Out($"Height Map: {heightMap.Length} {heightMap.GetHeight()} {heightMap.GetWidth()} GetAt(): {heightMap.GetAt(x, z)} Terrain height: {tHeight} Target Depth: {result}");
        //   //        if (heightMap.GetAt(x, z) > 200) return -1;
        //   //        if (result > tHeight) return -1;
        //   //        return result;
        //
        //   //        int numX = Mathf.Abs(x);
        //   //int numZ = Mathf.Abs(z);
        //   int numX = x;
        //   int numZ = z;
        //
        //   if (numX < 0 || numZ < 0) return -1;
        //   if (numX >= caveMapColor.GetLength(0) || numZ >= caveMapColor.GetLength(1))
        //       return -1f;
        //
        //   var color = caveMapColor[numX, numZ];
        //
        //
        //   if (color.r == 0 || color.g == 1)
        //   {
        //      // Log.Out($"Color: {color.ToString()} Gray Scale: {color.grayscale} X: {numX} Z: {numZ}  Color Code r or g is 0. Skipping. ");
        //       return -1;
        //   }
        //
        //
        //   //return color.r * 100;
        //
        //   // Check to make sure its gray
        // //  if (color.r == color.g && color.r == color.b)
        //   {
        //       // if (color.g > 0.98)
        //       // {
        //       //     Log.Out($"Color: {color.ToString()} Gray Scale: {color.grayscale} X: {numX} Z: {numZ}  Color is White ");
        //       //     return tHeight;
        //       // }
        //       // color.g == 0.553
        //       float result = color.g * 100;
        //
        //       // color.g = 55 / 255 = 0.58
        //       result = result / 255;
        //
        //       // 55 = 0.55 * 100
        //       result *= 100;
        //       // Round up the value
        //       var targetDepth = (tHeight * (result / 100)) + 3;
        //
        //       Log.Out($"Color: {color.ToString()} Gray Scale: {color.grayscale} X: {numX} Z: {numZ} THeight: {tHeight} Diff Depth ( tHeight - result): {tHeight - result}  Destination: {targetDepth}");
        //
        //       return tHeight - result;
        //       //return targetDepth;
        //   }
        //
        //   Log.Out($"Color: {color.ToString()} Gray Scale: {color.grayscale} X: {numX} Z: {numZ} Skipping because r, g and b are not the same value. ");
        //
        //   return -1f;
    }

    public static void AddCaveToChunk(Chunk chunk)
    {
        if (chunk == null)
            return;

        //  PlaceCaveEntrance(chunk);
        //if (caveMapColor == null)
        //    return;
        //if (caveMapColor.Length == 0)
        //    return;

        var chunkPos = chunk.GetWorldPos();


        AddLevel(chunk, fastNoise);
    }

    public static int GetCaveEntrane(Chunk chunk)
    {
        var chunkPos = chunk.GetWorldPos();

        var closesEntrance = Vector3i.zero;
        for (var x = 0; x < SphereCache.caveEntrances.Count; x++)
        {
            var Entrance = SphereCache.caveEntrances[x];
            if (closesEntrance == Vector3i.zero)
                closesEntrance = Entrance;

            var distance = Vector3.Distance(chunkPos, Entrance);

            if (Vector3.Distance(closesEntrance, chunkPos) > distance)
                closesEntrance = Entrance;

            // Log.Out($"Close Entrance: {closesEntrance} ChunkPos: {chunkPos}  Entrance: {Entrance}  Distance: {distance}");
        }

        //Log.Out($"Closes Entrance to {chunkPos} is {closesEntrance}");
        if (closesEntrance == Vector3i.zero) return -1;

        var entranceChunk = GameManager.Instance.World.GetChunkFromWorldPos(closesEntrance);
        if (entranceChunk == null)
        {
            //  Log.Out("Entrance chunk is not available.");
            return -1;
        }

        return entranceChunk.GetTerrainHeight(0, 0);
    }

    public static void PlaceCaveEntrance(Chunk chunk)
    {
        var chunkPos = chunk.GetWorldPos();

        var caveEntrance = Vector3i.zero;
        for (var x = 0; x < SphereCache.caveEntrances.Count; x++)
        {
            var Entrance = SphereCache.caveEntrances[x];
            if (Vector3.Distance(chunkPos, Entrance) < 200)
            {
                var entranceChunk = GameManager.Instance.World.GetChunkFromWorldPos(Entrance);
                if (entranceChunk == null) continue;
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

    public static void PlaceBlock(Chunk chunk, Vector3i position)
    {
        // Make sure the position is in bounds, and not currently air. no sense in changing it
        if (position.x > 15 || position.z > 15) return;
        if (position.x < 0 || position.z < 0) return;
        if (chunk.GetBlock(position.x, position.y, position.z).isair) return;

        chunk.SetBlockRaw(position.x, position.y, position.z, caveAir);
        chunk.SetDensity(position.x, position.y, position.z, MarchingCubes.DensityAir);
    }

    // Generate a prefab to push around.
    public static void CreateEmptyPrefab(Chunk chunk, Vector3i position)
    {
        int height = 4;
        if (position.y < height)
            height = position.y;
        var prefab = new Prefab(new Vector3i(3, 4, 3));
        prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, position, true);
    }

    // Changes the blocks. This works better when doing the cave openings, vs the prefab
    public static void PlaceAround(Chunk chunk, Vector3i position)
    {
        PlaceBlock(chunk, position);
        PlaceBlock(chunk, position + Vector3i.right);
        PlaceBlock(chunk, position + Vector3i.left);

        PlaceBlock(chunk, position + Vector3i.forward);
        PlaceBlock(chunk, position + Vector3i.forward + Vector3i.right);
        PlaceBlock(chunk, position + Vector3i.forward + Vector3i.left);

        PlaceBlock(chunk, position + Vector3i.back);
        PlaceBlock(chunk, position + Vector3i.back + Vector3i.right);
        PlaceBlock(chunk, position + Vector3i.back + Vector3i.left);
    }

    // Builds a cave area section
    public static void AddLevel(Chunk chunk, FastNoise fastNoise)
    {
        var chunkPos = chunk.GetWorldPos();

        var cavePrefab = Configuration.GetPropertyValue(AdvFeatureClass, "CavePrefab");

        int highestPoint = 0;
        // for (var chunkX = 0; chunkX < 16; chunkX++)
        // {
        //     for (var chunkZ = 0; chunkZ < 16; chunkZ++)
        //     {
        //         
        //         var height = chunk.GetTerrainHeight(chunkX, chunkZ);
        //         var targetBlock = chunk.GetBlock(chunkX, height, chunkZ);
        //         
        //         // if ( height > highestPoint )
        //         //     highestPoint = height;
        //     }
        // }
        //
        // return;
        //        var tHeight = chunk.GetTerrainHeight(0, 0);

        var providerFromImage =
            GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider() as WorldBiomeProviderFromImage;
        var tHeight = GameManager.Instance.World.ChunkCache.ChunkProvider.GetTerrainGenerator()
            .GetTerrainHeightAt(chunkPos.x, chunkPos.z);
//        Log.Out($"Terrain Generator: {tHeight}  Chunk Pos: {chunk.GetTerrainHeight(0, 0)} : Highest Point: {highestPoint}");
        tHeight = highestPoint;
        //var tHeight = GetCaveEntrane(chunk);
        //Log.Out($"Terrain Height: {tHeight}");
        //if (tHeight == -1)
        //    tHeight = 40;
        //var tHeight = FindHighestHeight();
        //var tHeight = FindHighestHeight();
        //  var tHeight = FindHighestHeight(chunk, tBaseHeight);

        //tHeight = 40;
        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                var pixel = GetPixel(worldX, worldZ);
                if ( pixel == 0) continue;
                
                // world position
                var targetDepth = 10;
                var _blockPos = new Vector3i(worldX, targetDepth, worldZ);

                // Chunk position
                var position = new Vector3i(chunkX, targetDepth, chunkZ);


                if (cavePrefab == "Large")
                {
                    CreateEmptyPrefab(chunk, _blockPos);
                }
                else if (cavePrefab == "Medium")
                {
                    PlaceAround(chunk, position);
                }
                else
                {
                    PlaceBlock(chunk, position + Vector3i.up);
                    if (position.y > 2)
                        PlaceBlock(chunk, position + Vector3i.down);
                    PlaceBlock(chunk, position);
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

     //   if (string.IsNullOrEmpty(prefab.PrefabName))
       //     prefab.PrefabName = strPOIname;

        return prefab;
    }

    public static void AddDecorationsToCave(Chunk chunk)
    {
        if (chunk == null)
            return;

        return;

        var chunkPos = chunk.GetWorldPos();
        int terrainHeight = chunk.GetTerrainHeight(8, 8);

        fastNoise = SphereCache.GetFastNoise(chunk);
        //  var noise = fastNoise.GetNoise(chunkPos.x,terrainHeight, chunkPos.z);
        var _random = GameManager.Instance.World.GetGameRandom();


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

        //PlaceCaveEntrance(chunk);


        AdvLogging.DisplayLog(AdvFeatureClass, "Decorating new Cave System...");

        // Place Prefabs
        var MaxPrefab = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxPrefabPerChunk"));
        for (var a = 0; a < MaxPrefab; a++)
        {
            // Random chance to place a prefab to try to sparse them out.
            _random.SetSeed(chunk.GetHashCode());
            if (MaxPrefab < 2)
                if (_random.RandomRange(0, 10) > 1)
                    continue;
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

            var y = _random.RandomRange(0, Max);

            if (y < 10)
                y = 5;

            var prefabDestination = Vector3i.zero;
            for (int checkLocation = 0; checkLocation < 10; checkLocation++)
            {
                var checkX = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
                var checkZ = GameManager.Instance.World.GetGameRandom().RandomRange(0, 16);
                if (Max <= 30)
                    Max = height;


                if (Max <= 30)
                    return;

                int checkY = GameManager.Instance.World.GetGameRandom().RandomRange(30, Max);
                if (y < 30)
                    checkY = GameManager.Instance.World.GetGameRandom().RandomRange(2, 30);

                var b = chunk.GetBlock(checkX, checkY, checkZ);

                if (b.isair)
                {
                    prefabDestination = chunk.ToWorldPos(new Vector3i(checkX, checkY, checkZ));
                    y = checkY;
                    break;
                }
            }

            // Decide what kind of prefab to spawn in.
            string strPOI;
            if (y < 30)
                strPOI = SphereCache.DeepCavePrefabs[_random.RandomRange(0, SphereCache.DeepCavePrefabs.Count)];
            else
                strPOI = SphereCache.POIs[_random.RandomRange(0, SphereCache.POIs.Count)];

            var newPrefab = FindOrCreatePrefab(strPOI);
            if (newPrefab != null)
            {
                if (prefabDestination != Vector3i.zero)
                {
                    var prefab = newPrefab.Clone();
                    prefab.RotateY(true, _random.RandomRange(4));

                    AdvLogging.DisplayLog(AdvFeatureClass, "Placing Prefab " + strPOI + " at " + prefabDestination);

                    try
                    {
                        // Winter Project counter-sinks all prefabs -8 into the ground. However, for underground spawning, we want to avoid this, as they are already deep enough
                        // Instead, temporarily replace the tag with a custom one, so that the Harmony patch for the CopyIntoLocal of the winter project won't execute.
                        var temp = prefab.Tags;
                        prefab.Tags  = FastTags<TagGroup.Poi>.Parse("SKIP_HARMONY_COPY_INTO_LOCAL");
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
            for (var y = 5; y < tHeight - 2; y++)
            {
                var b = chunk.GetBlock(chunkX, y, chunkZ);

                if (b.type != caveAir.type)
                    continue;


                var under = chunk.GetBlock(chunkX, y - 1, chunkZ);
                var above = chunk.GetBlock(chunkX, y + 1, chunkZ);

                // Check the floor for possible decoration
                if (under.Block.shape.IsTerrain())
                {
                    var blockValue2 =
                        BlockPlaceholderMap.Instance.Replace(bottomCaveDecoration, _random, worldX, worldZ);

                    // Place alternative blocks down deeper
                    if (y < 30)
                        blockValue2 =
                            BlockPlaceholderMap.Instance.Replace(bottomDeepCaveDecoration, _random, worldX, worldZ);

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
    }
}