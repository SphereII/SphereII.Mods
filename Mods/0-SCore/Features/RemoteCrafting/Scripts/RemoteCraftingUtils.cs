using System;
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

        private static string GetCurrentWorkstation(EntityPlayerLocal player)
        {
            var te = player?.PlayerUI?.xui?.CurrentWorkstationInputGrid?.WorkstationData?.TileEntity;
            if (te == null) return "";
            var pos = te.ToWorldPos();
            return GameManager.Instance.World.GetBlock(pos).Block.GetBlockName();
        }

        public static List<TileEntity> GetTileEntities(EntityAlive player)
        {
            var distance = 30f;
            var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
            if (!string.IsNullOrEmpty(strDistance))
                distance = StringParsers.ParseFloat(strDistance);
            var tileEntities = GetTileEntities(player, distance, false);
            return tileEntities;
        }

        public static bool CheckEnemyForCrafting()
        {
            return Configuration.CheckFeatureStatus("AdvancedRecipes", "BlockOnNearbyEnemies");
        }

        public static bool CheckEnemyForRepairing()
        {
            return Configuration.CheckFeatureStatus("BlockUpgradeRepair", "BlockOnNearbyEnemies");
        }

        public static bool CheckForLandClaimContainers()
        {
            return Configuration.CheckFeatureStatus("AdvancedRecipes", "LandClaimContainersOnly");
        }

        public static bool CheckForLandClaimPlayer()
        {
            return Configuration.CheckFeatureStatus("AdvancedRecipes", "LandClaimPlayerOnly");
        }

        public static List<TileEntity> GetTileEntities(EntityAlive player, float distance, bool forRepairs)
        {
            if (forRepairs && CheckEnemyForRepairing())
                if (IsEnemyNearby(player))
                {
                    return new List<TileEntity>();
                }

            if (!forRepairs && CheckEnemyForCrafting())
                if (IsEnemyNearby(player))
                {
                    return new List<TileEntity>();
                }

            var landClaimContainersOnly = CheckForLandClaimContainers();
            var world = GameManager.Instance.World;

            // If the player is not within a land claim zone, don't search for containers.
            var landClaimPlayerOnly = CheckForLandClaimPlayer();
            if (landClaimPlayerOnly)
            {
                var vector3I = new Vector3i(player.GetPosition());
                var persistentPlayer = world.GetGameManager().GetPersistentLocalPlayer();
                if (persistentPlayer == null)
                {
                    return new List<TileEntity>();
                }
                if (!world.IsMyLandProtectedBlock(vector3I, persistentPlayer))
                {
                    return new List<TileEntity>();
                }
            }

            var disabledsender = Configuration.GetPropertyValue(AdvFeatureClass, "disablesender").Split(',');
            var nottoWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "nottoWorkstation");
            var bindtoWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "bindtoWorkstation");
            var tileEntities = new List<TileEntity>();
            const string targetTypes = "Loot, SecureLoot, SecureLootSigned, Composite";
            var paths = SCoreUtils.ScanForTileEntities(player, targetTypes, true);
            foreach (var path in paths)
            {
                if (landClaimContainersOnly)
                {
                    var vector3I = new Vector3i(path);
                    var persistentPlayer = world.GetGameManager().GetPersistentLocalPlayer();
                    if (persistentPlayer == null)
                    {
                        continue;
                    }
                    if (!world.IsMyLandProtectedBlock(vector3I, persistentPlayer))
                    {
                        continue;
                    }
                }
                else
                {
                    var distanceToLeader = Vector3.Distance(player.position, path);
                    if (!(distanceToLeader < distance)) continue;
                }

                var tileEntity = player.world.GetTileEntity(new Vector3i(path));
                if (tileEntity == null)
                {
                    continue;
                }

                // Check if Broadcastmanager is running if running check if lootcontainer is in Broadcastmanager dictionary
                // note: Broadcastmanager.Instance.Check()) throws nullref if Broadcastmanager is not running.
                // works because Hasinstance is being checked first in the or.
                if (Broadcastmanager.HasInstance && !Broadcastmanager.Instance.Check(tileEntity.ToWorldPos()))
                {
                    continue;
                }
                if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity))
                {
                    continue;
                }
                if (disabledsender[0] != null)
                {
                    if (DisableSender(disabledsender, tileEntity))
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(nottoWorkstation))
                {
                    if (NotToWorkstation(nottoWorkstation, player, tileEntity))
                    {
                        continue;
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
            }

            return tileEntities;
        }

        public static bool DisableSender(IEnumerable<string> value, ITileEntity tileEntity)
        {
            if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity)) return false;
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
            if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity)) return false;

            // TODO: we want to refactor this to remove the complex LinQ.
            // bind storage to workstation
            if (value.Split(';').Where(x =>
                    x.Split(':')[0].Split(',').Any(ws => ws.Trim() == GetCurrentWorkstation(playerLocal)))
                .Any(x => x.Split(':')[1].Split(',').Any(y => y == lootTileEntity.lootListName))) result = true;
            // bind storage to other workstations if allowed
            if (value.Split(';').Any(x =>
                    x.Split(':')[0].Split(',').Any(ws => ws.Trim() == GetCurrentWorkstation(playerLocal)))
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
            if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity)) return false;

            foreach (var bind in value.Split(';'))
            {
                var workstation = bind.Split(':')[0].Split(',');
                var disablebinding = bind.Split(':')[1].Split(',');
                if ((workstation.Any(ws => ws.Trim() == GetCurrentWorkstation(playerLocal))) &&
                    (disablebinding.Any(x => x.Trim() == lootTileEntity.lootListName))) result = true;
            }

            return result;
        }

        public static bool IsLootContainerOpenByAnotherPlayer(TileEntity tileEntity, EntityAlive player)
        {
            if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var tileEntityLootable)) return true;
            if (tileEntityLootable.IsUserAccessing()) return false;
            return false;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player)
        {
            var items = new List<ItemStack>();
            var tileEntities = GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity))
                    continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player))
                {
                    continue;
                }

                items.AddRange(lootTileEntity.ItemGrid.items);
            }

            return items;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player, ItemValue itemValue)
        {
            var item = new List<ItemStack>();
            var tileEntities = GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity)) continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player)) continue;

                item.AddRange(lootTileEntity.ItemGrid.items);
            }

            var items = new List<ItemStack>();
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
            var tileEntities = GetTileEntities(player, distance, false);
            foreach (var tileEntity in tileEntities)
            {
                if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity))
                    continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player)) continue;
                item.AddRange(lootTileEntity.ItemGrid.items);
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
            var tileEntities = GetTileEntities(player, distance, false);
            var itemName = itemStack.itemValue?.ItemClass?.GetItemName() ?? "unknown";
            foreach (var tileEntity in tileEntities)
            {
                if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity)) continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player)) continue;
                if (CheckTileEntity(itemStack, lootTileEntity))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Moves an item stack from a drop box into a container near the box. Used by the drop box's
        /// timed sort, which runs on the server. The search is centred on the box and limited to
        /// containers the box's owner could use, so it does not depend on which player is the host.
        /// </summary>
        public static bool AddToContainersNearBox(TileEntityComposite box, ItemStack itemStack, float distance)
        {
            foreach (var tileEntity in GetTileEntitiesNearBox(box, distance))
            {
                if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity)) continue;
                if (CheckTileEntity(itemStack, lootTileEntity))
                {
                    if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Debug"))
                        Log.Out($"[DropBox] Timed sort: moved {itemStack.itemValue?.ItemClass?.GetItemName()} from {box.ToWorldPos()} to {tileEntity.ToWorldPos()}");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Containers within <paramref name="distance"/> of the drop box that the box's owner may use,
        /// nearest first. A box with no owner (not placed by a player) gets no containers.
        /// </summary>
        public static List<TileEntity> GetTileEntitiesNearBox(TileEntityComposite box, float distance)
        {
            var tileEntities = new List<TileEntity>();
            var owner = box?.Owner;
            if (owner == null) return tileEntities;

            var world = GameManager.Instance.World;
            var ownerData = GameManager.Instance.GetPersistentPlayerList()?.GetPlayerData(owner);
            if (ownerData == null) return tileEntities;

            var boxPos = box.ToWorldPos();
            if (CheckForLandClaimPlayer() && !world.IsMyLandProtectedBlock(boxPos, ownerData))
                return tileEntities;

            var landClaimContainersOnly = CheckForLandClaimContainers();
            var disabledSender = Configuration.GetPropertyValue(AdvFeatureClass, "disablesender").Split(',');
            var center = boxPos.ToVector3();
            var chunkRadius = Mathf.CeilToInt(distance / 16f);
            var chunkX = World.toChunkXZ(boxPos.x);
            var chunkZ = World.toChunkXZ(boxPos.z);

            for (var cz = chunkZ - chunkRadius; cz <= chunkZ + chunkRadius; cz++)
            {
                for (var cx = chunkX - chunkRadius; cx <= chunkX + chunkRadius; cx++)
                {
                    if (world.GetChunkSync(cx, cz) is not Chunk chunk) continue;
                    foreach (var tileEntity in chunk.GetTileEntities().list)
                    {
                        if (tileEntity == null || tileEntity == box) continue;
                        if (!IsContainerType(tileEntity.GetTileEntityType())) continue;

                        var pos = tileEntity.ToWorldPos();
                        if (Vector3.Distance(center, pos.ToVector3()) >= distance) continue;
                        if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out _)) continue;
                        if (IsDropBox(tileEntity.blockValue.Block)) continue;

                        // Locked containers must allow the box's owner.
                        if (tileEntity.TryGetSelfOrFeature<ILockable>(out var lockable) && lockable.IsLocked() &&
                            !lockable.IsUserAllowed(owner))
                            continue;

                        // Never sort into containers inside someone else's land claim.
                        if (world.GetLandClaimOwner(pos, ownerData) == EnumLandClaimOwner.Other) continue;
                        if (landClaimContainersOnly && !world.IsMyLandProtectedBlock(pos, ownerData)) continue;

                        if (Broadcastmanager.HasInstance && !Broadcastmanager.Instance.Check(pos)) continue;
                        if (disabledSender[0] != null && DisableSender(disabledSender, tileEntity)) continue;

                        tileEntities.Add(tileEntity);
                    }
                }
            }

            tileEntities.Sort((a, b) =>
                Vector3.Distance(center, a.ToWorldPos().ToVector3())
                    .CompareTo(Vector3.Distance(center, b.ToWorldPos().ToVector3())));
            return tileEntities;
        }

        private static bool IsContainerType(TileEntityType type)
        {
            return type is TileEntityType.Loot or TileEntityType.SecureLoot or TileEntityType.SecureLootSigned
                or TileEntityType.Composite;
        }

        /// <summary>A drop box is either a BlockDropBoxContainer or any block with the DropBox property.</summary>
        public static bool IsDropBox(Block block)
        {
            return block is BlockDropBoxContainer || block.Properties.Values.ContainsKey("DropBox");
        }

        private static bool CheckTileEntity(ItemStack itemStack, TEFeatureStorage lootTileEntity)
        {
            var itemName = itemStack.itemValue?.ItemClass?.GetItemName() ?? "unknown";
            var containerPos = lootTileEntity?.ToWorldPos().ToString() ?? "?";

            if (lootTileEntity == null || lootTileEntity.IsUserAccessing())
            {
                return false;
            }

            // Never distribute into a drop box, whether marked by its class or the DropBox property.
            if (IsDropBox(lootTileEntity.blockValue.Block))
            {
                return false;
            }

            if (!lootTileEntity.HasItem(itemStack.itemValue))
            {
                return false;
            }

            bool changed = false;
            lootTileEntity.SetUserAccessing(true);

            try
            {
                var result = lootTileEntity.TryStackItem(0, itemStack);
                if (result.allMoved)
                {
                    changed = true;
                    return true;
                }

                if (lootTileEntity.AddItem(itemStack))
                {
                    changed = true;
                    return true;
                }

            }
            finally
            {
                if (changed)
                    lootTileEntity.SetModified();

                lootTileEntity.SetUserAccessing(false);
            }

            return false;
        }

        public static void ConsumeItem(IEnumerable<ItemStack> itemStacks, EntityPlayerLocal localPlayer, int multiplier,
            IList<ItemStack> _removedItems, Bag bag, Inventory toolbelt)
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
                    if (!tileEntity.TryGetSelfOrFeature<TEFeatureStorage>(out var lootTileEntity))
                        continue;
                    // If someone is using the tool account, skip it.
                    if (IsLootContainerOpenByAnotherPlayer(tileEntity, localPlayer)) continue;
                    // If there's no items in this container, skip.


                    if (!lootTileEntity.HasItem(enumerable[i].itemValue)) continue;

                    for (var y = 0; y < lootTileEntity.ItemGrid.items.Length; y++)
                    {
                        var item = lootTileEntity.ItemGrid.items[y];
                        if (item.IsEmpty()) continue;
                        if (item.itemValue.ItemClass != enumerable[i].itemValue.ItemClass) continue;

                        // If we can completely satisfy the result, let's do that.
                        if (item.count >= num)
                        {
                            item.count -= num;
                            // Add the item to the removed items list so we can return it.
                            var itemStack = new ItemStack(item.itemValue.Clone(), num);
                            _removedItems.Add(itemStack);
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

            var player = self as EntityPlayer;
            if (distance == 20f)
            {
                var strDistanceE = Configuration.GetPropertyValue(AdvFeatureClass, "DistanceEnemy");
                if (!string.IsNullOrEmpty(strDistanceE))
                    distance = StringParsers.ParseFloat(strDistanceE);
            }

            // Search in the bounds are to try to find the most appealing entity to follow.
            var bb = new Bounds(self.position, new Vector3(distance, distance, distance));

            self.world.GetEntitiesInBounds(typeof(EntityAlive), bb, nearbyEntities);
            for (var i = nearbyEntities.Count - 1; i >= 0; i--)
            {
                var x = nearbyEntities[i] as EntityAlive;
                if (x == null) continue;
                if (x == self) continue;
                if (x.IsDead())
                {
                    continue;
                }

                if (player && player.Party != null)
                {
                    // Are they in the same party?
                    if (player.Party.ContainsMember(x.entityId)) continue;
                    if (x is EntityPlayer nearbyPlayer)
                    {
                        // Are they friends with each other?
                        if (player.IsFriendsWith(nearbyPlayer)) continue;
                    }
                }

                if (!EntityTargetingUtilities.CanDamage(x, self))
                {
                    continue;
                }

                // Check to see if they are our enemy first, before deciding if we should see them.
                if (EntityTargetingUtilities.IsFriend(x, self))
                {
                    continue;
                }

                // Otherwise they are an enemy.
                return true;
            }

            //Debug.LogWarning("no enemy");
            return false;
        }
    }
}