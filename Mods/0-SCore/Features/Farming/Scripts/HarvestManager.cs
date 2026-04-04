using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Maintains a per-NPC harvest inventory for trader-type entities (e.g. EntityAliveSDXV4).
///
/// Problem: EntityAliveSDXV4 extends EntityTrader. Its lootContainer is a TileEntityTrader,
/// whose write/read path serialises both TileEntityLootContainer.items[] AND TraderData in
/// one stream. TileEntityLootContainer.AddItem() internally calls SetModified(), which
/// triggers a TileEntityTrader network packet. Any mismatch between the items[] store and
/// TraderData.PrimaryInventory causes EndOfStreamException on the receiving client.
///
/// Solution: Store harvested crops in a plain TileEntityLootContainer that is completely
/// separate from the trader entity's lootContainer. This container is never attached to a
/// chunk or to the world tile-entity system, so SetModified() is never called on it during
/// harvest. The player accesses it via the standard looting window (OpenInventory dialog
/// command), which reads directly from the in-memory items[] array.
/// </summary>
public static class HarvestManager
{
    private const int ContainerWidth  = 8;
    private const int ContainerHeight = 8;

    // Keyed by entity ID. Entries persist for the server session; removed when the NPC is
    // picked up (via EntitySyncUtils.Collect) or killed.
    private static readonly Dictionary<int, TileEntityLootContainer> _containers =
        new Dictionary<int, TileEntityLootContainer>();

    // -------------------------------------------------------------------------
    // Client-side pending state
    // -------------------------------------------------------------------------
    // When a dedicated-server client opens a trader NPC's harvest inventory,
    // NetPackageHarvestInventoryData creates a temporary local TileEntityLootContainer
    // and stores it here. XUiC_LootWindowGroup_OnClose reads these fields on close,
    // serialises whatever items remain, and sends them back to the server via
    // NetPackageHarvestInventoryUpdate so the server-side container stays accurate.
    // Both fields are -1 / null when no harvest window is open.
    public static int ClientPendingEntityId                    = -1;
    public static TileEntityLootContainer ClientPendingContainer = null;

    /// <summary>Returns (or lazily creates) the harvest container for the given entity.</summary>
    public static TileEntityLootContainer GetOrCreate(int entityId)
    {
        if (!_containers.TryGetValue(entityId, out var container))
        {
            // XUiC_LootWindowGroup.OnOpen() calls TileEntity.get_blockValue(), which dereferences
            // this.chunk. A null chunk causes NullReferenceException before OnOpen can finish.
            // We resolve this two ways:
            //   1. Pass the entity's current chunk so blockValue returns a valid (if irrelevant) block.
            //   2. Set entityId so OnOpen takes the entity loot-stage path, which never casts
            //      blockValue.Block to BlockLoot (only the block-based path does that cast).
            Chunk chunk = null;
            var world = GameManager.Instance?.World;
            if (world != null)
            {
                var entity = world.GetEntity(entityId);
                if (entity != null)
                    chunk = world.GetChunkSync(
                        World.toChunkXZ((int)entity.position.x),
                        World.toChunkXZ((int)entity.position.z)) as Chunk;
            }

            container = new TileEntityLootContainer(chunk);
            container.entityId = entityId;
            container.lootListName = "traderNPC";
            container.SetContainerSize(new Vector2i(ContainerWidth, ContainerHeight), true);

            // bTouched = false tells the vanilla loot system the chest has never been opened
            // and should have loot generated from lootListName — which would replace our items.
            // bPlayerStorage = true suppresses the loot-respawn tick.
            container.bTouched = true;
            container.bPlayerStorage = true;

            _containers[entityId] = container;
        }
        return container;
    }

    /// <summary>
    /// Adds an item to the entity's harvest container.
    /// Writes directly to items[] without calling SetModified, so no network packet is sent.
    /// Stacks with existing entries where possible.
    /// </summary>
    /// <returns>True if the item fit; false if the container was full (caller should drop).</returns>
    public static bool AddItem(int entityId, ItemStack itemStack)
    {
        var container = GetOrCreate(entityId);
        var items = container.items;

        // Pass 1 — try to stack with an existing slot
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].IsEmpty() || items[i].itemValue.type != itemStack.itemValue.type) continue;

            var cls = ItemClass.GetForId(itemStack.itemValue.type);
            if (cls == null || !cls.CanStack()) continue;

            int space = cls.Stacknumber.Value - items[i].count;
            if (space <= 0) continue;

            int toAdd = Math.Min(itemStack.count, space);
            items[i].count += toAdd;
            itemStack = new ItemStack(itemStack.itemValue, itemStack.count - toAdd);
            if (itemStack.count <= 0) return true;
        }

        // Pass 2 — find an empty slot
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].IsEmpty()) continue;
            items[i] = itemStack.Clone();
            return true;
        }

        return false; // container full
    }

    /// <summary>
    /// Removes the harvest container for an entity.
    /// Call this when the NPC is picked up (EntitySyncUtils.Collect) or dies.
    /// </summary>
    public static void Remove(int entityId) => _containers.Remove(entityId);

    /// <summary>Returns true if a non-empty harvest container exists for the entity.</summary>
    public static bool Has(int entityId) => _containers.ContainsKey(entityId);

    // -------------------------------------------------------------------------
    // Persistence
    // -------------------------------------------------------------------------
    private const string SaveFile    = "HarvestManager.bin";
    private const int    SaveVersion = 1;

    /// <summary>
    /// Persists all harvest containers to disk in the current save game directory.
    /// Safe to call only on the server; silently no-ops on clients.
    /// </summary>
    public static void Save()
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (!Directory.Exists(GameIO.GetSaveGameDir())) return;

        var path = $"{GameIO.GetSaveGameDir()}/{SaveFile}";
        try
        {
            if (File.Exists(path))
                File.Copy(path, $"{path}.bak", true);

            using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var bw = new BinaryWriter(fs);
            bw.Write(SaveVersion);
            bw.Write(_containers.Count);
            foreach (var kvp in _containers)
            {
                bw.Write(kvp.Key);
                bw.Write(EntitySyncUtils.SerializeItemStackArray(kvp.Value.items) ?? string.Empty);
            }
            Log.Out($"[HarvestManager] Saved {_containers.Count} harvest container(s).");
        }
        catch (Exception e)
        {
            Log.Error($"[HarvestManager] Save failed: {e}");
        }
    }

    /// <summary>GameShutdown event handler — delegates to <see cref="Save()"/>.</summary>
    public static void Save(ref ModEvents.SGameShutdownData data) => Save();

    /// <summary>
    /// Loads harvest containers from disk. Registered as a GameStartDone handler so it
    /// runs after the world is ready. Only executes on the server.
    /// </summary>
    public static void Load(ref ModEvents.SGameStartDoneData data)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        var path = $"{GameIO.GetSaveGameDir()}/{SaveFile}";
        if (!Directory.Exists(GameIO.GetSaveGameDir()) || !File.Exists(path))
        {
            Log.Out("[HarvestManager] No save file found, starting fresh.");
            return;
        }

        TryLoad(path);
    }

    private static void TryLoad(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);
            int version = br.ReadInt32();
            int count   = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int    entityId   = br.ReadInt32();
                string serialized = br.ReadString();
                var    items      = EntitySyncUtils.DeserializeItemStackArray(serialized);
                var    container  = GetOrCreate(entityId);
                for (int j = 0; j < items.Length && j < container.items.Length; j++)
                    container.items[j] = items[j];
            }
            Log.Out($"[HarvestManager] Loaded {_containers.Count} harvest container(s) from {path}.");
        }
        catch (Exception e)
        {
            Log.Error($"[HarvestManager] Load failed for '{path}': {e}");
            var backup = $"{path}.bak";
            if (File.Exists(backup) && path != backup)
            {
                Log.Warning("[HarvestManager] Attempting to load from backup...");
                TryLoad(backup);
            }
        }
    }

  
}
