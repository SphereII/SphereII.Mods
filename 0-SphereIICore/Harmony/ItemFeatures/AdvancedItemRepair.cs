using HarmonyLib;
using System.Collections.Generic;
using System.Text;

/**
 * SphereII__AdvancedItems
 *
 * This class includes a Harmony patches to allow more repair flexbility for modders.
 * 
 *
 * <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
 * 
 * <property Class="RepairItems">
 *      <property name="resourceFeather" value="2" />
 * </property> 
 * 
 * * <property Class="ScrapItems">
 *      <property name="resourceFeather" value="1" />
 * </property> 
 * 
 */
public class SphereII__AdvancedItems
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "AdvancedItemRepair";

    // Used to display the repair requirements
    [HarmonyPatch(typeof(XUiC_ItemInfoWindow))]
    [HarmonyPatch("GetBindingValue")]
    public class SphereII_Item_Display
    {
        public static bool Prefix(ref string value, BindingItem binding, ItemClass ___itemClass)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (___itemClass == null)
                return true;

            string text = binding.FieldName;
            if (text == "itemRepairDescription")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "Reading Custom Repair description");

                string descriptionKey2 = ___itemClass.DescriptionKey;
                if (Localization.Exists(descriptionKey2))
                    value = Localization.Get(descriptionKey2);

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(Localization.Get("lblRepairItems"));

                List<ItemStack> stack = new List<ItemStack>();
                // Check if ScrapItems is specified
                if (___itemClass.Properties.Classes.ContainsKey("RepairItems"))
                {
                    DynamicProperties dynamicProperties3 = ___itemClass.Properties.Classes["RepairItems"];
                    stack = ItemsUtilities.ParseProperties(dynamicProperties3);
                }
                else if (___itemClass.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
                {
                    string strData = ___itemClass.Properties.Values["RepairItems"].ToString();
                    stack = ItemsUtilities.ParseProperties(strData);
                }
                else if (___itemClass.RepairTools == null || ___itemClass.RepairTools.Length <= 0)
                {
                    Recipe recipe = ItemsUtilities.GetReducedRecipes(___itemClass.GetItemName(), 2);
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
            return true;
        }
    }

    // Used to display the repair requirements
    [HarmonyPatch(typeof(XUiC_ItemActionList))]
    [HarmonyPatch("SetCraftingActionList")]
    public class SphereII_XUiC_ItemActionList_SetCraftingActionList
    {
        public static void Postfix(XUiC_ItemActionList __instance, XUiC_ItemActionList.ItemActionListTypes _actionListType, XUiController itemController)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            if (_actionListType == XUiC_ItemActionList.ItemActionListTypes.Item)
            {
                XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)itemController;
                ItemStack itemStack = xuiC_ItemStack.ItemStack;
                ItemValue itemValue = itemStack.itemValue;
                if (itemValue.MaxUseTimes > 0 && itemValue.UseTimes > 0f)
                {
                    if (ItemsUtilities.CheckProperty(itemValue.ItemClass, "RepairItems"))
                        __instance.AddActionListEntry(new ItemActionEntryRepair(itemController));

                    if (ItemsUtilities.CheckProperty(itemValue.ItemClass, "Resharpen"))
                        __instance.AddActionListEntry(new ItemActionEntryResharpenSDX(itemController));
                }
            }

        }
    }

    [HarmonyPatch(typeof(ItemActionEntryRepair))]
    [HarmonyPatch("OnDisabledActivate")]
    public class SphereII_ItemActionEntryRepair_OnDisabledActivate
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

            StateTypes stateTypes = ___state;
            if (stateTypes == StateTypes.RecipeLocked)
            {
                GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, ___lblReadBook);
                return false;
            }
            if (stateTypes != StateTypes.NotEnoughMaterials)
                return false;

            GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, ___lblNeedMaterials);
            List<ItemStack> stack = new List<ItemStack>();
            ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)__instance.ItemController).ItemStack.itemValue.type);
            if (forId.Properties.Classes.ContainsKey("RepairItems"))
            {
                DynamicProperties dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                stack = ItemsUtilities.ParseProperties(dynamicProperties3);
                ItemsUtilities.CheckIngredients(stack, __instance.ItemController.xui.playerUI.entityPlayer);
                return false;
            }
            else if (forId.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
            {
                string strData = forId.Properties.Values["RepairItems"].ToString();
                stack = ItemsUtilities.ParseProperties(strData);
                ItemsUtilities.CheckIngredients(stack, __instance.ItemController.xui.playerUI.entityPlayer);
                return false;
            }
            else if (forId.RepairTools == null || forId.RepairTools.Length <= 0)
            {
                Recipe recipe = ItemsUtilities.GetReducedRecipes(forId.GetItemName(), 2);
                ItemsUtilities.CheckIngredients(recipe.ingredients, __instance.ItemController.xui.playerUI.entityPlayer);
                return false;
            }

            return true;
        }
    }

    // Make sure all items are available.
    [HarmonyPatch(typeof(ItemActionEntryRepair))]
    [HarmonyPatch("OnActivated")]
    public class SphereII_ItemActionEntryRepair_OnActivated
    {
        public static bool Prefix(ItemActionEntryRepair __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            XUi xui = __instance.ItemController.xui;
            ItemValue itemValue = ((XUiC_ItemStack)__instance.ItemController).ItemStack.itemValue;
            ItemClass forId = ItemClass.GetForId(itemValue.type);
            EntityPlayerLocal player = xui.playerUI.entityPlayer;

            XUiC_CraftingWindowGroup childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();

            List<ItemStack> repairItems = new List<ItemStack>();

            // If the item has a repairItems, use that, instead of the vanilla version.
            if (forId.Properties.Classes.ContainsKey("RepairItems"))
            {
                Recipe recipe = new Recipe();
                DynamicProperties dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                recipe.ingredients = ItemsUtilities.ParseProperties(dynamicProperties3);

                // Get an adjusted Craftint time from the player.
                recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags.Parse(recipe.GetName()), true, true, true, true, 1, true);
                ItemsUtilities.ConvertAndCraft(recipe, player, __instance.ItemController);
                return false;
            }
            else if (forId.Properties.Contains("RepairItems")) // to support <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10" />
            {
                Recipe recipe = new Recipe();
                string strData = forId.Properties.Values["RepairItems"].ToString();
                recipe.ingredients = ItemsUtilities.ParseProperties(strData);

                // Get an adjusted Craftint time from the player.
                recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags.Parse(recipe.GetName()), true, true, true, true, 1, true);
                ItemsUtilities.ConvertAndCraft(recipe, player, __instance.ItemController);
                return false;

            }
            // If there's no RepairTools defined, then fall back to recipe reduction
            else if (forId.RepairTools == null || forId.RepairTools.Length <= 0)
            {
                // Determine, based on percentage left, 
                int RecipeCountReduction = 2;
                if (itemValue.PercentUsesLeft < 0.2)
                    RecipeCountReduction = 3;

                // Use the helper method to reduce the recipe count, and control displaying on the UI for consistenncy.
                ItemsUtilities.ConvertAndCraft(forId.GetItemName(), RecipeCountReduction, player, __instance.ItemController);
                return false;

            }

            // Fall back to possible RepairTools
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemActionEntryScrap))]
    [HarmonyPatch("OnActivated")]
    public class SphereII_ItemActionScrapping
    {
        public static bool Prefix(ItemActionEntryScrap __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;
            #region vanilla_code
            XUi xui = __instance.ItemController.xui;
            XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)__instance.ItemController;

            ItemStack itemStack = xuiC_ItemStack.ItemStack.Clone();
            Recipe scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
            if (scrapableRecipe == null)
                return true;

            XUiController xuiController = __instance.ItemController.xui.FindWindowGroupByName("workstation_workbench");
            if (xuiController == null || !xuiController.WindowGroup.isShowing)
                xuiController = xui.FindWindowGroupByName("crafting");

            XUiC_CraftingWindowGroup childByType = xuiController.GetChildByType<XUiC_CraftingWindowGroup>();
            if (childByType == null)
                return true;
            #endregion  vanilla_code

            LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(__instance.ItemController.xui.playerUI.entityPlayer);
            List<ItemStack> scrapItems = new List<ItemStack>();

            ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
            // Check if ScrapItems is specified
            if (forId.Properties.Classes.ContainsKey("ScrapItems"))
            {
                DynamicProperties dynamicProperties3 = forId.Properties.Classes["ScrapItems"];
                scrapItems = ItemsUtilities.ParseProperties(dynamicProperties3);
                ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                return false;
            }
            else if (forId.Properties.Contains("ScrapItems")) // Support for <property name="ScrapItems" value="resourceWood,0,resourceLeather,2" />
            {
                string strData = forId.Properties.Values["ScrapItems"].ToString();
                scrapItems = ItemsUtilities.ParseProperties(strData);
                ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                return false;
            }
            // Check if Repair Items is specified, if the ScrapItems wasn't.
            else if (forId.Properties.Classes.ContainsKey("RepairItems"))
            {
                DynamicProperties dynamicProperties3 = forId.Properties.Classes["RepairItems"];
                scrapItems = ItemsUtilities.ParseProperties(dynamicProperties3);
                ItemsUtilities.Scrap(scrapItems, itemStack, __instance.ItemController);
                return false;
            }
            else if (forId.RepairTools == null || forId.RepairTools.Length <= 0)
            {
                if (CraftingManager.GetRecipe(forId.GetItemName()) == null)
                    return true;

                if (CraftingManager.GetRecipe(forId.GetItemName()).tags.Test_AnySet(FastTags.Parse("usevanillascrap")))
                    return true;

                // If there's a recipe, reduce it
                Recipe recipe = ItemsUtilities.GetReducedRecipes(forId.GetItemName(), 2);

                ItemsUtilities.Scrap(recipe.ingredients, itemStack, __instance.ItemController);
                return false;
            }
            return true;
        }
    }

}
