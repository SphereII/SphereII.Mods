// using HarmonyLib;
// using UnityEngine;
//
// namespace Harmony.ErrorHandling
// {
//     
//     public class EntityGroupsGetRandomFromGroup
//     {
//         [HarmonyPatch(typeof(EntityGroups))]
//         [HarmonyPatch(nameof(EntityGroups.GetRandomFromGroup))]
//         public class EntityGroupsGetRandom
//         {
//             private static void Postfix(int __result, string _sEntityGroupName)
//             {
//                 if (__result == 0) return;
//                 Log.Out($"EntityGroupsSpawning: {_sEntityGroupName} : Spawning ID: {__result}");
//                 
//             }
//         }
//     }
// }
