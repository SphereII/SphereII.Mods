using System.Threading;
using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    // RegionFileManager's chunk-load path shares one RegionFileChunkReader - a single read
    // buffer, cached DeflateInputStream and load MemoryStream - across every caller with no
    // locking. Chunk loads normally come from the generation thread, but chunk resets
    // (regionreset / worldchunkreset console commands, ActionResetRegions game events) load
    // chunks from the MAIN thread through the same reader. Two threads in LoadChunk at once
    // tear the shared buffers mid-decompression ("invalid distance too far back",
    // "Wrong chunk header!"), and the failed load then DELETES the chunk from the region file.
    // The lock must span all of LoadChunk, not just the read: the shared load stream is still
    // being parsed by chunk.load after readIntoLoadStream returns.
    public class ChunkSnapshotUtilLoadChunkLock {
        [HarmonyPatch(typeof(ChunkSnapshotUtil))]
        [HarmonyPatch(nameof(ChunkSnapshotUtil.LoadChunk))]
        public class ChunkSnapshotUtilLoadChunk {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "RegionFileLoadChunkLock";
            private static readonly string Logging = "LogRegionFileLoadChunkLock";

            public static void Prefix(ChunkSnapshotUtil __instance, ref bool __state) {
                __state = false;
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                if (!Monitor.TryEnter(__instance)) {
                    // Contention here is exactly the race this patch exists to close.
                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, Logging))
                        Log.Out("RegionFileLoadChunkLock: LoadChunk waiting on a concurrent chunk load from another thread.");
                    Monitor.Enter(__instance);
                }

                __state = true;
            }

            public static void Finalizer(ChunkSnapshotUtil __instance, bool __state) {
                if (__state) Monitor.Exit(__instance);
            }
        }
    }
}
