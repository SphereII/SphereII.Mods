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
            public static void Postfix(string _sWorldName)
            {
                var path = PathAbstractions.WorldsSearchPaths.GetLocation(_sWorldName, null, null).FullPath + "/cave8.png";
                if (!File.Exists(path))
                {
                    Log.Out("No Cave Map.");
                    return;

                }

                Texture2D texture2D = null;
                texture2D = TextureUtils.LoadTexture(path, FilterMode.Point, false, false, null);
                if (texture2D != null)
                {
                    Log.Out($"Generated Texture from {path}: {texture2D.width} {texture2D.height}");
                    HeightMapTunneler.caveMapColor = new Color[texture2D.width, texture2D.height];
                    for (int y = 0; y < texture2D.height; y++)
                    {
                        var textwidth = "";
                        for (int x = 0; x < texture2D.width; x++)
                        {
                            var pixel = texture2D.GetPixel(x, y);
                            textwidth += " " + pixel.grayscale;
                            HeightMapTunneler.caveMapColor[x, y] = pixel;
                        }
                        Log.Out($"{y} 0 : {textwidth}");
                    }

                }
            }
        }
    }
}
    