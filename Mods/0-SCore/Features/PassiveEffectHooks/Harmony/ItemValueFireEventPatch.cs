// using HarmonyLib;
// using SCore.Features.ItemDegradation.Harmony;
// using SCore.Features.ItemDegradation.Utils;
// using UnityEngine;
//
// namespace SCore.Features.PassiveEffectHooks.Harmony
// {
//     [HarmonyPatch(typeof(ItemValue))]
//     [HarmonyPatch(nameof(ItemValue.FireEvent))]
//     public class ItemValueFireEventPatch
//     {
//     
//         // ItemValue's FireEvent doesn't cascade
//         public static void Postfix(ItemValue __instance, MinEventTypes _eventType, MinEventParams _eventParms)
//         {
//             var itemClass = __instance.ItemClass;
//             
//             // Non-ItemClassModifiers already go through all this. ItemClassModifiers do not though.
//             if (itemClass is not ItemClassModifier modifier) return;
//             
//             if (!ItemDegradationHelpers.CanDegrade(__instance)) return;
//             
//             itemClass?.FireEvent(_eventType, _eventParms);
//             foreach (var t in __instance.CosmeticMods)
//             {
//                 if ( t == null || t.IsEmpty()) continue;
//                 t.FireEvent(_eventType, _eventParms);
//             }
//             _eventParms.Self = GameManager.Instance.World.GetPrimaryPlayer();
//             var originalItem = _eventParms.ItemValue;
//             foreach (var mod in __instance.Modifications)
//             {
//                 if ( mod == null || mod.IsEmpty()) continue;
//                 _eventParms.ItemValue = mod;
//                 mod.FireEvent(_eventType, _eventParms);
//             }
//
//             _eventParms.ItemValue = originalItem;
//
//         }
//     }
// }