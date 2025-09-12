// using HarmonyLib;
// using UnityEngine;
//
// namespace SCore.Features.PassiveEffectHooks.Harmony
// {
//     [HarmonyPatch(typeof(EntityVehicle))]
//     [HarmonyPatch(nameof(EntityVehicle.PostInit))]
//     public class EntityVehiclePostInit
//     {
//         public static void Postfix(EntityVehicle __instance)
//         {
//             var minEventParams = new MinEventParams {
//                 ItemValue = __instance.vehicle.itemValue,
//                 Self = __instance
//             };
//
//             minEventParams.Self.MinEventContext = minEventParams;
//             minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfVehiclePostInit);
//         }
//     }
// }