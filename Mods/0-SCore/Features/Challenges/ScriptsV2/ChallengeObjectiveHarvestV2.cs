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
    public class ChallengeObjectiveHarvestV2 : BaseChallengeObjective
    {
        private string held_tags;
        public string itemClass;
        public string itemTag;
        public string blockTag;
        
        private string _descriptionOverride;
        
        public override ChallengeObjectiveType ObjectiveType {
            get { return (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveHarvestV2; }
        }

        public override void HandleAddHooks()
        {
            QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
            QuestEventManager.Current.HarvestItem += Current_HarvestItem;
        }

        public override void HandleRemoveHooks()
        {
            QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
        }
        private void Current_HarvestItem(ItemValue held, ItemStack stack, BlockValue bv)
        {
            // Check all the requirements
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;

            if ( !string.IsNullOrEmpty(held_tags))
                if (!SCoreChallengeUtils.IsHoldingItemHasTag(held_tags)) return;

            if (!string.IsNullOrEmpty(itemClass))
            {
                var result = false;
                foreach (var itemCl in itemClass.Split(','))
                {
                    var item = ItemClass.GetItem(itemCl);
                    if (stack.itemValue.type != item.type) continue;
                    result = true;
                    break;
                }

                if (!result) return;
            }

            if (!string.IsNullOrEmpty(itemTag))
            {
                var tag = FastTags<TagGroup.Global>.Parse(itemTag);
                if (!stack.itemValue.ItemClass.HasAnyTags(tag)) return;
            }
            Current++;
            CheckObjectiveComplete();
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);

            if (e.HasAttribute("held_tags"))
            {
                held_tags = e.GetAttribute("held_tags");
            }
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveHarvestV2 {
                itemClass = itemClass,
                blockTag = blockTag,
                held_tags = held_tags,
                _descriptionOverride = _descriptionOverride
            };
        }
    
    }
}