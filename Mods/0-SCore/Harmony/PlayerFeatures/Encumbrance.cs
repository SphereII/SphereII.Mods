using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;


namespace SCore.Harmony.PlayerFeatures {
    public class Encumbrance {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "Encumbrance";

        [HarmonyPatch(typeof(EntityPlayerLocal))]
        [HarmonyPatch("Awake")]
        public class EntityPlayerLocalInit {
            public static float CalculateSlots(ItemStack[] slots) {
                var flTotalEncumbrance = 0f;
                int num = 0;
                while (num < slots.Length && num < slots.Length)
                {
                    if (slots[num].IsEmpty())
                    {
                        num++;
                        continue;
                    }

                    var itemValue = slots[num].itemValue;
                    var itemWeight = GetWeightValue(itemValue);

                    // calculate the mods, if available. Since Items with mods don't stack, this won't effect
                    // the calculate below for the total weight.
                    foreach (var mod in itemValue.Modifications)
                    {
                        itemWeight += GetWeightValue(mod);
                    }

                    // Calculate the total weight of the stack
                    float flTotalWeight = itemWeight * slots[num].count;

                    flTotalEncumbrance += flTotalWeight;
                    num++;
                }

                return flTotalEncumbrance;
            }

            private static float GetWeightValue(ItemValue itemValue) {
                var defaultWeight = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MinimumWeight"));

                if ( itemValue == null || itemValue.IsEmpty()) return 0;

                var properties = itemValue.ItemClass.Properties;
 
                float itemWeight;
                if (properties.Values.ContainsKey("ItemWeight"))
                {
                    if (float.TryParse(properties.Values["ItemWeight"], out itemWeight))
                        return itemWeight;
                }
                else
                {
                    // if it doesn't have an item weight, check its block entry.
                    var blockValue = itemValue.ToBlockValue();
                    if (blockValue.Block.Properties.Values.ContainsKey("ItemWeight"))
                    {
                        if (float.TryParse(blockValue.Block.Properties.Values["ItemWeight"], out itemWeight))
                            return itemWeight;
                    }
                }

                return defaultWeight;
            }

            public static void CheckEncumbrance() {
                var localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                if (localPlayer == null) return;

                var flDefaultMax = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MaxEncumbrance"));
                var flMaxEncumbrance = flDefaultMax;
                if (localPlayer.Buffs.HasCustomVar("MaxEncumbrance"))
                    flMaxEncumbrance = localPlayer.Buffs.GetCustomVar("MaxEncumbrance");
                // Safety check. If the encumbrance goes to a negative value, reset it.
                if (flMaxEncumbrance < 0)
                {
                    Log.Out($"Max Encumbrance is below 0. Using {flDefaultMax}");
                    flMaxEncumbrance = flDefaultMax;
                }

                try
                {
                    var slots = localPlayer.bag.GetSlots();
                    var flTotalEncumbrance = CalculateSlots(slots);
                    AdvLogging.DisplayLog(AdvFeatureClass,
                        $"\tBackpack Total encumbrance: {flTotalEncumbrance} from {slots.Length} Slots.");

                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Encumbrance_ToolBelt"))
                    {
                        var toolbeltWeight = 0f;
                        var toolbeltSlots = localPlayer.inventory.GetSlots();
                        toolbeltWeight = CalculateSlots(toolbeltSlots);
                        flTotalEncumbrance += toolbeltWeight;
                        AdvLogging.DisplayLog(AdvFeatureClass,
                            $"\tTool belt Total encumbrance: {toolbeltWeight} from {toolbeltSlots.Length} Slots.");
                    }

                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Encumbrance_Equipment"))
                    {
                        var equipmentWeight = 0f;
                        var items = localPlayer.equipment.GetItems();
                        foreach (var item in items)
                        {
                            if (item == null) continue;
                            if (ItemClass.GetForId(item.type) == null) continue;

                            var itemWeight = GetWeightValue(item);
                            equipmentWeight += itemWeight;
                            foreach (var mod in item.Modifications)
                            {
                                if ( mod == null || mod.IsEmpty()) continue;
                                
                                var modWeight = GetWeightValue(mod);
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    $"\tEquipment has a mod:: {mod.ItemClass.GetItemName()}   Weight: {modWeight} ");
                                equipmentWeight += modWeight;
                            }

                            AdvLogging.DisplayLog(AdvFeatureClass,
                                $"\tEquipment encumbrance: {item.ItemClass.GetItemName()}   Weight: {itemWeight} ");
                        }

                        flTotalEncumbrance += equipmentWeight;
                        AdvLogging.DisplayLog(AdvFeatureClass,
                            $"\tEquipment Total encumbrance: {equipmentWeight} from {items.Length} Slots.");
                    }

                    // 1 being at Max loaded, and anything above is extra encumberance.
                    var over = flTotalEncumbrance / flMaxEncumbrance;

                    var cvar = Configuration.GetPropertyValue(AdvFeatureClass, "EncumbranceCVar");
                    localPlayer.Buffs.AddCustomVar(cvar, over);
                    AdvLogging.DisplayLog(AdvFeatureClass, $"Total encumbrance: {flTotalEncumbrance}");
                }
                catch (Exception ex)
                {
                    Log.Out($"Encumbrance Mod Error: {ex.ToString()}");
                }
            }

            private static void Postfix(EntityPlayerLocal __instance) {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                AdvLogging.DisplayLog(AdvFeatureClass, "Activating Encumbrance For Bag");
                __instance.bag.OnBackpackItemsChangedInternal += CheckEncumbrance;

                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Encumbrance_ToolBelt"))
                    __instance.inventory.OnToolbeltItemsChangedInternal += CheckEncumbrance;

                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Encumbrance_Equipment"))
                    __instance.equipment.OnChanged += CheckEncumbrance;
            }
        }
    }
}