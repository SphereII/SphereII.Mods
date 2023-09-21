using HarmonyLib;
using System;
using System.IO;
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
public class SphereII_FoodSpoilage
{
    public static readonly string KeyNextSpoilageTick = "NextSpoilageTick";
    public static readonly string PropSpoilable = "Spoilable";

    private static readonly string AdvFeatureClass = "FoodSpoilage";
    private static readonly string Feature = "FoodSpoilage";
    private static readonly bool foodSpoilage = Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);

    // hook into the ItemStack, which should cover all types of containers. This will run in the update task.
    // It is used to calculate the amount of spoilage necessary, and display the amount of freshness is left in the item.
    [HarmonyPatch(typeof(XUiC_ItemStack))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_ItemStack_Update
    {
        public static bool Prefix(XUiC_ItemStack __instance)
        {
            if (!foodSpoilage)
                return true;

            // Make sure we are dealing with legitimate stacks.
            if (__instance.ItemStack.IsEmpty())
                return true;

            if (__instance.ItemStack.itemValue == null)
                return true;

            if (__instance.IsLocked && __instance.IsDragAndDrop)
                return true;

            var completePreserve = false;

            // Reset the durability
            //__instance.durability.IsVisible = false;

            // If the item class has a Spoilable property, that means it can spoil over time.
            var itemClass = __instance.ItemStack.itemValue.ItemClass;
            if (itemClass != null && itemClass.Properties.Contains(PropSpoilable) && itemClass.Properties.GetBool(PropSpoilable))
            {
                String strDisplay = "XUiC_ItemStack: " + itemClass.GetItemName();
                float DegradationMax = 0f;
                float DegradationPerUse = 0f;

                if (itemClass.Properties.Contains("SpoilageMax"))
                    DegradationMax = itemClass.Properties.GetFloat("SpoilageMax");

                if (itemClass.Properties.Contains("SpoilagePerTick"))
                    DegradationPerUse = itemClass.Properties.GetFloat("SpoilagePerTick");

                // By default, have a spoiler hit every 100 ticks, but allow it to be over-rideable in the xml.
                int TickPerLoss = 100;

                // Check if there's a Global Ticks Per Loss Set
                BlockValue ConfigurationBlock = Block.GetBlockValue("ConfigFeatureBlock");
                TickPerLoss = int.Parse(Configuration.GetPropertyValue("FoodSpoilage", "TickPerLoss"));

                // Check if there's a item-specific TickPerLoss
                if (itemClass.Properties.Contains("TickPerLoss"))
                    TickPerLoss = itemClass.Properties.GetInt("TickPerLoss");
                strDisplay += " Ticks Per Loss: " + TickPerLoss;

                var worldTime = GameManager.Instance.World.GetWorldTime();
                int nextTick = GetNextSpoilageTick(__instance);
                if (nextTick <= 0)
                {
                    nextTick = CalculateNextSpoilageTick(worldTime, TickPerLoss);
                    SetNextSpoilageTick(__instance, nextTick);
                }

                // Throttles the amount of times it'll trigger the spoilage, based on the TickPerLoss
                if (nextTick < ToInt(worldTime))
                {
                    // How much spoilage to apply 
                    float PerUse = DegradationPerUse;

                    // Check if there's a player involved, which could change the spoilage rate.
                    //EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
                    //if(player)
                    //   PerUse = EffectManager.GetValue(PassiveEffects.DegradationPerUse, __instance.ItemStack.itemValue, 1f, player, null, __instance.ItemStack.itemValue.ItemClass.ItemTags, true, true, true, true, 1, true);
                    //else
                    //   PerUse = EffectManager.GetValue(PassiveEffects.DegradationPerUse, __instance.ItemStack.itemValue, 1f, null, null, __instance.ItemStack.itemValue.ItemClass.ItemTags, true, true, true, true, 1, true);

                    float BasePerUse = PerUse;
                    strDisplay += " Base Spoil: " + PerUse;

                    
                    float containerValue = 0;
                    // Additional Spoiler flags to increase or decrease the spoil rate
                    switch (__instance.StackLocation)
                    {
                        case XUiC_ItemStack.StackLocationTypes.ToolBelt:  // Tool belt Storage check
                            containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Toolbelt"));
                            strDisplay += " Storage Type: Tool Belt ( " + containerValue + " )";
                            PerUse += containerValue;
                            break;
                        case XUiC_ItemStack.StackLocationTypes.Backpack:        // Back pack storage check
                            containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Backpack"));
                            strDisplay += " Storage Type: Backpack ( " + containerValue + " )";
                            PerUse += containerValue;
                            break;
                        case XUiC_ItemStack.StackLocationTypes.LootContainer:    // Loot Container Storage check
                            TileEntityLootContainer container = __instance.xui.lootContainer;
                            if (container != null)
                            {
                                BlockValue Container = GameManager.Instance.World.GetBlock(container.ToWorldPos());
                                String lootContainerName = Localization.Get(Block.list[Container.type].GetBlockName());
                                strDisplay += " " + lootContainerName;

                                containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Container"));
                                strDisplay += " Storage Type: Container ( " + containerValue + " )";
                                PerUse += containerValue;

                                if (Container.Block.Properties.Contains("PreserveBonus"))
                                {
                                    strDisplay += " Preservation Bonus ( " + Container.Block.Properties.GetFloat("PreserveBonus") + " )";
                                    var preserveBonus = Container.Block.Properties.GetFloat("PreserveBonus");
                                    if (preserveBonus == -99f)
                                        completePreserve = true;

                                    PerUse -= preserveBonus;
                                }
                            }
                            else
                            {
                                strDisplay += " Storage Type: Container ( Undefined Configuration Block: +10 )";
                                PerUse += 10;
                            }
                            break;
                        case XUiC_ItemStack.StackLocationTypes.Creative:  // Ignore Creative Containers
                            return true;
                        default:
                            containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Container"));
                            strDisplay += " Storage Type: Generic ( Default Container) ( " + containerValue + " )";
                            PerUse += containerValue;
                            break;
                    }


                    strDisplay += " Spoiled This Tick: " + (PerUse - BasePerUse);
                    float MinimumSpoilage = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "MinimumSpoilage"));
                    MinimumSpoilage = Math.Max(0.1f, MinimumSpoilage);

                    // Worse case scenario, no matter what, Spoilage will increment.
                    if (PerUse <= MinimumSpoilage)
                    {
                        strDisplay += " Minimum spoilage Detected (PerUse: " + PerUse + " Minimum: " + MinimumSpoilage + " )";
                        PerUse = MinimumSpoilage;
                    }
                    // Calculate how many Spoils we may have missed over time. If we left our base and came back to our storage box, this will help accurately determine how much
                    // spoilage should apply.
                    String temp = "World Time: " + worldTime + " Minus NextSpoilageTick: " + nextTick + " Tick Per Loss: " + TickPerLoss;
                    AdvLogging.DisplayLog(AdvFeatureClass, temp);

                    int TotalSpoilageMultiplier = (ToInt(worldTime) - nextTick) / TickPerLoss;
                    if (TotalSpoilageMultiplier == 0)
                        TotalSpoilageMultiplier = 1;

                    float TotalSpoilage = PerUse * TotalSpoilageMultiplier;
                    strDisplay += " Spoilage Ticks Missed: " + TotalSpoilageMultiplier;
                    strDisplay += " Total Spoilage: " + TotalSpoilage;

                    // If we don't want any degradation, skip this step.
                    if (!completePreserve)
                        __instance.ItemStack.itemValue.UseTimes += TotalSpoilage;

                    // Update the NextSpoilageTick value
                    int NextSpoilageTick = CalculateNextSpoilageTick(worldTime, TickPerLoss);
                    SetNextSpoilageTick(__instance, NextSpoilageTick);

                    strDisplay += " Next Spoilage Tick: " + NextSpoilageTick;
                    strDisplay += " Recorded Spoilage: " + __instance.ItemStack.itemValue.UseTimes;
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);

                    // If the spoil time is is greater than the degradation, loop around the stack, removing each layer of items.
                    while (DegradationMax <= __instance.ItemStack.itemValue.UseTimes)
                    {
                        // If not defined, set the foodRottingFlesh as a spoiled product. Otherwise use the global / item.
                        String strSpoiledItem = Configuration.GetPropertyValue("FoodSpoilage", "SpoiledItem");
                        if (string.IsNullOrEmpty(strSpoiledItem))
                            strSpoiledItem = "foodRottingFlesh";

                        if (itemClass.Properties.Contains("SpoiledItem"))
                            strSpoiledItem = itemClass.Properties.GetString("SpoiledItem");

                        //EntityPlayerLocal player = __instance.xui.playerUI.entityPlayer;
                        var player = GameManager.Instance.World.GetPrimaryPlayer();
                        if (player && strSpoiledItem != "None")
                        {
                            var Count = 1;

                            if (Configuration.CheckFeatureStatus(AdvFeatureClass, "FullStackSpoil"))
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass, itemClass.GetItemName() + ":Full Stack Spoil");
                                Count = __instance.ItemStack.count;
                                __instance.ItemStack = new ItemStack(ItemClass.GetItem(strSpoiledItem, false), Count);
                                break;
                            }

                            ItemStack itemStack = new ItemStack(ItemClass.GetItem(strSpoiledItem, false), Count);

                            if (itemStack?.itemValue?.ItemClass != null && itemStack.itemValue.ItemClass.GetItemName() != itemClass.GetItemName())
                            {
                                if (!LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory.AddItem(itemStack, true))
                                {
                                    player.world.gameManager.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero, -1, 60f, false);
                                }
                            }
                        }

                        if (__instance.ItemStack.count > 2)
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, itemClass.GetItemName() + ": Reducing Stack by 1");
                            __instance.ItemStack.count--;
                            __instance.ItemStack.itemValue.UseTimes -= DegradationMax;
                        }
                        else
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, itemClass.GetItemName() + ": Stack Depleted. Removing.");
                            __instance.ItemStack = new ItemStack(ItemValue.None.Clone(), 0);
                            break;  // Nothing more to spoil
                        }
                        // break;
                    }
                    __instance.ForceRefreshItemStack();
                }
            }

            return true;

        }

        public static void Postfix(XUiC_ItemStack __instance)
        {
            if (!foodSpoilage)
                return;

            // Make sure we are dealing with legitimate stacks.
            if (__instance.ItemStack.IsEmpty())
                return;

            if (__instance.ItemStack.itemValue == null)
                return;

            if (__instance.IsLocked && __instance.IsDragAndDrop)
                return;

            //  if (__instance.ItemStack.itemValue.Meta < ToInt(GameManager.Instance.World.GetWorldTime()))
            {
                var itemClass = __instance.ItemStack.itemValue.ItemClass;
                if (itemClass != null && itemClass.Properties.Contains(PropSpoilable) && itemClass.Properties.GetBool(PropSpoilable))
                {
                    float DegradationMax = 1000f;
                    if (itemClass.Properties.Contains("SpoilageMax"))
                        DegradationMax = itemClass.Properties.GetFloat("SpoilageMax");

                    float PerCent = 1f - Mathf.Clamp01(__instance.ItemStack.itemValue.UseTimes / DegradationMax);
                    int TierColor = 7 + (int)Math.Round(8 * PerCent);
                    if (TierColor < 0)
                        TierColor = 0;
                    if (TierColor > 7)
                        TierColor = 7;

                    // allow over-riding of the color.
                    if (itemClass.Properties.Contains("QualityTierColor"))
                        TierColor = itemClass.Properties.GetInt("QualityTierColor");

                    // These used to be fields of the instance, not in A20
                    var controller = __instance.GetChildById("durability");
                    if (controller != null && controller.ViewComponent is XUiV_Sprite durability)
                    {
                        durability.IsVisible = true;
                        durability.Color = QualityInfo.GetQualityColor(TierColor);
                        durability.Fill = PerCent;
                    }

                    controller = __instance.GetChildById("durabilityBackground");
                    if (controller != null && controller.ViewComponent is XUiV_Sprite durabilityBackground)
                    {
                        durabilityBackground.IsVisible = true;
                    }
                }
            }
        }




        /// <summary>
        /// Calculates the tick for the next loss as a signed integer value.
        /// </summary>
        /// <param name="worldTime"></param>
        /// <param name="ticksPerLoss"></param>
        /// <returns></returns>
        private static int CalculateNextSpoilageTick(ulong worldTime, int ticksPerLoss)
        {
            ulong nextTickActual = worldTime + (ulong)ticksPerLoss;
            return ToInt(nextTickActual);
        }

        /// <summary>
        /// Gets the next spoilage tick from the item value. If not found, returns -1.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        private static int GetNextSpoilageTick(XUiC_ItemStack __instance)
        {
            if (__instance.ItemStack.itemValue.HasMetadata(KeyNextSpoilageTick, TypedMetadataValue.TypeTag.Integer))
            {
                return (int)__instance.ItemStack.itemValue.GetMetadata(KeyNextSpoilageTick);
            }

            return -1;
        }

        /// <summary>
        /// Sets the next spoilage tick in the item value.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="nextTick"></param>
        private static void SetNextSpoilageTick(XUiC_ItemStack __instance, int nextTick)
        {
            __instance.ItemStack.itemValue.SetMetadata(
                KeyNextSpoilageTick,
                nextTick,
                TypedMetadataValue.TypeTag.Integer);
        }

        /// <summary>
        /// Converts an unsigned long to a signed int by discarding high-order bits.
        /// This is "safer" than calling Convert.ToInt32 (which throws an OverflowException)
        /// or explicit casting (which results in overflow).
        /// </summary>
        /// <param name="uLong"></param>
        /// <returns></returns>
        private static int ToInt(ulong uLong)
        {
            return (int)(uLong & int.MaxValue);
        }
    }
}
