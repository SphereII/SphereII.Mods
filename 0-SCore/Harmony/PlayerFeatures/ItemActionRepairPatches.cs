using FullSerializer;
using HarmonyLib;
using Platform;
using SCore.Harmony.Recipes;
using System.Collections.Generic;
using System.Linq;
using UAI;
using UnityEngine;
using static DebugDrawNormals;

namespace SCore.Harmony.PlayerFeatures
{
    internal class ItemActionRepairPatches
    {
        private static readonly string AdvFeatureClass = "BlockUpgradeRepair";
        private static readonly string Feature = "ReadFromContainers";

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch("RemoveRequiredResource")]
        public class RemoveRequiredResource
        {
            private struct UpgradeInfo
            {
                // Token: 0x04006586 RID: 25990
                public string FromBlock;

                // Token: 0x04006587 RID: 25991
                public string ToBlock;

                // Token: 0x04006588 RID: 25992
                public string Item;

                // Token: 0x04006589 RID: 25993
                public int ItemCount;

                // Token: 0x0400658A RID: 25994
                public string Sound;

                // Token: 0x0400658B RID: 25995
                public int Hits;
            }

            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData data, UpgradeInfo ___currentUpgradeInfo)
            {

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    //Log.Out("ItemActionRepairPatches-RemoveRequiredResource Config is off");
                    return true;
                }

                //Log.Out("ItemActionRepairPatches-RemoveRequiredResource ___currentUpgradeInfo: " + ___currentUpgradeInfo.Item);

                if (string.IsNullOrEmpty(___currentUpgradeInfo.Item))
                {
                    //Log.Out("ItemActionRepairPatches-RemoveRequiredResource 1");
                    __result = true;
                    return false;
                }
                ItemValue itemValue = ItemClass.GetItem(___currentUpgradeInfo.Item, false);
                if (data.holdingEntity.inventory.DecItem(itemValue, ___currentUpgradeInfo.ItemCount, false) == ___currentUpgradeInfo.ItemCount)
                {
                    //Log.Out("ItemActionRepairPatches-RemoveRequiredResource 2");
                    EntityPlayerLocal entityPlayerLocal = data.holdingEntity as EntityPlayerLocal;
                    if (entityPlayerLocal != null && ___currentUpgradeInfo.ItemCount != 0)
                    {
                        //Log.Out("ItemActionRepairPatches-RemoveRequiredResource 3");
                        entityPlayerLocal.AddUIHarvestingItem(new ItemStack(itemValue, -___currentUpgradeInfo.ItemCount), false);
                    }
                    __result = true;
                    return false;
                }
                if (data.holdingEntity.bag.DecItem(itemValue, ___currentUpgradeInfo.ItemCount, false) == ___currentUpgradeInfo.ItemCount)
                {
                    //Log.Out("ItemActionRepairPatches-RemoveRequiredResource 4");
                    EntityPlayerLocal entityPlayerLocal2 = data.holdingEntity as EntityPlayerLocal;
                    if (entityPlayerLocal2 != null)
                    {
                        //Log.Out("ItemActionRepairPatches-RemoveRequiredResource 5");
                        entityPlayerLocal2.AddUIHarvestingItem(new ItemStack(itemValue, -___currentUpgradeInfo.ItemCount), false);
                    }
                    __result = true;
                    return false;
                }

                EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var tileEntities = EnhancedRecipeLists.GetTileEntities(primaryPlayer, distance);

                // counter quantity needed from item
                int q = ___currentUpgradeInfo.ItemCount;

                ItemStack _itemStack = new ItemStack(ItemClass.GetItem(___currentUpgradeInfo.Item, false), ___currentUpgradeInfo.ItemCount);

                //check player inventory for materials and reduce counter
                var slots = primaryPlayer.bag.GetSlots();
                q = q - slots
                    .Where(x => x.itemValue.ItemClass == _itemStack.itemValue.ItemClass)
                    .Sum(y => y.count);

                // check storage boxes
                foreach (var tileEntity in tileEntities)
                {
                    if (q <= 0) break;
                    var lootTileEntity = tileEntity as TileEntityLootContainer;
                    if (lootTileEntity == null) continue;

                    // If there's no items in this container, skip.
                    if (!lootTileEntity.HasItem(_itemStack.itemValue)) continue;

                    int num = ___currentUpgradeInfo.ItemCount;
                    if (lootTileEntity == null) break;
                    for (int y = 0; y < lootTileEntity.items.Length; y++)
                    {
                        var item = lootTileEntity.items[y];
                        if (item.IsEmpty()) continue;
                        if (item.itemValue.ItemClass == _itemStack.itemValue.ItemClass)
                        {
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
            private struct UpgradeInfo
            {
                // Token: 0x04006586 RID: 25990
                public string FromBlock;

                // Token: 0x04006587 RID: 25991
                public string ToBlock;

                // Token: 0x04006588 RID: 25992
                public string Item;

                // Token: 0x04006589 RID: 25993
                public int ItemCount;

                // Token: 0x0400658A RID: 25994
                public string Sound;

                // Token: 0x0400658B RID: 25995
                public int Hits;
            }

            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData data, BlockValue blockValue, ref UpgradeInfo ___currentUpgradeInfo, string ___allowedUpgradeItems, string ___restrictedUpgradeItems, string ___upgradeActionSound, float ___hitCountOffset)
            {


                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies"))
                {
                    var distanceE = 30f;
                    var strDistanceE = Configuration.GetPropertyValue(AdvFeatureClass, "DistanceEnemy");
                    if (!string.IsNullOrEmpty(strDistanceE))
                        distanceE = StringParsers.ParseFloat(strDistanceE);
                    if (IsEnemyNearby(data.holdingEntity, distanceE))
                    {
                        __result = false;
                        return false; // make sure you only skip if really necessary
                    }
                }

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource Config is off");
                    return true;
                }

                Block block = blockValue.Block;
                bool flag = block.Properties.Values.ContainsKey("UpgradeBlock.Item");
                UpgradeInfo upgradeInfo = default(UpgradeInfo);
                upgradeInfo.FromBlock = block.GetBlockName();
                upgradeInfo.ToBlock = block.Properties.Values[Block.PropUpgradeBlockClassToBlock];
                if (flag)
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 1");
                    upgradeInfo.Item = block.Properties.Values["UpgradeBlock.Item"];
                    if (___allowedUpgradeItems.Length > 0 && !___allowedUpgradeItems.ContainsCaseInsensitive(upgradeInfo.Item))
                    {
                        //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 2");
                        __result = false;
                        return false;
                    }
                    if (___restrictedUpgradeItems.Length > 0 && ___restrictedUpgradeItems.ContainsCaseInsensitive(upgradeInfo.Item))
                    {
                        //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 3");
                        __result = false;
                        return false;
                    }
                }
                if (___upgradeActionSound.Length > 0)
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 4");
                    upgradeInfo.Sound = ___upgradeActionSound;
                }
                else if (flag)
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 5");
                    upgradeInfo.Sound = string.Format("ImpactSurface/{0}hit{1}", data.holdingEntity.inventory.holdingItem.MadeOfMaterial.SurfaceCategory, ItemClass.GetForId(ItemClass.GetItem(upgradeInfo.Item, false).type).MadeOfMaterial.SurfaceCategory);
                }
                else
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 6");
                    upgradeInfo.Sound = "";
                }
                int num;
                if (!int.TryParse(block.Properties.Values["UpgradeBlock.UpgradeHitCount"], out num))
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 7");
                    __result = false;
                    return false;
                }
                upgradeInfo.Hits = (int)(((float)num + ___hitCountOffset < 1f) ? 1f : ((float)num + ___hitCountOffset));
                if (!int.TryParse(block.Properties.Values[Block.PropUpgradeBlockClassItemCount], out upgradeInfo.ItemCount) && flag)
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 8");
                    __result = false;
                    return false;
                }
                ___currentUpgradeInfo = upgradeInfo;

                //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource FromBlock: " + ___currentUpgradeInfo.FromBlock);
                //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource ToBlock: " + ___currentUpgradeInfo.ToBlock);
                //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource Item: " + ___currentUpgradeInfo.Item);
                //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource ItemCount: " + ___currentUpgradeInfo.ItemCount);
                //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource Sound: " + ___currentUpgradeInfo.Sound);
                //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource Hits: " + ___currentUpgradeInfo.Hits);

                if (___currentUpgradeInfo.FromBlock != null && flag)
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 9");
                    ItemValue item = ItemClass.GetItem(___currentUpgradeInfo.Item, false);
                    if (data.holdingEntity.inventory.GetItemCount(item, false, -1, -1) >= ___currentUpgradeInfo.ItemCount)
                    {
                        //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 10");
                        __result = true;
                        return false;
                    }
                    if (data.holdingEntity.bag.GetItemCount(item, -1, -1, false) >= ___currentUpgradeInfo.ItemCount)
                    {
                        //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 11");
                        __result = true;
                        return false;
                    }
                }
                else if (!flag)
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 12");
                    __result = true;
                    return false;
                }

                ///

                EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();

                ItemStack _itemStack = new ItemStack(ItemClass.GetItem(___currentUpgradeInfo.Item, false), ___currentUpgradeInfo.ItemCount);
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                int totalCount = EnhancedRecipeLists.SearchNearbyContainers(primaryPlayer, _itemStack.itemValue, distance).Sum(y => y.count);

                if (totalCount >= ___currentUpgradeInfo.ItemCount)
                {
                    //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 13");
                    __result = true;
                    return false;
                }

                ///

                //Log.Out("ItemActionRepairPatches-CanRemoveRequiredResource 14");
                __result = false;

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch("removeRequiredItem")]
        public class removeRequiredItem
        {
            private static bool Prefix(bool __result, ItemActionRepair __instance, ItemInventoryData _data, ItemStack _itemStack)
            {
                __result = false;

                //Log.Out("ItemActionRepairPatches-removeRequiredItem START");

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    //Log.Out("ItemActionRepairPatches-canRemoveRequiredItem Config is off");
                    return true;
                }

                __result = _data.holdingEntity.inventory.DecItem(_itemStack.itemValue, _itemStack.count, false) == _itemStack.count || _data.holdingEntity.bag.DecItem(_itemStack.itemValue, _itemStack.count, false) == _itemStack.count;

                if (__result)
                {
                    //Log.Out("ItemActionRepairPatches-canRemoveRequiredItem Have items in my inventory");
                    return false;
                }

                EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var tileEntities = EnhancedRecipeLists.GetTileEntities(primaryPlayer, distance);

                // counter quantity needed from item
                int q = _itemStack.count;

                //check player inventory for materials and reduce counter
                var slots = primaryPlayer.bag.GetSlots();
                q = q - slots
                    .Where(x => x.itemValue.ItemClass == _itemStack.itemValue.ItemClass)
                    .Sum(y => y.count);

                // check storage boxes
                foreach (var tileEntity in tileEntities)
                {
                    if (q <= 0) break;
                    var lootTileEntity = tileEntity as TileEntityLootContainer;
                    if (lootTileEntity == null) continue;

                    // If there's no items in this container, skip.
                    if (!lootTileEntity.HasItem(_itemStack.itemValue)) continue;

                    int num = _itemStack.count;
                    if (lootTileEntity == null) break;
                    for (int y = 0; y < lootTileEntity.items.Length; y++)
                    {
                        var item = lootTileEntity.items[y];
                        if (item.IsEmpty()) continue;
                        if (item.itemValue.ItemClass == _itemStack.itemValue.ItemClass)
                        {
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
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair))]
        [HarmonyPatch("canRemoveRequiredItem")]
        public class canRemoveRequiredItem
        {
            private static bool Prefix(ref bool __result, ItemActionRepair __instance, ItemInventoryData _data, ItemStack _itemStack)
            {
                __result = false;

                //Log.Out("ItemActionRepairPatches-canRemoveRequiredItem START");
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, "BlockOnNearbyEnemies"))
                {
                    var distanceE = 30f;
                    var strDistanceE = Configuration.GetPropertyValue(AdvFeatureClass, "DistanceEnemy");
                    if (!string.IsNullOrEmpty(strDistanceE))
                        distanceE = StringParsers.ParseFloat(strDistanceE);
                    if (IsEnemyNearby(_data.holdingEntity, distanceE))
                    {
                        __result = false;
                        return false; // make sure you only skip if really necessary
                    }
                }

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    //Log.Out("ItemActionRepairPatches-canRemoveRequiredItem Config is off");
                    return true;
                }

                if (_data.holdingEntity.inventory.GetItemCount(_itemStack.itemValue, false, -1, -1) >= _itemStack.count || _data.holdingEntity.bag.GetItemCount(_itemStack.itemValue, -1, -1, false) >= _itemStack.count)
                {
                    //Log.Out("ItemActionRepairPatches-canRemoveRequiredItem Have items in my inventory");
                    __result = true;
                    return false;
                }

                //Log.Out("ItemActionRepairPatches-canRemoveRequiredItem Don't have items in my inventory");

                EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                var distance = 30f;
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                int totalCount = EnhancedRecipeLists.SearchNearbyContainers(primaryPlayer, _itemStack.itemValue, distance).Sum(y => y.count);

                //Log.Out("ItemActionRepairPatches-canRemoveRequiredItem totalCount: " + totalCount);

                if (totalCount > 0)
                {
                    __result = true;
                    return false;
                }

                return false;
            }
        }

        private static bool IsEnemyNearby(EntityAlive Self, float distance = 20f)
        {
            var nearbyEntities = new List<Entity>();

            // Search in the bounds are to try to find the most appealing entity to follow.
            var bb = new Bounds(Self.position, new Vector3(distance, distance, distance));

            Self.world.GetEntitiesInBounds(typeof(EntityAlive), bb, nearbyEntities);
            for (var i = nearbyEntities.Count - 1; i >= 0; i--)
            {
                var x = nearbyEntities[i] as EntityAlive;
                if (x == null) continue;
                if (x == Self) continue;
                if (x.IsDead()) continue;
                if (!EntityTargetingUtilities.CanDamage(x, Self)) continue;
                // Check to see if they are our enemy first, before deciding if we should see them.
                if (EntityTargetingUtilities.IsFriend(x, Self)) continue;
                // Otherwise they are an enemy.
                return true;
            }
            Debug.LogWarning("no enemy");
            return false;
        }
    }
}
