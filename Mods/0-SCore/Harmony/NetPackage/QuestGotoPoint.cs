//using HarmonyLib;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Harmony.NetPackage
//{
//    [HarmonyPatch(typeof(NetPackageQuestGotoPoint))]
//    [HarmonyPatch("ProcessPackage")]
//    public class QuestGotoPointProcessPackage
//    {
//        public static PrefabInstance FindClosesPrefabs(Vector3 position, List<PrefabInstance> prefabs, List<Vector2> usedPOILocations)
//        {
//            PrefabInstance prefab = null;
//            float minDist = Mathf.Infinity;
//            foreach (var t in prefabs)
//            {
//                // Have we already went to this one?
//                Vector2 vector = new Vector2((float)t.boundingBoxPosition.x, (float)t.boundingBoxPosition.z);
//                if (usedPOILocations != null && usedPOILocations.Contains(vector))
//                    continue;

//                float dist = Vector3.Distance(t.boundingBoxPosition, position);
//                if (dist < minDist)
//                {
//                    prefab = t;
//                    minDist = dist;
//                }
//            }

//            return prefab;
//        }
//        public static bool Prefix(NetPackageQuestGotoPoint __instance, World _world, string ___biomeFilter, int ___playerId, int ___questCode, QuestTags ___questTags, byte ___difficulty,
//            Vector2 ___position, Vector3 ___size)
//        {
//            if (_world == null)
//                return true;

//            // Only check if the biomeFilter is being used as a POI name
//            if (string.IsNullOrEmpty(___biomeFilter))
//                return true;

//            var strPOIname = ___biomeFilter;
//            var listOfPrefabs = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs().FindAll(instance => instance.name.Contains(strPOIname));
//            if (listOfPrefabs == null) return true;

//            EntityPlayer entityAlive = GameManager.Instance.World.GetEntity(___playerId) as EntityPlayer;

//            // Find the closes Prefab
//            var prefabInstance = FindClosesPrefabs(entityAlive.position, listOfPrefabs, new List<Vector2>());
//            if (prefabInstance == null) return true;

//            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
//            {
//                for (var i = 0; i < 5; i++)
//                {
//                    if (__instance.GotoType != NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI) continue;

//                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
//                        NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(___playerId, ___questTags, ___questCode, __instance.GotoType, ___difficulty,
//                            prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, prefabInstance.boundingBoxSize.x, prefabInstance.boundingBoxSize.y,
//                            prefabInstance.boundingBoxSize.z, -1f, BiomeFilterTypes.AnyBiome, ___biomeFilter), false, ___playerId);
//                    return false;
//                }

//                return true;
//            }

//            EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
//            var quest = primaryPlayer.QuestJournal.FindActiveQuest(___questCode);
//            if (quest == null) return true;
//            foreach (var t in quest.Objectives)
//            {
//                if (t is ObjectiveRandomPOIGoto && __instance.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI)
//                    ((ObjectiveRandomPOIGoto)t).FinalizePoint(new Vector3(___position.x, primaryPlayer.position.y, ___position.y), ___size);
//            }
//            return true;
//        }
//    }
//}