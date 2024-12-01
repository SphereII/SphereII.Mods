 using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using Object = System.Object;

namespace SCore.Features.Caves.Scripts {
    public class SCoreCavesUtils {
        private static readonly string AdvFeatureClass = "CaveConfiguration";
        private static readonly string CavePath = "CavePath";

        private static Color[] _caveMapColor = new Color[]{};
        private static int width;
        private static int height;
        
        public static Vector3i GetCavePosition(int worldX, int y, int worldZ) {
            var depth = y - 10;
            return new Vector3i(worldX, depth, worldZ);
        }
        public static void SetTexture() {
            if (_caveMapColor?.Length > 0 )return;
            
            var caveStamp = Configuration.GetPropertyValue(AdvFeatureClass, CavePath);
            var path = "";
            path = ModManager.PatchModPathString(caveStamp);
            if (!File.Exists(path))
            {
                Log.Out("No Cave Map: " + path);
                return;
            }

            var texture2D = TextureUtils.LoadTexture(path);
            width = texture2D.width;
            height = texture2D.height;

            Log.Out($"Generating Texture from {path}: {width} {height}");
            _caveMapColor = new Color[width + height * width];
            for (var y = 0; y < texture2D.height; y++)
            {
                for (var x = 0; x < texture2D.width; x++)
                {
                    var pixel = texture2D.GetPixel(x, y);
                    if (pixel.r > 0.1)
                    {
                     //   Debug.Log($"Red Pixel at : {x},{y}");
                    }
                    _caveMapColor[x + y * width] = pixel;
                }
            }
            UnityEngine.Object.Destroy(texture2D);

        }

        public static float GetCavePixel(int x, int z) {
            var index = x + z * width;
            return _caveMapColor[index].grayscale;

        }
        
        public static float GetPOIPixel(int x, int z) {
            var index = x + z * width;
            return _caveMapColor[index].r;
            
        }
        
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
            return prefab;
        }
    }
}