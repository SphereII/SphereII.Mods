using HarmonyLib;
using System.Collections.Generic;
using System.Text;

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
            [HarmonyPatch("GetBindingValue")]
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
                var xuiCItemStack = (XUiC_ItemStack)itemController;
                var itemStack = xuiCItemStack.ItemStack;
                var itemValue = itemStack.itemValue;
                if (itemValue.MaxUseTimes <= 0 || !(itemValue.UseTimes > 0f)) return;

                if (ItemsUtilities.CheckProperty(itemValue.ItemClass, "RepairItems"))
                    __instance.AddActionListEntry(new ItemActionEntryRepair(itemController));

                if (ItemsUtilities.CheckProperty(itemValue.ItemClass, "Resharpen"))
                    __instance.AddActionListEntry(new ItemActionEntryResharpenSDX(itemController));
            }
        }

        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch("OnDisabledActivate")]
        public class RepairOnDisabledActivate
        {
            public enum StateTypes
            {
                Normal,
                RecipeLocked,
                NotEnoughMaterials
            }

            public static bool Prefix(ItemActionEntryRepair __instance, StateTypes ___state, string ___lblReadBook, string ___lblNeedMaterials)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var stateTypes = ___state;
                if (stateTypes == StateTypes.RecipeLocked)
                {
                    GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, ___lblReadBook);
                    return false;
                }

                if (stateTypes != StateTypes.NotEnoughMaterials)
                    return false;

                GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, ___lblNeedMaterials);
                List<ItemStack> stack;
                var forId = ItemClass.GetForId(((XUiC_ItemStack)__instance.ItemController).ItemStack.itemValue.type);
                if (forId.Properties.Classes.ContainsKey("RepairItems"))
                {
                    var dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                    stack = ItemsUtilities.ParseProperties(dynamicProperties3);
                    ItemsUtilities.CheckIngredients(stack, __instance.ItemController.xui.playerUI.entityPlayer);
                    return false;
                }

                if (forId.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
                {
                    var strData = forId.Properties.Values["RepairItems"];
                    stack = ItemsUtilities.ParseProperties(strData);
                    ItemsUtilities.CheckIngredients(stack, __instance.ItemController.xui.playerUI.entityPlayer);
                    return false;
                }

                if (forId.RepairTools != null && forId.RepairTools.Length > 0) return true;

                var recipe = ItemsUtilities.GetReducedRecipes(forId.GetItemName(), 2);
                ItemsUtilities.CheckIngredients(recipe.ingredients, __instance.ItemController.xui.playerUI.entityPlayer);
                return false;
            }
        }

        // Make sure all items are available.
        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch("OnActivated")]
        public class RepairOnActivated
        {
            public static bool Prefix(ItemActionEntryRepair __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var xui = __instance.ItemController.xui;
                var itemValue = ((XUiC_ItemStack)__instance.ItemController).ItemStack.itemValue;
                var forId = ItemClass.GetForId(itemValue.type);
                var player = xui.playerUI.entityPlayer;

                // If the item has a repairItems, use that, instead of the vanilla version.
                if (forId.Properties.Classes.ContainsKey("RepairItems"))
                {
                    var recipe = new Recipe();
                    var dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                    recipe.ingredients = ItemsUtilities.ParseProperties(dynamicProperties3);

                    // Get an adjusted Crafting time from the player.
                    recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags.Parse(recipe.GetName()));
                    ItemsUtilities.ConvertAndCraft(recipe, player, __instance.ItemController);
                    return false;
                }

                if (forId.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
                {
                    var recipe = new Recipe();
                    var strData = forId.Properties.Values["RepairItems"];
                    recipe.ingredients = ItemsUtilities.ParseProperties(strData);

                    // Get an adjusted Crafting time from the player.
                    recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags.Parse(recipe.GetName()));
                    ItemsUtilities.ConvertAndCraft(recipe, player, __instance.ItemController);
                    return false;
                }
                // If there's no RepairTools defined, then fall back to recipe reduction

                if (forId.RepairTools != null && forId.RepairTools.Length > 0) return true;

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
                if (scrapableRecipe == null)
                    return true;

                var xuiController = __instance.ItemController.xui.FindWindowGroupByName("workstation_workbench");
                if (xuiController == null || !xuiController.WindowGroup.isShowing)
                    xuiController = xui.FindWindowGroupByName("crafting");

                var childByType = xuiController.GetChildByType<XUiC_CraftingWindowGroup>();
                if (childByType == null)
                    return true;

                #endregion vanilla_code

                List<ItemStack> scrapItems;

                var forId = ItemClass.GetForId(itemStack.itemValue.type);
                // Check if ScrapItems is specified
                if (forId.Properties.Classes.ContainsKey("ScrapItems"))
                {
                    var dynamicProperties3 = forId.Properties.Classes["ScrapItems"];
                    scrapItems = ItemsUtilities.ParseProperties(dynamicProperties3);
                    ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                    return false;
                }

                if (forId.Properties.Contains("ScrapItems")) // Support for <property name="ScrapItems" value="resourceWood,0,resourceLeather,2" />
                {
                    var strData = forId.Properties.Values["ScrapItems"];
                    scrapItems = ItemsUtilities.ParseProperties(strData);
                    ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                    return false;
                }
                // Check if Repair Items is specified, if the ScrapItems wasn't.

                if (forId.Properties.Classes.ContainsKey("RepairItems"))
                {
                    var dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                    scrapItems = ItemsUtilities.ParseProperties(dynamicProperties3);
                    ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                    return false;
                }

                if (forId.RepairTools != null && forId.RepairTools.Length > 0) return true;

                if (CraftingManager.GetRecipe(forId.GetItemName()) == null)
                    return true;

                if (CraftingManager.GetRecipe(forId.GetItemName()).tags.Test_AnySet(FastTags.Parse("usevanillascrap")))
                    return true;

                // If there's a recipe, reduce it
                var recipe = ItemsUtilities.GetReducedRecipes(forId.GetItemName(), 2);

                ItemsUtilities.Scrap(recipe.ingredients, itemStack, __instance.ItemController);
                return false;
            }
        }
    }
}