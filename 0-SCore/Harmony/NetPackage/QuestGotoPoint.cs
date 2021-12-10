using HarmonyLib;
using UnityEngine;

namespace Harmony.NetPackage
{
    [HarmonyPatch(typeof(NetPackageQuestGotoPoint))]
    [HarmonyPatch("ProcessPackage")]
    public class QuestGotoPointProcessPackage
    {
        public static bool Prefix(NetPackageQuestGotoPoint __instance, World _world, string ___biomeFilter, int ___playerId, int ___questCode, QuestTags ___questTags, byte ___difficulty,
            Vector2 ___position, Vector3 ___size)
        {
            if (_world == null)
                return true;

            // Only check if the biomeFilter is being used as a POI name
            if (string.IsNullOrEmpty(___biomeFilter))
                return true;

            var prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs().Find(instance => instance.name.Contains(___biomeFilter));
            if (prefabInstance == null)
                return true;

            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                for (var i = 0; i < 5; i++)
                {
                    if (__instance.GotoType != NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI) continue;

                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                        NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(___playerId, ___questTags, ___questCode, __instance.GotoType, ___difficulty,
                            prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, prefabInstance.boundingBoxSize.x, prefabInstance.boundingBoxSize.y,
                            prefabInstance.boundingBoxSize.z, -1f, BiomeFilterTypes.AnyBiome, ___biomeFilter), false, ___playerId);
                    return false;
                }

                return true;
            }

            EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            var quest = primaryPlayer.QuestJournal.FindActiveQuest(___questCode);
            if (quest == null) return true;
            foreach (var t in quest.Objectives)
                if (t is ObjectiveRandomPOIGoto && __instance.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI)
                    ((ObjectiveRandomPOIGoto)t).FinalizePoint(new Vector3(___position.x, primaryPlayer.position.y, ___position.y), ___size);

            return true;
        }
    }
}