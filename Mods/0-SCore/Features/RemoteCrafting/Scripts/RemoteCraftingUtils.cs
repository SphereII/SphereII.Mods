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
        private const int NearbyContainerCacheTtlMs = 350;
        private const float NearbyContainerCachePositionBucketSize = 1f;

        private static NearbyContainerCacheEntry _nearbyContainerCache;

        private sealed class NearbyContainerCacheEntry
        {
            public NearbyContainerCacheKey Key { get; set; }
            public DateTime ExpiresAtUtc { get; set; }
            public List<TileEntity> TileEntities { get; set; }
        }

        private readonly struct NearbyContainerCacheKey : IEquatable<NearbyContainerCacheKey>
        {
            private readonly int _playerId;
            private readonly float _distance;
            private readonly bool _forRepairs;
            private readonly Vector3i _positionBucket;
            private readonly string _context;

            public NearbyContainerCacheKey(int playerId, float distance, bool forRepairs, Vector3i positionBucket, string context)
            {
                _playerId = playerId;
                _distance = distance;
                _forRepairs = forRepairs;
                _positionBucket = positionBucket;
                _context = context;
            }

            public bool Equals(NearbyContainerCacheKey other)
            {
                return _playerId == other._playerId &&
                       Math.Abs(_distance - other._distance) < 0.001f &&
                       _forRepairs == other._forRepairs &&
                       _positionBucket.Equals(other._positionBucket) &&
                       _context == other._context;
            }

            public override bool Equals(object obj)
            {
                return obj is NearbyContainerCacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = _playerId;
                    hashCode = (hashCode * 397) ^ _distance.GetHashCode();
                    hashCode = (hashCode * 397) ^ _forRepairs.GetHashCode();
                    hashCode = (hashCode * 397) ^ _positionBucket.GetHashCode();
                    hashCode = (hashCode * 397) ^ (_context != null ? _context.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        private sealed class NearbyContainerScanContext
        {
            public bool LandClaimContainersOnly { get; set; }
            public bool LandClaimPlayerOnly { get; set; }
            public string DisabledSenderRaw { get; set; }
            public string NotToWorkstationRaw { get; set; }
            public string BindToWorkstationRaw { get; set; }
            public string CurrentWorkstation { get; set; }
            public bool HasLocalWorkstationContext { get; set; }
            public bool CanUseNearbyContainerCache { get; set; }
            public bool InvertDisable { get; set; }
            public bool EnforceBindToWorkstation { get; set; }
            public string[] DisabledSenderValues { get; set; }
            public IReadOnlyList<WorkstationLootBinding> NotToWorkstationBindings { get; set; }
            public IReadOnlyList<WorkstationLootBinding> BindToWorkstationBindings { get; set; }
        }

        private readonly struct WorkstationLootBinding
        {
            public WorkstationLootBinding(string[] workstations, string[] lootLists)
            {
                Workstations = workstations;
                LootLists = lootLists;
            }

            public string[] Workstations { get; }
            public string[] LootLists { get; }
        }

        public static void InvalidateNearbyContainerCache()
        {
            _nearbyContainerCache = null;
        }

        private static bool TryGetNearbyContainerCache(NearbyContainerCacheKey key, out List<TileEntity> tileEntities)
        {
            var cache = _nearbyContainerCache;
            if (cache != null && DateTime.UtcNow <= cache.ExpiresAtUtc && cache.Key.Equals(key))
            {
                tileEntities = new List<TileEntity>(cache.TileEntities);
                return true;
            }

            tileEntities = null;
            return false;
        }

        private static void StoreNearbyContainerCache(NearbyContainerCacheKey key, List<TileEntity> tileEntities)
        {
            _nearbyContainerCache = new NearbyContainerCacheEntry
            {
                Key = key,
                ExpiresAtUtc = DateTime.UtcNow.AddMilliseconds(NearbyContainerCacheTtlMs),
                TileEntities = new List<TileEntity>(tileEntities)
            };
        }

        private static bool TryBuildNearbyContainerCacheKey(EntityAlive player, float distance, bool forRepairs,
            NearbyContainerScanContext scanContext, out NearbyContainerCacheKey key)
        {
            key = default;
            if (player == null || player.world == null || GameManager.Instance?.World == null || scanContext == null ||
                !scanContext.CanUseNearbyContainerCache) return false;

            var position = player.GetPosition();
            var positionBucket = new Vector3i(
                Mathf.FloorToInt(position.x / NearbyContainerCachePositionBucketSize),
                Mathf.FloorToInt(position.y / NearbyContainerCachePositionBucketSize),
                Mathf.FloorToInt(position.z / NearbyContainerCachePositionBucketSize));

            var landClaimContainersOnly = scanContext.LandClaimContainersOnly;
            var landClaimPlayerOnly = scanContext.LandClaimPlayerOnly;
            var disabledsender = scanContext.DisabledSenderRaw;
            var nottoWorkstation = scanContext.NotToWorkstationRaw;
            var bindtoWorkstation = scanContext.BindToWorkstationRaw;
            var invertDisable = scanContext.InvertDisable; // Invertdisable
            var enforceBindToWorkstation = scanContext.EnforceBindToWorkstation; // enforcebindtoWorkstation
            var workstation = scanContext.CurrentWorkstation; // GetCurrentWorkstation is resolved once during context creation.

            var context = string.Join("|", landClaimContainersOnly, landClaimPlayerOnly, disabledsender, nottoWorkstation,
                bindtoWorkstation, invertDisable, enforceBindToWorkstation, workstation);
            key = new NearbyContainerCacheKey(player.entityId, distance, forRepairs, positionBucket, context);
            return true;
        }

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

            var scanContext = BuildNearbyContainerScanContext(player, landClaimContainersOnly, landClaimPlayerOnly);
            var canUseNearbyContainerCache = TryBuildNearbyContainerCacheKey(player, distance, forRepairs, scanContext,
                out var nearbyContainerCacheKey);
            if (canUseNearbyContainerCache && TryGetNearbyContainerCache(nearbyContainerCacheKey, out var cachedTileEntities))
            {
                return cachedTileEntities;
            }

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
                if (!ShouldIncludeTileEntity(scanContext, tileEntity))
                {
                    continue;
                }

                tileEntities.Add(tileEntity);
            }

            if (canUseNearbyContainerCache)
            {
                StoreNearbyContainerCache(nearbyContainerCacheKey, tileEntities);
            }

            return tileEntities;
        }

        private static NearbyContainerScanContext BuildNearbyContainerScanContext(EntityAlive player,
            bool landClaimContainersOnly, bool landClaimPlayerOnly)
        {
            var disabledSenderRaw = Configuration.GetPropertyValue(AdvFeatureClass, "disablesender");
            var notToWorkstationRaw = Configuration.GetPropertyValue(AdvFeatureClass, "nottoWorkstation");
            var bindToWorkstationRaw = Configuration.GetPropertyValue(AdvFeatureClass, "bindtoWorkstation");
            var invertDisable = bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Invertdisable"));
            var enforceBindToWorkstation = bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "enforcebindtoWorkstation"));

            var hasLocalWorkstationContext = false;
            var canUseNearbyContainerCache = true;
            var currentWorkstation = string.Empty;
            if (player is EntityPlayerLocal playerLocal)
            {
                hasLocalWorkstationContext = true;
                try
                {
                    currentWorkstation = GetCurrentWorkstation(playerLocal);
                }
                catch
                {
                    canUseNearbyContainerCache = false;
                }
            }

            return new NearbyContainerScanContext
            {
                LandClaimContainersOnly = landClaimContainersOnly,
                LandClaimPlayerOnly = landClaimPlayerOnly,
                DisabledSenderRaw = disabledSenderRaw,
                NotToWorkstationRaw = notToWorkstationRaw,
                BindToWorkstationRaw = bindToWorkstationRaw,
                CurrentWorkstation = currentWorkstation,
                HasLocalWorkstationContext = hasLocalWorkstationContext,
                CanUseNearbyContainerCache = canUseNearbyContainerCache,
                InvertDisable = invertDisable,
                EnforceBindToWorkstation = enforceBindToWorkstation,
                DisabledSenderValues = SplitCsv(disabledSenderRaw),
                NotToWorkstationBindings = ParseWorkstationBindings(notToWorkstationRaw, true),
                BindToWorkstationBindings = ParseWorkstationBindings(bindToWorkstationRaw, false)
            };
        }

        private static string[] SplitCsv(string value)
        {
            return value?.Split(',') ?? Array.Empty<string>();
        }

        private static List<WorkstationLootBinding> ParseWorkstationBindings(string value, bool trimLootLists)
        {
            var bindings = new List<WorkstationLootBinding>();
            if (string.IsNullOrEmpty(value))
            {
                return bindings;
            }

            foreach (var binding in value.Split(';'))
            {
                var parts = binding.Split(':');
                var workstations = parts.Length > 0 ? SplitAndTrim(parts[0], true) : Array.Empty<string>();
                var lootLists = parts.Length > 1 ? SplitAndTrim(parts[1], trimLootLists) : Array.Empty<string>();
                bindings.Add(new WorkstationLootBinding(workstations, lootLists));
            }

            return bindings;
        }

        private static string[] SplitAndTrim(string value, bool trimEntries)
        {
            var values = value?.Split(',') ?? Array.Empty<string>();
            if (!trimEntries)
            {
                return values;
            }

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }

            return values;
        }

        private static bool ShouldIncludeTileEntity(NearbyContainerScanContext scanContext, TileEntity tileEntity)
        {
            if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out _))
            {
                return false;
            }

            if (scanContext.DisabledSenderValues.Length > 0 && scanContext.DisabledSenderValues[0] != null &&
                DisableSender(scanContext.DisabledSenderValues, scanContext.InvertDisable, tileEntity))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(scanContext.NotToWorkstationRaw) && NotToWorkstation(scanContext, tileEntity))
            {
                return false;
            }

            return string.IsNullOrEmpty(scanContext.BindToWorkstationRaw) || BindToWorkstation(scanContext, tileEntity);
        }

        public static bool DisableSender(IEnumerable<string> value, ITileEntity tileEntity)
        {
            var disableSenderValues = value as string[] ?? value.ToArray();
            var invertdisable = bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Invertdisable"));
            return DisableSender(disableSenderValues, invertdisable, tileEntity);
        }

        private static bool DisableSender(IReadOnlyCollection<string> value, bool invertDisable, ITileEntity tileEntity)
        {
            if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity)) return false;
            if (!invertDisable)
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

        private static bool BindToWorkstation(NearbyContainerScanContext scanContext, TileEntity tileEntity)
        {
            var result = false;
            if (!scanContext.HasLocalWorkstationContext) return false;
            if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity)) return false;

            if (scanContext.BindToWorkstationBindings.Where(binding =>
                    binding.Workstations.Any(ws => ws == scanContext.CurrentWorkstation))
                .Any(binding => binding.LootLists.Any(lootList => lootList == lootTileEntity.lootListName))) result = true;
            if (scanContext.BindToWorkstationBindings.Any(binding =>
                    binding.Workstations.Any(ws => ws == scanContext.CurrentWorkstation))
                || scanContext.EnforceBindToWorkstation)
                return result;
            {
                if (scanContext.BindToWorkstationBindings.Any(binding =>
                        binding.LootLists.Any(lootList => lootList == lootTileEntity.lootListName)))
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

        private static bool NotToWorkstation(NearbyContainerScanContext scanContext, TileEntity tileEntity)
        {
            var result = false;
            if (!scanContext.HasLocalWorkstationContext) return false;
            if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity)) return false;

            foreach (var binding in scanContext.NotToWorkstationBindings)
            {
                if ((binding.Workstations.Any(ws => ws == scanContext.CurrentWorkstation)) &&
                    (binding.LootLists.Any(lootList => lootList == lootTileEntity.lootListName))) result = true;
            }

            return result;
        }

        public static bool IsLootContainerOpenByAnotherPlayer(TileEntity tileEntity, EntityAlive player)
        {
            if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var tileEntityLootable)) return true;
            if (tileEntityLootable.IsUserAccessing()) return false;
            return false;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player)
        {
            var items = new List<ItemStack>();
            var tileEntities = GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity))
                    continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player))
                {
                    continue;
                }

                items.AddRange(lootTileEntity.items);
            }

            return items;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player, ItemValue itemValue)
        {
            var item = new List<ItemStack>();
            var tileEntities = GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity)) continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player)) continue;

                item.AddRange(lootTileEntity.items);
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
                if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity))
                    continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player)) continue;
                item.AddRange(lootTileEntity.items);
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
            return AddToNearbyContainer(player, itemStack, tileEntities);
        }

        public static bool AddToNearbyContainer(EntityAlive player, ItemStack itemStack, IEnumerable<TileEntity> tileEntities)
        {
            if (tileEntities == null) return false;

            foreach (var tileEntity in tileEntities)
            {
                if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity)) continue;
                if (IsLootContainerOpenByAnotherPlayer(tileEntity, player)) continue;
                if (CheckTileEntity(itemStack, lootTileEntity))
                {
                    return true;
                }
            }

            return false;
        }


        private static bool CheckTileEntity(ItemStack itemStack, ITileEntityLootable lootTileEntity)
        {
            var itemName = itemStack.itemValue?.ItemClass?.GetItemName() ?? "unknown";
            var containerPos = (lootTileEntity as TileEntity)?.ToWorldPos().ToString() ?? "?";

            if (lootTileEntity == null || lootTileEntity.IsUserAccessing())
            {
                return false;
            }

            // Skip the drop box itself — don't distribute back into drop boxes
            if (lootTileEntity.blockValue.Block.Properties.Values.ContainsKey("DropBox"))
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
                {
                    lootTileEntity.SetModified();
                    InvalidateNearbyContainerCache();
                }

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
                    if (!tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var lootTileEntity))
                        continue;
                    // If someone is using the tool account, skip it.
                    if (IsLootContainerOpenByAnotherPlayer(tileEntity, localPlayer)) continue;
                    // If there's no items in this container, skip.


                    if (!lootTileEntity.HasItem(enumerable[i].itemValue)) continue;

                    var containerChanged = false;
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
                            var itemStack = new ItemStack(item.itemValue.Clone(), num);
                            _removedItems.Add(itemStack);
                            num = 0;
                        }
                        else
                        {
                            // Remove only the amount this container can actually satisfy, then continue to the next source.
                            var removedCount = Math.Min(item.count, num);
                            item.count -= removedCount;
                            num -= removedCount;
                            _removedItems.Add(new ItemStack(item.itemValue.Clone(), removedCount));
                        }

                        //Update the slot on the container, and do the Setmodified(), so that the dedis can get updated.
                        if (item.count < 1)
                        {
                            // Add it to the removed list.
                            _removedItems.Add(item.Clone());
                            lootTileEntity.UpdateSlot(y, ItemStack.Empty.Clone());
                            containerChanged = true;
                        }
                        else
                        {
                            lootTileEntity.UpdateSlot(y, item);
                            containerChanged = true;
                        }
                    }

                    if (containerChanged)
                    {
                        lootTileEntity.SetModified();
                        InvalidateNearbyContainerCache();
                    }
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