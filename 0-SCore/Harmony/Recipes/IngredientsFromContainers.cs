using HarmonyLib;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAI;
using UnityEngine;

namespace SCore.Harmony.Recipes
{

    public class EnhancedRecipeLists
    {

        private static readonly string AdvFeatureClass = "AdvancedRecipes";
        private static readonly string Feature = "ReadFromContainers";
        public static List<TileEntity> GetTileEntities(EntityAlive player)
        {
            var distance = 30f;
            var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
            if (!string.IsNullOrEmpty(strDistance))
                distance = StringParsers.ParseFloat(strDistance);

            var tileEntities = new List<TileEntity>();
            var _targetTypes = "Loot, SecureLoot, SecureLootSigned";
            var paths = SCoreUtils.ScanForTileEntities(player, _targetTypes, true);
            foreach (var path in paths)
            {
                var distanceToLeader = Vector3.Distance(player.position, path);
                if (distanceToLeader < distance)
                {
                    var tileEntity = player.world.GetTileEntity(0, new Vector3i(path));
                    if (tileEntity == null) continue;
                    switch (tileEntity.GetTileEntityType())
                    {
                        case TileEntityType.Loot:
                            var lootTileEntity = tileEntity as TileEntityLootContainer;
                            if (lootTileEntity == null) break;
                            tileEntities.Add(tileEntity);
                            break;

                        case TileEntityType.SecureLootSigned:
                        case TileEntityType.SecureLoot:

                            var secureTileEntity = tileEntity as TileEntitySecureLootContainer;
                            if (secureTileEntity == null) break;

                            PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
                            if (secureTileEntity.IsUserAllowed(internalLocalUserIdentifier) == false)
                                break;

                            tileEntities.Add(tileEntity);
                            break;

                        default:
                            break;
                    }

                }
            }
            return tileEntities;
        }
        public static List<ItemStack> SearchNearbyContainers(EntityAlive player)
        {
            var _items = new List<ItemStack>();
            var tileEntities = EnhancedRecipeLists.GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                switch (tileEntity.GetTileEntityType())
                {
                    case TileEntityType.Loot:
                        var lootTileEntity = tileEntity as TileEntityLootContainer;
                        if (lootTileEntity == null) break;
                        _items.AddRange(lootTileEntity.GetItems());
                        break;
                    case TileEntityType.SecureLootSigned:
                    case TileEntityType.SecureLoot:

                        var secureTileEntity = tileEntity as TileEntitySecureLootContainer;
                        if (secureTileEntity == null) break;

                        PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
                        if (secureTileEntity.IsUserAllowed(internalLocalUserIdentifier) == false)
                            break;

                        _items.AddRange(secureTileEntity.GetItems());
                        break;

                    default:
                        break;
                }

            }
            return _items;
        }
        [HarmonyPatch(typeof(XUiC_RecipeList))]
        [HarmonyPatch("BuildRecipeInfosList")]
        public class BuildRecipeInfosList
        {
            public static bool Prefix(XUiC_RecipeList __instance, ref List<ItemStack> _items)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                var player = __instance.xui.playerUI.entityPlayer;

                _items.AddRange(EnhancedRecipeLists.SearchNearbyContainers(player));
                return true;
            }
        }

        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("GetAllItemStacks")]
        public class GetAllItemStacks
        {
            public static void Postfix(ref List<ItemStack> __result, EntityPlayerLocal ___localPlayer)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                __result.AddRange(EnhancedRecipeLists.SearchNearbyContainers(___localPlayer));
            }
        }

        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("GetItemCount")]
        [HarmonyPatch(new[] { typeof(ItemValue) })]
        public class GetItemCount
        {
            public static void Postfix(ref int __result, EntityPlayerLocal ___localPlayer, ItemValue _itemValue)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                foreach (var item in EnhancedRecipeLists.SearchNearbyContainers(___localPlayer))
                {
                    if (item == null) continue;
                    if (item.IsEmpty()) continue;
                    if (item.itemValue == null) continue;
                    if ((!item.itemValue.HasModSlots || !item.itemValue.HasMods()) && item.itemValue.type == _itemValue.type)
                        __result += item.count;

                }

            }
        }


        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("HasItems")]
        public class HasItems
        {
            public static bool Postfix(bool __result, IList<ItemStack> _itemStacks, EntityPlayerLocal ___localPlayer, int _multiplier)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return __result;

                if (__result == true) return __result;

                var totalCount = 0;
                var tileEntities = EnhancedRecipeLists.GetTileEntities(___localPlayer);

                foreach (var itemStack in _itemStacks)
                {
                    int num = itemStack.count * _multiplier;
                    // check player inventory
                    var slots = ___localPlayer.bag.GetSlots();
                    totalCount = totalCount + slots
                        .Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                        .Sum(y=>y.count);
                    // check container
                    foreach (var tileEntity in tileEntities)
                    {
                        var lootTileEntity = tileEntity as TileEntityLootContainer;
                        totalCount = totalCount + lootTileEntity.items
                            .Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                            .Sum(y => y.count);
                        if (totalCount >= num) return true;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("RemoveItems")]
        public class RemoveItems
        {
            public static bool Prefix(IList<ItemStack> _itemStacks, EntityPlayerLocal ___localPlayer, int _multiplier)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var tileEntities = EnhancedRecipeLists.GetTileEntities(___localPlayer);
                foreach (var itemStack in _itemStacks)
                {
                    // counter quantity needed from item
                    int q = itemStack.count * _multiplier;
                    //check player inventory for materials and reduce counter
                    var slots = ___localPlayer.bag.GetSlots();
                    q = q - slots
                        .Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                        .Sum(y => y.count);

                    // check storage boxes
                    foreach (var tileEntity in tileEntities)
                    {
                        if (q <= 0) break;
                        var lootTileEntity = tileEntity as TileEntityLootContainer;
                        if (lootTileEntity == null) continue;
                        foreach (var item in lootTileEntity.items.Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass))
                        {
                            //if more or equal items available remove needed items
                            if (item.count >= q)
                            {
                                item.count = item.count - q;
                                q = 0;
                            }
                            // if less items available substact items and reduce counter until no items are left in the stack
                            else if (item.count < q)
                            {
                                while (item.count != 0)
                                {
                                    item.count--;
                                    q--;
                                }
                            }
                            else return true;
                        }
                    }
                }
                return true;
            }
        }
    }
}