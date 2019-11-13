using Audio;
using UnityEngine;
public class QuestActionPlaySoundSDX: QuestActionShowTip
{
    public override void SetupAction()
    {
    }

    public override void PerformAction()
    {
        
        EntityAlive myEntity = null;
        if (OwnerQuest.OwnerJournal.OwnerPlayer != null)
            myEntity = OwnerQuest.OwnerJournal.OwnerPlayer as EntityAlive;

        if (myEntity != null)
        {
            myEntity.PlayOneShot(base.ID, true);
            BuffClass buff = BuffManager.GetBuff(base.Value);
            if (buff != null )
                if (!myEntity.Buffs.HasBuff(base.Value))
                    myEntity.Buffs.AddBuff(base.Value);

            
        }
    }

    public override BaseQuestAction Clone()
    {
        QuestActionPlaySoundSDX questActionPlaySound = new QuestActionPlaySoundSDX();
        base.CopyValues(questActionPlaySound);
        return questActionPlaySound;
    }
}
