using HarmonyLib;

namespace SCore.Features.RemoteCrafting.Harmony
{
    /// <summary>
    /// Patches TileEntitySecureLootContainerSigned to handle blocks that inherit from
    /// BlockSecureLootSigned but have no TextMesh in their prefab.  In that case
    /// SetBlockEntityData returns early and leaves smartTextMesh null, which causes
    /// CanRenderString to throw a NullReferenceException when the sign window closes.
    /// </summary>
    [HarmonyPatch(typeof(TileEntitySecureLootContainerSigned), nameof(TileEntitySecureLootContainerSigned.CanRenderString))]
    public class TileEntitySecureLootContainerSigned_CanRenderString
    {
        public static bool Prefix(TileEntitySecureLootContainerSigned __instance, ref bool __result)
        {
            if (__instance.smartTextMesh == null)
            {
                __result = false;
                return false; // skip original method
            }
            return true; // smartTextMesh is set — let the original run
        }
    }
}
