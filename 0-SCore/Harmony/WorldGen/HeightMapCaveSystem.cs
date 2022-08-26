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

                var path = "";
                path = ModManager.PatchModPathString(caveStamp);
                if ( string.IsNullOrEmpty(path ))
                    path = PathAbstractions.WorldsSearchPaths.GetLocation(_sWorldName, null, null).FullPath + "/cave6.png";

                if (!File.Exists(path))
                {
                    Log.Out("No Cave Map: " + path);
                    return;

                }

                Texture2D texture2D = null;
                texture2D = TextureUtils.LoadTexture(path, FilterMode.Point, false, false, null);
                if (texture2D != null)
                {
                    Log.Out($"Generated Texture from {path}: {texture2D.width} {texture2D.height}");
                    HeightMapTunneler.caveMapColor = new Color[texture2D.width, texture2D.height];
                    bool foundCave = false;
                    for (int y = 0; y < texture2D.height; y++)
                    {
                        var textwidth = "";
                        for (int x = 0; x < texture2D.width; x++)
                        {
                            var pixel = texture2D.GetPixel(x, y);
                            textwidth += " " + pixel.grayscale;
                            if (pixel.grayscale > 0 && !foundCave && pixel.grayscale != 1)
                            {
                                Debug.Log("Cave Teleport: " + x + " " + y);
                                foundCave = true;
                            }
                            HeightMapTunneler.caveMapColor[x, y] = pixel;
                        }
                        Debug.Log($"{y} 0 : {textwidth}");
                    }

                }
            }
        }
    }
}
    