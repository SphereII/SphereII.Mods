using System;
using System.Collections.Generic;
using Features.RemoteCrafting;
using SCore.Features.Caves.Scripts;
using UnityEngine;
using UnityEngine.Animations;

public static class SCoreCavesSplat3 {
    private static readonly string AdvFeatureClass = "CaveConfiguration";
    private static readonly string CavePath = "CavePath";
    private static readonly string Feature = "CaveEnabled";

    private static FastNoise fastNoise;

    private static bool flip = false;

    // Special air that has stability
    private static BlockValue caveAir = new BlockValue((uint)Block.GetBlockByName("air").blockID);

    private static readonly BlockValue bottomCaveDecoration =
        new BlockValue((uint)Block.GetBlockByName("cntCaveFloorRandomLootHelper").blockID);

    private static readonly BlockValue bottomDeepCaveDecoration =
        new BlockValue((uint)Block.GetBlockByName("cntDeepCaveFloorRandomLootHelper").blockID);

    private static readonly BlockValue topCaveDecoration =
        new BlockValue((uint)Block.GetBlockByName("cntCaveCeilingRandomLootHelper").blockID);


    private static PoiMapElement GetElement(int worldX, int worldZ) {
        var world = GameManager.Instance.World;
        var chunkProviderGenerateWorldFromRaw = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
        var poiFromImage = chunkProviderGenerateWorldFromRaw?.poiFromImage;
        if (poiFromImage == null) return null;
        if (!poiFromImage.m_Poi.Contains(worldX, worldZ)) return null;
        var data = poiFromImage.m_Poi.GetData(worldX, worldZ);
        if (data == 0) return null;
        var poiForColor = world.Biomes.getPoiForColor(data);

        return poiForColor;
    }

    private static bool IsRoad(PoiMapElement poiMapElement) {
        if (poiMapElement == null) return false;
        // All roads

        // terrGravel = 16
        // terrAsphalt = 17
        return poiMapElement.m_BlockValue.type != 240;

        // just terrGravel
        return poiMapElement.m_BlockValue.type == 16;
    }

    private static bool CheckForPrefab(PoiMapElement poiMapElement, Chunk chunk, Vector3i position) {
        if (!IsPrefabAt(position.x, position.z))
        {
            return false;
        }

        var random = GameManager.Instance.World.GetGameRandom();
        var poiName = SphereCache.POIs[random.RandomRange(0, SphereCache.POIs.Count)];
        var newPrefab = FindOrCreatePrefab(poiName);
        if (newPrefab == null)
        {
            Debug.Log("Could not generate prefab");
            return false;
        }

        var prefabClone = newPrefab.Clone();
        Debug.Log($"Creating Prefab at {position}");
        var temp = prefabClone.Tags;
        prefabClone.Tags = FastTags<TagGroup.Poi>.Parse("SKIP_HARMONY_COPY_INTO_LOCAL");
        prefabClone.yOffset = 0;
        prefabClone.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, position,
            true);
        var entityInstanceIds = new List<int>();
        prefabClone.CopyEntitiesIntoChunkStub(chunk, position, entityInstanceIds, true);

        // Trying to track a crash in something.
        //prefab.CopyIntoLocal(GameManager.Instance.World.ChunkClusters[0], destination, true, true);
        // Restore any of the tags that might have existed before.
        prefabClone.Tags = temp;
        //  prefab.SnapTerrainToArea(GameManager.Instance.World.ChunkClusters[0], destination);
        //  prefabClone.CopyIntoLocal(GameManager.Instance.World.ChunkClusters[0],
        //    new Vector3i(position.x, position.y, position.z), true, false, FastTags<TagGroup.Global>.none);
        return true;
    }

    public static PrefabInstance GetPrefabAt(int worldX, int worldZ) {
        return GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPosInside(worldX, worldZ);
    }

    private static bool IsPrefabAt(int worldX, int worldZ) {
        var decorateChunkPIs = new List<PrefabInstance>();
        GameManager.Instance.GetDynamicPrefabDecorator()
            .GetPrefabsAtXZ(worldX, worldX + 15, worldZ, worldZ + 15, decorateChunkPIs);
        return decorateChunkPIs.Count > 0;
    }

    public static void AddCaveToChunk(Chunk chunk) {
        if (chunk == null)
            return;

        SphereCache.GetFastNoise(chunk);
        var chunkPos = chunk.GetWorldPos();
        var caveOpen = false;
        var caveCount = 0;

        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;
                var terrainHeight = chunk.GetTerrainHeight(chunkX, chunkZ);

                var position = GetPosition(worldX, terrainHeight, worldZ);
                var poiMapElement = flip ? GetElement(worldZ, worldX) : GetElement(worldX, worldZ);
                if (!IsRoad(poiMapElement)) continue;
                
                // terrGravel
                if (poiMapElement.m_BlockValue.type == 16)
                {
                    
                    // Leave special blocks in it's place, maybe tileEntity, as cave points. THen pass over again,
                    // replacing the points with the empty prefab.
                    caveCount++;
                    if (caveCount < 3)
                    {
                        CreateEmptyPrefab(chunk, position);
                        caveCount = 0;
                    }
                }
            }
        }

        for (var chunkX = 0; chunkX < 16; chunkX++)
        {
            for (var chunkZ = 0; chunkZ < 16; chunkZ++)
            {
                var worldX = chunkPos.x + chunkX;
                var worldZ = chunkPos.z + chunkZ;
                var terrainHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                var poiMapElement = flip ? GetElement(worldZ, worldX) : GetElement(worldX, worldZ);
                if (!IsRoad(poiMapElement)) continue;
        
                // Make cave opening.
                if (poiMapElement.m_BlockValue.type == 17)
                {
                    if ( !IsVoid(chunk, chunkX, chunkZ)) continue;
                    CreateCaveOpening(chunk, chunkX, chunkZ);
                    return;
                }
            }
        }
    }

    private static bool IsVoid(Chunk chunk, int chunkX, int chunkZ) {
        return VoidsUnder(chunk, chunkX, chunkZ) > 4;
    }
    private static int VoidsUnder(Chunk chunk, int chunkX, int chunkZ) {
        var terrainHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
        var voids = 2;
        for (var x = -2; x < 2; x++)
        {
            for (var z = -2; z < 2; z++)
            {
                var position = new Vector3i(chunkX + x, terrainHeight - 8, chunkZ +z);
                if (position.x > 15 || position.z > 15)continue;
                if (position.x < 0 || position.z < 0) continue;
                if (chunk.GetBlock(position.x, position.y, position.z).isair)
                    voids++;
            }
        }

        return voids;
    }
    private static void CreateCaveOpening(Chunk chunk, int chunkX, int chunkZ) {
        int terrainHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
        var destination = new Vector3i(chunkX, terrainHeight + 1, chunkZ);
        var random = GameManager.Instance.World.GetGameRandom();
        // Move it around for a bit of variety
        for (var travel = 0; travel < 10; travel++)
        {
            if (random.RandomFloat < 0.1f)
                destination.x++;
            if (random.RandomFloat < 0.2f)
                destination.x--;

            if (random.RandomFloat < 0.3f)
                destination.z--;
            if (random.RandomFloat < 0.4f)
                destination.z++;

            PlaceAround(chunk, destination);
            destination.y--;
            destination = new Vector3i(chunkX, destination.y, chunkZ);

        }
    }

    private static Vector3i GetPosition(int worldX, int y, int worldZ) {
        var depth = y - 10;
        return new Vector3i(worldX, depth, worldZ);
    }


    public static void PlaceBlock(Chunk chunk, Vector3i position) {
        // Make sure the position is in bounds, and not currently air. no sense in changing it
        if (position.x > 15 || position.z > 15) return;
        if (position.x < 0 || position.z < 0) return;
        if (chunk.GetBlock(position.x, position.y, position.z).isair) return;

        chunk.SetBlockRaw(position.x, position.y, position.z, caveAir);
        chunk.SetDensity(position.x, position.y, position.z, MarchingCubes.DensityAir);
    }

    // Generate a prefab to push around.
    public static void CreateEmptyPrefab(Chunk chunk, Vector3i position) {
        
        var prefab = new Prefab(new Vector3i(1, 3, 1));
        prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, position, true);
    }


    // Changes the blocks. This works better when doing the cave openings, vs the prefab
    public static void PlaceAround(Chunk chunk, Vector3i position) {
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
    public static void AddLevel(Chunk chunk, FastNoise fastNoise) {
    }

    // Helper method is check the prefab decorator first to see if its there, then create it if it does not exist.
    public static Prefab FindOrCreatePrefab(string strPOIname) {
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

    public static void AddDecorationsToCave(Chunk chunk) {
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
                        prefab.Tags = FastTags<TagGroup.Poi>.Parse("SKIP_HARMONY_COPY_INTO_LOCAL");
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