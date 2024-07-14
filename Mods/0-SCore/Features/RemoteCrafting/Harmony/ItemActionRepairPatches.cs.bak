using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using SCore.Features.RemoteCrafting.Scripts;
using UnityEngine;

namespace Features.RemoteCrafting
{
    /// <summary>
    /// Patches to support repairing from remote storage
    /// </summary>
    public class ItemActionRepairPatches
    {
        private const string AdvFeatureClass = "BlockUpgradeRepair";
        private const string Feature = "ReadFromContainers";

        private struct UpgradeInfo
        {
            public string FromBlock;
            public string ToBlock;
            public string Item;
            public int ItemCount;
            public string Sound;
            public int Hits;
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch("RemoveRequiredResource")]
        public class RemoveRequiredResource
        {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData data, UpgradeInfo ___currentUpgradeInfo)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (string.IsNullOrEmpty(___currentUpgradeInfo.Item))
                {
                    __result = true;
                    return false;
                }
                var itemValue = ItemClass.GetItem(___currentUpgradeInfo.Item);
                if (data.holdingEntity.inventory.DecItem(itemValue, ___currentUpgradeInfo.ItemCount) == ___currentUpgradeInfo.ItemCount)
                {
                    var entityPlayerLocal = data.holdingEntity as EntityPlayerLocal;
                    if (entityPlayerLocal != null && ___currentUpgradeInfo.ItemCount != 0)
                    {
                        entityPlayerLocal.AddUIHarvestingItem(new ItemStack(itemValue, -___currentUpgradeInfo.ItemCount));
                    }
                    __result = true;
                    return false;
                }
                if (data.holdingEntity.bag.DecItem(itemValue, ___currentUpgradeInfo.ItemCount) == ___currentUpgradeInfo.ItemCount)
                {
                    var entityPlayerLocal2 = data.holdingEntity as EntityPlayerLocal;
                    if (entityPlayerLocal2 != null)
                    {
                        entityPlayerLocal2.AddUIHarvestingItem(new ItemStack(itemValue, -___currentUpgradeInfo.ItemCount));
                    }
                    __result = true;
                    return false;
                }

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var tileEntities = RemoteCraftingUtils.GetTileEntities(primaryPlayer, distance);

                // counter quantity needed from item
                var q = ___currentUpgradeInfo.ItemCount;

                var itemStack = new ItemStack(ItemClass.GetItem(___currentUpgradeInfo.Item, false), ___currentUpgradeInfo.ItemCount);

                //check player inventory for materials and reduce counter
                var slots = primaryPlayer.bag.GetSlots();
                q = q - slots
                    .Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                    .Sum(y => y.count);

                // check storage boxes
                foreach (var tileEntity in tileEntities)
                {
                    if (q <= 0) break;
                    if (tileEntity is not TileEntityLootContainer lootTileEntity) continue;

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

                primaryPlayer.AddUIHarvestingItem(new ItemStack(itemValue, -___currentUpgradeInfo.ItemCount), false);

                __result = true;

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch("CanRemoveRequiredResource")]
        public class CanRemoveRequiredResource
        {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData data, BlockValue blockValue, ref UpgradeInfo ___currentUpgradeInfo, string ___allowedUpgradeItems, string ___restrictedUpgradeItems, string ___upgradeActionSound, float ___hitCountOffset)
            {
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies"))
                {
                    var distanceE = 30f;
                    var strDistanceE = Configuration.GetPropertyValue(AdvFeatureClass, "DistanceEnemy");
                    if (!string.IsNullOrEmpty(strDistanceE))
                        distanceE = StringParsers.ParseFloat(strDistanceE);
                    if (RemoteCraftingUtils.IsEnemyNearby(data.holdingEntity, distanceE))
                    {
                        __result = false;
                        return false; // make sure you only skip if really necessary
                    }
                }

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return true;
                }

                var block = blockValue.Block;
                var flag = block.Properties.Values.ContainsKey("UpgradeBlock.Item");
                var upgradeInfo = default(UpgradeInfo);
                upgradeInfo.FromBlock = block.GetBlockName();
                upgradeInfo.ToBlock = block.Properties.Values[Block.PropUpgradeBlockClassToBlock];
                upgradeInfo.Sound = "";
                if (flag)
                {
                    upgradeInfo.Item = block.Properties.Values["UpgradeBlock.Item"];
                    if (___allowedUpgradeItems.Length > 0 && !___allowedUpgradeItems.ContainsCaseInsensitive(upgradeInfo.Item))
                    {
                        __result = false;
                        return false;
                    }
                    if (___restrictedUpgradeItems.Length > 0 && ___restrictedUpgradeItems.ContainsCaseInsensitive(upgradeInfo.Item))
                    {
                        __result = false;
                        return false;
                    }
                }
                if (___upgradeActionSound.Length > 0)
                {
                    upgradeInfo.Sound = ___upgradeActionSound;
                }
                else if (flag)
                {
                    var item = ItemClass.GetItem(upgradeInfo.Item);
                    if (item != null)
                    {
                        var itemClass = ItemClass.GetForId(item.type);
                        if (itemClass != null)
                        {
                            upgradeInfo.Sound =
                                $"ImpactSurface/{data.holdingEntity.inventory.holdingItem.MadeOfMaterial.SurfaceCategory}hit{itemClass.MadeOfMaterial.SurfaceCategory}";
                        }
                    }
                }
                if (!int.TryParse(block.Properties.Values["UpgradeBlock.UpgradeHitCount"], out var num))
                {
                    __result = false;
                    return false;
                }
                upgradeInfo.Hits = (int)(num + ___hitCountOffset < 1f ? 1f : num + ___hitCountOffset);
                if (!int.TryParse(block.Properties.Values[Block.PropUpgradeBlockClassItemCount], out upgradeInfo.ItemCount) && flag)
                {
                    __result = false;
                    return false;
                }
                ___currentUpgradeInfo = upgradeInfo;
                if (___currentUpgradeInfo.FromBlock != null && flag)
                {
                    var item = ItemClass.GetItem(___currentUpgradeInfo.Item, false);
                    if (data.holdingEntity.inventory.GetItemCount(item) >= ___currentUpgradeInfo.ItemCount)
                    {
                        __result = true;
                        return false;
                    }
                    if (data.holdingEntity.bag.GetItemCount(item) >= ___currentUpgradeInfo.ItemCount)
                    {
                        __result = true;
                        return false;
                    }
                }
                else if (!flag)
                {
                    __result = true;
                    return false;
                }


                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();

                var itemStack = new ItemStack(ItemClass.GetItem(___currentUpgradeInfo.Item), ___currentUpgradeInfo.ItemCount);
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var totalCount = RemoteCraftingUtils.SearchNearbyContainers(primaryPlayer, itemStack.itemValue, distance).Sum(y => y.count);

                if (totalCount >= ___currentUpgradeInfo.ItemCount)
                {
                    __result = true;
                    return false;
                }

                __result = false;

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch("removeRequiredItem")]
        public class RemoveRequiredItem
        {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData _data, ItemStack _itemStack)
            {
                __result = false;

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return true;
                }

                __result = _data.holdingEntity.inventory.DecItem(_itemStack.itemValue, _itemStack.count) == _itemStack.count || _data.holdingEntity.bag.DecItem(_itemStack.itemValue, _itemStack.count, false) == _itemStack.count;

                if (__result)
                {
                    return false;
                }

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var tileEntities = RemoteCraftingUtils.GetTileEntities(primaryPlayer, distance);

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
                    if (tileEntity is not TileEntityLootContainer lootTileEntity) continue;

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
        [HarmonyPatch("canRemoveRequiredItem")]
        public class CanRemoveRequiredItem
        {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData _data, ItemStack _itemStack)
            {
                __result = false;

                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies"))
                {
                    var distanceE = 30f;
                    var strDistanceE = Configuration.GetPropertyValue(AdvFeatureClass, "DistanceEnemy");
                    if (!string.IsNullOrEmpty(strDistanceE))
                        distanceE = StringParsers.ParseFloat(strDistanceE);
                    if (RemoteCraftingUtils.IsEnemyNearby(_data.holdingEntity, distanceE))
                    {
                        __result = false;
                        return false; // make sure you only skip if really necessary
                    }
                }

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return true;
                }

                if (_data.holdingEntity.inventory.GetItemCount(_itemStack.itemValue) >= _itemStack.count || _data.holdingEntity.bag.GetItemCount(_itemStack.itemValue) >= _itemStack.count)
                {
                    __result = true;
                    return false;
                }

                var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var totalCount = RemoteCraftingUtils.SearchNearbyContainers(primaryPlayer, _itemStack.itemValue, distance).Sum(y => y.count);

                if (totalCount <= 0) return false;
                __result = true;
                return false;

            }
        }

   
    }
}
