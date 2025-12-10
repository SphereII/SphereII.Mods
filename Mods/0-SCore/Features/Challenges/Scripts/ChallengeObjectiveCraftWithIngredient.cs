using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to allow a player to craft with a certain ingredient, rather than a recipe itself.
     *
     * <objective type="CraftWithIngredient, SCore" count="2" ingredient="resourceLegendaryParts"/>
     * <objective type="CraftWithIngredient, SCore" count="2" item_tags="tag"/>

     */
    public class ChallengeObjectiveCraftWithIngredient : BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithIngredient;

        private string ingredientString;
        private string ingredientTagString;
        
        public string LocalizationKey = "craftWithIngredient";
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                {
                    return $"{Localization.Get(LocalizationKey)} {Localization.Get(ingredientString)}";
                }

                return Localization.Get(_descriptionOverride);
            }
        }

        public override void HandleAddHooks() {
            QuestEventManager.Current.CraftItem += Current_CraftItem;
        }


        public override void HandleRemoveHooks() {
            QuestEventManager.Current.CraftItem -= Current_CraftItem;
        }

        private void Current_CraftItem(ItemStack stack) {
            var itemClass = stack.itemValue?.ItemClass;
            if (itemClass == null) return;
            var recipe = CraftingManager.GetRecipe(itemClass.GetItemName());
            if (recipe?.ingredients == null ) return;
            if ( recipe.ingredients.Count == 0) return;
            
            foreach (var ingred in recipe.ingredients)
            {
                var itemName = ingred.itemValue.ItemClass.GetItemName();
                if (!string.IsNullOrEmpty(ingredientTagString))
                {
                    var tags = FastTags<TagGroup.Global>.Parse(ingredientTagString);
                    if (ingred.itemValue.ItemClass.HasAnyTags(tags))
                    {
                        Current += ingred.count;
                        CheckObjectiveComplete();
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(ingredientString)) continue;
                if (!string.Equals(itemName, ingredientString, StringComparison.InvariantCultureIgnoreCase)) continue;

                Current += ingred.count;
                CheckObjectiveComplete();

            }
        }


        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("ingredient"))
            {
                ingredientString = e.GetAttribute("ingredient");
            }

            if ( e.HasAttribute("item_tags"))
                ingredientTagString = e.GetAttribute("item_tags");
            
            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveCraftWithIngredient {
                ingredientString = ingredientString,
                ingredientTagString = ingredientTagString,
                _descriptionOverride = _descriptionOverride
            };
        }
    }
}