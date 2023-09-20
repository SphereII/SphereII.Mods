using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Platform;
using UAI;
using UnityEngine;

namespace SCore.Features.RemoteCrafting.Scripts
{
    [UsedImplicitly]
    public class RemoteCraftingUtils
    {
        private const string AdvFeatureClass = "AdvancedRecipes";

        public static List<TileEntity> GetTileEntities(EntityAlive player)
        {
            var distance = 30f;
            var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
            if (!string.IsNullOrEmpty(strDistance))
                distance = StringParsers.ParseFloat(strDistance);
            var tileEntities = GetTileEntities(player, distance);
            return tileEntities;
        }

        public static List<TileEntity> GetTileEntities(EntityAlive player, float distance)
        {
            var disabledsender = Configuration.GetPropertyValue(AdvFeatureClass, "disablesender").Split(',');
            var nottoWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "nottoWorkstation");
            var bindtoWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "bindtoWorkstation");
            var tileEntities = new List<TileEntity>();
            const string targetTypes = "Loot, SecureLoot, SecureLootSigned";
            var paths = SCoreUtils.ScanForTileEntities(player, targetTypes, true);
            foreach (var path in paths)
            {
                var distanceToLeader = Vector3.Distance(player.position, path);
                if (!(distanceToLeader < distance)) continue;
                var tileEntity = player.world.GetTileEntity(0, new Vector3i(path));
                if (tileEntity == null) continue;

                // Check if Broadcastmanager is running if running check if lootcontainer is in Broadcastmanager dictionary
                // note: Broadcastmanager.Instance.Check()) throws nullref if Broadcastmanager is not running.
                // works because Hasinstance is being checked first in the or.
                if (!Broadcastmanager.HasInstance || Broadcastmanager.Instance.Check(tileEntity.ToWorldPos()))
                {
                    switch (tileEntity.GetTileEntityType())
                    {
                        case TileEntityType.Loot:
                            if (tileEntity is not TileEntityLootContainer lootTileEntity) break;
                            // if sending disabled skip container
                            if (disabledsender[0] != null)
                            {
                                if (DisableSender(disabledsender, tileEntity))
                                {
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(nottoWorkstation))
                            {
                                if (NotToWorkstation(nottoWorkstation, player, tileEntity))
                                {
                                    goto default;
                                }
                            }

                            if (!string.IsNullOrEmpty(bindtoWorkstation))
                            {
                                if (BindToWorkstation(bindtoWorkstation, player, tileEntity))
                                {
                                    tileEntities.Add(tileEntity);
                                }
                            }
                            else
                            {
                                tileEntities.Add(tileEntity);
                            }

                            break;
                        case TileEntityType.SecureLootSigned:
                        case TileEntityType.SecureLoot:
                            var secureTileEntity = tileEntity as TileEntitySecureLootContainer;
                            if (secureTileEntity == null) break;
                            if (secureTileEntity.IsLocked())
                            {
                                var internalLocalUserIdentifier =
                                    PlatformManager.InternalLocalUserIdentifier;
                                if (!secureTileEntity.IsUserAllowed(internalLocalUserIdentifier)) break;
                            }

                            // if sending disabled skip container
                            if (disabledsender[0] != null)
                            {
                                if (DisableSender(disabledsender, tileEntity))
                                {
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(nottoWorkstation))
                            {
                                if (NotToWorkstation(nottoWorkstation, player, tileEntity))
                                {
                                    goto default;
                                }
                            }

                            if (!string.IsNullOrEmpty(bindtoWorkstation))
                            {
                                if (BindToWorkstation(bindtoWorkstation, player, tileEntity))
                                {
                                    tileEntities.Add(tileEntity);
                                }
                            }
                            else
                            {
                                tileEntities.Add(tileEntity);
                            }

                            break;
                        case TileEntityType.None:
                        case TileEntityType.DewCollector:
                        case TileEntityType.LandClaim:
                        case TileEntityType.Trader:
                        case TileEntityType.VendingMachine:
                        case TileEntityType.Forge:
                        case TileEntityType.Campfire:
                        case TileEntityType.SecureDoor:
                        case TileEntityType.Workstation:
                        case TileEntityType.Sign:
                        case TileEntityType.GoreBlock:
                        case TileEntityType.Powered:
                        case TileEntityType.PowerSource:
                        case TileEntityType.PowerRangeTrap:
                        case TileEntityType.Light:
                        case TileEntityType.Trigger:
                        case TileEntityType.Sleeper:
                        case TileEntityType.PowerMeleeTrap:
                        default:
                            break;
                    }
                }
            }

            return tileEntities;
        }

        public static bool DisableSender(IEnumerable<string> value, TileEntity tileEntity)
        {
            if (tileEntity is not TileEntityLootContainer lootTileEntity) return false;
            var invertdisable = bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Invertdisable"));
            if (!invertdisable)
            {
                if (value.Any(x => x.Trim() == lootTileEntity.lootListName))
                {
                    return true;
                }
            }
            else
            {
                if (value.All(x => x.Trim() != lootTileEntity.lootListName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool BindToWorkstation(string value, EntityAlive player, TileEntity tileEntity)
        {
            var result = false;
            if (player is not EntityPlayerLocal playerLocal) return false;
            if (tileEntity is not TileEntityLootContainer lootTileEntity) return false;

            // TODO: we want to refactor this to remove the complex LinQ.
            // bind storage to workstation
            if (value.Split(';').Where(x =>
                    x.Split(':')[0].Split(',').Any(ws => ws.Trim() == playerLocal.PlayerUI.xui.currentWorkstation))
                .Any(x => x.Split(':')[1].Split(',').Any(y => y == lootTileEntity.lootListName))) result = true;
            // bind storage to other workstations if allowed
            if (value.Split(';').Any(x =>
                    x.Split(':')[0].Split(',').Any(ws => ws.Trim() == playerLocal.PlayerUI.xui.currentWorkstation))
                || bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "enforcebindtoWorkstation")))
                return result;
            {
                if (value.Split(';').Any(x => x.Split(':')[1].Split(',').Any(y => y == lootTileEntity.lootListName)))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        private static bool NotToWorkstation(string value, EntityAlive player, TileEntity tileEntity)
        {
            var result = false;
            if (player is not EntityPlayerLocal playerLocal) return false;
            if (tileEntity is not TileEntityLootContainer lootTileEntity) return false;
            foreach (var bind in value.Split(';'))
            {
                var workstation = bind.Split(':')[0].Split(',');
                var disablebinding = bind.Split(':')[1].Split(',');
                if ((workstation.Any(ws => ws.Trim() == playerLocal.PlayerUI.xui.currentWorkstation)) &&
                    (disablebinding.Any(x => x.Trim() == lootTileEntity.lootListName))) result = true;
            }

            return result;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player)
        {
            var items = new List<ItemStack>();
            var tileEntities = GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                if (tileEntity is not TileEntityLootContainer lootTileEntity) continue;
                if (lootTileEntity.IsUserAccessing()) continue;
                items.AddRange(lootTileEntity.GetItems());
            }

            return items;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player, ItemValue itemValue)
        {
            var item = new List<ItemStack>();
            var items = new List<ItemStack>();
            var tileEntities = GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                if (tileEntity is not TileEntityLootContainer lootTileEntity) continue;

                // If the container is open, don't use it.
                if (lootTileEntity.IsUserAccessing()) continue;

                item.AddRange(lootTileEntity.GetItems());
            }

            foreach (var t in item)
            {
                if ((!t.itemValue.HasModSlots || !t.itemValue.HasMods()) &&
                    t.itemValue.type == itemValue.type)
                {
                    items.Add(t);
                }
            }

            return items;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player, ItemValue itemValue, float distance)
        {
            var item = new List<ItemStack>();
            var items = new List<ItemStack>();
            var tileEntities = GetTileEntities(player, distance);
            foreach (var tileEntity in tileEntities)
            {
                if (tileEntity is not TileEntityLootContainer lootTileEntity) continue;

                // If the container is open, don't include it.
                if (lootTileEntity.IsUserAccessing()) continue;

                item.AddRange(lootTileEntity.GetItems());
            }

            foreach (var t in item)
            {
                if ((!t.itemValue.HasModSlots || !t.itemValue.HasMods()) &&
                    t.itemValue.type == itemValue.type)
                {
                    items.Add(t);
                }
            }

            return items;
        }

        public static bool AddToNearbyContainer(EntityAlive player, ItemStack itemStack, float distance)
        {
            var tileEntities = GetTileEntities(player, distance);
            foreach (var tileEntity in tileEntities)
            {
                if (tileEntity is not TileEntityLootContainer lootTileEntity) continue;

                // If the container is open, don't include it.
                if (lootTileEntity.IsUserAccessing()) continue;

                // Don't try to add to a drop box.
                if (lootTileEntity.blockValue.Block.Properties.Values.ContainsKey("DropBox")) continue;

                // Can we quickly find a incomplete stack?
                if (lootTileEntity.TryStackItem(0, itemStack)) return true;

                var matchingItem = false;
                // Loop through the items and see if we have any matching items.
                foreach (var item in lootTileEntity.GetItems())
                {
                    // We match with something.
                    if (item.itemValue.type != itemStack.itemValue.type) continue;

                    matchingItem = true;
                    break;
                }

                // If we don't match, don't try to add.
                if (!matchingItem) continue;
                
                // We added a full stack! No need to keep processing.
                if (lootTileEntity.AddItem(itemStack)) return true;
            }

            return false;
        }

        public static void ConsumeItem(IEnumerable<ItemStack> itemStacks, EntityPlayerLocal localPlayer, int multiplier,  IList<ItemStack> _removedItems, Bag bag, Inventory toolbelt)
        {
            var tileEntities = GetTileEntities(localPlayer);
            var enumerable = itemStacks as ItemStack[] ?? itemStacks.ToArray();
            for (var i = 0; i < enumerable.Count(); i++)
            {
                // Grab from the backpack first.
                 var num = enumerable[i].count * multiplier;
                 if (bag != null)
                 {
                     num -= bag.DecItem(enumerable[i].itemValue, num, true, _removedItems);
                     if (num > 0)
                     {
                         // Check tool belt
                         if (toolbelt != null)
                         {
                             num -= toolbelt.DecItem(enumerable[i].itemValue, num, true, _removedItems);
                         }
                     }
                 }

                 // We've met our goals for this.
                if (num <= 0) continue;

                // check storage boxes
                foreach (var tileEntity in tileEntities)
                {
                    if (num <= 0) break;
                    if (tileEntity is not TileEntityLootContainer lootTileEntity) continue;

                    // If someone is using the tool account, skip it.
                    if (lootTileEntity.IsUserAccessing()) continue;

                    // If there's no items in this container, skip.
                    if (!lootTileEntity.HasItem(enumerable[i].itemValue)) continue;

                    for (var y = 0; y < lootTileEntity.items.Length; y++)
                    {
                        var item = lootTileEntity.items[y];
                        if (item.IsEmpty()) continue;
                        if (item.itemValue.ItemClass != enumerable[i].itemValue.ItemClass) continue;

                        // If we can completely satisfy the result, let's do that.
                        if (item.count >= num)
                        {
                            item.count -= num;
                            // Add the item to the removed items list so we can return it.
                            _removedItems.Add(new ItemStack(item.itemValue.Clone(), num));
                            num = 0;
                            
                        }
                        else
                        {
                            // Otherwise, let's just count down until we meet the requirement.
                            while (num >= 0)
                            {
                                item.count--;
                                num--;
                                if (item.count <= 0)
                                {
                                    _removedItems.Add(new ItemStack(item.itemValue.Clone(), num));
                                    break;
                                }
                            }
                        }

                        //Update the slot on the container, and do the Setmodified(), so that the dedis can get updated.
                        if (item.count < 1)
                        {
                            // Add it to the removed list.
                            _removedItems.Add(item.Clone());
                            lootTileEntity.UpdateSlot(y, ItemStack.Empty.Clone());

                        }
                        else
                        {
                            lootTileEntity.UpdateSlot(y, item);
                        }
                    }
                    lootTileEntity.SetModified();
                }
                
            }
        }


        public static bool IsEnemyNearby(EntityAlive self, float distance = 20f)
        {
            var nearbyEntities = new List<Entity>();

            // Search in the bounds are to try to find the most appealing entity to follow.
            var bb = new Bounds(self.position, new Vector3(distance, distance, distance));

            self.world.GetEntitiesInBounds(typeof(EntityAlive), bb, nearbyEntities);
            for (var i = nearbyEntities.Count - 1; i >= 0; i--)
            {
                var x = nearbyEntities[i] as EntityAlive;
                if (x == null) continue;
                if (x == self) continue;
                if (x.IsDead()) continue;
                if (!EntityTargetingUtilities.CanDamage(x, self)) continue;
                // Check to see if they are our enemy first, before deciding if we should see them.
                if (EntityTargetingUtilities.IsFriend(x, self)) continue;
                // Otherwise they are an enemy.
                return true;
            }

            Debug.LogWarning("no enemy");
            return false;
        }
    }
}