
    using UnityEngine;

    public static class CaveTunnelerV2 {
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

        public static WorldDecoratorPOIFromImage poiFromImage;
        public static IBiomeProvider biomeProvider;

        public static void AddCaveToChunk(Chunk chunk)
        {
            if (chunk == null)
                return;

            if (GameManager.Instance.World.ChunkCache.ChunkProvider is not ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw)
            {
                Debug.Log("Not a World From Raw.");
                return;
            }

            if (poiFromImage == null)
            {
                biomeProvider = chunkProviderGenerateWorldFromRaw.GetBiomeProvider();
            }
            var chunkPos = chunk.GetWorldPos();


            AddLevel(chunk, fastNoise);
        }

        public static float GetPixel(int x, int z) {
           
            var value = poiFromImage.m_Poi.colors.GetValue(x, z);
            return value;
        }

        private static void AddLevel(Chunk chunk, FastNoise fastNoise1) {
            var chunkPos = chunk.GetWorldPos();

            for (var chunkX = 0; chunkX < 16; chunkX++)
            {
                for (var chunkZ = 0; chunkZ < 16; chunkZ++)
                {
                    var worldX = chunkPos.x + chunkX;
                    var worldZ = chunkPos.z + chunkZ;
                    var block = biomeProvider.GetTopmostBlockValue(worldX, worldZ);
                    if ( block.Block.GetBlockName()=="terrForestGround") continue;
                  //  Debug.Log($"Position: {worldX}, {worldZ} : Block: {block.Block.GetBlockName()}");
                }
            }
        }
    }
