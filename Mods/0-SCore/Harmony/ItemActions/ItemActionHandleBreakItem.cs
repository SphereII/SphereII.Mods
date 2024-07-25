// using UnityEngine;
// using HarmonyLib;
//
// namespace SCore.Harmony.ItemActions
// {
//     public class ItemActionHandleBreakItem
//     {
//         [HarmonyPatch(typeof(ItemAction))]
//         [HarmonyPatch("HandleItemBreak")]
//         public class ItemActionHit
//         {
//             public static void Postfix(ItemActionData _actionData)
//             {
//                 var item = _actionData.invData.item;
//                 if (!item.Properties.Contains("DowngradeItem")) return;
//                 var newItemClass = item.Properties.Values["DowngradeItem"];
//                 var newItem = ItemClass.GetItem(newItemClass);
//                 Debug.Log($"New Item: {newItemClass}");
//                 _actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
//                 var holdingIndex = _actionData.invData.holdingEntity.inventory.holdingItemIdx;
//                 _actionData.invData.holdingEntity.inventory.SetItem(holdingIndex, newItem, 1, true);
//
//
//             }
//         }
//     }
// }
