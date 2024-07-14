using System.Xml;
using System.Xml.Linq;

public class MinEventActionGiveQuestSDX : MinEventActionRemoveBuff
{
    private string strQuest = "";

    // This loops through all the targets, giving each target the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="GiveQuestSDX, SCore" target="self" quest="myNewQuest" />
    public override void Execute(MinEventParams _params)
    {
        for (var j = 0; j < targets.Count; j++)
        {
            var entity = targets[j] as EntityAliveSDX;
            if (entity != null)
            {
                if (string.IsNullOrEmpty(strQuest))
                    continue;

                entity.GiveQuest(strQuest);
            }

            // If the target is a player.
            var Playerentity = targets[j] as EntityPlayerLocal;
            if (Playerentity != null)
            {
                if (string.IsNullOrEmpty(strQuest))
                    continue;

                var myQuest = QuestClass.CreateQuest(strQuest);
                myQuest.QuestGiverID = -1;
                Playerentity.QuestJournal.AddQuest(myQuest);
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
                    strQuest = _attribute.Value;
                    return true;
                }
        }

        return flag;
    }
}