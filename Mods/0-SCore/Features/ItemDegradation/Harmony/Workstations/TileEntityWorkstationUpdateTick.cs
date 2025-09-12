using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.Workstations
{
    [HarmonyPatch(typeof(TileEntityWorkstation))]
    [HarmonyPatch(nameof(TileEntityWorkstation.UpdateTick))]
    public class TileEntityWorkstationUpdateTick
    {
        // Degrade the block over time.
        public static bool Prefix(TileEntityWorkstation __instance , out float __state)
        {
            __state = (GameTimer.Instance.ticks - __instance.lastTickTime) / 20f;
            
            return true;
        }
        public static void Postfix(TileEntityWorkstation __instance, World world, float __state)
        {
            if (!__instance.IsBurning) return;
            ItemDegradationHelpers.CheckBlockForDegradation(__instance.blockValue, __instance.ToWorldPos(), (int)__state);
        }
    }
}