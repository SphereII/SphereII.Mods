using System;

class RewardQuestSDX : RewardQuest
{

    public override BaseReward Clone()
    {
        RewardQuestSDX rewardQuest = new RewardQuestSDX();
        base.CopyValues(rewardQuest);
        rewardQuest.IsChainQuest = this.IsChainQuest;
        return rewardQuest;
    }

    internal static QuestClass GetQuest(string questID)
    {
        if (!QuestClass.s_Quests.ContainsKey(questID))
        {
            return null;
        }
        return QuestClass.s_Quests[questID];
    }


    public override void GiveReward(EntityPlayer player)
    {
        Quest quest = QuestClass.CreateQuest(base.ID);
        if (base.OwnerQuest != null && quest != null )
        {
            QuestClass newQuest = GetQuest(base.OwnerQuest.ID);
            if( newQuest != null  )
                quest.PreviousQuest = newQuest.Name;
        }

        if (GameManager.Instance.World.Entities.dict.ContainsKey(base.OwnerQuest.SharedOwnerID))
        {
            EntityAliveSDX questEntity = GameManager.Instance.World.Entities.dict[base.OwnerQuest.SharedOwnerID] as EntityAliveSDX;
            if (questEntity == null)
                return;
            questEntity.QuestJournal.AddQuest(quest);


        }
    }

}

