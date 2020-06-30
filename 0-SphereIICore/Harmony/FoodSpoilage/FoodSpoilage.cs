using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
    private static string AdvFeatureClass = "FoodSpoilage";
    private static string Feature = "FoodSpoilage";


    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch("Read")]
    public class SphereII_itemValue_Read
    {
        public static void Postfix(BinaryReader _br, ref ItemValue __instance)
        {
            if (_br.PeekChar() >= 0)
            {
                __instance.CurrentSpoilage = (int)_br.ReadSingle();
                __instance.NextSpoilageTick = (int)_br.ReadSingle();
            }

        }
    }

    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch("Clone")]
    public class SphereII_itemValue_Clone
    {
        public static ItemValue Postfix(ItemValue __result, ItemValue __instance)
        {
            __result.CurrentSpoilage = __instance.CurrentSpoilage;
            __result.NextSpoilageTick = __instance.NextSpoilageTick;
            return __result;
        }
    }
    
    // NextSpoilageTick in ItemValue is an int, but when writen it gets converted over to a ushort, and re-read as an int. This could be due to refactoring of base code,
    // since none of these calls need to be ushort, change the ushort to just cast as an int.
    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch("Write")]
    public class SphereII_ItemValue_Write
    {
        public static void Postfix(BinaryWriter _bw, ItemValue __instance)
        {
            _bw.Write((float)__instance.CurrentSpoilage);
            _bw.Write((float)__instance.NextSpoilageTick);
        }
     
    }


    // hook into the ItemStack, which should cover all types of containers. This will run in the update task.
    [HarmonyPatch(typeof(XUiC_ItemStack))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_ItemStack_Update
    {

        public static void Postfix(XUiC_ItemStack __instance, bool ___bLocked, bool ___isDragAndDrop)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            // Make sure we are dealing with legitimate stacks.
            if (__instance.ItemStack.IsEmpty())
                return;

            if (__instance.ItemStack.itemValue == null)
                return;

            if (___bLocked && ___isDragAndDrop)
                return;

            //  if (__instance.ItemStack.itemValue.NextSpoilageTick < (int)GameManager.Instance.World.GetWorldTime())
            {
                if (__instance.ItemStack.itemValue.ItemClass != null && __instance.ItemStack.itemValue.ItemClass.Properties.Contains("Spoilable"))
                {
                    float DegradationMax = 1000f;
                    if (__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoilageMax"))
                        DegradationMax = __instance.ItemStack.itemValue.ItemClass.Properties.GetFloat("SpoilageMax");


                    __instance.durability.IsVisible = true;
                    __instance.durabilityBackground.IsVisible = true;
                    float PerCent = 1f - Mathf.Clamp01(__instance.ItemStack.itemValue.CurrentSpoilage / DegradationMax);
                    int TierColor = 7 + (int)Math.Round(8 * PerCent);
                    if (TierColor < 0)
                        TierColor = 0;
                    if (TierColor > 7)
                        TierColor = 7;

                    // allow over-riding of the color.
                    if(__instance.ItemStack.itemValue.ItemClass.Properties.Contains("QualityTierColor"))
                        TierColor = __instance.ItemStack.itemValue.ItemClass.Properties.GetInt("QualityTierColor");

                    __instance.durability.Color = QualityInfo.GetQualityColor(TierColor);
                    __instance.durability.Fill = PerCent;

                }
            }
        }


        public static bool Prefix(XUiC_ItemStack __instance, bool ___bLocked, bool ___isDragAndDrop)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            // Make sure we are dealing with legitimate stacks.
            if (__instance.ItemStack.IsEmpty())
                return true;

            if (__instance.ItemStack.itemValue == null)
                return true;

            if (___bLocked && ___isDragAndDrop)
                return true;

            // Reset the durability
            //__instance.durability.IsVisible = false;

            // If the item class has a SpoilageTime, that means it can spoil over time.        
            if (__instance.ItemStack.itemValue.ItemClass != null && __instance.ItemStack.itemValue.ItemClass.Properties.Contains("Spoilable"))
            {
                String strDisplay = "XUiC_ItemStack: " + __instance.ItemStack.itemValue.ItemClass.GetItemName();
                float DegradationMax = 0f;
                float DegradationPerUse = 0f;

                if (__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoilageMax"))
                    DegradationMax = __instance.ItemStack.itemValue.ItemClass.Properties.GetFloat("SpoilageMax");

                if (__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoilagePerTick"))
                    DegradationPerUse = __instance.ItemStack.itemValue.ItemClass.Properties.GetFloat("SpoilagePerTick");

                // By default, have a spoiler hit every 100 ticks, but allow it to be over-rideable in the xml.
                int TickPerLoss = 100;

                // Check if there's a Global Ticks Per Loss Set
                BlockValue ConfigurationBlock = Block.GetBlockValue("ConfigFeatureBlock");
                TickPerLoss = int.Parse(Configuration.GetPropertyValue("FoodSpoilage", "TickPerLoss"));

                // Check if there's a item-specific TickPerLoss
                if (__instance.ItemStack.itemValue.ItemClass.Properties.Contains("TickPerLoss"))
                    TickPerLoss = __instance.ItemStack.itemValue.ItemClass.Properties.GetInt("TickPerLoss");
                strDisplay += " Ticks Per Loss: " + TickPerLoss;

                // NextSpoilageTick will hold the world time + how many ticks until the next spoilage.
                if (__instance.ItemStack.itemValue.NextSpoilageTick == 0)
                    __instance.ItemStack.itemValue.NextSpoilageTick = (int)GameManager.Instance.World.GetWorldTime() + TickPerLoss;

                // Throttles the amount of times it'll trigger the spoilage, based on the TickPerLoss
                if (__instance.ItemStack.itemValue.NextSpoilageTick < (int)GameManager.Instance.World.GetWorldTime())
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
                                    PerUse -= Container.Block.Properties.GetFloat("PreserveBonus");
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
                    String temp = "World Time: " + (int)GameManager.Instance.World.GetWorldTime() + " Minus NextSpoilageTick: " + __instance.ItemStack.itemValue.NextSpoilageTick + " Tick Per Loss: " + TickPerLoss;
                    AdvLogging.DisplayLog(AdvFeatureClass, temp);

                    int TotalSpoilageMultiplier = (int)(GameManager.Instance.World.GetWorldTime() - __instance.ItemStack.itemValue.NextSpoilageTick) / TickPerLoss;
                    if (TotalSpoilageMultiplier == 0)
                        TotalSpoilageMultiplier = 1;

                    float TotalSpoilage = PerUse * TotalSpoilageMultiplier;
                    strDisplay += " Spoilage Ticks Missed: " + TotalSpoilageMultiplier;
                    strDisplay += " Total Spoilage: " + TotalSpoilage;
                    __instance.ItemStack.itemValue.CurrentSpoilage += TotalSpoilage;

                    strDisplay += " Next Spoilage Tick: " + (int)GameManager.Instance.World.GetWorldTime() + TickPerLoss;
                    strDisplay += " Recorded Spoilage: " + __instance.ItemStack.itemValue.CurrentSpoilage;
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);

                    // Update the NextSpoilageTick value
                    __instance.ItemStack.itemValue.NextSpoilageTick = (int)GameManager.Instance.World.GetWorldTime() + TickPerLoss;
                    __instance.ItemStack.itemValue.NextSpoilageTick = (int)GameManager.Instance.World.GetWorldTime() + TickPerLoss;

                    // If the spoil time is is greater than the degradation, loop around the stack, removing each layer of items.
                    while (DegradationMax <= __instance.ItemStack.itemValue.CurrentSpoilage)
                    //if(DegradationMax <= __instance.ItemStack.itemValue.CurrentSpoilage)
                    {


                        // If not defined, set the foodRottingFlesh as a spoiled product. Otherwise use the global / item.
                        String strSpoiledItem = Configuration.GetPropertyValue("FoodSpoilage", "SpoiledItem");
                        if (string.IsNullOrEmpty(strSpoiledItem))
                            strSpoiledItem = "foodRottingFlesh";

                        if (__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoiledItem"))
                            strSpoiledItem = __instance.ItemStack.itemValue.ItemClass.Properties.GetString("SpoiledItem");



                        //EntityPlayerLocal player = __instance.xui.playerUI.entityPlayer;
                        EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
                        if (player)
                        {
                            int Count = 1;

                            if (Configuration.CheckFeatureStatus(AdvFeatureClass, "FullStackSpoil"))
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass, __instance.ItemStack.itemValue.ItemClass.GetItemName() + ":Full Stack Spoil");
                                Count = __instance.ItemStack.count;
                                __instance.ItemStack = new ItemStack(ItemClass.GetItem(strSpoiledItem, false), Count);
                                break;

                            }
                            ItemStack itemStack = new ItemStack(ItemClass.GetItem(strSpoiledItem, false), Count);

                            if (itemStack.itemValue.ItemClass.GetItemName() != __instance.ItemStack.itemValue.ItemClass.GetItemName())
                            {
                                if (!LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack, true))
                                {
                                    player.world.gameManager.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero, -1, 60f, false);
                                }
                            }
                        }

                        if (__instance.ItemStack.count > 2)
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, __instance.ItemStack.itemValue.ItemClass.GetItemName() + ": Reducing Stack by 1");
                            __instance.ItemStack.count--;
                            __instance.ItemStack.itemValue.CurrentSpoilage -= DegradationMax;
                        }
                        else
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, __instance.ItemStack.itemValue.ItemClass.GetItemName() + ": Stack Depleted. Removing.");
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
    }



}


