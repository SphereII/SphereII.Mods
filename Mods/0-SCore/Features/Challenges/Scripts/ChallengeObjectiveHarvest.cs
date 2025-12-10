using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective that fires when you hire an npc

     <objective type="Harvest, SCore" count="20" item="resourceWood" />
     <objective type="Harvest, SCore" count="20" item="resourceWood" biome="burnt_forest" />
     <objective type="Harvest, SCore" count="20" item="resourceWood" item="meleeItem" />
     <objective type="Harvest, SCore" count="20" item_tags="woodtag" />
     <objective type="Harvest, SCore" count="20" block_tags="blocktag" />
     */
    public class ChallengeObjectiveHarvest : ChallengeObjectiveSCoreBase {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveHarvest;

        public string LocalizationKey = "onHarvest";
        private string held_tags;
        
        private string _descriptionOverride;


        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                {
                    var display = Localization.Get(LocalizationKey);
                    display += $"{SCoreChallengeUtils.GenerateString(held_tags)}";
                    display += $"{SCoreChallengeUtils.GenerateString(itemClass)}";
                    display += $"{SCoreChallengeUtils.GenerateString(blockName)}";
                    display += $"{SCoreChallengeUtils.GenerateString(biome)}";
                    return display;
                }
                return Localization.Get(_descriptionOverride);
            }
        }
            

      //  public override string DescriptionText => Localization.Get(LocalizationKey);

        public override void HandleAddHooks() {
            QuestEventManager.Current.HarvestItem += Current_HarvestItem;
        }

        public override void HandleRemoveHooks() {
            QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
        }

        public virtual void Current_HarvestItem(ItemValue held, ItemStack stack, BlockValue bv) {

            if (!string.IsNullOrEmpty(held_tags))
            {
                if (!SCoreChallengeUtils.IsHoldingItemHasTag(held_tags))
                {

                    return;
                }
            }

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

                if (!result)
                {
            

                    return;
                }
            }

            

            if (!string.IsNullOrEmpty(itemTag))
            {
                var tag = FastTags<TagGroup.Global>.Parse(itemTag);
                if (!stack.itemValue.ItemClass.HasAnyTags(tag))
                {
                    

                    return;
                }
            }

            if (!CheckBiome()) return;
            if (!CheckBlockTags(bv)) return;

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
            return new ChallengeObjectiveHarvest {
                biome = this.biome,
                itemClass = itemClass,
                blockTag = blockTag,
                held_tags = held_tags,
                _descriptionOverride = _descriptionOverride,
                itemTag = itemTag
                
            };
        }
    }
}