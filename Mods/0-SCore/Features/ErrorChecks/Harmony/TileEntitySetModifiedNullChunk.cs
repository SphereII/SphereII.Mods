using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    // Loading a prefab that contains a legacy-format sign crashes with a NullReferenceException:
    // Prefab.readTileEntities -> TileEntityLegacyUtils.ReadLegacySignIntoComposite builds a
    // chunkless TileEntityComposite and immediately calls SetOwner -> SetModified -> setModified,
    // which (on a host) builds a NetPackageTileEntity. That reads TileEntity.blockValue, a
    // computed property = this.chunk.GetBlock(...), and the chunk is null during a prefab read.
    // Vanilla forgets to set bDisableModifiedCheck on this migration path. The whole prefab's
    // active block data is then skipped, so its signs / containers never load.
    //
    // A tile entity with no chunk only exists mid-construction during a prefab/stream read - it
    // has no chunk-scoped context to broadcast - so skipping setModified in that state is safe
    // (SetChunkModified already null-guards chunk) and lets legacy prefabs load their tile
    // entities instead of dropping them.
    public class TileEntitySetModifiedNullChunk {
        [HarmonyPatch(typeof(TileEntity))]
        [HarmonyPatch("setModified")]
        public class TileEntitySetModified {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "FixLegacyTileEntityNullChunk";

            public static bool Prefix(TileEntity __instance) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

                // No chunk means we're mid-read (e.g. legacy prefab migration); skip the
                // broadcast that would dereference the missing chunk.
                return __instance.chunk != null;
            }
        }
    }
}
