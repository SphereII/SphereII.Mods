using System.Collections.Generic;
using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    // Block.SpawnFX assumes its effect name is two comma-separated tokens and indexes the second
    // without checking:
    //
    //     string[] array = _fxName.Split(',');
    //     ... new ParticleEffect(array[0], ..., array[1], ...)
    //
    // Every vanilla DestroyFX value is "particle,sound", so vanilla never trips this. A modded block
    // that sets DestroyFX to a bare particle name throws IndexOutOfRangeException instead, and the
    // exception names neither the block nor the value.
    //
    // The damage is worse than a missing particle. Block.OnBlockDamaged calls SpawnDestroyFX on the
    // final hit and only then clears the block:
    //
    //     QuestEventManager.Current.BlockDestroyed(...);
    //     SpawnDestroyFX(...);                          <- throws
    //     _world.SetBlockRPC(_bvRef, BlockValue.Air);   <- never reached
    //
    // So the block absorbs the killing blow, fires its destroyed quest event, and is then never
    // removed. It reads to the player as an indestructible block that throws an error on every hit.
    //
    // This skips the malformed effect and lets destruction finish, naming the block and the value
    // once per bad string so it can actually be fixed in XML.
    public class BlockSpawnFXMalformedName {
        private static readonly string AdvFeatureClass = "ErrorHandling";
        private static readonly string Feature = "FixMalformedDestroyFX";

        private static readonly HashSet<string> Reported = new HashSet<string>();
        private static readonly object ReportLock = new object();

        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch(nameof(Block.SpawnFX))]
        public class BlockSpawnFX {
            public static bool Prefix(WorldBase _world, BlockValueRef _bvRef, string _fxName) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

                // Vanilla only needs a second token to exist. An empty one is legal and is left alone.
                if (!string.IsNullOrEmpty(_fxName) && _fxName.IndexOf(',') >= 0) return true;

                ReportOnce(_world, _bvRef, _fxName);

                // Skip the effect so OnBlockDamaged reaches SetBlockRPC and the block is removed.
                return false;
            }
        }

        private static void ReportOnce(WorldBase _world, BlockValueRef _bvRef, string _fxName) {
            var key = _fxName ?? "<null>";

            lock (ReportLock) {
                if (!Reported.Add(key)) return;
            }

            Log.Warning(
                $"[SCore] Malformed block FX name \"{key}\" on {DescribeBlock(_world, _bvRef)}. " +
                "Block.SpawnFX expects \"particleName,soundName\" and indexes the second token " +
                "unconditionally, so a single-token value throws and the block is never cleared to " +
                "air. Skipping the effect so the block can be destroyed. Fix the DestroyFX property " +
                "in XML - every vanilla value is a particle and a sound, e.g. " +
                "\"blockdestroy_wood,trapWoodDestroy\".");
        }

        private static string DescribeBlock(WorldBase _world, BlockValueRef _bvRef) {
            try {
                if (!_bvRef.TryGetBlockPos(out var pos)) return "an unknown block (no block position)";

                var blockValue = _world.GetBlock(pos);
                var block = Block.list[blockValue.type];
                return block == null
                    ? $"block type {blockValue.type} at {pos}"
                    : $"block '{block.GetBlockName()}' at {pos}";
            }
            catch {
                return "an unknown block";
            }
        }
    }
}
