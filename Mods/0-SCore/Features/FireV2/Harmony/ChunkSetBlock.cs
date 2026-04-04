using HarmonyLib;
using UnityEngine;

namespace Features.Fire.Harmony {
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch(nameof(Chunk.SetBlock))]
    public class ChunkSetBlock {
        public static void Postfix(Chunk __instance, int ___m_X, int ___m_Z, int x, int y, int z, bool _fromReset) {
            if (FireManager.Instance == null) return;
            if (!FireManager.Instance.Enabled) return;

            var vector3I = new Vector3i((___m_X << 4) + x, y, (___m_Z << 4) + z);

            if (_fromReset)
            {
                // POI reset — always clear, regardless of current burn state.
                FireManager.Instance.ClearFire(vector3I);
                return;
            }

            // Non-reset block change (player destruction, explosion, block replacement, etc.).
            // If this position was burning and the block is now air, clear the fire immediately.
            // Without this, the fire particle lingers until the next UpdateFires cycle
            // (up to CheckInterval seconds) because ProcessSingleFire is the only other
            // place that detects a block-gone-air condition.
            if (!FireManager.Instance.IsBurning(vector3I)) return;

            var newBlock = GameManager.Instance.World.GetBlock(vector3I);
            if (newBlock.isair)
                FireManager.Instance.ClearFire(vector3I);
        }
    }
}