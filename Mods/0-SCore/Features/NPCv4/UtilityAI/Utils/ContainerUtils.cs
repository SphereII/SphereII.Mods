using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    /// <summary>
    /// V4 container and tile-entity utilities: world scanning, looting, and container validation.
    /// <para>
    /// Differences from <see cref="SCoreUtils"/>:
    /// <list type="bullet">
    ///   <item><see cref="ScanForTileEntities(EntityAlive,string,bool)"/> caches results
    ///         per entity + target-type combination for <see cref="AIConstants.TileEntityCacheTtl"/>
    ///         seconds, avoiding redundant chunk iteration every consideration tick.</item>
    ///   <item>The inner <c>NearestPathSorter</c> class is replaced with a
    ///         <c>sqrMagnitude</c>-based lambda comparison.</item>
    ///   <item>All magic numbers replaced with <see cref="AIConstants"/> constants.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class ContainerUtils
    {
        // ── Scan Cache ────────────────────────────────────────────────────────────

        // Key: (entityId, targetTypesHashCode)  Value: (paths, timestamp)
        private static readonly Dictionary<(int, int), (List<Vector3> paths, float time)> _scanCache
            = new Dictionary<(int, int), (List<Vector3>, float)>();

        // ── ScanForTileEntities ────────────────────────────────────────────────────

        /// <inheritdoc cref="ScanForTileEntities(EntityAlive,string,bool)"/>
        public static List<Vector3> ScanForTileEntities(Context context, string targetTypes = "")
        {
            return ScanForTileEntities(context.Self, targetTypes);
        }

        /// <summary>
        /// Returns a distance-sorted list of world positions for tile entities near
        /// <paramref name="self"/> that match the <paramref name="targetTypes"/> filter.
        /// Results are cached per entity + filter string for
        /// <see cref="AIConstants.TileEntityCacheTtl"/> seconds.
        /// </summary>
        /// <param name="self">The entity performing the scan.</param>
        /// <param name="targetTypes">
        /// Comma-separated tile-entity type names, optionally qualified with a block name
        /// after a colon (e.g. <c>"Loot:crateWooden,Workstation"</c>).
        /// Passing an empty string or <c>"basic"</c> uses a default set of common types.
        /// </param>
        /// <param name="ignoreTouch">When <c>true</c>, already-touched loot containers are included.</param>
        public static List<Vector3> ScanForTileEntities(EntityAlive self, string targetTypes = "",
            bool ignoreTouch = false)
        {
            // Normalise default filter.
            if (string.IsNullOrEmpty(targetTypes) || targetTypes.ToLower().Contains("basic"))
                targetTypes = "LandClaim,Loot,VendingMachine,Forge,Campfire,Workstation,PowerSource,Composite";

            var cacheKey = (self.entityId, targetTypes.GetHashCode());
            var now      = Time.time;

            if (_scanCache.TryGetValue(cacheKey, out var cached) &&
                now - cached.time < AIConstants.TileEntityCacheTtl)
                return cached.paths;

            // Cache miss — perform the scan.
            var paths       = new List<Vector3>();
            var blockPos    = self.GetBlockPosition();
            var chunkX      = World.toChunkXZ(blockPos.x);
            var chunkZ      = World.toChunkXZ(blockPos.z);
            var radius      = AIConstants.TileEntityScanChunkRadius;

            // Pre-split the filter string once rather than inside the innermost loop.
            var filterEntries = targetTypes.Split(',');

            for (var i = -radius; i <= radius; i++)
            {
                for (var j = -radius; j <= radius; j++)
                {
                    var chunk = (Chunk) self.world.GetChunkSync(chunkX + j, chunkZ + i);
                    if (chunk == null) continue;

                    foreach (var tileEntity in chunk.GetTileEntities().list)
                    {
                        foreach (var filterEntryRaw in filterEntries)
                        {
                            var filterEntry = filterEntryRaw;
                            var blockNames  = "";

                            // Optional block-name qualifier after ":".
                            if (filterEntry.Contains(":"))
                            {
                                var parts   = filterEntry.Split(':');
                                filterEntry = parts[0];
                                blockNames  = parts[1];
                            }

                            var targetType = EnumUtils.Parse<TileEntityType>(filterEntry.Trim(), true);
                            if (tileEntity.GetTileEntityType() != targetType) continue;

                            switch (tileEntity.GetTileEntityType())
                            {
                                case TileEntityType.None:
                                    continue;
                                case TileEntityType.Loot:
                                    if (!ignoreTouch && ((TileEntityLootContainer) tileEntity).bTouched)
                                        continue;
                                    break;
                                case TileEntityType.SecureLoot:
                                    if (!ignoreTouch && ((TileEntitySecureLootContainer) tileEntity).bTouched)
                                        continue;
                                    break;
                            }

                            if (!string.IsNullOrEmpty(blockNames) &&
                                !blockNames.Contains(tileEntity.blockValue.Block.GetBlockName()))
                                continue;

                            paths.Add(tileEntity.ToWorldPos().ToVector3());
                        }
                    }
                }
            }

            // Sort by squared distance — no allocating IComparer object needed.
            paths.Sort((a, b) => self.GetDistanceSq(a).CompareTo(self.GetDistanceSq(b)));

            _scanCache[cacheKey] = (paths, now);
            return paths;
        }

        // ── GetItemFromContainer ──────────────────────────────────────────────────

        /// <summary>
        /// Loots items from <paramref name="tileContainer"/> into the entity's own loot inventory,
        /// scaling loot game-stage from the leader player if one exists.
        /// </summary>
        public static void GetItemFromContainer(Context context, TileEntityLootContainer tileContainer)
        {
            var blockPos = tileContainer.ToWorldPos();

            if (string.IsNullOrEmpty(tileContainer.lootListName)) return;
            if (tileContainer.bTouched) return;

            tileContainer.bTouched    = true;
            tileContainer.bWasTouched = true;

            if (tileContainer.items == null) return;

            context.Self.SetLookPosition(blockPos);
            context.Self.MinEventContext.TileEntity = tileContainer;
            context.Self.FireEvent(MinEventTypes.onSelfOpenLootContainer);

            var lootContainer = LootContainer.GetLootContainer(tileContainer.lootListName);
            if (lootContainer == null)
            {
                context.Self.SetLookPosition(Vector3.zero);
                return;
            }

            var leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityPlayer;
            var lootGameStage = leader != null ? leader.unModifiedGameStage : 1f;

            var items = lootContainer.Spawn(
                context.Self.rand,
                tileContainer.items.Length,
                lootGameStage,
                0f,
                leader,
                new FastTags<TagGroup.Global>(),
                lootContainer.UniqueItems,
                true);

            for (var i = 0; i < items.Count; i++)
                context.Self.lootContainer.AddItem(items[i].Clone());

            context.Self.FireEvent(MinEventTypes.onSelfLootContainer);
            context.Self.SetLookPosition(Vector3.zero);
        }

        // ── CheckContainer ────────────────────────────────────────────────────────

        /// <summary>
        /// Validates that the entity is on the ground, facing <paramref name="position"/>,
        /// within reach, and that a valid tile entity exists there; then loots it.
        /// Returns <c>true</c> when the container was successfully interacted with.
        /// </summary>
        public static bool CheckContainer(Context context, Vector3 position)
        {
            if (!context.Self.onGround)
                return false;

            context.Self.SetLookPosition(position);

            var lookRay = new Ray(context.Self.position, context.Self.GetLookVector());
            context.Self.SetLookPosition(Vector3.zero);

            if (!Voxel.Raycast(context.Self.world, lookRay, AIConstants.ContainerRaycastLength, false, false))
                return false;

            if (!Voxel.voxelRayHitInfo.bHitValid)
                return false;

            var distSq = (position - context.Self.position).sqrMagnitude;
            if (distSq > AIConstants.ContainerReachDistSq)
                return false;

            var tileEntity = context.Self.world.GetTileEntity(
                Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(position));

            if (tileEntity is TileEntityLootContainer lootContainer)
                GetItemFromContainer(context, lootContainer);

            EntityUtilities.Stop(context.Self.entityId);
            return true;
        }
    }
}
