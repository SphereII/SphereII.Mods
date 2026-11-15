using HarmonyLib;

namespace SCore.Features.QuestPOIVariants.Harmony
{
    /// <summary>
    /// XUiC_Location (the HUD's location name) only rereads the POI's prefab when the player moves into a
    /// different POI. When a POI is swapped to a variant while the player stands in it, forget the cached
    /// POI so the name, tier and tags update straight away instead of after leaving and coming back.
    /// </summary>
    [HarmonyPatch(typeof(XUiC_Location))]
    [HarmonyPatch(nameof(XUiC_Location.Update))]
    public class XUiCLocationVariantRefreshPatch
    {
        private static void Prefix(XUiC_Location __instance)
        {
            var prefabInstance = __instance.lastPrefabInstance;
            if (prefabInstance != null && prefabInstance.prefab != __instance.lastPrefab)
                __instance.lastPrefabInstance = null;
        }
    }
}
