using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Harmony.ItemActions
{
    /**
     * SCore_AdvancedItems
     * 
     * This class includes a Harmony patches to allow more repair flexbility for modders.
     * <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
     * <property Class="RepairItems">
     *     <property name="resourceFeather" value="2" />
     * </property>
     * *
     * <property Class="ScrapItems">
     *     <property name="resourceFeather" value="1" />
     * </property>
     */
    public class AdvancedItems
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "AdvancedItemRepair";

        public class ItemDisplay
        {
            private static bool GetBindingValueHelper(ref string value, string binding, ItemClass ___itemClass)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (___itemClass == null)
                    return true;

                var text = binding;
                if (text != "itemRepairDescription") return true;

                AdvLogging.DisplayLog(AdvFeatureClass, "Reading Custom Repair description");

                var descriptionKey2 = ___itemClass.DescriptionKey;
                if (Localization.Exists(descriptionKey2))
                    value = Localization.Get(descriptionKey2);

                var stringBuilder = new StringBuilder();
                stringBuilder.Append(Localization.Get("lblRepairItems"));

                var stack = new List<ItemStack>();
                // Check if ScrapItems is specified
                if (___itemClass.Properties.Classes.ContainsKey("RepairItems"))
                {
                    var dynamicProperties3 = ___itemClass.Properties.Classes["RepairItems"];
                    stack = ItemsUtilities.ParseProperties(dynamicProperties3);
                }
                else if (___itemClass.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
                {
                    var strData = ___itemClass.Properties.Values["RepairItems"];
                    stack = ItemsUtilities.ParseProperties(strData);
                }
                else if (___itemClass.RepairTools == null || ___itemClass.RepairTools.Length <= 0)
                {
                    var recipe = ItemsUtilities.GetReducedRecipes(___itemClass.GetItemName(), 2);
                    stack = recipe.ingredients;
                }

                if (stack.Count > 0)
                {
                    stringBuilder.Append(ItemsUtilities.GetStackSummary(stack));
                    value = stringBuilder.ToString();
                }
                else
                {
                    stringBuilder.Append(" You cannot repair this.");
                    value = stringBuilder.ToString();
                }

                return false;
            }


            // Used to display the repair requirements
            [HarmonyPatch(typeof(XUiC_ItemInfoWindow))]
            [HarmonyPatch(nameof(XUiC_ItemInfoWindow.GetBindingValueInternal))]
            [HarmonyPatch(new[] { typeof(string), typeof(string) })]
            public static bool Prefix(ref string value, string binding, ItemClass ___itemClass)
            {
                return GetBindingValueHelper(ref value, binding, ___itemClass);
            }
        }

        // Used to display the repair requirements
        [HarmonyPatch(typeof(XUiC_ItemActionList))]
        [HarmonyPatch("SetCraftingActionList")]
        public class ItemActionListSetCraftingActionList
        {
            public static void Postfix(XUiC_ItemActionList __instance, XUiC_ItemActionList.ItemActionListTypes _actionListType, XUiController itemController)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                
                if (_actionListType != XUiC_ItemActionList.ItemActionListTypes.Item) return;
                var itemValue = ItemClassUtils.GetItemValue(itemController);
                if (itemValue == null) return ;
                if (itemValue.MaxUseTimes <= 0 || !(itemValue.UseTimes > 0f)) return;
                
                 if (ItemsUtilities.CheckProperty(itemValue.ItemClass, "RepairItems"))
                     __instance.AddActionListEntry(new ItemActionEntryRepair(itemController));
                
                if (ItemsUtilities.CheckProperty(itemValue.ItemClass, "SharpenItem"))
                    __instance.AddActionListEntry(new ItemActionEntryResharpenSDX(itemController));
            }
        }

        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch(nameof(ItemActionEntryRepair.OnDisabledActivate))]
        public class RepairOnDisabledActivate
        {
            public static bool Prefix(ItemActionEntryRepair __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                switch (__instance.state)
                {
                    case ItemActionEntryRepair.StateTypes.RecipeLocked:
                            GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairMustReadBook"));
                        return false;
                    case ItemActionEntryRepair.StateTypes.NotEnoughMaterials:
                        GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairMissingMats"));
                        break;
                    case ItemActionEntryRepair.StateTypes.Normal:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                List<ItemStack> stack;
                var itemValue = ItemClassUtils.GetItemValue(__instance.ItemController);
                if (itemValue == null) return false;

                var forId = ItemClass.GetForId(itemValue.type);
                if (forId.Properties.Classes.ContainsKey("RepairItems"))
                {
                    var dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                    stack = ItemsUtilities.ParseProperties(dynamicProperties3);
                    if ( !ItemsUtilities.CheckIngredients(stack, __instance.ItemController.xui.playerUI.entityPlayer))
                        return false;
                    
                }

                if (forId.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
                {
                    var strData = forId.Properties.Values["RepairItems"];
                    stack = ItemsUtilities.ParseProperties(strData);
                    if ( !ItemsUtilities.CheckIngredients(stack, __instance.ItemController.xui.playerUI.entityPlayer)) 
                        return false;
                }

                if (forId.RepairTools != null && forId.RepairTools.Length > 0) return true;

                var recipe = ItemsUtilities.GetReducedRecipes(forId.GetItemName(), 2);
                if (!ItemsUtilities.CheckIngredients(recipe.ingredients, __instance.ItemController.xui.playerUI.entityPlayer) )
                    return false;
                return false;
            }
        }

        // Make sure all items are available.
        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch(nameof(ItemActionEntryRepair.OnActivated))]
        public class RepairOnActivated
        {
            public static bool Prefix(ItemActionEntryRepair __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var xui = __instance.ItemController.xui;
                var itemValue = ItemClassUtils.GetItemValue(__instance.ItemController);
                if (itemValue == null) return false;
                var forId = ItemClass.GetForId(itemValue.type);
                var player = xui.playerUI.entityPlayer;

                // If the item has a repairItems, use that, instead of the vanilla version.
                if (forId.Properties.Classes.ContainsKey("RepairItems"))
                {
                    var recipe = new Recipe();
                    var dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                    recipe.ingredients = ItemsUtilities.ParseProperties(dynamicProperties3);

                    // Get an adjusted Crafting time from the player.
                    recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
                    ItemsUtilities.ConvertAndCraft(recipe, player, __instance.ItemController);
                    return false;
                }

                if (forId.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
                {
                    var recipe = new Recipe();
                    var strData = forId.Properties.Values["RepairItems"];
                    recipe.ingredients = ItemsUtilities.ParseProperties(strData);

                    // Get an adjusted Crafting time from the player.
                    recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
                    ItemsUtilities.ConvertAndCraft(recipe, player, __instance.ItemController);
                    return false;
                }
                // If there's no RepairTools defined, then fall back to recipe reduction

                if (forId.RepairTools is { Length: > 0 }) return true;

                // Determine, based on percentage left, 
                var recipeCountReduction = 2;
                if (itemValue.PercentUsesLeft < 0.2)
                    recipeCountReduction = 3;

                // Use the helper method to reduce the recipe count, and control displaying on the UI for consistency.
                ItemsUtilities.ConvertAndCraft(forId.GetItemName(), recipeCountReduction, player, __instance.ItemController);
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemActionEntryScrap))]
        [HarmonyPatch("OnActivated")]
        public class SCoreItemActionScrapping
        {
            public static bool Prefix(ItemActionEntryScrap __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                #region vanilla_code

                var xui = __instance.ItemController.xui;
                var xuiCItemStack = (XUiC_ItemStack)__instance.ItemController;
               
                var itemStack = xuiCItemStack.ItemStack.Clone();
                var scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
                if (scrapableRecipe == null) return true;

                var xuiController = __instance.ItemController.xui.FindWindowGroupByName("workstation_workbench");
                if (xuiController == null || !xuiController.WindowGroup.isShowing)
                    xuiController = xui.FindWindowGroupByName("crafting");

                var childByType = xuiController.GetChildByType<XUiC_CraftingWindowGroup>();
                if (childByType == null) return true;

                #endregion vanilla_code

                List<ItemStack> scrapItems;

                var forId = ItemClass.GetForId(itemStack.itemValue.type);
                 if (forId.RepairTools is { Length: > 0 }) return true;

                 var blockList = CheckProperties(forId.GetBlock()?.Properties);
                 if (blockList.Count > 0)
                 {
                     ItemsUtilities.Scrap(blockList, itemStack, __instance.ItemController);
                     return false;
                 }

                 var list = CheckProperties(forId.Properties);
                 if (list.Count > 0)
                 {
                     Debug.Log($"ItemUtilis.Scrap(): {list.Count}");
                     ItemsUtilities.Scrap(list, itemStack, __instance.ItemController);
                     return false;
                 }

                // // Check if ScrapItems is specified
                // if (forId.Properties.Classes.ContainsKey("ScrapItems"))
                // {
                //     var dynamicProperties3 = forId.Properties.Classes["ScrapItems"];
                //     scrapItems = ItemsUtilities.ParseProperties(dynamicProperties3);
                //     ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                //     return false;
                // }
                //
                //
                // if (forId.Properties.Contains("ScrapItems")) // Support for <property name="ScrapItems" value="resourceWood,0,resourceLeather,2" />
                // {
                //     var strData = forId.Properties.Values["ScrapItems"];
                //     scrapItems = ItemsUtilities.ParseProperties(strData);
                //     ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                //     return false;
                // }
                // // Check if Repair Items is specified, if the ScrapItems wasn't.
                // if (forId.Properties.Classes.ContainsKey("RepairItems"))
                // {
                //     var dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                //     scrapItems = ItemsUtilities.ParseProperties(dynamicProperties3);
                //     ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                //     return false;
                // }
                //
                //
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "DisableScrapFallback"))
                {
                    Debug.Log("Disabling Scrap FAll back");
                    return true;
                }

                if (CraftingManager.GetRecipe(forId.GetItemName()) == null) return true;

                if (CraftingManager.GetRecipe(forId.GetItemName()).tags.Test_AnySet(FastTags<TagGroup.Global>.Parse("usevanillascrap")))
                    return true;
                // If there's a recipe, reduce it
                var recipe = ItemsUtilities.GetReducedRecipes(forId.GetItemName(), 2);
                if (recipe == null) return false;
                ItemsUtilities.Scrap(recipe.ingredients, itemStack, __instance.ItemController);
                return false;
            }


            private static List<ItemStack> CheckProperties(DynamicProperties dynamicProperties)
            {
                if ( dynamicProperties == null) return new List<ItemStack>();
                if (dynamicProperties.Contains("ScrapItems")) // Support for <property name="ScrapItems" value="resourceWood,0,resourceLeather,2" />
                {
                    var strData = dynamicProperties.Values["ScrapItems"];
                    return ItemsUtilities.ParseProperties(strData);
                }
                // Check if Repair Items is specified, if the ScrapItems wasn't.
                if (dynamicProperties.Classes.ContainsKey("RepairItems"))
                {
                    var dynamicProperties3 = dynamicProperties.Classes["RepairItems"];
                    return ItemsUtilities.ParseProperties(dynamicProperties3);
                }
                if (dynamicProperties.Classes.ContainsKey("ScrapItems"))
                {
                    var dynamicProperties3 = dynamicProperties.Classes["ScrapItems"];
                    return ItemsUtilities.ParseProperties(dynamicProperties3);
                }
                return new List<ItemStack>();
            }
            private static List<ItemStack> CheckForBlock(Block block)
            {
                if ( block == null ) return new List<ItemStack>();
                if (block.Properties.Contains("ScrapItems")) // Support for <property name="ScrapItems" value="resourceWood,0,resourceLeather,2" />
                {
                    var strData = block.Properties.Values["ScrapItems"];
                    return ItemsUtilities.ParseProperties(strData);
                }
                // Check if Repair Items is specified, if the ScrapItems wasn't.
                if (block.Properties.Classes.ContainsKey("RepairItems"))
                {
                    var dynamicProperties3 = block.Properties.Classes["RepairItems"];
                    return ItemsUtilities.ParseProperties(dynamicProperties3);
                }
                if (block.Properties.Classes.ContainsKey("ScrapItems"))
                {
                    var dynamicProperties3 = block.Properties.Classes["ScrapItems"];
                    return ItemsUtilities.ParseProperties(dynamicProperties3);
                }
                return new List<ItemStack>();
            }
        }
    }
}