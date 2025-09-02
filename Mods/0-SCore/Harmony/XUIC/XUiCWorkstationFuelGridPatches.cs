// using HarmonyLib;
// using UnityEngine;
//
// namespace SCore.Harmony.XUIC
// {
//     [HarmonyPatch(typeof(XUiC_WorkstationFuelGrid))]
//     [HarmonyPatch(nameof(XUiC_WorkstationFuelGrid.AddItem))]
//     [HarmonyPatch(new[] { typeof(ItemClass), typeof(ItemStack) })]
//
//     public class XUiCWorkstationFueldGridInitPatches
//     {
//         public static bool Prefix(ref bool __result, XUiC_WorkstationFuelGrid __instance,ItemClass _itemClass, ItemStack _itemStack)
//         {
//             var xuiCItemStack = __instance.itemControllers[0];
//             if (!XUi.IsGameRunning() || xuiCItemStack == null || xuiCItemStack.ItemStack.IsEmpty()) return true;
//             var itemClass = ItemClass.list[xuiCItemStack.ItemStack.itemValue.type];
//             if (itemClass == null) return true;
//
//             var block = __instance.WorkstationData.TileEntity.blockValue.Block; 
//             var fuelItems = block.Properties.GetString("FuelItem");
//             if (string.IsNullOrEmpty(fuelItems)) return true;
//
//             foreach (var fullItem in fuelItems.Split(','))
//             {
//                 if (itemClass.GetItemName() == fullItem)
//                 {
//                     return true;
//                 }
//             }
//             
//             __result = false;
//             return false;
//         }
//     }
//     
// }