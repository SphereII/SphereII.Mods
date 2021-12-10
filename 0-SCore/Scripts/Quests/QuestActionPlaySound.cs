public class QuestActionPlaySoundSDX : QuestActionShowTip
{
    public override void SetupAction()
    {
    }


    public override void PerformAction(Quest action)
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

    public override BaseQuestAction Clone()
    {
        var questActionPlaySound = new QuestActionPlaySoundSDX();
        CopyValues(questActionPlaySound);
        return questActionPlaySound;
    }
}