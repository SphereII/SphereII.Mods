public class DialogActionGiveQuestSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var newQuest = QuestClass.CreateQuest(ID);
        if (newQuest == null)
            return;

        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        if (playerUI.xui.Dialog.Respondent != null)
            newQuest.QuestGiverID = playerUI.xui.Dialog.Respondent.entityId;
        player.QuestJournal.RemoveQuest(newQuest);
        player.QuestJournal.AddQuest(newQuest);

    }
}
