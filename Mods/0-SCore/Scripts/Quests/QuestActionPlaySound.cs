public class QuestActionPlaySoundSDX : BaseQuestAction
{
    public void SetupAction()
    {
    }


    public  void PerformAction(Quest action)
    {
        EntityAlive myEntity = null;
        if (OwnerQuest.OwnerJournal.OwnerPlayer != null)
            myEntity = OwnerQuest.OwnerJournal.OwnerPlayer;

        if (myEntity != null)
        {
            myEntity.PlayOneShot(ID, true);
            var buff = BuffManager.GetBuff(Value);
            if (buff != null)
                if (!myEntity.Buffs.HasBuff(Value))
                    myEntity.Buffs.AddBuff(Value);
        }
    }

    public  BaseQuestAction Clone()
    {
        var questActionPlaySound = new QuestActionPlaySoundSDX();
        CopyValues(questActionPlaySound);
        return questActionPlaySound;
    }
}