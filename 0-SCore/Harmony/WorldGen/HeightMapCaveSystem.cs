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
        [HarmonyPatch(typeof(GameManager))]
        [HarmonyPatch("createWorld")]
        public class SCoreHeightMapCaveSystemGameManager
        {
            private static readonly string AdvFeatureClass = "CaveConfiguration";
            private static readonly string CavePath = "CavePath";
            private static readonly string Feature = "CaveEnabled";
            public static void Postfix(string _sWorldName)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                var caveStamp = Configuration.GetPropertyValue(AdvFeatureClass, CavePath);
               
                //this.heightMapWidth = GameUtils.WorldInfo..HeightmapSize.x;
                //this.heightMapHeight = base.WorldInfo.HeightmapSize.y;
                //this.heightMapScale = base.WorldInfo.Scale;
                //this.bFixedWaterLevel = base.WorldInfo.FixedWaterLevel;

                var path = "";
                path = ModManager.PatchModPathString(caveStamp);
                if ( string.IsNullOrEmpty(path ))
                    path = PathAbstractions.WorldsSearchPaths.GetLocation(_sWorldName, null, null).FullPath + "/cave1.png";

                if (!File.Exists(path))
                {
                    Log.Out("No Cave Map: " + path);
                    return;

                }

                Texture2D original = null;


                original = TextureUtils.LoadTexture(path, FilterMode.Point, false, false, null);
                //if (texture2D != null)
                //{
                //    Log.Out($"Generating Texture from {path}: {texture2D.width} {texture2D.height}");
                //    Texture2D flipped = new Texture2D(texture2D.width, texture2D.height);
                //    int xN = texture2D.width;
                //    int yN = texture2D.height;

                //    for (int i = 0; i < xN; i++)
                //    {
                //        for (int j = 0; j < yN; j++)
                //        {
                //            flipped.SetPixel(xN - i - 1, j, texture2D.GetPixel(i, j));
                //        }
                //    }

                //    flipped.Apply();

                //    var width = flipped.width;
                //    var height = flipped.height;  
                //    if ( width != height)
                //    {
                //        Log.Out($"Cave map's dimensions are wrong: w: {width} h: {height}... truncating.... this cave should be square.");
                //        if (width > height)
                //            width = height;
                //        else
                //            height = width;
                //    }

                //    var m_Dtm = new ArrayWithOffset<byte>(width, height);

                //    Color[] pixels = flipped.GetPixels();
                //    var rawdata = new ushort[pixels.Length];

                //    for (int i = 0; i < width; i++)
                //    {
                //        for (int j = 0; j < height; j++)
                //        {
                //            m_Dtm[i + m_Dtm.MinPos.x, j + m_Dtm.MinPos.y] = (byte)(pixels[width * j + i].grayscale * 255f);
                //            rawdata[width * j + i] = (ushort)((int)((byte)(pixels[width * j + i].grayscale * 255f)) | (int)((byte)((pixels[width * j + i].grayscale * 255f - 255f) * 255f)) << 8);
                //        }
                //    }

                //    Log.Out($"Generated Texture from {path}: {width} {height} Raw Data: {rawdata.Length}");

                //    HeightMapTunneler.heightMap = new HeightMap(m_Dtm.DimX, m_Dtm.DimY, 255f, rawdata, 0);
                //    Log.Out($"Height Map: {HeightMapTunneler.heightMap.Length} {HeightMapTunneler.heightMap.GetHeight()} {HeightMapTunneler.heightMap.GetWidth()} ");

                if (original == null) return;

                // Flip the texture
                Texture2D texture2D = new Texture2D(original.width, original.height);
                int xN = original.width;
                int yN = original.height;

                for (int i = 0; i < xN; i++)
                {
                    for (int j = 0; j < yN; j++)
                    {
                        texture2D.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                    }
                }
                texture2D.Apply();

                Log.Out($"Generating Texture from {path}: {texture2D.width} {texture2D.height}");
                HeightMapTunneler.caveMapColor = new Color[texture2D.width, texture2D.height];
                for (int y = 0; y < texture2D.height; y++)
                {
                    for (int x = 0; x < texture2D.width; x++)
                    {
                        var pixel = texture2D.GetPixel(x, y);
                        HeightMapTunneler.caveMapColor[x, y] = pixel;
                    }
                }

            
            }
        }
    }
}
    