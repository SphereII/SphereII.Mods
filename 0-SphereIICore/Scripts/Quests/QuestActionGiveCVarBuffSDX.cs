using System;

public class QuestActionGiveCVarBuffSDX : BaseQuestAction
{
    public override void SetupAction()
    {
    }

    public override void PerformAction()
    {
        float value = StringParsers.ParseFloat(base.Value, 0, -1, System.Globalization.NumberStyles.Float);
        if (base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.HasCustomVar(base.ID))
            value += base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.GetCustomVar(base.ID);
        base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.SetCustomVar(base.ID, value, true);
    }

    public override BaseQuestAction Clone()
    {
        QuestActionGiveCVarBuffSDX questActionShowTip = new QuestActionGiveCVarBuffSDX();
        base.CopyValues(questActionShowTip);
        return questActionShowTip;
    }
}
