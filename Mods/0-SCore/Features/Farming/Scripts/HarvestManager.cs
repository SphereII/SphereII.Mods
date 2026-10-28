using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Maintains a per-NPC harvest inventory for trader-type entities (e.g. EntityAliveSDXV4).
///
/// Problem: EntityAliveSDXV4 extends EntityTrader. Its lootContainer is a TileEntityTrader,
/// whose write/read path serialises both SCoreLootContainer.items[] AND TraderData in
/// one stream. SCoreLootContainer.AddItem() internally calls SetModified(), which
/// triggers a TileEntityTrader network packet. Any mismatch between the items[] store and
/// TraderData.PrimaryInventory causes EndOfStreamException on the receiving client.
///
/// Solution: Store harvested crops in a plain SCoreLootContainer that is completely
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
    private static readonly Dictionary<int, SCoreLootContainer> _containers =
        new Dictionary<int, SCoreLootContainer>();

    // -------------------------------------------------------------------------
    // Client-side pending state
    // -------------------------------------------------------------------------
    // When a dedicated-server client opens a trader NPC's harvest inventory,
    // NetPackageHarvestInventoryData creates a temporary local SCoreLootContainer
    // and stores it here. XUiC_LootWindowGroup_OnClose reads these fields on close,
    // serialises whatever items remain, and sends them back to the server via
    // NetPackageHarvestInventoryUpdate so the server-side container stays accurate.
    // Both fields are -1 / null when no harvest window is open.
    public static int ClientPendingEntityId                    = -1;
    public static SCoreLootContainer ClientPendingContainer = null;

    /// <summary>Returns (or lazily creates) the harvest container for the given entity.</summary>
    public static SCoreLootContainer GetOrCreate(int entityId)
    {
        if (!_containers.TryGetValue(entityId, out var container))
        {
            // XUiC_LootWindowGroup.OnOpen() calls TileEntity.get_blockValue(), which dereferences
            // this.chunk. A null chunk causes NullReferenceException before OnOpen can finish.
            // We resolve this two ways:
            //   1. Pass the entity's current chunk so blockValue returns a valid (if irrelevant) block.
            //   2. Set entityId so OnOpen takes the entity loot-stage path, which never casts
            //      blockValue.Block to BlockCompositeTileEntity (only the block-based path does that cast).
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

            container = new SCoreLootContainer(chunk);
            container.EntityId = entityId;
            container.lootListName = "traderNPC";
            container.SetContainerSize(new Vector2i(ContainerWidth, ContainerHeight), true);

            // bTouched = false tells the vanilla loot system the chest has never been opened
            // and should have loot generated from lootListName — which would replace our items.
            // bPlayerStorage = true suppresses the loot-respawn tick.
            container.bTouched = true;
            container.bPlayerStorage = true;

            _containers[entityId] = container;
        }

        // Keep the container positioned on the NPC. XUiC_LootWindow auto-closes the loot
        // window when the player is farther than (cCollectItemDistance + 30) from
        // te.ToWorldCenterPos(). Created with only a chunk and localChunkPos=(0,0,0), the
        // container reported the chunk's bedrock corner (Y=0) ~60m below the player, so the
        // window slammed shut the instant it opened. Refresh every call since the NPC moves.
        PositionAtEntity(container, entityId);
        return container;
    }

    /// <summary>
    /// Point the container's tile-entity position at the entity's current block position so
    /// the loot window's distance auto-close measures against the NPC, not the chunk origin.
    /// Public so callers that build their own SCoreLootContainer (e.g.
    /// NetPackageHarvestInventoryData on a dedicated-server client) can apply the same fix.
    /// </summary>
    public static void PositionAtEntity(SCoreLootContainer container, int entityId)
    {
        var world = GameManager.Instance?.World;
        var entity = world?.GetEntity(entityId);
        if (entity == null) return;
        int bx = (int)entity.position.x, by = (int)entity.position.y, bz = (int)entity.position.z;
        if (world.GetChunkSync(World.toChunkXZ(bx), World.toChunkXZ(bz)) is Chunk chunk)
            container.chunk = chunk;
        container.localChunkPos = new Vector3i(bx - World.toChunkXZ(bx) * 16, by, bz - World.toChunkXZ(bz) * 16);
    }

    /// <summary>
    /// Adds an item to the entity's harvest container.
    /// Writes directly to items[] without calling SetModified, so no network packet is sent.
    /// Stacks with existing entries where possible.
    /// </summary>
    /// <returns>
    /// True if the item fit. False if the container was full; <paramref name="itemStack"/>'s
    /// count is then reduced to what did not fit, so a caller that drops it drops only the rest.
    /// </returns>
    public static bool AddItem(int entityId, ItemStack itemStack)
    {
        var leftover = AddToItems(GetOrCreate(entityId).items, itemStack);
        if (leftover <= 0) return true;

        itemStack.count = leftover;
        return false; // container full
    }

    /// <summary>Stacks first, then fills empty slots. Returns the count that did not fit.</summary>
    private static int AddToItems(ItemStack[] items, ItemStack itemStack)
    {
        var remaining = itemStack.count;

        // Pass 1 — try to stack with an existing slot
        var cls = ItemClass.GetForId(itemStack.itemValue.type);
        if (cls != null && cls.CanStack())
        {
            for (int i = 0; i < items.Length && remaining > 0; i++)
            {
                if (items[i].IsEmpty() || items[i].itemValue.type != itemStack.itemValue.type) continue;

                int space = cls.Stacknumber.Value - items[i].count;
                if (space <= 0) continue;

                int toAdd = Math.Min(remaining, space);
                items[i].count += toAdd;
                remaining -= toAdd;
            }
        }

        // Pass 2 — empty slots, a full stack at most per slot
        var maxStack = cls != null && cls.CanStack() ? Math.Max(1, cls.Stacknumber.Value) : 1;
        for (int i = 0; i < items.Length && remaining > 0; i++)
        {
            if (!items[i].IsEmpty()) continue;
            int toAdd = Math.Min(remaining, maxStack);
            SetSlot(items, i, new ItemStack(itemStack.itemValue, toAdd));
            remaining -= toAdd;
        }

        return remaining;
    }

    /// <summary>
    /// Writes a slot in place. Since 3.3 each slot is an ItemStack bound to its grid; replacing
    /// the array entry with a new ItemStack breaks that binding and skips change notifications.
    /// </summary>
    public static void SetSlot(ItemStack[] items, int index, ItemStack stack)
    {
        if (stack == null || stack.IsEmpty())
            items[index].Set(ItemValue.None.Clone(), 0);
        else
            items[index].Set(stack.itemValue.Clone(), stack.count);
    }

    /// <summary>
    /// Removes the harvest container for an entity.
    /// Call this when the NPC is picked up (EntitySyncUtils.Collect) or dies.
    /// </summary>
    public static void Remove(int entityId)
    {
        _containers.Remove(entityId);
        _remoteSessions.Remove(entityId);
    }

    /// <summary>Returns true if a non-empty harvest container exists for the entity.</summary>
    public static bool Has(int entityId) => _containers.ContainsKey(entityId);

    // -------------------------------------------------------------------------
    // Server-side remote sessions (dedicated-server clients)
    // -------------------------------------------------------------------------
    // A remote client edits a copy of the container and sends it back on close. The player's
    // takes are already in their own inventory by then and cannot be undone, so the server must
    // never let two copies be out at once, and must apply only what the player changed: farming
    // adds crops and NPC reloads use ammo while the window is open.

    private class RemoteSession
    {
        public int PlayerId;
        public ItemStack[] Snapshot;
    }

    // Keyed by NPC entity ID.
    private static readonly Dictionary<int, RemoteSession> _remoteSessions =
        new Dictionary<int, RemoteSession>();

    /// <summary>
    /// True when someone other than <paramref name="playerId"/> has the container open: a remote
    /// player with a session, or the host in its own loot window.
    /// </summary>
    public static bool IsOpenByOther(int entityId, int playerId)
    {
        if (_containers.TryGetValue(entityId, out var container) && container.IsUserAccessing())
            return true;

        if (!_remoteSessions.TryGetValue(entityId, out var session) || session.PlayerId == playerId)
            return false;

        // A session whose player has left the world cannot be closed by them any more.
        if (GameManager.Instance.World?.GetEntity(session.PlayerId) is EntityPlayer) return true;

        _remoteSessions.Remove(entityId);
        return false;
    }

    /// <summary>True when a remote player has the container open. Used to keep the host out.</summary>
    public static bool HasRemoteSession(int entityId)
    {
        if (!_remoteSessions.TryGetValue(entityId, out var session)) return false;
        if (GameManager.Instance.World?.GetEntity(session.PlayerId) is EntityPlayer) return true;

        _remoteSessions.Remove(entityId);
        return false;
    }

    /// <summary>
    /// Records that <paramref name="playerId"/> was sent the container's current contents.
    /// The caller checks <see cref="IsOpenByOther"/> first.
    /// </summary>
    public static void BeginRemoteSession(int entityId, int playerId)
    {
        var items = GetOrCreate(entityId).items;
        var snapshot = new ItemStack[items.Length];
        for (var i = 0; i < items.Length; i++)
            snapshot[i] = items[i].Clone();

        _remoteSessions[entityId] = new RemoteSession { PlayerId = playerId, Snapshot = snapshot };
    }

    /// <summary>
    /// Applies a remote player's closed window to the container and ends their session.
    /// If nothing changed on the server since the snapshot, the player's copy is taken as-is,
    /// layout included. Otherwise only the player's change (per item, by count) is applied on
    /// top of the server's current contents.
    /// </summary>
    public static void EndRemoteSession(int entityId, int playerId, ItemStack[] clientItems)
    {
        if (!_remoteSessions.TryGetValue(entityId, out var session) || session.PlayerId != playerId)
        {
            Log.Warning($"[HarvestManager] Ignoring a harvest window update for NPC {entityId} from player {playerId}: they have no open session.");
            return;
        }

        _remoteSessions.Remove(entityId);

        // The NPC was picked up or died while the window was open.
        if (!_containers.TryGetValue(entityId, out var container)) return;

        var items = container.items;
        if (SameContents(items, session.Snapshot))
        {
            for (var i = 0; i < items.Length; i++)
                SetSlot(items, i, i < clientItems.Length ? clientItems[i] : null);
            return;
        }

        // Server changed the container while the window was open. Apply the player's change only.
        var delta = new Dictionary<string, int>();
        var values = new Dictionary<string, ItemValue>();
        Tally(session.Snapshot, delta, values, -1);
        Tally(clientItems, delta, values, +1);

        foreach (var pair in delta)
        {
            if (pair.Value < 0)
            {
                var shortfall = RemoveFromItems(items, pair.Key, -pair.Value);
                if (shortfall > 0)
                    Log.Warning($"[HarvestManager] NPC {entityId}: player {playerId} took {-pair.Value}x {values[pair.Key].ItemClass?.GetItemName()}, but the server had already used {shortfall} of them.");
            }
            else if (pair.Value > 0)
            {
                var leftover = AddToItems(items, new ItemStack(values[pair.Key], pair.Value));
                if (leftover > 0)
                    DropNearEntity(entityId, new ItemStack(values[pair.Key].Clone(), leftover));
            }
        }
    }

    /// <summary>Ends every session a player holds. Their window can no longer report back.</summary>
    public static void EndSessionsForPlayer(int playerId)
    {
        var ended = new List<int>();
        foreach (var pair in _remoteSessions)
            if (pair.Value.PlayerId == playerId) ended.Add(pair.Key);

        foreach (var entityId in ended)
        {
            _remoteSessions.Remove(entityId);
            Log.Warning($"[HarvestManager] Player {playerId} left with NPC {entityId}'s harvest window open; its contents stay as the server last had them.");
        }
    }

    public static void OnPlayerDisconnected(ref ModEvents.SPlayerDisconnectedData data)
    {
        if (data.ClientInfo == null) return;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        EndSessionsForPlayer(data.ClientInfo.entityId);
    }

    private static bool SameContents(ItemStack[] a, ItemStack[] b)
    {
        var length = Math.Max(a.Length, b.Length);
        for (var i = 0; i < length; i++)
        {
            var sa = i < a.Length ? a[i] : null;
            var sb = i < b.Length ? b[i] : null;
            var emptyA = sa == null || sa.IsEmpty();
            var emptyB = sb == null || sb.IsEmpty();
            if (emptyA && emptyB) continue;
            if (emptyA != emptyB || sa.count != sb.count) return false;
            if (EntitySyncUtils.ItemKey(sa.itemValue) != EntitySyncUtils.ItemKey(sb.itemValue)) return false;
        }

        return true;
    }

    private static void Tally(ItemStack[] stacks, Dictionary<string, int> delta, Dictionary<string, ItemValue> values, int sign)
    {
        foreach (var stack in stacks)
        {
            if (stack == null || stack.IsEmpty()) continue;
            var key = EntitySyncUtils.ItemKey(stack.itemValue);
            delta.TryGetValue(key, out var count);
            delta[key] = count + sign * stack.count;
            if (!values.ContainsKey(key)) values[key] = stack.itemValue.Clone();
        }
    }

    /// <summary>Removes up to <paramref name="count"/> matching items. Returns how many were missing.</summary>
    private static int RemoveFromItems(ItemStack[] items, string key, int count)
    {
        for (var i = 0; i < items.Length && count > 0; i++)
        {
            if (items[i].IsEmpty() || EntitySyncUtils.ItemKey(items[i].itemValue) != key) continue;

            var take = Math.Min(count, items[i].count);
            count -= take;
            if (take >= items[i].count)
                SetSlot(items, i, null);
            else
                items[i].count -= take;
        }

        return count;
    }

    private static void DropNearEntity(int entityId, ItemStack stack)
    {
        var entity = GameManager.Instance.World?.GetEntity(entityId);
        if (entity == null) return;
        GameManager.Instance.ItemDropServer(stack, entity.position, Vector3.zero);
    }

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
                    SetSlot(container.items, j, items[j]);
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
