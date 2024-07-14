using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WorldGenerationEngineFinal;

public class PathingWormTunneler
{
    public static List<Vector2> CaveMaps = new List<Vector2>();

    public static void Init()
    {

        foreach (var cave in SphereCache.caveEntrances)
        {
            int caveRadius = 200;
            Vector2i startPos = new Vector2i(cave.x, cave.z);
            var exitX = Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(cave.x, cave.x + caveRadius * 16));
            var exitZ = Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(cave.z, cave.z + caveRadius * 16));
            var vector = new Vector2i(exitX, exitZ);
            var path3 = new Path(startPos, vector, 4, true, true);
            if (path3.IsValid)
            {
                foreach (var path in path3.FinalPathPoints)
                    PathingWormTunneler.CaveMaps.Add(path);
            }
        }
    }
    public static void AddCaveToChunk(Chunk chunk)
    {
        if (chunk == null)
            return;


        if (PathingWormTunneler.CaveMaps.Count == 0)
        {
            Log.Out("Cave Map is empty.. populating...");
            PathingWormTunneler.Init();
        }
        var chunkPos = chunk.GetWorldPos();

        // Find middle of chunk for its height
        int tHeight = chunk.GetTerrainHeight(8, 8);

        // If the chunk isn't in the cache, don't generate.
        if (!SphereCache.caveChunks.Contains(chunkPos))
            return;
        AddLevel(chunk);
        // Decorate is done via another patch in Caves.cs
    }
    public static void AddLevel(Chunk chunk, int y = 30)
    {
        var chunkPos = chunk.GetWorldPos();

        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = (chunkPos.x + chunkX);
                var worldZ = (chunkPos.z + chunkZ);

                var vector = new Vector2(worldX, worldZ);
                if (PathingWormTunneler.CaveMaps.Contains(vector))
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

}

