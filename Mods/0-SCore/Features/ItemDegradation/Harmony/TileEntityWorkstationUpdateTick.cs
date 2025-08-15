using HarmonyLib;

namespace SCore.Features.ItemDegradation.Harmony
{
    [HarmonyPatch(typeof(TileEntityWorkstation))]
    [HarmonyPatch(nameof(TileEntityWorkstation.UpdateTick))]
    public class TileEntityWorkstationUpdateTick
    {
        public static void Postfix(TileEntityWorkstation __instance, World world)
        {
            if (!__instance.IsBurning) return;
           
            foreach (var mod in __instance.Tools)
            {
                OnSelfItemDegrade.CheckForDegradation(mod);
            }
        }
    }
}