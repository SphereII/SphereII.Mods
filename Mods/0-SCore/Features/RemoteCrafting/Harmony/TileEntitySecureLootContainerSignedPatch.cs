using HarmonyLib;

namespace SCore.Features.RemoteCrafting.Harmony
{
    /// <summary>
    /// Patches TEFeatureSignable to handle blocks that have no TextMesh in their prefab.
    /// In that case SetBlockEntityData returns early and leaves smartTextMesh null/empty,
    /// which causes CanRenderString to throw a NullReferenceException when the sign window closes.
    /// </summary>
    [HarmonyPatch(typeof(TEFeatureSignable), nameof(TEFeatureSignable.CanRenderString))]
    public class TileEntitySecureLootContainerSigned_CanRenderString
    {
        public static bool Prefix(TEFeatureSignable __instance, ref bool __result)
        {
            if (__instance.smartTextMesh == null || __instance.smartTextMesh.Length == 0)
            {
                __result = false;
                return false; // skip original method
            }
            return true; // smartTextMesh is set — let the original run
        }
    }
}
