using System.Xml;
using System.Xml.Linq;

public class MinEventActionPumpQuestSDX : MinEventActionRemoveBuff
{
    // This loops through all the targets, refreshing the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, SCore" target="self"  />
    public override void Execute(MinEventParams _params)
    {
        for (var j = 0; j < targets.Count; j++)
        {
            var entity = targets[j] as EntityAliveSDX;
            if (entity != null)
            {
                for (var k = 0; k < entity.questJournal.quests.Count; k++)
                    for (var l = 0; l < entity.questJournal.quests[k].Objectives.Count; l++)
                        entity.questJournal.quests[k].Objectives[l].Refresh();
                continue;
            }

            var entityPlayer = targets[j] as EntityPlayerLocal;
            if (entityPlayer != null)
                for (var k = 0; k < entityPlayer.QuestJournal.quests.Count; k++)
                    for (var l = 0; l < entityPlayer.QuestJournal.quests[k].Objectives.Count; l++)
                        entityPlayer.QuestJournal.quests[k].Objectives[l].Refresh();
        }
    }
}

public class MinEventActionTeleportToQuest : MinEventActionTargetedBase
{
    // This loops through all the targets, refreshing the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="TeleportToQuest, SCore" quest="sorcery_trader_arcane"  />
    string questName;
    public override void Execute(MinEventParams _params)
    {
        var entityPlayer = _params.Self as EntityPlayer;
        foreach (var quest in entityPlayer.QuestJournal.quests)
        {
            Log.Out($"Quest: {quest.ID} POI: {quest.GetPOIName()} Position: {quest.Position}");
            if (quest.ID == questName)
            {
                quest.GetPositionData(out var pos, Quest.PositionDataTypes.POIPosition);
                //quest.GetPositionData(out var pos, Quest.PositionDataTypes.POIPosition);
                //quest.GetPositionData(out var pos, Quest.PositionDataTypes.Activate);
                entityPlayer.Teleport(pos);
                break;
            }
        }

    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
                if (name == "quest")
                {
                    questName = _attribute.Value;
                    return true;
                }
        }

        return flag;
    }
}