using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    // EntityCreationData.write stores the entity blob behind a ushort length prefix but writes
    // the full byte array: a blob over 65535 bytes saves a wrapped-around length, and the next
    // Chunk.load misparses the chunk's entity list and the whole chunk gets deleted. Detection
    // only - the record format is vanilla's and cannot be fixed from here, but this names the
    // offending entity in the log BEFORE the chunk dies, instead of leaving an unexplained
    // "EXCEPTION: In load chunk" on the next session.
    public class EntityCreationDataOversized {
        [HarmonyPatch(typeof(EntityCreationData))]
        [HarmonyPatch(nameof(EntityCreationData.write))]
        public class EntityCreationDataWrite {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "LogOversizedEntityData";

            public static void Prefix(EntityCreationData __instance) {
                if (__instance.entityData == null || __instance.entityData.Length <= ushort.MaxValue) return;
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                Log.Error(
                    $"EntityCreationData: entity data for class {__instance.entityClass} id {__instance.id} " +
                    $"('{__instance.entityName}') is {__instance.entityData.Length} bytes, over the {ushort.MaxValue} byte " +
                    "save limit. This record will corrupt its chunk's entity list on the next load.");
            }
        }
    }
}
