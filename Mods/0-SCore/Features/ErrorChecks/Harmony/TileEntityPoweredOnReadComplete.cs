using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    // A powered tile entity can outlive its block: POI decoration overwrites blocks raw (no
    // OnBlockRemoved fires), so a trigger TE created during prefab placement - spawn cubes
    // extending BlockMotionSensor are the common case - can be saved at a position whose block
    // is no longer powered (air, or even terrain). On the next load, Chunk.read calls
    // TileEntityPowered.OnReadComplete, which runs:
    //     base.OnReadComplete();      // empty
    //     InitializePowerData();      // leaves PowerItem null when the block can't back one
    //     CheckForNewWires();         // dereferences the null PowerItem -> throws
    // Either throw propagates out of Chunk.read and ChunkSnapshotUtil.LoadChunk DELETES the
    // whole chunk (POI, player builds and NPCs included).
    //
    // Guarding OnReadComplete (not just InitializePowerData) is required: skipping only the init
    // leaves PowerItem null and CheckForNewWires still NREs. base.OnReadComplete is empty, so
    // skipping the whole method for an orphan loses nothing. The orphan is removed on the main
    // thread so it is not saved again; a null PowerItem is a state vanilla already tolerates
    // (client-side powered TEs never have one).
    public class TileEntityPoweredOnReadComplete {
        [HarmonyPatch(typeof(TileEntityPowered))]
        [HarmonyPatch(nameof(TileEntityPowered.OnReadComplete))]
        public class TileEntityPoweredOnReadCompletePatch {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "FixOrphanedPoweredTileEntities";

            public static bool Prefix(TileEntityPowered __instance) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

                var chunk = __instance.chunk;
                if (chunk == null) return true;

                // Same block source the vanilla cast reads; the chunk's block data is fully
                // populated before its tile entities are read.
                var block = chunk.GetBlock(__instance.localChunkPos).Block;
                if (!OrphanedPoweredTileEntity.IsOrphaned(block)) return true;

                var worldPos = __instance.ToWorldPos();
                Log.Warning(
                    $"FixOrphanedPoweredTileEntities: {__instance.GetType().Name} at {worldPos} sits on " +
                    $"block '{(block != null ? block.GetBlockName() : "null")}', which owns no tile entity; skipping " +
                    "power init and removing the orphaned tile entity (vanilla would have deleted the whole chunk).");

                ThreadManager.AddSingleTaskMainThread("SCore.RemoveOrphanedPoweredTE", delegate {
                    try {
                        var world = GameManager.Instance != null ? GameManager.Instance.World : null;
                        if (world == null) return;

                        // Re-check before removing: only clean up if the position is still orphaned.
                        if (world.GetChunkFromWorldPos(worldPos) is Chunk liveChunk &&
                            OrphanedPoweredTileEntity.IsOrphaned(world.GetBlock(worldPos).Block))
                            liveChunk.RemoveTileEntityAt<TileEntityPowered>(world, World.toBlock(worldPos));
                    }
                    catch (System.Exception ex) {
                        Log.Warning("FixOrphanedPoweredTileEntities: orphan cleanup failed: " + ex.Message);
                    }
                });

                return false;
            }
        }
    }
}
