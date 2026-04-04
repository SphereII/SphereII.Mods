using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class PortalManager
{
    private const string SaveFile = "Portals.dat";
    private const byte CurrentVersion = 2;

    private ThreadManager.ThreadInfo dataSaveThreadInfo;

    // Primary name registry: position -> sign text (e.g. "source=A,destination=B")
    private Dictionary<Vector3i, string> PortalMap = new Dictionary<Vector3i, string>();

    // Resolved direct links: source position -> destination position.
    // Populated eagerly whenever both portals are known. Allows GetDestination to work
    // without the destination chunk being loaded, and survives game restarts.
    private Dictionary<Vector3i, Vector3i> DestinationMap = new Dictionary<Vector3i, Vector3i>();

    private static PortalManager instance = null;

    // Lazy property — avoids capturing a null singleton at construction time
    private static ConnectionManager ConnectionMgr => SingletonMonoBehaviour<ConnectionManager>.Instance;

    public static PortalManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PortalManager();
                instance.Init();
            }
            return instance;
        }
    }

    public void Init()
    {
        // Register handlers unconditionally — ConnectionManager may not be ready yet
        // when Init() is called early from Block.Init() during block registration.
        // IsServer checks are deferred into each handler.
        Log.Out("Portal Manager: registering handlers.");
        ModEvents.GameStartDone.RegisterHandler(Load);
        ModEvents.PlayerSpawnedInWorld.RegisterHandler(SyncFullListToClient);
    }

    private void SyncFullListToClient(ref ModEvents.SPlayerSpawnedInWorldData data)
    {
        if (!ConnectionMgr.IsServer) return;
        if (data.ClientInfo == null) return;

        Log.Out($"Portal Manager: Syncing {PortalMap.Count} portals to player {data.ClientInfo.entityId}");
        data.ClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePortalMapSync>().Setup(PortalMap));
    }

    // -----------------------------------------------------------------------
    // Map management
    // -----------------------------------------------------------------------

    /// <summary>
    /// Replaces the local PortalMap from a server sync packet, then re-resolves all links.
    /// Only called on clients.
    /// </summary>
    public void ReplaceMap(Dictionary<Vector3i, string> newMap)
    {
        if (ConnectionMgr.IsServer) return;
        PortalMap.Clear();
        foreach (var entry in newMap)
            PortalMap.Add(entry.Key, entry.Value);
        RebuildDestinationMap();
    }

    /// <summary>
    /// Directly adds or updates a name entry without any network routing.
    /// Called from NetPackage handlers to avoid the client->server->client loop.
    /// Also attempts to resolve the link immediately.
    /// </summary>
    public void AddEntry(Vector3i position, string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (!PortalMap.TryAdd(position, name))
            PortalMap[position] = name;
        TryResolveLink(position);
    }

    /// <summary>
    /// Directly removes a name entry and any resolved link for it.
    /// Called from NetPackage handlers to avoid the client->server->client loop.
    /// </summary>
    public void RemoveEntry(Vector3i position)
    {
        PortalMap.Remove(position);

        // Clear both directions of the resolved link
        if (DestinationMap.TryGetValue(position, out var dest))
        {
            DestinationMap.Remove(position);
            // Only clear the reverse if it still points back at us
            if (DestinationMap.TryGetValue(dest, out var reverse) && reverse == position)
                DestinationMap.Remove(dest);
        }
    }

    public void AddPosition(Vector3i position, string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        var parentPos = GetParentPosition(position);

        if (!ConnectionMgr.IsServer)
        {
            // Client: forward to server; server will broadcast back to all clients
            ConnectionMgr.SendToServer(
                NetPackageManager.GetPackage<NetPackagePortalAddPosition>().Setup(parentPos, name));
            return;
        }

        // Server: update local map, broadcast to all clients, persist
        AddEntry(parentPos, name);
        ConnectionMgr.SendPackage(
            NetPackageManager.GetPackage<NetPackagePortalAddPosition>().Setup(parentPos, name));
        Save();
    }

    public void RemovePosition(Vector3i position)
    {
        position = GetParentPosition(position);

        if (!ConnectionMgr.IsServer)
        {
            ConnectionMgr.SendToServer(
                NetPackageManager.GetPackage<NetPackagePortalRemovePosition>().Setup(position));
            return;
        }

        RemoveEntry(position);
        ConnectionMgr.SendPackage(
            NetPackageManager.GetPackage<NetPackagePortalRemovePosition>().Setup(position));
        Save();
    }

    // -----------------------------------------------------------------------
    // Link resolution
    // -----------------------------------------------------------------------

    /// <summary>
    /// Attempts to resolve a direct pos->pos link for the given position by scanning
    /// PortalMap for a matching destination name. Caches the result in DestinationMap.
    /// </summary>
    private void TryResolveLink(Vector3i position)
    {
        if (!PortalMap.TryGetValue(position, out var name)) return;
        var item = new PortalItem(position, name);
        if (item.Destination == "NA") return;

        foreach (var portal in PortalMap)
        {
            if (portal.Key == position) continue;
            var portalItem = new PortalItem(portal.Key, portal.Value);
            if (item.Destination != portalItem.Source) continue;

            // Resolved — cache both directions
            DestinationMap[position] = portal.Key;
            DestinationMap[portal.Key] = position;
            return;
        }
    }

    /// <summary>
    /// Rebuilds the entire DestinationMap from the current PortalMap.
    /// Called after Load or ReplaceMap so all links are immediately available.
    /// </summary>
    private void RebuildDestinationMap()
    {
        DestinationMap.Clear();
        foreach (var pos in PortalMap.Keys)
            TryResolveLink(pos);
        Log.Out($"Portal Manager: rebuilt {DestinationMap.Count / 2} link(s) from {PortalMap.Count} portal(s).");
    }

    // -----------------------------------------------------------------------
    // Queries
    // -----------------------------------------------------------------------

    public void Display()
    {
        Log.Out("Portal Mapping:");
        foreach (var temp in PortalMap)
            Log.Out($"  {temp.Key} -> name: '{temp.Value}'  dest: {(DestinationMap.TryGetValue(temp.Key, out var d) ? d.ToString() : "unresolved")}");
    }

    public int CountLocations(string source)
    {
        var counter = 0;
        foreach (var temp in PortalMap)
        {
            if (new PortalItem(temp.Key, temp.Value).Source == source)
                counter++;
        }
        return counter;
    }

    public Vector3i GetParentPosition(Vector3i position)
    {
        var blockValue = GameManager.Instance.World.GetBlock(position);
        if (blockValue.ischild)
            return blockValue.Block.multiBlockPos.GetParentPos(position, blockValue);
        return position;
    }

    public bool AddPosition(Vector3i position)
    {
        var tileEntity = GameManager.Instance.World.GetTileEntity(0, position) as TileEntityPoweredPortal;
        var text = tileEntity?.signText.Text ?? string.Empty;

        if (string.IsNullOrEmpty(text))
        {
            var sign = GameManager.Instance.World.GetTileEntity(0, position) as TileEntitySign;
            if (sign != null) text = sign.signText.Text;
        }

        position = GetParentPosition(position);
        if (string.IsNullOrEmpty(text)) return false;
        Instance.AddPosition(position, text);
        return true;
    }

    public bool IsLinked(Vector3i source)
    {
        source = GetParentPosition(source);
        var destination = GetDestination(source);
        if (destination == Vector3i.zero) return false;

        // If the destination chunk is loaded, verify the block is still a portal.
        // If the chunk isn't loaded, trust the PortalMap entry.
        var world = GameManager.Instance.World;
        if (world.GetChunkFromWorldPos(destination) != null)
        {
            var destBlock = world.GetBlock(destination);
            bool isPortal = destBlock.Block is BlockPoweredPortal || destBlock.Block is BlockPortal2;
            if (!isPortal)
            {
                // Block was removed — clean up the stale link
                RemoveEntry(destination);
                return false;
            }
        }

        if (!GameManager.IsDedicatedServer)
        {
            (world.GetBlock(source).Block as BlockPoweredPortal)?.ToggleAnimator(source, true);
            if (world.GetChunkFromWorldPos(destination) != null)
                (world.GetBlock(destination).Block as BlockPoweredPortal)?.ToggleAnimator(destination, true);
        }

        return true;
    }

    public string GetDestinationName(Vector3i source)
    {
        source = GetParentPosition(source);
        var destination = GetDestination(source);
        if (destination == Vector3i.invalid) return Localization.Get("portal_not_configured");

        if (PortalMap.TryGetValue(destination, out string destinationName))
            return new PortalItem(destination, destinationName).Destination;

        return Localization.Get("portal_not_configured");
    }

    /// <summary>
    /// Returns the destination position for a given source portal.
    /// Uses the pre-resolved DestinationMap first (works even if destination chunk is unloaded),
    /// then falls back to name-based scan of PortalMap.
    /// </summary>
    public Vector3i GetDestination(Vector3i source)
    {
        source = GetParentPosition(source);

        // Fast path: use pre-resolved direct link
        if (DestinationMap.TryGetValue(source, out var cached) && cached != Vector3i.zero)
        {
            // Verify destination is still registered (not removed while we were offline)
            if (PortalMap.ContainsKey(cached))
                return cached;

            // Stale link — destination portal was removed; clean up and fall through
            DestinationMap.Remove(source);
        }

        // Slow path: name-based scan
        if (!PortalMap.TryGetValue(source, out var sourceName)) return Vector3i.zero;
        var item = new PortalItem(source, sourceName);
        if (item.Destination == "NA") return Vector3i.zero;

        foreach (var portal in PortalMap)
        {
            var portalItem = new PortalItem(portal.Key, portal.Value);
            if (item.Destination == portalItem.Source)
            {
                if (source == portal.Key) continue;
                // Cache the resolved link for future lookups
                DestinationMap[source] = portal.Key;
                DestinationMap[portal.Key] = source;
                return portal.Key;
            }
        }

        return CheckForPrefabLocation(item);
    }

    public Vector3i CheckForPrefabLocation(PortalItem item)
    {
        var prefabInstance = item.GetPrefabInstance();
        if (prefabInstance == null) return Vector3i.zero;

        var size = prefabInstance.prefab.size;
        var first = prefabInstance.boundingBoxPosition;
        for (int i = 0; i < size.z; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                for (int k = 0; k < size.y; k++)
                {
                    BlockValue blockValue = prefabInstance.prefab.GetBlock(j, k, i);
                    if (blockValue.isair) continue;
                    if (blockValue.Block.HasTileEntity && blockValue.Block.Properties.Values.ContainsKey("Location"))
                    {
                        var location = blockValue.Block.Properties.Values["Location"];
                        var vector3i = new Vector3i(
                            j + prefabInstance.boundingBoxPosition.x,
                            k + prefabInstance.boundingBoxPosition.y,
                            i + prefabInstance.boundingBoxPosition.z);
                        AddPosition(vector3i, location);
                        if (first == prefabInstance.boundingBoxPosition)
                            first = vector3i;
                    }
                }
            }
        }
        return first;
    }

    // Dialog / Buff teleportation.
    public Vector3i GetDestination(string location)
    {
        var destinationItem = new PortalItem(Vector3i.zero, location);
        var legacy = destinationItem.Source == destinationItem.Destination;

        var destination = Vector3i.zero;
        foreach (var portal in PortalMap)
        {
            var portalItem = new PortalItem(portal.Key, portal.Value);
            if (location == portalItem.Destination || destinationItem.Destination == portalItem.Destination)
            {
                destination = portal.Key;
                break;
            }
            if (legacy && location == portalItem.Source)
            {
                destination = portal.Key;
                break;
            }
        }

        if (destination == Vector3i.zero)
            return CheckForPrefabLocation(destinationItem);

        var tileEntity = GameManager.Instance.World.GetTileEntity(0, destination) as TileEntityPoweredPortal;
        if (tileEntity == null) return Vector3i.zero;
        if (tileEntity.RequiredPower <= 0) return destination;
        if (tileEntity.IsPowered) return destination;
        return Vector3i.zero;
    }

    // -----------------------------------------------------------------------
    // Persistence
    // -----------------------------------------------------------------------

    public void Write(BinaryWriter _bw)
    {
        _bw.Write(CurrentVersion);

        // PortalMap: position -> name string
        _bw.Write(PortalMap.Count);
        foreach (var entry in PortalMap)
        {
            StreamUtils.Write(_bw, entry.Key);
            _bw.Write(entry.Value);
        }

        // DestinationMap: source position -> destination position (resolved links)
        _bw.Write(DestinationMap.Count);
        foreach (var entry in DestinationMap)
        {
            StreamUtils.Write(_bw, entry.Key);
            StreamUtils.Write(_bw, entry.Value);
        }
    }

    public void Read(BinaryReader _br)
    {
        byte version = _br.ReadByte();

        if (version == 1)
        {
            // Legacy string format: "x, y, z:name;x, y, z:name;..."
            var positions = _br.ReadString();
            if (!string.IsNullOrEmpty(positions))
            {
                foreach (var entry in positions.Split(';'))
                {
                    if (string.IsNullOrEmpty(entry)) continue;
                    var sep = entry.IndexOf(':');
                    if (sep < 0) continue;
                    var name = entry.Substring(sep + 1);
                    if (string.IsNullOrEmpty(name)) continue;
                    var pos = StringParsers.ParseVector3i(entry.Substring(0, sep));
                    if (!PortalMap.ContainsKey(pos))
                        PortalMap.Add(pos, name);
                }
            }
            // DestinationMap rebuilt below (after return falls through to RebuildDestinationMap in Load)
            return;
        }

        // Version 2: binary format
        int portalCount = _br.ReadInt32();
        for (int i = 0; i < portalCount; i++)
        {
            var pos = StreamUtils.ReadVector3i(_br);
            var name = _br.ReadString();
            PortalMap[pos] = name;
        }

        int destCount = _br.ReadInt32();
        for (int i = 0; i < destCount; i++)
        {
            var src = StreamUtils.ReadVector3i(_br);
            var dst = StreamUtils.ReadVector3i(_br);
            DestinationMap[src] = dst;
        }
    }

    private int saveDataThreaded(ThreadManager.ThreadInfo _threadInfo)
    {
        var pooledStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
        string path = $"{GameIO.GetSaveGameDir()}/{SaveFile}";
        if (!Directory.Exists(GameIO.GetSaveGameDir())) return -1;

        if (File.Exists(path))
            File.Copy(path, $"{GameIO.GetSaveGameDir()}/{SaveFile}.bak", true);

        pooledStream.Position = 0L;
        StreamUtils.WriteStreamToFile(pooledStream, path);
        MemoryPools.poolMemoryStream.FreeSync(pooledStream);
        return -1;
    }

    public void Save()
    {
        if (dataSaveThreadInfo != null && ThreadManager.ActiveThreads.ContainsKey("silent_PortalDataSave"))
            return;

        var pooledStream = MemoryPools.poolMemoryStream.AllocSync(true);
        using (var writer = MemoryPools.poolBinaryWriter.AllocSync(false))
        {
            writer.SetBaseStream(pooledStream);
            Write(writer);
        }
        dataSaveThreadInfo = ThreadManager.StartThread("silent_PortalDataSave", null,
            saveDataThreaded, null, pooledStream, null, false);
    }

    public void Load(ref ModEvents.SGameStartDoneData data)
    {
        if (!ConnectionMgr.IsServer) return;
        Log.Out("Portal Manager: loading portal data...");
        PortalMap.Clear();
        DestinationMap.Clear();

        string path = $"{GameIO.GetSaveGameDir()}/{SaveFile}";
        if (!Directory.Exists(GameIO.GetSaveGameDir()) || !File.Exists(path))
        {
            Log.Out("Portal Manager: no save file found, starting fresh.");
            return;
        }

        bool loaded = TryLoadFile(path);
        if (!loaded)
        {
            string backup = $"{GameIO.GetSaveGameDir()}/{SaveFile}.bak";
            Log.Warning($"Portal Manager: primary file failed, trying backup...");
            TryLoadFile(backup);
        }

        // Always rebuild DestinationMap after loading — ensures links are resolved
        // even if loaded from a v1 save (which has no DestinationMap) or if the
        // DestinationMap in a v2 save is incomplete.
        RebuildDestinationMap();

        Log.Out($"Portal Manager: loaded {PortalMap.Count} portal(s), {DestinationMap.Count / 2} link(s).");
    }

    private bool TryLoadFile(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            using var fileStream = File.OpenRead(path);
            using var reader = MemoryPools.poolBinaryReader.AllocSync(false);
            reader.SetBaseStream(fileStream);
            Read(reader);
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Portal Manager: failed to load '{path}': {e.Message}");
            return false;
        }
    }
}
