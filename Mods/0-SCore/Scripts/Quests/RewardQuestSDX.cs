internal class RewardQuestSDX : RewardQuest
{
    public override BaseReward Clone()
    {
        var rewardQuest = new RewardQuestSDX();
        CopyValues(rewardQuest);
        rewardQuest.IsChainQuest = IsChainQuest;
        return rewardQuest;
    }

    internal static QuestClass GetQuest(string questID)
    {
        if (!QuestClass.s_Quests.ContainsKey(questID)) return null;
        return QuestClass.s_Quests[questID];
    }


    public override void GiveReward(EntityPlayer player)
    {
        var quest = QuestClass.CreateQuest(ID);
        if (OwnerQuest != null && quest != null)
        {
            var newQuest = GetQuest(OwnerQuest.ID);
            if (newQuest != null)
                quest.PreviousQuest = newQuest.Name;
        }

        if (GameManager.Instance.World.Entities.dict.ContainsKey(OwnerQuest.SharedOwnerID))
        {
            var questEntity = GameManager.Instance.World.Entities.dict[OwnerQuest.SharedOwnerID] as EntityAliveSDX;
            if (questEntity == null)
                return;
            questEntity.questJournal.AddQuest(quest);
        }
    }
}