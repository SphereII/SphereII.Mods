using System.Collections.Generic;
using UnityEngine;

namespace SCore.Features.Caves.Scripts {
    public class SCoreCavesTexture {
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

        public static void CreateEmptyPrefab(Chunk chunk, Vector3i position) {
            var prefab = new Prefab(new Vector3i(3, 3, 3));
            prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, position, true);
        }


        public static void AddCaveToChunk(Chunk chunk) {
            if (chunk == null)
                return;

            SphereCache.GetFastNoise(chunk);
            var chunkPos = chunk.GetWorldPos();
            for (var chunkX = 0; chunkX < 16; chunkX++)
            {
                for (var chunkZ = 0; chunkZ < 16; chunkZ++)
                {
                    var worldX = chunkPos.x + chunkX;
                    var worldZ = chunkPos.z + chunkZ;
                    var color = SCoreCavesUtils.GetCavePixel(worldX, worldZ);
                    if (color == 0) continue;

                    var terrainHeight = chunk.GetTerrainHeight(chunkX, chunkZ);
                    var position = SCoreCavesUtils.GetCavePosition(worldX, terrainHeight, worldZ);
                    CreateEmptyPrefab(chunk, position);
                }
            }
        }

        private static bool CheckForPrefab(Chunk chunk, Vector3i position) {
            var color = SCoreCavesUtils.GetPOIPixel(position.x, position.z);
            if (color < 1.000) return false;

            var random = GameManager.Instance.World.GetGameRandom();
            var poiName = SphereCache.POIs[random.RandomRange(0, SphereCache.POIs.Count)];
            var newPrefab = SCoreCavesUtils.FindOrCreatePrefab(poiName);
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

            prefabClone.Tags = temp;
            return true;
        }

        public static void AddDecorationsToCave(Chunk chunk) {
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
                    var position = SCoreCavesUtils.GetCavePosition(worldX, tHeight, worldZ);

                    CheckForPrefab(chunk, position);
                    // One test world, we blew through a threshold.
                    if (tHeight > 250)
                        tHeight = 240;

                    // Move from the bottom up, leaving the last few blocks untouched.
                    //for (int y = tHeight; y > 5; y--)
                    for (var y = 5; y < tHeight - 10; y++)
                    {
                        var b = chunk.GetBlock(chunkX, y, chunkZ);

                        // if (b.type != caveAir.type)
                        //     continue;

                        var under = chunk.GetBlock(chunkX, y - 1, chunkZ);
                        var above = chunk.GetBlock(chunkX, y + 1, chunkZ);

                        // Check the floor for possible decoration
                        if (under.Block.shape.IsTerrain())
                        {
                            var blockValue2 =
                                BlockPlaceholderMap.Instance.Replace(bottomCaveDecoration, random, worldX, worldZ);

                            // Place alternative blocks down deeper
                            if (y < 30)
                                blockValue2 = BlockPlaceholderMap.Instance.Replace(bottomDeepCaveDecoration, random,
                                    worldX, worldZ);

                            chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);

                            // Check to see if we can place a prefab here.
                            if (prefabCounter >= MaxPrefab) continue;

                            if (random.RandomRange(0, 10) > 1) continue;

                            continue;
                        }

                        // Check the ceiling to see if its a ceiling decoration
                        if (above.Block.shape.IsTerrain())
                        {
                            var blockValue2 =
                                BlockPlaceholderMap.Instance.Replace(topCaveDecoration, random, worldX, worldZ);
                            chunk.SetBlock(GameManager.Instance.World, chunkX, y, chunkZ, blockValue2);
                        }
                    }
                }
            }
        }
    }
}