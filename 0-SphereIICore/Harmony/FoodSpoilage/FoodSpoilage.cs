using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;


public class FoodSpoilage_Mod
{
    private static string AdvFeatureClass = "FoodSpoilage";
    private static string Feature = "FoodSpoilage";

    // Meta in ItemValue is an int, but when writen it gets converted over to a ushort, and re-read as an int. This could be due to refactoring of base code,
    // since none of these calls need to be ushort, change the ushort to just cast as an int.
    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch("Write")]
    public class SphereII_ItemValue_Write
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);

            for(int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Conv_U2)
                    codes[i].opcode = OpCodes.Conv_I;
            }

            return codes.AsEnumerable();
        }
    }

    // hook into the ItemStack, which should cover all types of containers. This will run in the update task.
    [HarmonyPatch(typeof(XUiC_ItemStack))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_ItemStack_Update
    {

        public static void Postfix(XUiC_ItemStack __instance, bool ___bLocked, bool ___isDragAndDrop)
        {
            if(!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            // Make sure we are dealing with legitimate stacks.
            if(__instance.ItemStack.IsEmpty())
                return;

            if(__instance.ItemStack.itemValue == null)
                return;

            if(___bLocked && ___isDragAndDrop)
                return;

            //  if (__instance.ItemStack.itemValue.Meta < (int)GameManager.Instance.World.GetWorldTime())
            {
                if(__instance.ItemStack.itemValue.ItemClass != null && __instance.ItemStack.itemValue.ItemClass.Properties.Contains("Spoilable"))
                {
                    float DegradationMax = 1000f;
                    if(__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoilageMax"))
                        DegradationMax = __instance.ItemStack.itemValue.ItemClass.Properties.GetFloat("SpoilageMax");

                    __instance.durability.IsVisible = true;
                    __instance.durabilityBackground.IsVisible = true;
                    float PerCent = 1f - Mathf.Clamp01(__instance.ItemStack.itemValue.UseTimes / DegradationMax);
                    int TierColor = 7 + (int)Math.Round(8 * PerCent);
                    if(TierColor < 0)
                        TierColor = 0;
                    if(TierColor > 7)
                        TierColor = 7;

                    __instance.durability.Color = QualityInfo.GetQualityColor(TierColor);
                    __instance.durability.Fill = PerCent;

                }
            }
        }


        public static bool Prefix(XUiC_ItemStack __instance, bool ___bLocked, bool ___isDragAndDrop)
        {
            if(!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            // Make sure we are dealing with legitimate stacks.
            if(__instance.ItemStack.IsEmpty())
                return true;

            if(__instance.ItemStack.itemValue == null)
                return true;

            if(___bLocked && ___isDragAndDrop)
                return true;

            // Reset the durability
            //__instance.durability.IsVisible = false;

            // If the item class has a SpoilageTime, that means it can spoil over time.        
            if(__instance.ItemStack.itemValue.ItemClass != null && __instance.ItemStack.itemValue.ItemClass.Properties.Contains("Spoilable"))
            {
                String strDisplay = "XUiC_ItemStack: " + __instance.ItemStack.itemValue.ItemClass.GetItemName();
                float DegradationMax = 0f;
                float DegradationPerUse = 0f;

                if(__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoilageMax"))
                    DegradationMax = __instance.ItemStack.itemValue.ItemClass.Properties.GetFloat("SpoilageMax");

                if(__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoilagePerTick"))
                    DegradationPerUse = __instance.ItemStack.itemValue.ItemClass.Properties.GetFloat("SpoilagePerTick");

                // By default, have a spoiler hit every 100 ticks, but allow it to be over-rideable in the xml.
                int TickPerLoss = 100;

                // Check if there's a Global Ticks Per Loss Set
                BlockValue ConfigurationBlock = Block.GetBlockValue("ConfigFeatureBlock");
                TickPerLoss = int.Parse(Configuration.GetPropertyValue("FoodSpoilage", "TickPerLoss"));

                // Check if there's a item-specific TickPerLoss
                if(__instance.ItemStack.itemValue.ItemClass.Properties.Contains("TickPerLoss"))
                    TickPerLoss = __instance.ItemStack.itemValue.ItemClass.Properties.GetInt("TickPerLoss");
                strDisplay += " Ticks Per Loss: " + TickPerLoss;

                // Meta will hold the world time + how many ticks until the next spoilage.
                if(__instance.ItemStack.itemValue.Meta == 0)
                    __instance.ItemStack.itemValue.Meta = (int)GameManager.Instance.World.GetWorldTime() + TickPerLoss;

                // Throttles the amount of times it'll trigger the spoilage, based on the TickPerLoss
                if(__instance.ItemStack.itemValue.Meta < (int)GameManager.Instance.World.GetWorldTime())
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
                    switch(__instance.StackLocation)
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
                            if(container != null)
                            {
                                BlockValue Container = GameManager.Instance.World.GetBlock(container.ToWorldPos());
                                String lootContainerName = Localization.Get(Block.list[Container.type].GetBlockName(), string.Empty);
                                strDisplay += " " + lootContainerName;

                                containerValue = float.Parse(Configuration.GetPropertyValue("FoodSpoilage", "Container"));
                                strDisplay += " Storage Type: Container ( " + containerValue + " )";
                                PerUse += containerValue;

                                if(Container.Block.Properties.Contains("PreserveBonus"))
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
                    if(PerUse <= MinimumSpoilage)
                    {
                        strDisplay += " Minimum spoilage Detected (PerUse: " + PerUse + " Minimum: " + MinimumSpoilage + " )";
                        PerUse = MinimumSpoilage;
                    }
                    // Calculate how many Spoils we may have missed over time. If we left our base and came back to our storage box, this will help accurately determine how much
                    // spoilage should apply.
                    int TotalSpoilageMultiplier = ((int)GameManager.Instance.World.GetWorldTime() - __instance.ItemStack.itemValue.Meta) / TickPerLoss;
                    if(TotalSpoilageMultiplier == 0)
                        TotalSpoilageMultiplier = 1;

                    float TotalSpoilage = PerUse * TotalSpoilageMultiplier;
                    strDisplay += " Spoilage Ticks Missed: " + TotalSpoilageMultiplier;
                    strDisplay += " Total Spoilage: " + TotalSpoilage;
                    __instance.ItemStack.itemValue.UseTimes += TotalSpoilage;

                    strDisplay += " Next Spoilage Tick: " + (int)GameManager.Instance.World.GetWorldTime() + TickPerLoss;
                    strDisplay += " Recorded Spoilage: " + __instance.ItemStack.itemValue.UseTimes;
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);

                    // Update the Meta value
                    __instance.ItemStack.itemValue.Meta = (int)GameManager.Instance.World.GetWorldTime() + TickPerLoss;

                    // If the spoil time is is greater than the degradation, loop around the stack, removing each layer of items.
                    while(DegradationMax <= __instance.ItemStack.itemValue.UseTimes)
                    //if(DegradationMax <= __instance.ItemStack.itemValue.UseTimes)
                    {
                        __instance.ItemStack.itemValue.UseTimes -= DegradationMax;

                        // If not defined, set the foodRottingFlesh as a spoiled product. Otherwise use the global / item.
                        String strSpoiledItem = Configuration.GetPropertyValue("FoodSpoilage", "SpoiledItem");
                        if ( string.IsNullOrEmpty( strSpoiledItem))
                            strSpoiledItem = "foodRottingFlesh";

                        if(__instance.ItemStack.itemValue.ItemClass.Properties.Contains("SpoiledItem"))
                            strSpoiledItem = __instance.ItemStack.itemValue.ItemClass.Properties.GetString("SpoiledItem");


                        EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
                        if(player)
                        {
                            ItemStack itemStack = new ItemStack(ItemClass.GetItem(strSpoiledItem, false), 1);
                            if(itemStack.itemValue.ItemClass.GetItemName() != __instance.ItemStack.itemValue.ItemClass.GetItemName())
                                if(!LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack, true))
                                {
                                    player.world.gameManager.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero, -1, 60f, false);
                                }
                        }
                        if(__instance.ItemStack.count > 2)
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, __instance.ItemStack.itemValue.ItemClass.GetItemName() + ": Reducing Stack by 1");
                            __instance.ItemStack.count--;
                        }
                        else
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, __instance.ItemStack.itemValue.ItemClass.GetItemName() + ": Stack Depleted. Removing.");
                            __instance.ItemStack = new ItemStack(ItemValue.None.Clone(), 0);
                            break;  // Nothing more to spoil
                        }




                    }
                    __instance.ForceRefreshItemStack();
                }
            }

            return true;

        }
    }



}


