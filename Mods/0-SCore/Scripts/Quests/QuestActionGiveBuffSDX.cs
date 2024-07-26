using UnityEngine;

public class QuestActionGiveBuffSDX : BaseQuestAction
{
    public override void SetupAction()
    {
    }

    public override void PerformAction(Quest action) {
        action.OwnerJournal.OwnerPlayer.Buffs.AddBuff(Value);
    }

    public override BaseQuestAction Clone()
    {
        var questActionShowTip = new QuestActionGiveBuffSDX();
        CopyValues(questActionShowTip);
        return questActionShowTip;
    }
}