using System;
using HarmonyLib;

namespace SCore.Features.QuestPOIVariants.Harmony
{
    /// <summary>
    /// Before vanilla checks a canvas's sign, load the sign's prefab library if it isn't registered yet.
    /// This covers POIs swapped to a variant prefab before a restart: the variant's _signs.xml is
    /// otherwise never loaded, so vanilla logs "has no data but is not recorded as deleted" and the
    /// sign stays blank. See PrefabSignLibraryLoader.
    /// </summary>
    [HarmonyPatch(typeof(TEFeatureCanvas))]
    [HarmonyPatch(nameof(TEFeatureCanvas.OnLoad))]
    public class TEFeatureCanvasOnLoadPatch
    {
        private static void Prefix(TEFeatureCanvas __instance)
        {
            try
            {
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

                var state = __instance.Canvas != null ? __instance.Canvas.State : __instance.pendingCanvasState;
                if (state == null) return;

                var signId = state.SignId;
                if (!signId.IsValid || SignDataManager.Instance.TryGetSignData(signId, out _)) return;

                PrefabSignLibraryLoader.EnsureLibrary(signId.libraryId);
            }
            catch (Exception ex)
            {
                // Never block the vanilla load.
                Log.Error($"[SCore] QuestPOIVariants: failed to load sign library for canvas: {ex}");
            }
        }
    }
}
