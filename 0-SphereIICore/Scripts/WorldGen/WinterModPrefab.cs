using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

public static class WinterModPrefab
{
    static BlockValue snow = new BlockValue((uint)Block.GetBlockByName("terrSnow", false).blockID);
    static BlockValue ice = new BlockValue((uint)Block.GetBlockByName("terrIce", false).blockID);

    //  static BlockValue ice = new BlockValue((uint)Block.GetBlockByName("terrIce", false).blockID);

    private static Random Rand = new Random(Guid.NewGuid().GetHashCode());
    public static bool Logging = false;
    public static bool Rpc = true;

    public static void SetSnowChunk(Chunk chunk, Vector3i position, Vector3i size)
    {
        //var position = chunk.GetWorldPos();
        Write("SetSnowChunk " + position);
        ProcessChunk(chunk, position, size, false, Logging, Rpc, false);

        //SetSnow(position.x, position.z, 16, 16, GameManager.Instance.World.ChunkCache, false, true);
    }

    public static void SetSnowPrefab(Prefab prefab, ChunkCluster cluster, Vector3i position, QuestTags _questTag)
    {
        bool AlreadyFilled = false;
        if (_questTag != QuestTags.none)
        {
            AlreadyFilled = true;
            prefab.yOffset -= 8;
            position.y -= 8;
        }
        SetSnow(position.x, position.z, prefab.size.x, prefab.size.z, cluster, Rpc, Logging, AlreadyFilled, prefab.size.y);
    }

    private static void ProcessChunk(Chunk chunk, Vector3i position, Vector3i size, bool regen, bool log, bool notifyRpc, bool isPrefab)
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
        for (int chunkX = 0; chunkX < 16; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < 16; chunkZ++)
            {

                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;

                if (worldX < minX || worldX > maxX || worldZ < minZ || worldZ > maxZ)
                {
                    //    Write("Out of bounds " + position + " s: " + size + " wp: " + worldX + "," + worldZ);
                    continue;
                }

                // Grab the chunk's terrain height to use a  baseline
                var tHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                var tHeightWithSnow = tHeight + 8;

                // If we are resetting for a quest, the terrain has already been moved up.
                if (regen)
                {
                    tHeightWithSnow = tHeight;
                }

                for (int y = 255; y >= 0; y--)
                {

                    // Test if we still need to go down further
                    var b = chunk.GetBlock(chunkX, y, chunkZ);
                    if (b.type == 0)
                        continue; //still air 

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
                    {
                        Write(worldX + "," + worldZ + " found block (" + b.Block.GetBlockName() + ") id " + b.type + " at height " + y + " setting depth of " + snowHeight + " Prefab Height: " + size.y + "  cx " + chunkX + "/" + chunk.X + "," + chunkZ + "/" + chunk.Z + "  t " + tHeight + " Height With Snow: " + tHeightWithSnow);
                    }

                    // Are we re-generating or is this a fresh? if we are regenerating, we don't want to keep adding another 8 blocks of snow

                    //if (!regen)
                    //    snowHeight = 0;


                    Write("Current Block is: " + b.Block.GetBlockName());
                    int snowY = y;
                    // Raise the level by one, since we don't want to replace the roof with snow, or the current block.
                    int StartingSnow = y + 1;
                    for (snowY = StartingSnow; snowY < y + snowHeight; snowY++) //&& snowY < tHeight
                    {


                        if (snowY > 255)
                            continue;

                        var check = chunk.GetBlock(chunkX, snowY, chunkZ);

                        //if (log)
                        //    Write("Set " + worldX + "," + snowY + "," + worldZ + " to snow || world  " + chunk.GetWorldPos() + " applied: " + (check.type != snow.type) + "  rpc: " + Rpc);

                        if (check.type != snow.type)
                        {

                            if (notifyRpc)
                            {
                                var pos = chunk.GetWorldPos();
                                pos.x += chunkX;
                                pos.y = snowY;
                                pos.z += chunkZ;
                                //GameManager.Instance.World.SetBlockRPC(pos, snow, MarchingCubes.DensityTerrain);
                                Changes.Add(new BlockChangeInfo(pos, snow, MarchingCubes.DensityTerrain));
                            }
                            else
                            {

                                chunk.SetBlock(GameManager.Instance.World, chunkX, snowY, chunkZ, snow);

                                var density = snowY == y + snowHeight - 1 ? (sbyte)-Rand.Next(1, 127) : MarchingCubes.DensityTerrain;
                                chunk.SetDensity(chunkX, snowY, chunkZ, density);
                            }
                        }
                    }
                    Write("Total SnowY was: " + snowY);

                    break;
                }

            }

            Write("Changes: " + (Changes == null ? "null" : Changes.Count.ToString()));
            if (Changes != null)
            {
                GameManager.Instance.SetBlocksRPC(Changes);

            }
        }
    }


    public static void SetSnow(int startX, int startZ, int width, int depth, ChunkCluster cluster, bool notifyRpc, bool log, bool AlreadyFilled, int height)
    {

        Write("Set snow " + startX + "," + startZ + " by " + width + "," + depth);

        //WorldBase world = cluster.GetWorld();
        Chunk chunk = cluster.GetChunkSync(World.toChunkXZ(startX), World.toChunkXZ(startZ));

        var worldPos = new Vector3i(startX, 0, startZ);
        var size = new Vector3i(width, height, depth);

        List<long> processed = new List<long>();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z <= depth; z++)
            {
                int worldX = x + startX;
                int worldZ = z + startZ;
                int chunkWorldX = World.toChunkXZ(worldX);
                int chunkWorldZ = World.toChunkXZ(worldZ);
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