using System;
using System.Xml.Linq;
using UnityEngine;

/*
 This requirement checks all the quests that are In Progress for the following objectives that are also in progress.
 
 It takes two attributes:  id, or objective.  
 
objective:
 		<objective type="TreasureChest">

id:
    This is different depending on what the quest itself determines is the id. 
    For TreasureChest, the id is the block
    For EntityKill, it would be the entity Names.
    For Goto, it's Location.

  <requirement name="RequirementQuestObjective, SCore" id="cntBuriedLootStashChest" />
  <requirement name="RequirementQuestObjective, SCore" objective="TreasureChest" />
  <requirement name="RequirementQuestObjective, SCore" objective="ObjectiveTreasureChest" />

For the objective type TreasureChest, there's an additiona radius check to make sure the player is within bounds.
 
 */
public class RequirementQuestObjective : TargetedCompareRequirementBase
{
    private string _id = string.Empty;
    private string _objective = string.Empty;
    public override bool IsValid(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(_id) && string.IsNullOrEmpty(_objective)) return false;
        var result = false;
        var player = _params.Self as EntityPlayerLocal;
        if (player == null) return false;
        foreach (var quest in player.QuestJournal.quests)
        {
            foreach (var objective in quest.Objectives)
            {
                if (objective.ObjectiveState != BaseObjective.ObjectiveStates.InProgress) continue;
                if (!string.IsNullOrEmpty(_id))
                {
                    if (!objective.ID.EqualsCaseInsensitive(_id)) continue;
                    result = true;
                    break;
                }

                if (!string.IsNullOrEmpty(_objective))
                {
                    var type = objective.GetType().ToString();
                    if (!type.EqualsCaseInsensitive(_objective)) continue;
                    if (objective is ObjectiveTreasureChest treasureChest)
                    {
                        var position = new Vector3(treasureChest.position.x, player.position.y, treasureChest.position.z);
                        quest.GetPositionData(out var offset, Quest.PositionDataTypes.TreasureOffset);
                        var currentDistance = Vector3.Distance(player.position, position + offset * (float)treasureChest.CurrentRadius);
                        if (currentDistance > treasureChest.CurrentRadius) continue;
                    }
                    result = true;
                    break;
                }
            }

            if (result) break;
        }

        
        if (invert)
            return !result;
        return result;
    }
    
    public override bool ParseXAttribute(XAttribute _attribute)
    {
        if (_attribute.Name.LocalName == "id")
        {
            _id = _attribute.Value.ToLower();
            return true;
        }

        if (_attribute.Name.LocalName == "objective")
        {
            var temp = _attribute.Value.ToLower();
            if (temp.StartsWith("objective"))
                _objective = temp;
            else
                _objective = $"objective{temp}";
            
            return true;
        }

        return base.ParseXAttribute(_attribute);
    }
}
