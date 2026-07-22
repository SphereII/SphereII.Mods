using System;
using System.Reflection;
using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    // MapRendering.MapRenderer.RenderFullMap (the web panel / "rendermap" command) opens a
    // SECOND RegionFileManager over the live save's region files. All region file locking is
    // per-instance and every file open is FileShare.ReadWrite, so its reads tear against the
    // live manager's writes, its failed loads delete chunks from the live save, and its
    // cleanup compacts region files the live manager has cached sector tables for. Warning
    // only - admins may accept the risk on an idle server.
    //
    // MapRenderer's dependencies live in Webserver.dll, which only ships with dedicated
    // servers, so this patch resolves its target lazily and skips itself (Prepare = false)
    // when the assembly is absent - a hard [HarmonyPatch(typeof(...))] would break patching
    // on clients.
    public class MapRendererRenderFullMapWarning {
        [HarmonyPatch]
        public class MapRendererRenderFullMap {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "WarnRenderMapLiveServer";

            public static bool Prepare() {
                try {
                    Assembly.Load("Webserver");
                    var type = AccessTools.TypeByName("MapRendering.MapRenderer");
                    return type != null && AccessTools.Method(type, "RenderFullMap") != null;
                }
                catch (Exception) {
                    return false;
                }
            }

            public static MethodBase TargetMethod() {
                return AccessTools.Method(AccessTools.TypeByName("MapRendering.MapRenderer"), "RenderFullMap");
            }

            public static void Prefix() {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                var world = GameManager.Instance != null ? GameManager.Instance.World : null;
                if (world == null) return;

                Log.Warning(
                    "rendermap: Rendering the full map opens a second region file reader over the LIVE save while the " +
                    "game is writing to it. This can tear chunk reads (\"EXCEPTION: In load chunk\") and cause chunks " +
                    "to be deleted and regenerated. Run rendermap only while no players are connected, or take a backup first.");
            }
        }
    }
}
