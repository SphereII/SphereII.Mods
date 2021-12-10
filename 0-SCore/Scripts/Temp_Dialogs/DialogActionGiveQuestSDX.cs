public class DialogActionGiveQuestSDX : DialogActionAddBuff
{
    private readonly string name = string.Empty;

    public override void PerformAction(EntityPlayer player)
    {
        var NewQuest = QuestClass.CreateQuest(ID);
        if (NewQuest == null)
            return;

        player.QuestJournal.RemoveQuest(NewQuest);
        player.QuestJournal.AddQuest(NewQuest);
    }
}