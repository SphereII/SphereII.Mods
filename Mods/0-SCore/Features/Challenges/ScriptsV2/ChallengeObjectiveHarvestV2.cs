using System.Xml.Linq;
using UnityEngine;

namespace Challenges
{
    /*
     *         	<challenge name="Harvesting02" title_key="Harvesting" icon="ui_game_symbol_wood" >
            	<requirement name="RequirementBlockHasHarvestTags, SCore" tags="allHarvest,oreWoodHarvest"/>
            	<requirement name="HoldingItemHasTags" tags="miningTool,shovel"/>
            	<objective type="HarvestV2, SCore" count="200" />
        	</challenge>
     */
    public class ChallengeObjectiveHarvestV2 : ChallengeObjectiveHarvest
    {
	    public override ChallengeObjectiveType ObjectiveType {
		    get { return (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveHarvestV2; }
	    }
	    public override BaseChallengeObjective Clone()
	    {
		    return new ChallengeObjectiveHarvestV2();
	    }
    }
}