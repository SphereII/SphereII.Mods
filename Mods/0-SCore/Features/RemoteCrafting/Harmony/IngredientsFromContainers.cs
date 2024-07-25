using System;
using HarmonyLib;
using System.Collections.Generic;
using SCore.Features.RemoteCrafting.Scripts;
using UnityEngine;

namespace Features.RemoteCrafting
{
    /// <summary>
    /// Patches to support the Crafting From Remote Storage
    /// </summary>
    public class EnhancedRecipeLists
    {
        private const string AdvFeatureClass = "AdvancedRecipes";
        private const string Feature = "ReadFromContainers";

        /// <summary>
        /// Used to determine which recipes the player can craft based on the availability of ingredients in local containers. 
        /// </summary>
        [HarmonyPatch(typeof(XUiC_RecipeList))]
        [HarmonyPatch("BuildRecipeInfosList")]
        public class BuildRecipeInfosList
        {
            public static bool Prefix(XUiC_RecipeList __instance, ref List<ItemStack> _items)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                var player = __instance.xui.playerUI.entityPlayer;
                _items.AddRange(RemoteCraftingUtils.SearchNearbyContainers(player));
                return true;
            }
        }

        /// <summary>
        /// Extends what is considered to be in the player's backpack / tool belt to include local containers.
        /// </summary>
        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("GetAllItemStacks")]
        public class GetAllItemStacks
        {
            public static void Postfix(ref List<ItemStack> __result, EntityPlayerLocal ___localPlayer)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                __result.AddRange(RemoteCraftingUtils.SearchNearbyContainers(___localPlayer));
            }
        }

        /// <summary>
        /// Hijacks the GetBindingValue so we can get an accurate count of all the items we have available, including from local storage.
        /// </summary>
        [HarmonyPatch(typeof(XUiC_IngredientEntry))]
        [HarmonyPatch("GetBindingValue")]
        public class GetBindingValue
        {
            public static bool Prefix(XUiC_IngredientEntry __instance, ref bool __result, ref string value,
                string bindingName, CachedStringFormatter<int> ___needcountFormatter,
                CachedStringFormatter<int> ___havecountFormatter, bool ___materialBased, ItemStack ___ingredient,
                string ___material, XUiC_RecipeCraftCount ___craftCountControl,
                CachedStringFormatterXuiRgbaColor ___itemicontintcolorFormatter)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                var flag = ___ingredient != null;
                switch (bindingName)
                {
                    case "haveneedcount":
                    {
                        var text = (flag
                            ? ___needcountFormatter.Format(___ingredient.count * ___craftCountControl.Count)
                            : "");
                        var value1 = 0;
                        var childByType = __instance.WindowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
                        if (childByType != null)
                        {
                            if (___materialBased)
                            {
                                value = (flag
                                    ? (___havecountFormatter.Format(childByType.GetWeight(___material)) + "/" + text)
                                    : "");
                            }
                            else
                            {
                                value = (flag
                                    ? (___havecountFormatter.Format(
                                           __instance.xui.PlayerInventory.GetItemCount(___ingredient.itemValue)) + "/" +
                                       text)
                                    : "");
                            }
                        }
                        else
                        {
                            var childByType2 = __instance.WindowGroup.Controller
                                .GetChildByType<XUiC_WorkstationInputGrid>();
                            if (childByType2 != null)
                            {
                                value = (flag
                                    ? (___havecountFormatter.Format(
                                        childByType2.GetItemCount(___ingredient.itemValue)) + "/" + text)
                                    : "");
                            }
                            else
                            {
                                value = (flag
                                    ? (___havecountFormatter.Format(
                                           __instance.xui.PlayerInventory.GetItemCount(___ingredient.itemValue)) + "/" +
                                       text)
                                    : "");
                                if (flag)
                                {
                                    // add items from lootcontainers
                                    value1 = __instance.xui.PlayerInventory.GetItemCount(___ingredient.itemValue);
                                    var array = RemoteCraftingUtils.SearchNearbyContainers(__instance.xui.playerUI.entityPlayer,
                                        ___ingredient.itemValue).ToArray();
                                    foreach (var t in array)
                                    {
                                        if (t != null && t.itemValue.type != 0 &&
                                            ___ingredient.itemValue.type == t.itemValue.type)
                                        {
                                            value1 += t.count;
                                        }
                                    }

                                    value = ___havecountFormatter.Format(value1) + "/" + text;
                                }
                            }
                        }

                        __result = true;
                        return false;
                    }
                    default:
                        return true;
                }
            }
        }

        /// <summary>
        /// Expands the HasItems search to include local containers.
        /// </summary>
        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("HasItems")]
        public class HasItems
        {
            public static bool Postfix(bool __result, IList<ItemStack> _itemStacks, EntityPlayerLocal ___localPlayer,
                int _multiplier)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return __result;

                if (__result) return true;

                // We need to make sure we satisfy all of the items.
                var itemsWeHave = 0;
                foreach (var itemStack in _itemStacks)
                {
                    var totalCount = 0;
                    // This is how many we need.
                    var num = itemStack.count * _multiplier;
                    // check player inventory
                    var slots = ___localPlayer.bag.GetSlots();
                    foreach (var entry in slots)
                    {
                        if (entry.IsEmpty()) continue;
                        if (itemStack.itemValue.GetItemOrBlockId() != entry.itemValue.GetItemOrBlockId()) continue;
                        totalCount += entry.count;
                        // We have enough.
                        if (totalCount >= num)
                        {
                            break;
                        }

                    }
                    // We have enough.
                    if (totalCount >= num)
                    {
                        itemsWeHave++;
                        continue;
                    }

                    // Check the toolbelt now.
                    slots = ___localPlayer.inventory.GetSlots();
                    foreach (var entry in slots)
                    {
                        if (entry.IsEmpty()) continue;
                        if (itemStack.itemValue.GetItemOrBlockId() != entry.itemValue.GetItemOrBlockId()) continue;
                        totalCount += entry.count;
                        // We have enough.
                        if (totalCount >= num)
                            break;
                    }

                    // We have enough.
                    if (totalCount >= num)
                    {
                        itemsWeHave++;
                    continue;
                    
                    }

                    // check container
                    var containers = RemoteCraftingUtils.SearchNearbyContainers(___localPlayer, itemStack.itemValue);
                    foreach (var stack in containers)
                    {
                        if (stack.IsEmpty()) continue;
                        if (itemStack.itemValue.GetItemOrBlockId() != stack.itemValue.GetItemOrBlockId()) continue;
                        totalCount += stack.count;
                        // We have enough.
                        if (totalCount >= num)
                            break;
                    }

                    // We don't have enough for this.
                    if (totalCount < num)
                    {
                        return false;
                    }
                    if (totalCount >= num)
                    {
                        itemsWeHave++;
                    }
                }

                return itemsWeHave >= _itemStacks.Count;
            }
        }

        /// <summary>
        /// Removes from local storage containers items which we've consumed.
        /// </summary>
        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("RemoveItems")]
        public class RemoveItems
        {
            public static bool Prefix(XUiM_PlayerInventory __instance, IList<ItemStack> _itemStacks, EntityPlayerLocal ___localPlayer, int _multiplier,  IList<ItemStack> _removedItems
            ,	Bag ___backpack, Inventory ___toolbelt)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
         
                RemoteCraftingUtils.ConsumeItem(_itemStacks, ___localPlayer, _multiplier,  _removedItems, ___backpack, ___toolbelt);
                return false;
            }
        }

        // Code from OCB7D2D/OcbPinRecipes
        // Patch world unload to cleanup and save on exit
        [HarmonyPatch(typeof(World))]
        [HarmonyPatch("UnloadWorld")]
        public class WorldUnloadWorld
        {
            static void Postfix()
            {
                if (!Broadcastmanager.HasInstance) return;
                Broadcastmanager.Cleanup();
            }
        }
    }

   
}