// using HarmonyLib;
// using UnityEngine;
// using WorldGenerationEngineFinal;
//
//     public class WorldGenerationEngineFinalPathPatch {
//         private const string AdvFeatureClass = "CaveConfiguration";
//         private const string Feature = "CaveStampGenerator";
//
//         [HarmonyPatch(typeof(Path))]
//         [HarmonyPatch(MethodType.Constructor)]
//         [HarmonyPatch(new[] { typeof(Vector2i), typeof(Vector2i), typeof(int), typeof(bool), typeof(bool) })]
//         public class PathConstructor01 {
//             public static void Postfix(ref Path __instance) {
//                 Debug.Log("Adjusting Path: 01");
//                 __instance.radius = 4;
//                 __instance.lanes = 1;
//
//             }
//         }
//
//         [HarmonyPatch(typeof(Path))]
//         [HarmonyPatch(MethodType.Constructor)]
//         [HarmonyPatch(new[] { typeof(Vector2i), typeof(Vector2i), typeof(float), typeof(bool), typeof(bool) })]
//         public class PathConstructor02 {
//             public static void Postfix(ref Path __instance) {
//                 Debug.Log("Adjusting Path: 02");
//                 __instance.radius = 4;
//                 __instance.lanes = 1;
//             }
//         }
//
//         [HarmonyPatch(typeof(Path))]
//         [HarmonyPatch(MethodType.Constructor)]
//         [HarmonyPatch(new[] { typeof(bool), typeof(float), typeof(bool) })]
//         public class PathConstructor03 {
//             
//             public static void Postfix(ref Path __instance) {
//                 Debug.Log("Adjusting Path: 03");
//                 __instance.radius = 4;
//                 __instance.lanes = 1;
//             }
//         }
//         
//         [HarmonyPatch(typeof(Path))]
//         [HarmonyPatch("DrawPathToRoadIds")]
//         public class PathDrawPathToRoadIDs {
//             public static void Postfix(ref Path __instance) {
//                 Debug.Log($"Radius: {__instance.radius}  Lanes: {__instance.lanes}");
//              
//             }
//         }
//     }
