//using HarmonyLib;
//using System;
//using UnityEngine;

//namespace Harmony.PrefabFeatures
//{
//    /**
//     * Adds all prefabs placed in the world to the dynamic prefab decorator
//     * 
//     * This class includes a Harmony patches to disable the landclaim block on the trader, making them vulnerable.
//     */
//    public class DynamicPrefabDecorator
//    {
//        [HarmonyPatch(typeof(Prefab))]
//        [HarmonyPatch("CopyIntoLocal")]
//        public class PrefabCopyIntoLocal
//        {
//            public static void Postfix(Prefab __instance, Vector3i _destinationPos)
//            {
//                var prefabs =GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs().FindAll(instance => instance.name.Contains(__instance.PrefabName));
//                foreach (var prefab in prefabs)
//                    if (prefab.boundingBoxPosition == _destinationPos)
//                        return;



//                Log.Out($"PreFab::CopyIntoLocal(): Adding Prefab {__instance.PrefabName} to Dynamic decorator as it was not found.");
//                GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().CreateNewPrefabAndActivate(__instance.location, _destinationPos, __instance, false);
//            }
//        }
//    }
//}