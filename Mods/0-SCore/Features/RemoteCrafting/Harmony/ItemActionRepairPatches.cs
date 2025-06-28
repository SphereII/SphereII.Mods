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
                var itemValue = ItemClass.GetItem(__instance.GetUpgradeItemName(block), false);
                int num;
                if (!int.TryParse(block.Properties.Values[Block.PropUpgradeBlockClassItemCount], out num))
                {
                    return false;
                }

                if (data.holdingEntity.inventory.DecItem(itemValue, num) == num)
                {
                    var entityPlayerLocal = data.holdingEntity as EntityPlayerLocal;
                    if (entityPlayerLocal != null && num != 0)
                    {
                        entityPlayerLocal.AddUIHarvestingItem(new ItemStack(itemValue, -num));
                    }

                    __result = true;
                    return false;
                }

                if (data.holdingEntity.bag.DecItem(itemValue, num) == num)
                {
                    var entityPlayerLocal2 = data.holdingEntity as EntityPlayerLocal;
                    if (entityPlayerLocal2 != null)
                    {
                        entityPlayerLocal2.AddUIHarvestingItem(new ItemStack(itemValue, -num));
                    }

                    __result = true;
                    return false;
                }

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var tileEntities = RemoteCraftingUtils.GetTileEntities(primaryPlayer, distance, true);

                // counter quantity needed from item
                var q = num;

                var itemStack = new ItemStack(ItemClass.GetItem(__instance.GetUpgradeItemName(block)), num);

                //check player inventory for materials and reduce counter
                var slots = primaryPlayer.bag.GetSlots();
                q = q - slots
                    .Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                    .Sum(y => y.count);

                // check storage boxes
                foreach (var tileEntity in tileEntities)
                {
                    if (q <= 0) break;

                    if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity))
                        continue;

                    // If there's no items in this container, skip.
                    if (!lootTileEntity.HasItem(itemStack.itemValue)) continue;

                    for (var y = 0; y < lootTileEntity.items.Length; y++)
                    {
                        var item = lootTileEntity.items[y];
                        if (item.IsEmpty()) continue;
                        if (item.itemValue.ItemClass != itemStack.itemValue.ItemClass) continue;

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

                primaryPlayer.AddUIHarvestingItem(new ItemStack(itemValue, -num));

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
                
                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();

                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies"))
                {
                    var distanceE = 30f;
                    var strDistanceE = Configuration.GetPropertyValue(AdvFeatureClass, "DistanceEnemy");
                    if (!string.IsNullOrEmpty(strDistanceE))
                        distanceE = StringParsers.ParseFloat(strDistanceE);
                    if (RemoteCraftingUtils.IsEnemyNearby(data.holdingEntity, distanceE))
                    {
                        GameManager.ShowTooltip(primaryPlayer, Localization.Get("xuiCraftFromContainersSCore"));
                        return; // make sure you only skip if really necessary
                    }
                }

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return;
                }

                var block = blockValue.Block;
                var upgradeItemName = __instance.GetUpgradeItemName(block);
                int quantity;
                if (!int.TryParse(block.Properties.Values[Block.PropUpgradeBlockClassItemCount], out quantity))
                {
                    return;
                }

                var itemStack = new ItemStack(ItemClass.GetItem(upgradeItemName), quantity);
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var totalCount = RemoteCraftingUtils
                    .SearchNearbyContainers(primaryPlayer, itemStack.itemValue, distance).Sum(y => y.count);

                if (totalCount >= quantity)
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
                __result = false;

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();

                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies"))
                {

                    if (RemoteCraftingUtils.IsEnemyNearby(_data.holdingEntity, 30f))
                    {
                        // Show a tool tip, but allow the base feature to run, so we can still repair using our backpack items.
                        GameManager.ShowTooltip(primaryPlayer, Localization.Get("xuiCraftFromContainersSCore"));
                        return true; // make sure you only skip if really necessary
                    }
                }

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return true;
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