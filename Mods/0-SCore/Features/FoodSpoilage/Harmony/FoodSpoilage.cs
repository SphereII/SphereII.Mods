using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * SphereII_FoodSpoilage
 *
 * This class includes a Harmony patches to enable Food spoilage, including trigger times and delays. The main trigger spoilage code occurs
 * on the XUiC_ItemStack, so all stacks of items will be affected, if the right XML is used. This needs to be enabled through the Config/blocks.xml, as well as XML changes
 * to food or other items you want to degrade over time.
 *
 * XML Usage ( Taken from the SphereII Food Spoilage Mod )
 *
 *   <!-- Spoilage: Every 500 Ticks, take a loss of 1, out of a total of 1000. -->
 * <append xpath="/items">
 *   <item name="foodSpoilageTest">
 *     <property name="Extends" value="foodShamSandwich"/>
 *     <property name="DisplayType" value="melee"/>
 *     <property name="Tags" value="perkMasterChef"/>  <!-- tags must match the tags in the effect_group -->

 *     <property name="Spoilable" value="true" />
 *     <property name="SpoiledItem" value="foodRottingFlesh" />    <!-- Optional to over-ride ConfigBlockSpoilage globa. When spoiled, this item will turn into this item.-->
 *     <property name="TickPerLoss" value="500" /> <!-- Optional to over-ride ConfigBlockSpoilage global. Example value=10   10 ticks per Spoilage increase. -->

 *     <property name="ShowQuality" value="false"/>

 *     <property name="SpoilageMax" value="1000" />
 *     <property name="SpoilagePerTick" value="1" />
 *   </item>
  * </append>
 *
 * <!-- Append Template -->
 * <append xpath="/items/item[starts-with(@name, 'food') and not(contains(@name, 'foodCan'))]">
 *   <property name="Spoilable" value="true" />
 *   <property name="ShowQuality" value="false"/>
 *   <property name="SpoiledItem" value="foodRottingFlesh" />
 *   <!-- Optional to over-ride ConfigBlockSpoilage globa. When spoiled, this item will turn into this item.-->
 *   <property name="TickPerLoss" value="5000" />
 *   <!-- Optional to over-ride ConfigBlockSpoilage global. Example value=10   10 ticks per Spoilage increase. -->
 *   <property name="SpoilageMax" value="1000" />
 *   <property name="SpoilagePerTick" value="1" />
 * </append>
 * <append xpath="/items/item[starts-with(@name, 'food') and not(contains(@name, 'foodCan'))]/property[@name='Tags']/@value">,perkMasterChef</append>
 *
 */
public class SphereII_FoodSpoilage {
    private const string KeyNextSpoilageTick = "NextSpoilageTick";
    private const string KeySpoilageAmount = "SpoilageValue";

    private const string PropSpoilable = "Spoilable";

    private const string AdvFeatureClass = "FoodSpoilage";
    private const string Feature = "FoodSpoilage";
    private static readonly bool FoodSpoilage = Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);

    private static readonly bool UseAlternateItemValue =
        Configuration.CheckFeatureStatus(AdvFeatureClass, "UseAlternateItemValue");

    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch("Clone")]
    public class ItemValueClone {
        private static void Postfix(ref ItemValue __result, ItemValue __instance) {
            if (!FoodSpoilage)
                return;
            if (!UseAlternateItemValue)
                return;

            if (__instance.ItemClass == null || !__instance.ItemClass.Properties.Contains(PropSpoilable) ||
                !__instance.ItemClass.Properties.GetBool(PropSpoilable))
                return;

            if (__instance.Metadata == null) return;
            __result.Metadata = new Dictionary<string, TypedMetadataValue>();
            foreach (var text in __instance.Metadata.Keys)
            {
                __result.SetMetadata(text, __instance.Metadata[text].Clone());
                //__result.Metadata.Add(text, __instance.Metadata[text] ?? __instance.Metadata[text].Clone());
            }
        }
    }

    // hook into the ItemStack, which should cover all types of containers. This will run in the update task.
    // It is used to calculate the amount of spoilage necessary, and display the amount of freshness is left in the item.
    [HarmonyPatch(typeof(XUiC_ItemStack))]
    [HarmonyPatch("Update")]
    public class FoodSpoilageXUiCItemStackUpdate {
        private static float GetCurrentSpoilage(ItemValue itemValue) {
            var spoilageAmountObj = itemValue.GetMetadata(KeySpoilageAmount);
            if (spoilageAmountObj is float spoilageAmount)
            {
                return spoilageAmount;
            }

            itemValue.SetMetadata(KeySpoilageAmount, 1f, TypedMetadataValue.TypeTag.Float);
            return 1f;
        }

        private static void SetSpoilageMax(ItemValue itemValue, float degradationMax) {
            itemValue.SetMetadata(KeySpoilageAmount, degradationMax, TypedMetadataValue.TypeTag.Float);
        }

        private static float UpdateCurrentSpoilage(ItemValue itemValue, float spoiled) {
            var currentSpoilageAmount = GetCurrentSpoilage(itemValue);
            currentSpoilageAmount += spoiled;

            itemValue.SetMetadata(KeySpoilageAmount, currentSpoilageAmount, TypedMetadataValue.TypeTag.Float);
            return currentSpoilageAmount;
        }

        // Can we skip this item stack?
        private static bool IsSkippable(XUiC_ItemStack __instance) {
            if (!FoodSpoilage)
                return true;

            // Don't process creative stacks.
            if (__instance.StackLocation == XUiC_ItemStack.StackLocationTypes.Creative)
                return true;

            // Make sure we are dealing with legitimate stacks.
            var instanceItemStack = __instance.ItemStack;
            if (instanceItemStack.IsEmpty())
                return true;

            if (__instance.IsLocked && __instance.IsDragAndDrop)
                return true;

            // If the item class has a Spoilable property, that means it can spoil over time.
            var itemValue = instanceItemStack.itemValue;
            var itemClass = itemValue?.ItemClass;
            if (itemClass == null || !itemClass.Properties.Contains(PropSpoilable) ||
                !itemClass.Properties.GetBool(PropSpoilable))
            {
                return true;
            }

            return false;
        }

        public static bool Prefix(XUiC_ItemStack __instance) {
            if (IsSkippable(__instance)) return true;

            var itemClass = __instance.ItemStack.itemValue.ItemClass;
  
            // Make sure our starting information is correct.
            var currentSpoilage = GetCurrentSpoilage(__instance.ItemStack.itemValue);

            var strDisplay =
                $"XUiC_ItemStack: {itemClass.GetItemName()} :: {__instance.ItemStack.count} Slot: {__instance.SlotNumber} ";
            var degradationMax = 0f;
            var degradationPerUse = 0f;

            if (itemClass.Properties.Contains("SpoilageMax"))
                degradationMax = itemClass.Properties.GetFloat("SpoilageMax");

            if (itemClass.Properties.Contains("SpoilagePerTick"))
                degradationPerUse = itemClass.Properties.GetFloat("SpoilagePerTick");

            // Check if there's a Global Ticks Per Loss Set
            var tickPerLoss = int.Parse(Configuration.GetPropertyValue("FoodSpoilage", "TickPerLoss"));

            // Check if there's a item-specific TickPerLoss
            if (itemClass.Properties.Contains("TickPerLoss"))
                tickPerLoss = itemClass.Properties.GetInt("TickPerLoss");
            strDisplay += " Ticks Per Loss: " + tickPerLoss;

            var worldTime = GameManager.Instance.World.GetWorldTime();
            var nextTick = GetNextSpoilageTick(__instance.ItemStack.itemValue);
            if (nextTick <= 0)
            {
                nextTick = CalculateNextSpoilageTick(worldTime, tickPerLoss);
                SetNextSpoilageTick(__instance.ItemStack.itemValue, nextTick);
            }

            // Throttles the amount of times it'll trigger the spoilage, based on the TickPerLoss
            if (nextTick >= ToInt(worldTime))
            {
                return true;
            }

            // How much spoilage to apply 
            var perUse = degradationPerUse;

            var basePerUse = perUse;
            strDisplay += " Base Spoil: " + perUse;

            float containerValue = 0;
            // Additional Spoiler flags to increase or decrease the spoil rate
            switch (__instance.StackLocation)
            {
                case XUiC_ItemStack.StackLocationTypes.ToolBelt: // Tool belt Storage check
                    containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Toolbelt"));
                    strDisplay += " Storage Type: Tool Belt ( " + containerValue + " )";
                    perUse += containerValue;
                    break;
                case XUiC_ItemStack.StackLocationTypes.Backpack: // Back pack storage check
                    containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Backpack"));
                    strDisplay += " Storage Type: Backpack ( " + containerValue + " )";
                    perUse += containerValue;
                    break;
                case XUiC_ItemStack.StackLocationTypes.LootContainer: // Loot Container Storage check
                    var container = __instance.xui.lootContainer;
                    if (container != null)
                    {
                        var blockValue = GameManager.Instance.World.GetBlock(container.ToWorldPos());
                        var lootContainerName = Localization.Get(Block.list[blockValue.type].GetBlockName());
                        strDisplay += " " + lootContainerName;

                        containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Container"));
                        strDisplay += " Storage Type: Container ( " + containerValue + " )";
                        perUse += containerValue;

                        if (blockValue.Block.Properties.Contains("PreserveBonus"))
                        {
                            strDisplay += " Preservation Bonus ( " +
                                          blockValue.Block.Properties.GetFloat("PreserveBonus") + " )";
                            var preserveBonus = blockValue.Block.Properties.GetFloat("PreserveBonus");
                            if (preserveBonus == -99f)
                            {
                                // Setting the next spoilage tick to reset the stack.
                                nextTick = CalculateNextSpoilageTick(worldTime, tickPerLoss);
                                SetNextSpoilageTick(__instance.ItemStack.itemValue, nextTick);
                                __instance.ForceRefreshItemStack();
                                return true;
                            }

                            perUse -= preserveBonus;
                        }
                    }
                    else
                    {
                        strDisplay += " Storage Type: Container ( Undefined Configuration Block: +10 )";
                        perUse += 10;
                    }

                    break;
                case XUiC_ItemStack.StackLocationTypes.Creative: // Ignore Creative Containers
                    return true;
                case XUiC_ItemStack.StackLocationTypes.Equipment:
                case XUiC_ItemStack.StackLocationTypes.Vehicle:
                case XUiC_ItemStack.StackLocationTypes.Workstation:
                case XUiC_ItemStack.StackLocationTypes.Merge:
                case XUiC_ItemStack.StackLocationTypes.DewCollector:
                default:
                    containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Container"));
                    strDisplay += " Storage Type: Generic ( Default Container) ( " + containerValue + " )";
                    perUse += containerValue;
                    break;
            }


            strDisplay += " Spoiled This Tick: " + (perUse - basePerUse);
            var minimumSpoilage = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "MinimumSpoilage"));
            minimumSpoilage = Math.Max(0.1f, minimumSpoilage);

            // Worse case scenario, no matter what, Spoilage will increment.
            if (perUse <= minimumSpoilage)
            {
                strDisplay += " Minimum spoilage Detected (PerUse: " + perUse + " Minimum: " + minimumSpoilage + " )";
                perUse = minimumSpoilage;
            }

            // Calculate how many Spoils we may have missed over time. If we left our base and came back to our storage box, this will help accurately determine how much
            // spoilage should apply.
            var temp = "World Time: " + worldTime + " Minus NextSpoilageTick: " + nextTick + " Tick Per Loss: " +
                       tickPerLoss;
            AdvLogging.DisplayLog(AdvFeatureClass, temp);

            var totalSpoilageMultiplier = (ToInt(worldTime) - nextTick) / tickPerLoss;
            if (totalSpoilageMultiplier == 0)
                totalSpoilageMultiplier = 1;

            var totalSpoilage = perUse * totalSpoilageMultiplier;
            strDisplay += " Spoilage Ticks Missed: " + totalSpoilageMultiplier;
            strDisplay += " Total Spoilage: " + totalSpoilage;

            currentSpoilage = UpdateCurrentSpoilage(__instance.ItemStack.itemValue, totalSpoilage);

            // Update the NextSpoilageTick value
            var nextSpoilageTick = CalculateNextSpoilageTick(worldTime, tickPerLoss);
            SetNextSpoilageTick(__instance.ItemStack.itemValue, nextSpoilageTick);
            strDisplay += " Next Spoilage Tick: " + nextSpoilageTick;
            strDisplay += " Recorded Spoilage: " + currentSpoilage;
            AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);
  
            var freshness = false;
            if (itemClass.Properties.Contains("FreshnessOnly"))
                freshness = itemClass.Properties.GetBool("FreshnessOnly");
            if (freshness)
            {
                var perCent = 1f - Mathf.Clamp01(currentSpoilage / degradationMax);
                __instance.ItemStack.itemValue.SetMetadata("Freshness", perCent, TypedMetadataValue.TypeTag.Float);
                __instance.ForceRefreshItemStack();
                return true;
            }


            // If the spoil time is is greater than the degradation, loop around the stack, removing each layer of items.
            while (degradationMax <= currentSpoilage)
            {
                // If not defined, set the foodRottingFlesh as a spoiled product. Otherwise use the global / item.
                var strSpoiledItem = Configuration.GetPropertyValue("FoodSpoilage", "SpoiledItem");
                if (string.IsNullOrEmpty(strSpoiledItem))
                    strSpoiledItem = "foodRottingFlesh";

                if (itemClass.Properties.Contains("SpoiledItem"))
                    strSpoiledItem = itemClass.Properties.GetString("SpoiledItem");

                var player = GameManager.Instance.World.GetPrimaryPlayer();
                if (player && strSpoiledItem != "None")
                {
                    var count = 1;

                    var fullStackSpoil = false;
                    if (itemClass.Properties.Contains("FullStackSpoil"))
                        fullStackSpoil = itemClass.Properties.GetBool("FullStackSpoil");

                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, "FullStackSpoil") || fullStackSpoil)
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, itemClass.GetItemName() + ":Full Stack Spoil");
                        count = __instance.ItemStack.count;
                        __instance.ItemStack = new ItemStack(ItemClass.GetItem(strSpoiledItem, false), count);
                        break;
                    }

                    var itemStackSpoiled = new ItemStack(ItemClass.GetItem(strSpoiledItem, false), count);
                    if (itemStackSpoiled?.itemValue?.ItemClass != null &&
                        itemStackSpoiled.itemValue.ItemClass.GetItemName() != itemClass.GetItemName())
                    {
                        if (!LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory.AddItem(itemStackSpoiled, true))
                        {
                            player.world.gameManager.ItemDropServer(itemStackSpoiled, player.GetPosition(),
                                Vector3.zero, -1,
                                60f, false);
                        }
                    }
                }

                if (__instance.ItemStack.count >= 2)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, itemClass.GetItemName() + ": Reducing Stack by 1");
                    __instance.ItemStack.count--;
                    currentSpoilage = UpdateCurrentSpoilage(__instance.ItemStack.itemValue, -degradationMax);
                }
                else
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, itemClass.GetItemName() + ": Stack Depleted. Removing.");
                    __instance.ItemStack = new ItemStack(ItemValue.None.Clone(), 0);
                    break; // Nothing more to spoil
                }
            }

            // Set the current spoilage value.
            var perCentFresh = 1f - Mathf.Clamp01(currentSpoilage / degradationMax);
            __instance.ItemStack.itemValue.SetMetadata("Freshness", perCentFresh, TypedMetadataValue.TypeTag.Float);
            __instance.ForceRefreshItemStack();

            return true;
        }

        public static bool IsFresh(ItemValue itemValue) {
            if (!itemValue.HasMetadata("Freshness")) return true;
            var freshNess = (float)itemValue.GetMetadata("Freshness");
            return !(freshNess < 0.1f);
        }

        public static void Postfix(XUiC_ItemStack __instance) {
            if (IsSkippable(__instance)) return;
            var itemStack = __instance.ItemStack;
            var itemClass = itemStack.itemValue.ItemClass;

            var degradationMax = 1000f;
            if (itemClass.Properties.Contains("SpoilageMax"))
                degradationMax = itemClass.Properties.GetFloat("SpoilageMax");


            var currentSpoilage = GetCurrentSpoilage(__instance.ItemStack.itemValue);
            var perCent = 1f - Mathf.Clamp01(currentSpoilage / degradationMax);
            var tierColor = 7 + (int)Math.Round(8 * perCent);
            if (tierColor < 0)
                tierColor = 0;
            if (tierColor > 7)
                tierColor = 7;

            // allow over-riding of the color.
            if (itemClass.Properties.Contains("QualityTierColor"))
                tierColor = itemClass.Properties.GetInt("QualityTierColor");

            // These used to be fields of the instance, not in A20
            var controller = __instance.GetChildById("durability");
            if (controller?.ViewComponent is XUiV_Sprite durability)
            {
                durability.IsVisible = IsFresh(__instance.ItemStack.itemValue);
                durability.Color = QualityInfo.GetQualityColor(tierColor);
                durability.Fill = perCent;
            }

            controller = __instance.GetChildById("durabilityBackground");
            if (controller?.ViewComponent is XUiV_Sprite durabilityBackground)
            {
                durabilityBackground.IsVisible = IsFresh(__instance.ItemStack.itemValue);
            }
        }


        /// <summary>
        /// Calculates the tick for the next loss as a signed integer value.
        /// </summary>
        /// <param name="worldTime"></param>
        /// <param name="ticksPerLoss"></param>
        /// <returns></returns>
        private static int CalculateNextSpoilageTick(ulong worldTime, int ticksPerLoss) {
            ulong nextTickActual = worldTime + (ulong)ticksPerLoss;
            return ToInt(nextTickActual);
        }

        /// <summary>
        /// Gets the next spoilage tick from the item value. If not found, returns -1.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        private static int GetNextSpoilageTick(ItemValue itemValue) {
            var nextSpoilageTickObject = itemValue.GetMetadata(KeyNextSpoilageTick);
            if (nextSpoilageTickObject is int nextSpoilageTick)
            {
                return nextSpoilageTick;
            }

            return -1;
        }

        /// <summary>
        /// Sets the next spoilage tick in the item value.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="nextTick"></param>
        private static void SetNextSpoilageTick(ItemValue itemValue, int nextTick) {
            itemValue.SetMetadata(KeyNextSpoilageTick, nextTick, TypedMetadataValue.TypeTag.Integer);
        }

        /// <summary>
        /// Converts an unsigned long to a signed int by discarding high-order bits.
        /// This is "safer" than calling Convert.ToInt32 (which throws an OverflowException)
        /// or explicit casting (which results in overflow).
        /// </summary>
        /// <param name="uLong"></param>
        /// <returns></returns>
        private static int ToInt(ulong uLong) {
            return (int)(uLong & int.MaxValue);
        }
    }
}