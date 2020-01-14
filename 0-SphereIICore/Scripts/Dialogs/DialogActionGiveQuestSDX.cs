using UnityEngine;
public class DialogActionGiveQuestSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        Quest NewQuest = QuestClass.CreateQuest(ID);
        if (NewQuest == null)
            return;

        player.QuestJournal.RemoveQuest(NewQuest);
        player.QuestJournal.AddQuest(NewQuest);
        
    }

    private string name = string.Empty;
}
