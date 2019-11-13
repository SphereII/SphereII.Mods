using UnityEngine;
public class MinEventActionPumpQuestSDX : MinEventActionRemoveBuff
{
    // This loops through all the targets, refreshing the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, Mods" target="self"  />
    public override void Execute(MinEventParams _params)
    {
        for (int j = 0; j < this.targets.Count; j++)
        {
            EntityAliveSDX entity = this.targets[j] as EntityAliveSDX;
            if (entity != null)
            {
                for (int k = 0; k < entity.QuestJournal.quests.Count; k++)
                {
                    for (int l = 0; l < entity.QuestJournal.quests[k].Objectives.Count; l++)
                    {
                        entity.QuestJournal.quests[k].Objectives[l].Refresh();
                    }
                }
                continue;
            }

            EntityPlayerLocal entityPlayer = this.targets[j] as EntityPlayerLocal;
            if (entityPlayer != null)
            {
                for (int k = 0; k < entityPlayer.QuestJournal.quests.Count; k++)
                {
                    for (int l = 0; l < entityPlayer.QuestJournal.quests[k].Objectives.Count; l++)
                    {
                        entityPlayer.QuestJournal.quests[k].Objectives[l].Refresh();
                    }
                }
            }
        }
    }
}
