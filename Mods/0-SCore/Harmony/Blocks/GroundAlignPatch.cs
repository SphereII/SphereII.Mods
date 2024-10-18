using HarmonyLib;

namespace SCore.Harmony.Blocks {
    public class GroundAlignPatch {
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("GroundAlign")]
        public class Block_GroundAlign {
            public static bool Prefix(BlockEntityData _data) {
                if (_data == null) return false;
                return _data.transform != null && _data.transform.gameObject != null;
            }
        }
    }
}