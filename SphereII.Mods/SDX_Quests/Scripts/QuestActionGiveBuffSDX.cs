using System;

public class QuestActionGiveBuffSDX : BaseQuestAction
{
    public override void SetupAction()
    {
    }

    public override void PerformAction()
    {
        // Check if its a player entity.
        EntityAlive myEntity = null;
        if (OwnerQuest.OwnerJournal.OwnerPlayer != null)
            myEntity = OwnerQuest.OwnerJournal.OwnerPlayer as EntityAlive;

        // If it's not a player entity, check for the SharedOwnerID
        if (myEntity == null)
        {
            // make sure that the entity is in the world before polling it.
            if (GameManager.Instance.World.Entities.dict.ContainsKey(OwnerQuest.SharedOwnerID))
                myEntity = GameManager.Instance.World.Entities.dict[OwnerQuest.SharedOwnerID] as EntityAlive;
        }

        if (myEntity == null)
            return;

        myEntity.Buffs.AddBuff(base.Value, -1, true);
    }

    public override BaseQuestAction Clone()
    {
        QuestActionGiveBuffSDX questActionShowTip = new QuestActionGiveBuffSDX();
        base.CopyValues(questActionShowTip);
        return questActionShowTip;
    }
}
