public class DialogActionGiveQuestSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var newQuest = QuestClass.CreateQuest(ID);
        if (newQuest == null)
            return;

        player.QuestJournal.RemoveQuest(newQuest);
        player.QuestJournal.AddQuest(newQuest);

    }
}
