
    using System.Collections.Generic;
    using UnityEngine;
    using WorldGenerationEngineFinal;

    public class SCoreTunneler
    {
        private static readonly string AdvFeatureClass = "CaveConfiguration";
        private static FastNoise fastNoise;

        // Special air that has stability
        private BlockValue caveAir = new BlockValue((uint) Block.GetBlockByName("air").blockID);
        private BlockValue pillarBlock = new BlockValue((uint) Block.GetBlockByName("terrDesertGround").blockID);

        private BlockValue bottomCaveDecoration = new BlockValue((uint) Block.GetBlockByName("cntCaveFloorRandomLootHelper").blockID);

        private BlockValue bottomDeepCaveDecoration = new BlockValue((uint) Block.GetBlockByName("cntDeepCaveFloorRandomLootHelper").blockID);

        private BlockValue topCaveDecoration = new BlockValue((uint) Block.GetBlockByName("cntCaveCeilingRandomLootHelper").blockID);
        private int _deepCaveThreshold = 30;

        public Color[,] caveMapColor;
        
        public virtual void AddCaveToChunk(Chunk chunk)
        {
            if (chunk == null)
                return;

            string worldSizeName;

            // foreach (KeyValuePair<string, Vector2i> keyValuePair in WorldBuilder.WorldSizeMapper)
            // {
            //     string text;
            //     
            //     Vector2i vector2i;
            //     keyValuePair.Deconstruct(out text, out vector2i);
            //     string text2 = text;
            //     Vector2i vector2i2 = vector2i;
            //     var WorldSize = GamePrefs.GetInt(EnumGamePrefs.WorldGenSize);
            //     if (WorldSize >= vector2i2.x && WorldSize < vector2i2.y)
            //     {
            //         worldSizeName = text2;
            //     }
            // }

            var worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
            var worldPath = GameIO.GetUserGameDataDir() + "/GeneratedWorlds/" + worldName + "/";

        //    var worldPath = GamePrefs.GetString(E)
        }
        
        public virtual void PlaceBlock(Chunk chunk, Vector3i position, bool isPillar = false)
        {
            // Make sure the position is in bounds, and not currently air. no sense in changing it
            if (position.x > 15 || position.z > 15) return;
            if (position.x < 0 || position.z < 0) return;
            if (chunk.GetBlock(position.x, position.y, position.z).isair) return;

            if (!isPillar)
            {
                chunk.SetBlockRaw(position.x, position.y, position.z, caveAir);
                chunk.SetDensity(position.x, position.y, position.z, MarchingCubes.DensityAir);
                return;
            }

            chunk.SetBlockRaw(position.x, position.y, position.z, pillarBlock);
            chunk.SetDensity(position.x, position.y, position.z, MarchingCubes.DensityTerrain);
        }

        // Generate a prefab to push around.
        public virtual void CreateEmptyPrefab(Chunk chunk, Vector3i position)
        {
            var random = GameManager.Instance.World.GetGameRandom();
            var x = random.RandomRange(2, 4);
            var z = random.RandomRange(2, 4);
            var y = random.RandomRange(3, 5);
            //var prefab = new Prefab(new Vector3i(4, 3, 4));
            var prefab = new Prefab(new Vector3i(x, y, z));
            prefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, position, true);
        }
        
        public virtual void PlaceAround(Chunk chunk, Vector3i position, bool isPillar = false)
        {
            PlaceBlock(chunk, position, isPillar);
            PlaceBlock(chunk, position + Vector3i.right, isPillar);
            PlaceBlock(chunk, position + Vector3i.left, isPillar);

            PlaceBlock(chunk, position + Vector3i.forward, isPillar);
            PlaceBlock(chunk, position + Vector3i.forward + Vector3i.right, isPillar);
            PlaceBlock(chunk, position + Vector3i.forward + Vector3i.left, isPillar);

            PlaceBlock(chunk, position + Vector3i.back, isPillar);
            PlaceBlock(chunk, position + Vector3i.back + Vector3i.right, isPillar);
            PlaceBlock(chunk, position + Vector3i.back + Vector3i.left, isPillar);
        }

        public virtual void AddLevel(Chunk chunk, FastNoise fastNoise, int DepthFromTerrain = 10)
        {
            var chunkPos = chunk.GetWorldPos();

            var biomeRestriction = Configuration.GetPropertyValue(AdvFeatureClass, "AllowedBiomes");

            var caveThresholdXZ = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdXZ"));
            var caveThresholdY = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveThresholdY"));
            for (var chunkX = 0; chunkX < 16; chunkX++)
            {
                for (var chunkZ = 0; chunkZ < 16; chunkZ++)
                {
                    var worldX = chunkPos.x + chunkX;
                    var worldZ = chunkPos.z + chunkZ;
                }
            }
        }

    }
