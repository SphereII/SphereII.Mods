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
        Debug.Log("Cave System does not exist in this Chunk. Generating...");
        var mapGenerator = new MapGenerator();
        var map = mapGenerator.GenerateMap(startPos, width, height);

        // Add the new map to the cache.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (map[x, z] == 0)
                    SphereCache.caveMap.Add(new Vector2(startPos.x + x, startPos.z + z));
            }
        }
    }

    public static void AddCaveToChunk(Chunk chunk)
    {
        var chunkPos = chunk.GetWorldPos();
        int y = 40;

        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = (chunkPos.x + chunkX);
                var worldZ = (chunkPos.z + chunkZ);

                var vector = new Vector2(worldX, worldZ);
                if (SphereCache.caveMap.Contains(vector))
                {
                    for (var caveHeight = 0; caveHeight < 4; caveHeight++)
                    {
                        chunk.SetBlockRaw(chunkX, y + caveHeight, chunkZ, BlockValue.Air);
                        chunk.SetDensity(chunkX, y + caveHeight, chunkZ, MarchingCubes.DensityAir);
                    }
                }
            }
        }
    }

    public static void AddDecorationsToCave(Chunk chunk)
    {
        if (chunk == null)
            return;

        var chunkPos = chunk.GetWorldPos();

        AdvLogging.DisplayLog(AdvFeatureClass, "Decorating new Cave System...");
        var random = GameManager.Instance.World.GetGameRandom();
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