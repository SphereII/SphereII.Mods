using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class WinterProjectPrefab
{
    private static BlockValue snow = new BlockValue((uint) Block.GetBlockByName("terrSnow").blockID);
    private static BlockValue ice = new BlockValue((uint) Block.GetBlockByName("terrIce").blockID);

    //  static BlockValue ice = new BlockValue((uint)Block.GetBlockByName("terrIce", false).blockID);

    private static readonly Random Rand = new Random(Guid.NewGuid().GetHashCode());
    public static bool Logging = false;
    public static bool Rpc = true;

    public static void SetSnowChunk(Chunk chunk, Vector3i position, Vector3i size)
    {
        //var position = chunk.GetWorldPos();
        Write("SetSnowChunk " + position);
        ProcessChunk(chunk, position, size, false, Logging, Rpc, false);

        //SetSnow(position.x, position.z, 16, 16, GameManager.Instance.World.ChunkCache, false, true);
    }

    public static void SetSnowPrefab(Prefab prefab, ChunkCluster cluster, Vector3i position, FastTags _questTag)
    {
        var AlreadyFilled = false;
        // if (_questTag != QuestTags.none)
        // {
        //     AlreadyFilled = true;
        //     prefab.yOffset -= 8;
        //     position.y -= 8;
        // }

        SetSnow(position.x, position.z, prefab.size.x, prefab.size.z, cluster, Rpc, Logging, AlreadyFilled,
            prefab.size.y);
    }

    private static void ProcessChunk(Chunk chunk, Vector3i position, Vector3i size, bool regen, bool log,
        bool notifyRpc, bool isPrefab)
    {
        if (chunk == null)
        {
            Debug.Log("Chunk is null " + position);
            return;
        }


        notifyRpc = GameManager.Instance.World.ChunkCache.DisplayedChunkGameObjects.ContainsKey(chunk.Key);

        List<BlockChangeInfo> Changes = null;
        if (notifyRpc)
            Changes = new List<BlockChangeInfo>();

        var minX = position.x;
        var maxX = position.x + size.x - 1;

        var minZ = position.z;
        var maxZ = position.z + size.z - 1;

        Write("Prefab Height: " + size.y);
        var chunkPos = chunk.GetWorldPos();
        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                if (worldX < minX || worldX > maxX || worldZ < minZ || worldZ > maxZ)
                    //    Write("Out of bounds " + position + " s: " + size + " wp: " + worldX + "," + worldZ);
                    continue;

                // Grab the chunk's terrain height to use a  baseline
                var tHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                var tHeightWithSnow = tHeight + 8;

                // If we are resetting for a quest, the terrain has already been moved up.
                if (regen) tHeightWithSnow = tHeight;

                //  for (var y = 255; y >= 0; y--)
                for (var y = 255; y >= 0; y--)
                {
                    // Test if we still need to go down further
                    var b = chunk.GetBlock(chunkX, y, chunkZ);
                    if (b.type == 0)
                        continue; //still air 

                    if (b.Block is BlockModelTree) break;

                    //if (Block.list[b.type].blockMaterial.StabilitySupport == false)
                    //    continue;

                    // This is the height of the snow where we are aiming at. It'll at least be 8 blocks high, but depending on the terrain or obstacles we meet
                    // on the way down, it could be lower.
                    var snowHeight = Math.Min(8, Math.Abs(tHeightWithSnow - y));
                    Write("Aiming for a depth of " + snowHeight + " of snow.");
                    if (y > tHeightWithSnow)
                    {
                        Write("Adjusting snowHeight for depth: we are above the terrain");
                        snowHeight = 3;
                    }


                    // level off the snow with the rest of the terrain health
                    if (y < tHeightWithSnow + 2)
                        snowHeight = Math.Abs(y - tHeightWithSnow) + 1;

                    //// Prefab size
                    //if (size.y < 9)
                    //{
                    //    Write("Little POI Detected: Old Height: " + snowHeight + " Prefab Size: " + size.y);
                    //    if (!Block.list[b.type].shape.IsTerrain())
                    //        continue;

                    //}

                    if (log)
                        Write(worldX + "," + worldZ + " found block (" + b.Block.GetBlockName() + ") id " + b.type +
                              " at height " + y + " setting depth of " + snowHeight + " Prefab Height: " +
                              size.y + "  cx " + chunkX + "/" + chunk.X + "," + chunkZ + "/" + chunk.Z + "  t " +
                              tHeight + " Height With Snow: " + tHeightWithSnow);

                    // Are we re-generating or is this a fresh? if we are regenerating, we don't want to keep adding another 8 blocks of snow

                    //if (!regen)
                    //    snowHeight = 0;


                    Write("Current Block is: " + b.Block.GetBlockName());
                    var snowY = y;
                    // Raise the level by one, since we don't want to replace the roof with snow, or the current block.
                    var StartingSnow = y + 1;
                    for (snowY = StartingSnow; snowY < y + snowHeight; snowY++) //&& snowY < tHeight
                    {
                        if (snowY > 255)
                            continue;

                        var check = chunk.GetBlock(chunkX, snowY, chunkZ);

                        //if (log)
                        //    Write("Set " + worldX + "," + snowY + "," + worldZ + " to snow || world  " + chunk.GetWorldPos() + " applied: " + (check.type != snow.type) + "  rpc: " + Rpc);

                        if (check.type == snow.type) continue;
                        chunk.SetBlock(GameManager.Instance.World, chunkX, snowY, chunkZ, snow);
                        chunk.SetDensity(chunkX, snowY, chunkZ, MarchingCubes.DensityTerrain);
                    }

                    Write("Total SnowY was: " + snowY);

                    break;
                }
            }

            Write("Changes: " + (Changes == null ? "null" : Changes.Count.ToString()));
            if (Changes != null) GameManager.Instance.SetBlocksRPC(Changes);
        }
    }


    public static void SetSnow(int startX, int startZ, int width, int depth, ChunkCluster cluster, bool notifyRpc,
        bool log, bool AlreadyFilled, int height)
    {
        Write("Set snow " + startX + "," + startZ + " by " + width + "," + depth);

        //WorldBase world = cluster.GetWorld();
        var chunk = cluster.GetChunkSync(World.toChunkXZ(startX), World.toChunkXZ(startZ));

        var worldPos = new Vector3i(startX, 0, startZ);
        var size = new Vector3i(width, height, depth);

        var processed = new List<long>();

        for (var x = 0; x < width; x++)
        {
            for (var z = 0; z <= depth; z++)
            {
                var worldX = x + startX;
                var worldZ = z + startZ;
                var chunkWorldX = World.toChunkXZ(worldX);
                var chunkWorldZ = World.toChunkXZ(worldZ);
                //var chunkX = World.toBlockXZ(worldX);
                //var chunkZ = World.toBlockXZ(worldZ);
                if (chunk == null || chunk.X != chunkWorldX || chunk.Z != chunkWorldZ)
                {
                    Write("Getting chunk at " + chunkWorldX + "," + chunkWorldZ + "     " + worldX + "," + worldZ);
                    chunk = cluster.GetChunkSync(chunkWorldX, chunkWorldZ);

                    if (chunk != null)
                    {
                        if (processed.Contains(chunk.Key))
                            continue;
                        processed.Add(chunk.Key);
                        ProcessChunk(chunk, worldPos, size, AlreadyFilled, log, notifyRpc, true);
                    }
                }
            }
        }

        Write("Set Snow Complete");
    }

    private static void Write(string msg)
    {
        if (Logging)
            Debug.Log(msg);
    }
}