using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;


namespace SCore.Harmony.PlayerFeatures
{
    public  class Encumbrance
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "Encumbrance";

        [HarmonyPatch(typeof(EntityPlayerLocal))]
        [HarmonyPatch("Awake")]
        public class EntityPlayerLocalInit
        {
            public static float CalculateSlots(ItemStack[] slots)
            {
                // By default, set everything as 0.1 weight (pounds? ounces? magic?  )
                var minimumEncumberance = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MinimumWeight"));

                var flTotalEncumbrance = 0f;
                int num = 0;
                while (num < slots.Length && num < slots.Length)
                {
                    if (slots[num].IsEmpty())
                    {
                        num++;
                        continue;
                    }


                    float itemWeight = minimumEncumberance;

                    // Check if the item value has an item weight attached to it, and if so, use that number instead.
                    if (ItemClass.list[slots[num].itemValue.type].Properties.Values.ContainsKey("ItemWeight"))
                    {

                        float.TryParse(ItemClass.list[slots[num].itemValue.type].Properties.Values["ItemWeight"], out itemWeight);
                    }
                    else 
                    {
                        // if it doesn't have an item weight, check its block entry.
                        var blockValue = slots[num].itemValue.ToBlockValue();
                        if ( blockValue.Block.Properties.Values.ContainsKey("ItemWeight"))
                            float.TryParse(blockValue.Block.Properties.Values["ItemWeight"], out itemWeight);
                    }

                    // Calculate the total weight of the stack
                    float flTotalWeight = itemWeight * slots[num].count;

                    flTotalEncumbrance += flTotalWeight;
                    num++;
                }
                return flTotalEncumbrance;
            }
            public static void CheckEncumbrance()
            {
                int num = 0;

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
                    ItemStack[] slots = localPlayer.bag.GetSlots();
                    var flTotalEncumbrance = CalculateSlots(slots);
                    AdvLogging.DisplayLog(AdvFeatureClass, $"\tBackpack Total encumbrance: {flTotalEncumbrance} from {slots.Length} Slots.");

                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Encumbrance_ToolBelt"))
                    {
                        var toolbeltWeight = 0f;
                        ItemStack[] toolbeltSlots = localPlayer.inventory.GetSlots();
                        toolbeltWeight = CalculateSlots(toolbeltSlots);
                        flTotalEncumbrance += toolbeltWeight;
                        AdvLogging.DisplayLog(AdvFeatureClass, $"\tToolbelt Total encumbrance: {toolbeltWeight} from {toolbeltSlots.Length} Slots.");
                    }                        

                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Encumbrance_Equipment"))
                    {
                        var equipmentWeight = 0f;
                        var items = localPlayer.equipment.GetItems();
                        foreach( var item in items )
                        {
                            if ( item == null ) continue;
                            if (ItemClass.GetForId(item.type) == null) continue;

                            var itemWeight = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "MinimumWeight"));
                            if ( item.ItemClass.Properties.Values.ContainsKey("ItemWeight"))
                                float.TryParse(item.ItemClass.Properties.Values["ItemWeight"], out itemWeight);
                            equipmentWeight += itemWeight;
                            AdvLogging.DisplayLog(AdvFeatureClass, $"\tEquipment encumbrance: {item.ItemClass.GetItemName()}   Weight: {itemWeight} ");
                        }

                        flTotalEncumbrance += equipmentWeight;
                        AdvLogging.DisplayLog(AdvFeatureClass, $"\tEquipment Total encumbrance: {equipmentWeight} from {items.Length} Slots.");
                    }

                    // 1 being at Max loaded, and anything above is extra encumberance.
                    float over = flTotalEncumbrance / flMaxEncumbrance;

                    var cvar = Configuration.GetPropertyValue(AdvFeatureClass, "EncumbranceCVar");
                    localPlayer.Buffs.AddCustomVar(cvar, over);
                    AdvLogging.DisplayLog(AdvFeatureClass, $"Total encumbrance: {flTotalEncumbrance}");


                }
                catch (Exception ex)
                {
                    Log.Out($"Encumbrance Mod Error: {ex.ToString()}");
                }
            }
            private static void Postfix(EntityPlayerLocal __instance)
            {
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
