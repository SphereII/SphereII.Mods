using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to allow a player to craft with a certain tag
     *
     * <objective type="CraftWithTags, SCore" count="2" item_tags="tag01"/>
     */
    public class ChallengeObjectiveCraftWithTags : BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithTags;

        private string item_tags;

        public string LocalizationKey = "challengeObjectiveCraftWithTags";
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    return Localization.Get($"{LocalizationKey}", false) + " " + item_tags + ":";
                return Localization.Get(_descriptionOverride);
            }
        }
       
        public override void HandleAddHooks()
        {
            QuestEventManager.Current.CraftItem += Current_CraftItem;
        }

        
        public override void HandleRemoveHooks()
        {
            QuestEventManager.Current.CraftItem -= Current_CraftItem;
        }

        private void Current_CraftItem(ItemStack stack) {
            var itemClass = stack.itemValue.ItemClass;
            if (itemClass == null) return;
            var tags = FastTags<TagGroup.Global>.Parse(item_tags);
            if (!itemClass.HasAnyTags(tags)) return;
            Current++;
            CheckObjectiveComplete();
        }


        public override void ParseElement(XElement e)
        {
            base.ParseElement(e);
            if (e.HasAttribute("item_tags"))
            {
                item_tags = e.GetAttribute("item_tags");
            }
            
            if (e.HasAttribute("description_key"))
                LocalizationKey =e.GetAttribute("description_key");
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }
        
        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectiveCraftWithTags()
            {
             item_tags = item_tags,
             LocalizationKey = LocalizationKey
             ,
             _descriptionOverride = _descriptionOverride


            };
        }

    }
}