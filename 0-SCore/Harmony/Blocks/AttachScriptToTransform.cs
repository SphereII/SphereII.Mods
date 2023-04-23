// using System;
// using System.Collections.Generic;
// using HarmonyLib;
// using UnityEngine;
//
// namespace Harmony.Blocks
// {
//     public class AttachScriptToTransform
//     {
//         public static void GetAllChildren(Transform parent, ref List <Transform> transforms)
//         {
//             if (parent == null) return;
//             foreach (Transform t in parent) 
//             {
//                 transforms.Add(t);
//                 GetAllChildren(t, ref transforms);
//             }
//         }
//
//         private static void CheckScripsToTransform(Block block, Vector3i blockPos, BlockValue blockValue,  BlockEntityData ebcd)
//         {
//             if (ebcd == null || ebcd.transform == null) return;
//             if (!block.Properties.Classes.ContainsKey("AttachScriptToTransform")) return;
//
//             var data = new Dictionary<string, string>();
//             var dyn = block.Properties.Classes["AttachScriptToTransform"];
//             foreach (var keyValuePair in dyn.Values.Dict.Dict)
//             {
//                 data.Add(keyValuePair.Key, keyValuePair.Value.ToString());
//             }
//
//             // No properties?
//             if (data.Count == 0) return;
//             
//             var childs = new List<Transform>();
//             GetAllChildren(ebcd.transform, ref childs);
//             if (childs.Count == 0)
//             {
//                 Debug.Log($"No Transforms: {block.GetBlockName()}");
//                 return;
//             }
//             
//             MeshRenderer[] componentsInChildren = ebcd.transform.GetComponentsInChildren<MeshRenderer>();
//             foreach( var component in componentsInChildren)
//             {
//                 Debug.Log($"Component: {component.name}");
//             }
//             foreach (var child in childs)
//             {
//                 Debug.Log($"Child: {child.name}");
//
//
//                 if (!data.ContainsKey(child.name)) continue;
//                 var script = data[child.name];
//                 var type = Type.GetType(script);
//                 if (type == null)
//                 {
//                     Debug.Log($"No Script: {script}");
//                     continue;
//                 }
//                 //Scripts already attached
//                 var component = child.gameObject.GetComponent(type);
//                 if (component != null) continue;
//                 child.gameObject.AddComponent(type);
//                 Debug.Log($"Adding {script} to {child.name} for {child.name}");
//             }
//
//         }
//         [HarmonyPatch(typeof(Block))]
//         [HarmonyPatch("OnBlockEntityTransformBeforeActivated")]
//         public class OnBlockLoaded
//         {
//             public static void Postfix(Block __instance, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue,  BlockEntityData _ebcd)
//             {
//                 CheckScripsToTransform(__instance, _blockPos, _blockValue, _ebcd);
//             }
//         }
//
//         [HarmonyPatch(typeof(Block))]
//         [HarmonyPatch("OnBlockAdded")]
//         public class OnBlockAdded
//         {
//             public static void Postfix(Block __instance, WorldBase world,Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
//             {
//                 if (!__instance.Properties.Classes.ContainsKey("AttachScriptToTransform")) return;
//                 var blockEntity = _chunk.GetBlockEntity(_blockPos);
//                 if (blockEntity == null || !blockEntity.bHasTransform)
//                 {
//                     _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
//                     {
//                         bNeedsTemperature = true
//                     });
//                 }
//              
//
//             }
//         }
//
//     }
//
// }