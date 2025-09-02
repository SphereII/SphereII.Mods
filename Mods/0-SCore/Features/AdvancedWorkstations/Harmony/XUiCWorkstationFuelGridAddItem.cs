using HarmonyLib;

namespace SCore.Features.AdvancedWorkstations.Harmony
{
    [HarmonyPatch(typeof(XUiC_WorkstationFuelGrid))]
    [HarmonyPatch(nameof(XUiC_WorkstationFuelGrid.AddItem))]
    [HarmonyPatch(new[] { typeof(ItemClass), typeof(ItemStack) })]
    public class XUiCWorkstationFueldGridInitPatches
    {
        public static bool Prefix(ref bool __result, XUiC_WorkstationFuelGrid __instance,ItemClass _itemClass, ItemStack _itemStack)
        {
            return __instance is not XUiC_WorkstationFuelGridSDX;
        }
    }
    
}