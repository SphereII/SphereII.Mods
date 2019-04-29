using System.Xml;
using UnityEngine;
public class MinEventActionGiveQuestSDX : MinEventActionRemoveBuff
{

    string strQuest = "";
    // This loops through all the targets, giving each target the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="GiveQuestSDX, Mods" target="self" quest="myNewQuest" />
    public override void Execute(MinEventParams _params)
    {
        for (int j = 0; j < this.targets.Count; j++)
        {
            EntityAliveSDX entity = this.targets[j] as EntityAliveSDX;
            if (entity != null)
            {
                if (string.IsNullOrEmpty(this.strQuest))
                    continue;

                entity.GiveQuest(this.strQuest);
            }

            // If the target is a player.
            EntityPlayerLocal Playerentity = this.targets[j] as EntityPlayerLocal;
            if (Playerentity != null)
            {
                if (string.IsNullOrEmpty(this.strQuest))
                    continue;

                Quest myQuest = QuestClass.CreateQuest(this.strQuest);
                myQuest.QuestGiverID = -1;
                Playerentity.QuestJournal.AddQuest( myQuest);
                
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
                if (name == "quest" )
                {
                    this.strQuest = _attribute.Value;
                    return true;
                }
            }
        }
        return flag;
    }
}
