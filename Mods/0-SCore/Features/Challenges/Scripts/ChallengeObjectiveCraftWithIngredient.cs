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
     */
    public class ChallengeObjectiveCraftWithIngredient : BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithIngredient;

        private string ingredientString;

        public string LocalizationKey = "craftWithIngredient";
        public override string DescriptionText => Localization.Get(LocalizationKey);

        
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
            var recipe = CraftingManager.GetRecipe(itemClass.GetItemName());
            foreach (var ingred in recipe.ingredients)
            {
                var itemName = ingred.itemValue.ItemClass.GetItemName();
                if ( !string.Equals(itemName, ingredientString, StringComparison.InvariantCultureIgnoreCase)) continue;
                Current += ingred.count;
                CheckObjectiveComplete();
            }
         
        }


        public override void ParseElement(XElement e)
        {
            base.ParseElement(e);
            if (e.HasAttribute("ingredient"))
            {
                ingredientString = e.GetAttribute("ingredient");
            }
            
            if (e.HasAttribute("description_key"))
                LocalizationKey =e.GetAttribute("description_key");
        }
        
        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectiveCraftWithIngredient
            {
             ingredientString =  ingredientString
            };
        }

    }
}