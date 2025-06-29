using HarmonyLib;
using UnityEngine;

namespace Features.Fire.Harmony {
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch(nameof(Chunk.SetBlock))]
    public class ChunkSetBlock {
        public static void Postfix(Chunk __instance, int ___m_X, int ___m_Z, int x, int y, int z, bool _fromReset) {
            if (!_fromReset) return;
            if (FireManager.Instance == null) return ;
            if (FireManager.Instance.Enabled == false) return ;
            // If the POI is being reset, clear the fire.
            var vector3I = new Vector3i((___m_X << 4) + x, y, (___m_Z << 4) + z);
           // FireManager.Instance?.ExtinguishFire(vector3I, -1, false);
           FireManager.Instance?.ClearFire(vector3I);
        }
    }
}