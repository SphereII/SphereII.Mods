using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace SCore.Harmony.Quests
{
    [HarmonyPatch(typeof(QuestEventManager))]
    [HarmonyPatch(nameof(QuestEventManager.GetQuestList))]
    public class QuestEventManagerGetQuestList
    {
        private static bool Prefix(ref List<Quest> __result, QuestEventManager __instance, World world, int npcEntityID, int playerEntityID)
        {
            if (!__instance.npcQuestData.TryGetValue(npcEntityID, out var npcquestData)) return true;
            if (!npcquestData.PlayerQuestList.TryGetValue(playerEntityID, out var playerQuestData)) return true;

            if (QuestEventManager.Current.CheckResetQuestTrader(playerEntityID, npcEntityID))
            {
                playerQuestData.QuestList?.Clear();
                playerQuestData.QuestList = null;
                QuestEventManager.Current.ClearTraderResetQuestsForPlayer(playerEntityID);
            }
            else if ((int)(world.GetWorldTime() - playerQuestData.LastUpdate) > 24000)
            {
                playerQuestData.QuestList?.Clear();
                playerQuestData.QuestList = null;
            }
            __result= playerQuestData.QuestList;
            return false;
        }

    }
}
