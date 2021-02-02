using System.Xml;
public class MinEventActionGiveQuestSDX : MinEventActionRemoveBuff
{

    string strQuest = "";
    // This loops through all the targets, giving each target the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="GiveQuestSDX, Mods" target="self" quest="myNewQuest" />
    public override void Execute(MinEventParams _params)
    {
        for (int j = 0; j < targets.Count; j++)
        {
            EntityAliveSDX entity = targets[j] as EntityAliveSDX;
            if (entity != null)
            {
                if (string.IsNullOrEmpty(strQuest))
                    continue;

                entity.GiveQuest(strQuest);
            }

            // If the target is a player.
            EntityPlayerLocal Playerentity = targets[j] as EntityPlayerLocal;
            if (Playerentity != null)
            {
                if (string.IsNullOrEmpty(strQuest))
                    continue;

                Quest myQuest = QuestClass.CreateQuest(strQuest);
                myQuest.QuestGiverID = -1;
                Playerentity.QuestJournal.AddQuest(myQuest);

            }

        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "quest")
                {
                    strQuest = _attribute.Value;
                    return true;
                }
            }
        }
        return flag;
    }
}
