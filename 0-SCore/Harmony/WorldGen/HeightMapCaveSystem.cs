using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WorldGenerationEngineFinal;

namespace Harmony.WorldGen
{
    public class SCoreheightMapCaveSystem
    {
        // Make the world darker underground
        [HarmonyPatch(typeof(WorldBiomeProviderFromImage))]
        [HarmonyPatch("InitData")]
        public class SCoreHeightMapCaveSystemGameManager
        {
            private static readonly string AdvFeatureClass = "CaveConfiguration";
            private static readonly string CavePath = "CavePath";
            private static readonly string Feature = "CaveEnabled";
            public static void Postfix()
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                var configurationType = Configuration.GetPropertyValue(AdvFeatureClass, "GenerationType");
                if (configurationType != "HeightMap")
                    return;


         
                //GameManager.Instance.World.ChunkCache.ChunkProvider.GetTerrainGenerator().GetTerrainHeightAt()
              //  GameManager.Instance.World.ChunkCache.ChunkProvider.GetOverviewMap();
                var caveStamp = Configuration.GetPropertyValue(AdvFeatureClass, CavePath);

                var path = "";
                path = ModManager.PatchModPathString(caveStamp);
                if (!File.Exists(path))
                {
                    Log.Out("No Cave Map: " + path);
                    return;

                }

                Texture2D texture2D = TextureUtils.LoadTexture(path, FilterMode.Point, false, false, null);
                Log.Out($"Generating Texture from {path}: {texture2D.width} {texture2D.height}");
                HeightMapTunneler.caveMapColor = new Color[texture2D.width, texture2D.height];
                for (int y = 0; y < texture2D.height; y++)
                {
                    for (int x = 0; x < texture2D.width; x++)
                    {
                        var pixel = texture2D.GetPixel(x, y);
                        if (pixel.r > 0.9)
                            SphereCache.caveEntrances.Add(new Vector3i(x, 1, y));

                        HeightMapTunneler.caveMapColor[x, y] = pixel;
                    }
                }

            }
        }
    }
}
