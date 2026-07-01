using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using SCore.Features.RemoteCrafting.Scripts;
using UnityEngine;

namespace Features.RemoteCrafting {
    /// <summary>
    /// Patches to support repairing from remote storage
    /// </summary>
    public class ItemActionRepairPatches {
        private const string AdvFeatureClass = "BlockUpgradeRepair";
        private const string Feature = "ReadFromContainers";

        private struct UpgradeInfo {
            public string FromBlock;
            public string ToBlock;
            public string Item;
            public int ItemCount;
            public string Sound;
            public int Hits;
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch(nameof(ItemActionRepair.RemoveRequiredResource))]
        public class RemoveRequiredResource {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData data,
                BlockValue blockValue) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var block = blockValue.Block;
                if (!block.Properties.TryGetValue(Block.PropUpgradeBlockClass, Block.PropUpgradeBlockItemCount, out var itemCountStr) ||
                    !int.TryParse(itemCountStr, out int num))
                {
                    return false;
                }
                var itemValue = ItemClass.GetItem(__instance.GetUpgradeItemName(block), false);
                var entityPlayerLocal = data.holdingEntity as EntityPlayerLocal;

                // Pull from inventory first, then bag, tracking what's still owed.
                var remaining = num;

                var removedFromInventory = data.holdingEntity.inventory.DecItem(itemValue, remaining);
                remaining -= removedFromInventory;
                if (removedFromInventory > 0)
                    entityPlayerLocal?.AddUIHarvestingItem(new ItemStack(itemValue, -removedFromInventory));

                if (remaining > 0)
                {
                    var removedFromBag = data.holdingEntity.bag.DecItem(itemValue, remaining);
                    remaining -= removedFromBag;
                    if (removedFromBag > 0)
                        entityPlayerLocal?.AddUIHarvestingItem(new ItemStack(itemValue, -removedFromBag));
                }

                if (remaining <= 0)
                {
                    __result = true;
                    return false;
                }

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var tileEntities = RemoteCraftingUtils.GetTileEntities(primaryPlayer, distance, true);

                // counter quantity still needed from nearby containers
                var q = remaining;

                foreach (var tileEntity in tileEntities)
                {
                    if (q <= 0) break;

                    if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity))
                        continue;

                    // If there's no items in this container, skip.
                    if (!lootTileEntity.HasItem(itemValue)) continue;

                    for (var y = 0; y < lootTileEntity.items.Length; y++)
                    {
                        if (q <= 0) break;

                        var item = lootTileEntity.items[y];
                        if (item.IsEmpty()) continue;
                        if (item.itemValue.ItemClass != itemValue.ItemClass) continue;

                        // If we can completely satisfy the result, let's do that.
                        if (item.count >= q)
                        {
                            item.count -= q;
                            q = 0;
                        }
                        else
                        {
                            q -= item.count;
                            item.count = 0;
                        }

                        //Update the slot on the container, and do the Setmodified(), so that the dedis can get updated.
                        if (item.count < 1)
                            lootTileEntity.UpdateSlot(y, ItemStack.Empty.Clone());
                        else
                            lootTileEntity.UpdateSlot(y, item);
                        lootTileEntity.SetModified();
                    }
                }

                if (q > 0)
                {
                    // Not enough resources were actually available - don't report success.
                    __result = false;
                    return false;
                }

                var removedFromContainers = remaining - q;
                primaryPlayer.AddUIHarvestingItem(new ItemStack(itemValue, -removedFromContainers));

                __result = true;

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch(nameof(ItemActionRepair.CanRemoveRequiredResource))]
        public class CanRemoveRequiredResource {
            private static void Postfix(ref bool __result, ItemActionRepair __instance, ItemInventoryData data,
                BlockValue blockValue) {
                if (__result) return;
                
                
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return;
                }
                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();

                var block = blockValue.Block;
                var upgradeItemName = __instance.GetUpgradeItemName(block);

                if (!block.Properties.TryGetValue(Block.PropUpgradeBlockClass, Block.PropUpgradeBlockItemCount, out var itemCountStr) ||
                    !int.TryParse(itemCountStr, out int quantity)) return;
                var itemValue = ItemClass.GetItem(upgradeItemName);

                // CHECK FOR ENEMIES
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies")) {
                    var distanceE = 30f;
                    var strDistanceE = Configuration.GetPropertyValue(AdvFeatureClass, "DistanceEnemy");
                    if (!string.IsNullOrEmpty(strDistanceE)) distanceE = StringParsers.ParseFloat(strDistanceE);

                    if (RemoteCraftingUtils.IsEnemyNearby(data.holdingEntity, distanceE)) {
                        // If enemies are nearby, we ONLY allow the result to be true if it's in the bag/inventory
                        var hasInPerson = data.holdingEntity.inventory.GetItemCount(itemValue) >= quantity ||
                                           data.holdingEntity.bag.GetItemCount(itemValue) >= quantity;

                        if (!hasInPerson) {
                            GameManager.ShowTooltip(primaryPlayer, Localization.Get("xuiCraftFromContainersSCore"));
                            __result = false;
                            return;
                        }
                    }
                }

                var heldCount = data.holdingEntity.inventory.GetItemCount(itemValue) +
                                data.holdingEntity.bag.GetItemCount(itemValue);
                var needed = quantity - heldCount;
                if (needed <= 0)
                {
                    __result = true;
                    return;
                }

                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var totalCount = RemoteCraftingUtils
                    .SearchNearbyContainers(primaryPlayer, itemValue, distance).Sum(y => y.count);

                if (totalCount >= needed)
                {
                    __result = true;
                    return;
                }

                __result = false;

                
            }

          
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch(nameof(ItemActionRepair.removeRequiredItem))]
        public class RemoveRequiredItem {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData _data,
                ItemStack _itemStack) {
                __result = false;

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return true;
                }

                __result =
                    _data.holdingEntity.inventory.DecItem(_itemStack.itemValue, _itemStack.count) == _itemStack.count ||
                    _data.holdingEntity.bag.DecItem(_itemStack.itemValue, _itemStack.count, false) == _itemStack.count;

                if (__result)
                {
                    return false;
                }

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var tileEntities = RemoteCraftingUtils.GetTileEntities(primaryPlayer, distance, true);

                // counter quantity needed from item
                var q = _itemStack.count;

                //check player inventory for materials and reduce counter
                var slots = primaryPlayer.bag.GetSlots();
                q -= slots
                    .Where(x => x.itemValue.ItemClass == _itemStack.itemValue.ItemClass)
                    .Sum(y => y.count);

                // check storage boxes
                foreach (var tileEntity in tileEntities)
                {
                    if (q <= 0) break;
                    if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity)) continue;
                    // If there's no items in this container, skip.
                    if (!lootTileEntity.HasItem(_itemStack.itemValue)) continue;

                    for (var y = 0; y < lootTileEntity.items.Length; y++)
                    {
                        var item = lootTileEntity.items[y];
                        if (item.IsEmpty()) continue;
                        if (item.itemValue.ItemClass != _itemStack.itemValue.ItemClass) continue;

                        // If we can completely satisfy the result, let's do that.
                        if (item.count >= q)
                        {
                            item.count -= q;
                            q = 0;
                        }
                        else
                        {
                            // Otherwise, let's just count down until we meet the requirement.
                            while (q >= 0)
                            {
                                item.count--;
                                q--;
                                if (item.count <= 0)
                                    break;
                            }
                        }

                        //Update the slot on the container, and do the Setmodified(), so that the dedis can get updated.
                        if (item.count < 1)
                            lootTileEntity.UpdateSlot(y, ItemStack.Empty.Clone());
                        else
                            lootTileEntity.UpdateSlot(y, item);
                        lootTileEntity.SetModified();
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch(nameof(ItemActionRepair.canRemoveRequiredItem))]
        public class CanRemoveRequiredItem {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData _data,
                ItemStack _itemStack) {
                
                
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return true;
                }
                
                __result = false;

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();

                // CHECK FOR ENEMIES
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies")) {
                    if (RemoteCraftingUtils.IsEnemyNearby(_data.holdingEntity, 30f)) {
                        // Check if player has it in personal inventory
                        bool hasInPerson = _data.holdingEntity.inventory.GetItemCount(_itemStack.itemValue) >= _itemStack.count ||
                                           _data.holdingEntity.bag.GetItemCount(_itemStack.itemValue) >= _itemStack.count;

                        if (hasInPerson) {
                            return true; // Return true to let vanilla handle the local-only repair
                        }

                        // Block the repair and show tooltip
                        GameManager.ShowTooltip(primaryPlayer, Localization.Get("xuiCraftFromContainersSCore"));
                        __result = false; 
                        return false; // Skip vanilla so it doesn't trigger your remote GetItemCount patch
                    }
                }


                if (_data.holdingEntity.inventory.GetItemCount(_itemStack.itemValue) >= _itemStack.count ||
                    _data.holdingEntity.bag.GetItemCount(_itemStack.itemValue) >= _itemStack.count)
                {
                    __result = true;
                    return false;
                }

                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var totalCount = RemoteCraftingUtils
                    .SearchNearbyContainers(primaryPlayer, _itemStack.itemValue, distance).Sum(y => y.count);

                if (totalCount <= 0) return false;
                __result = true;
                return false;
            }
        }
    }
}