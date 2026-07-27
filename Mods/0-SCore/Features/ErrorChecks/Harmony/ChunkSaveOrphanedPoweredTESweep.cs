using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ErrorChecks.Harmony {
    // Companion to FixOrphanedPoweredTileEntities (the load-side guard). A powered tile entity
    // can be left on a non-powered block - most often a spawn cube whose block is overwritten
    // by a later decoration pass during world generation, so the chunk is written corrupt
    // before it is ever loaded. This prefix on Chunk.save drops those orphans from the tile
    // entity list BEFORE serialization, so the corrupt pair never reaches disk and never
    // propagates to clients. The load guard still heals any orphan already on disk; this stops
    // new ones being written.
    //
    // The removal is a plain collection drop (not RemoveTileEntityAt), so it runs no world
    // side effects and is safe on the save thread: a generation-time orphan has not yet had
    // InitializePowerData run, so it owns no PowerItem or PowerManager node to unwind.
    public class ChunkSaveOrphanedPoweredTESweep {
        [HarmonyPatch(typeof(Chunk))]
        [HarmonyPatch(nameof(Chunk.save))]
        public class ChunkSave {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "FixOrphanedPoweredTileEntities";

            public static void Prefix(Chunk __instance) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                var tileEntities = __instance.GetTileEntities();
                if (tileEntities == null || tileEntities.Count == 0) return;

                List<Vector3i> orphans = null;
                var list = tileEntities.list;
                for (int i = 0; i < list.Count; i++) {
                    if (!(list[i] is TileEntityPowered te)) continue;
                    if (!OrphanedPoweredTileEntity.IsOrphaned(__instance.GetBlock(te.localChunkPos).Block)) continue;

                    (orphans ?? (orphans = new List<Vector3i>())).Add(te.localChunkPos);
                }

                if (orphans == null) return;

                foreach (var pos in orphans) {
                    tileEntities.Remove(pos);
                    Log.Warning(
                        $"FixOrphanedPoweredTileEntities: dropped an orphaned powered tile entity at chunk-local {pos} " +
                        $"in chunk {__instance.X},{__instance.Z} before save (block owns no tile entity); would have corrupted the chunk on load.");
                }
            }
        }
    }
}
