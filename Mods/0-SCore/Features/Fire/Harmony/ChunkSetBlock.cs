using HarmonyLib;
using UnityEngine;

namespace Features.Fire.Harmony {
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("SetBlock")]
    public class ChunkSetBlock {
        public static void Postfix(Chunk __instance, int ___m_X, int ___m_Z, int x, int y, int z, bool _fromReset) {
            if (!_fromReset) return;
            // If the POI is being reset, clear the fire.
            var vector3I = new Vector3i((___m_X << 4) + x, y, (___m_Z << 4) + z);
            FireManager.Instance?.RemoveFire(vector3I);
        }
    }
}