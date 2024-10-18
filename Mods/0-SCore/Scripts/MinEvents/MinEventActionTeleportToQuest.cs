using System.Xml.Linq;

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
            //  Log.Out($"Quest: {quest.ID} POI: {quest.GetPOIName()} Position: {quest.Position}");
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