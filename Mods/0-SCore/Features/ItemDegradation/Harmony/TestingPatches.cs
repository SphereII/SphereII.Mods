// using HarmonyLib;
// using UnityEngine;
//
// namespace SCore.Features.ItemDegradation.Harmony
// {
//     [HarmonyPatch(typeof(MinEventActionBase))]
//     [HarmonyPatch(nameof(MinEventActionBase.CanExecute))]
//     public class TestingPatches
//     {
//         public static void Postfix(bool __result, MinEventActionBase __instance, MinEventTypes _eventType, MinEventParams _params)
//         {
//             if (_eventType != MinEventTypes.onSelfItemActivate) return;
//             Debug.Log("OnSelfItemActivate:");
//             Debug.Log($" CanExecuteBase(): {__result}");
//             if (__instance.Requirements.Count == 0)
//             {
//                 Debug.Log("\tNo Requirements");
//                 return;
//             }
//
//             var flag = true;
//             Debug.Log($"Requirement Count: {__instance.Requirements.Count}");
//             for (int j = 0; j < __instance.Requirements.Count; j++)
//             {
//                 flag = __instance.Requirements[j].IsValid(_params);
//                 Debug.Log($"Requirement: {__instance.Requirements[j].ToString()} :: {flag}");
//                 if (flag)
//                 {
//                     Debug.Log("Breaking.");
// //                    break;
//                 }
//             }
//         }
//     }
// }
