using HarmonyLib;
using UnityEngine;

namespace SCore.Harmony.Blocks {
    public class TerrainAlignmentUtilsPatch {
        [HarmonyPatch(typeof(TerrainAlignmentUtils))]
        [HarmonyPatch("AlignToTerrain")]
        public class TerrainAlignmentUtils_AlignToTerrain {
            public static bool Prefix(Block block, Vector3i _blockPos, BlockEntityData _ebcd) {
                if (_ebcd == null) return false;
                /*
                if (_ebcd.bHasTransform && _ebcd.transform == null)
                {
                    Debug.Log($"Transform is Null, but it shouldn't be: {block.GetBlockName()} {_blockPos}");
                }
                */
                return _ebcd?.transform != null;
            }
        }
    }
}