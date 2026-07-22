using System.Threading;
using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    // RegionFileV2.OptimizeLayout() compacts a region file in place (truncate + rewrite) without
    // taking the instance lock that ReadData()/WriteData() hold. A chunk load on the generation
    // thread that overlaps a compaction on the save thread reads garbage ("Wrong chunk header!",
    // "EXCEPTION: In load chunk"), and the failed load then deletes that chunk from the region
    // file. Holding the instance lock for the whole compaction makes it mutually exclusive with
    // the locked reads and writes.
    public class RegionFileOptimizeLayoutLock {
        [HarmonyPatch(typeof(RegionFileV2))]
        [HarmonyPatch(nameof(RegionFileV2.OptimizeLayout))]
        public class RegionFileV2OptimizeLayout {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "RegionFileOptimizeLayoutLock";
            private static readonly string Logging = "LogRegionFileOptimizeLayoutLock";

            public static void Prefix(RegionFileV2 __instance, ref bool __state) {
                __state = false;
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                if (!Monitor.TryEnter(__instance)) {
                    // Contention here is exactly the race this patch exists to close.
                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, Logging))
                        Log.Out("RegionFileOptimizeLayoutLock: OptimizeLayout waiting on a concurrent read/write of the same region file.");
                    Monitor.Enter(__instance);
                }

                __state = true;
            }

            public static void Finalizer(RegionFileV2 __instance, bool __state) {
                if (__state) Monitor.Exit(__instance);
            }
        }
    }
}