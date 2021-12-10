public class DialogRequirementHasCompletedQuestSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        if (string.IsNullOrEmpty(Value))
            return false;

        var invert = false;
        if (Value.StartsWith("!"))
        {
            invert = true;
            Value = Value.Replace("!", "");
        }
        var myQuest = player.QuestJournal.FindQuest(Value.ToLower());
        if (myQuest == null)
            return false;

        if (invert)
            return myQuest.CurrentState != Quest.QuestState.Completed;

        return myQuest.CurrentState == Quest.QuestState.Completed;
    }
}


