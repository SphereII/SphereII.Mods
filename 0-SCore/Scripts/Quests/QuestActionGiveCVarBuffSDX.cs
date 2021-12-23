using System.Globalization;

public class QuestActionGiveCVarBuffSDX : BaseQuestAction
{
    public override void SetupAction()
    {
    }


    public override void PerformAction(Quest action)
    {
        var value = StringParsers.ParseFloat(Value, 0, -1, NumberStyles.Float);
        if (OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.HasCustomVar(ID))
            value += OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.GetCustomVar(ID);
        OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.SetCustomVar(ID, value);
    }

    public override BaseQuestAction Clone()
    {
        var questActionShowTip = new QuestActionGiveCVarBuffSDX();
        CopyValues(questActionShowTip);
        return questActionShowTip;
    }
}