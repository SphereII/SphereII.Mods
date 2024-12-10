using HarmonyLib;
using UnityEngine;

namespace SCore.Harmony.Blocks {
    public class ChunkPoolBlockEntityTransform {
        private static readonly string AdvFeatureClass = "ErrorHandling";
        private static readonly string Feature = "EnablePoolBlockEntityTransformCheck";
        private static readonly string Logging = "LogPoolBlockEntityTransformCheck";

        [HarmonyPatch(typeof(Chunk))]
        [HarmonyPatch("poolBlockEntityTransform")]
        public class ChunkpoolBlockEntityTransform {
            public static bool Prefix(BlockEntityData _bed) {
                // Check if this feature is enabled.
             //   if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
                if (_bed.transform != null && _bed.transform.gameObject != null) return true;
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, Logging))
                {
                    Debug.Log("Error: There's a Block Entity without a Transform!");
                    Debug.Log($"Block Position: {_bed.pos} : BlockValue: {_bed.blockValue.Block.ToString()}");
                }

                return false;

            }
        }
        
        [HarmonyPatch(typeof(Chunk))]
        [HarmonyPatch("SetBlockEntityRendering")]
        public class ChunksetBlockEntityRendering {
            public static bool Prefix(BlockEntityData _bed) {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
                if (_bed.transform != null && _bed.transform.gameObject != null) return true;
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, Logging))
                {
                    Debug.Log("Error: There's a Block Entity without a Transform!");
                    Debug.Log($"Block Position: {_bed.pos} : BlockValue: {_bed.blockValue.Block.ToString()}");
                }

                return false;

            }
        }
    }
}