using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class PortalManager
{
    private const float SAVE_TIME_SEC = 60f;
    private const string saveFile = "Portals.dat";
    private static byte Version = 1;
    private float saveTime = 60f;
    private ThreadManager.ThreadInfo dataSaveThreadInfo;

    public bool isInitialized = false;
    private Dictionary<Vector3i, string> PortalMap = new Dictionary<Vector3i, string>();
    private static PortalManager instance = null;

    private readonly ConnectionManager _connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;

    public static PortalManager Instance {
        get {
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
        if (!_connectionManager.IsServer) return;
        Log.Out("Starting Portal Manager...");
        ModEvents.GameStartDone.RegisterHandler(Load);
        ModEvents.PlayerSpawnedInWorld.RegisterHandler(SyncFullListToClient);

    }

    private void SyncFullListToClient(ref ModEvents.SPlayerSpawnedInWorldData data)
    {
        if (data.ClientInfo == null) return;

        Debug.Log($"Portal Manager Syncing To Player: {data.ClientInfo.entityId}");
        var package = NetPackageManager.GetPackage<NetPackagePortalMapSync>();
        package.Setup(PortalMap);
        Debug.Log($"Syncing Portal Map To : {data.ClientInfo.entityId} :: Count: {PortalMap.Count}");
        data.ClientInfo.SendPackage(package);
    }

 


    public void ReplaceMap(Dictionary<Vector3i, string> newMap)
    {
        // This method should only be run on a client, not the server.
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        PortalMap.Clear();

        // Copy all entries from the new map into the local map.
        foreach (var entry in newMap)
        {
            PortalMap.Add(entry.Key, entry.Value);
        }
    }


    public void AddPosition(Vector3i position, string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        
        // Convert to parent position immediately
        var parentPos = GetParentPosition(position);

        // This is a new portal placed on a client.
        if (!_connectionManager.IsServer)
        {
            Debug.Log($"Sending Data to Server: {parentPos} {name}");
            // Send the parentPos, not the original position.
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackagePortalAddPosition>().Setup(parentPos, name));
            return;
        }

        Debug.Log($"Adding Data to Portal Map.: {parentPos} {name}");
        // Server or network call, so proceed with adding the position.
        if (!PortalMap.TryAdd(parentPos, name))
        {
            if (PortalMap[parentPos].Equals(name)) return;
            PortalMap[parentPos] = name;
        }

        Debug.Log($"Server sending Data to Clients.: {parentPos} {name}");
        // Send to all other clients
        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackagePortalAddPosition>().Setup(parentPos, name));
        Save();
    }

    // New overload for removing a position
    public void RemovePosition(Vector3i position)
    {
        // Convert to parent position immediately
        position = GetParentPosition(position);
        
        if (!_connectionManager.IsServer )
        {
            // Send the parent position to the server
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackagePortalRemovePosition>().Setup(position));
            return;
        }

        // Server logic uses the parent position
        PortalMap.Remove(position);

        if (_connectionManager.IsServer)
        {
            _connectionManager.SendPackage(
                NetPackageManager.GetPackage<NetPackagePortalRemovePosition>().Setup(position));
            Save();
        }
    }


    public void Display()
    {
        Log.Out("Portal Mapping:");
        foreach (var temp in PortalMap)
        {
            Log.Out($"{temp.Key} : {temp.Value}");
        }
    }

    public int CountLocations(string source)
    {
        var counter = 0;
        foreach (var temp in PortalMap)
        {
            var item = new PortalItem(temp.Key, temp.Value);
            if (item.Source == source)
                counter++;
        }

        return counter;
    }

    // Helper method to consistently get the parent position
    public Vector3i GetParentPosition(Vector3i position)
    {
        var blockValue = GameManager.Instance.World.GetBlock(position);
        if ( blockValue.ischild)
            return blockValue.Block.multiBlockPos.GetParentPos(position, blockValue);
        return position;
    }
    
    public bool AddPosition(Vector3i position)
    {
        // If this sign isn't registered, try to register it now.
        var tileEntity = GameManager.Instance.World.GetTileEntity(0, position) as TileEntityPoweredPortal;
        var text = "";
        if (tileEntity != null)
            text = tileEntity.signText.Text;

        var tileEntitySign = GameManager.Instance.World.GetTileEntity(0, position) as TileEntitySign;
        if (tileEntitySign != null)
            text = tileEntitySign.signText.Text;

        // Convert position to parent position before calling the AddPosition overload
        position = GetParentPosition(position);
        
        Debug.Log($"Portal Manager: Adding Position: {position} :: {text}");
        if (string.IsNullOrEmpty(text)) return false;
        Instance.AddPosition(position, text);
        return true;
    }

    // public void AddPosition(Vector3i position, string name)
    // {
    //     if (string.IsNullOrEmpty(name)) return;
    //
    //     if (CountLocations(name) == 2)
    //     {
    //         // Location already exists.
    //         return;
    //     }
    //
    //     if (PortalMap.ContainsKey(position))
    //     {
    //         // Already in there, so no need to re-add.
    //         if (PortalMap[position].Equals(name)) return;
    //         PortalMap[position] = name;
    //     }
    //     else
    //         PortalMap.Add(position, name);
    //
    //     Save();
    // }

    public bool IsLinked(Vector3i source)
    {
        // Convert source to parent position before lookup
        source = GetParentPosition(source);
        
        var destination = GetDestination(source);
        if (destination == Vector3i.zero) return false;

        var block = GameManager.Instance.World.GetBlock(source).Block as BlockPoweredPortal;
        block?.ToggleAnimator(source, true);

        block = GameManager.Instance.World.GetBlock(destination).Block as BlockPoweredPortal;
        block?.ToggleAnimator(destination, true);

        return true;
    }

    public string GetDestinationName(Vector3i source)
    {
        // Convert source to parent position before lookup
        source = GetParentPosition(source);

        var destination = GetDestination(source);
        if (destination == Vector3i.invalid) return Localization.Get("portal_not_configured");
        
        // Since GetDestination handles the parent lookup, we use the returned destination key directly.
        if (PortalMap.TryGetValue(destination, out string destinationName))
        {
            var item = new PortalItem(destination, destinationName);
            return item.Destination;
        }

        return Localization.Get("portal_not_configured");
    }

    // Portal blocks
    public Vector3i GetDestination(Vector3i source)
    {
        // Convert source to parent position before looking up in the map
        source = GetParentPosition(source);
    
        if (!PortalMap.TryGetValue(source, out var sourceName)) return Vector3i.zero;
        var item = new PortalItem(source, sourceName);
        if (item.Destination == "NA") return Vector3i.zero;

        // Loop around every teleport position, matchng up the name.
        foreach (var portal in PortalMap)
        {
            var portalItem = new PortalItem(portal.Key, portal.Value);
            if (item.Destination == portalItem.Source)
            {
                // don't teleport to the same location.
                if (source == portal.Key) continue;
                return portal.Key;
            }
        }

        return CheckForPrefabLocation(item);
    }

    public Vector3i CheckForPrefabLocation(PortalItem item)
    {
        var prefabInstance = item.GetPrefabInstance();
        if (prefabInstance == null) return Vector3i.zero;

        //        return prefabInstance.boundingBoxPosition;
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
                    if (blockValue.Block.HasTileEntity)
                    {
                        if (blockValue.Block.Properties.Values.ContainsKey("Location"))
                        {
                            var location = blockValue.Block.Properties.Values["Location"];
                            int num2 = i + prefabInstance.boundingBoxPosition.z;
                            int num4 = j + prefabInstance.boundingBoxPosition.x;
                            int num5 = k + prefabInstance.boundingBoxPosition.y;
                            var vector3i = new Vector3i(num4, num5, num2);
                            
                            // NOTE: The AddPosition call here will perform its own parent position check 
                            // if the block at 'vector3i' is a child block.
                            AddPosition(vector3i, location);

                            if (first == prefabInstance.boundingBoxPosition)
                                first = vector3i;
                        }
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

        // If the source and destination is the same, then this means its a legacy teleport location, likely coming from a teleport from minevents where its only one way
        var legacy = destinationItem.Source == destinationItem.Destination;

        var destination = Vector3i.zero;
        foreach (var portal in PortalMap)
        {
            var portalItem = new PortalItem(portal.Key, portal.Value);
            if (location == portalItem.Destination)
            {
                destination = portal.Key;
                break;
            }

            if (destinationItem.Destination == portalItem.Destination)
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

        // If the destination is zero here, check if its a prefab
        if (destination == Vector3i.zero)
        {
            return CheckForPrefabLocation(destinationItem);
        }

        // If the portal needs power, make sure its on.
        var tileEntity = GameManager.Instance.World.GetTileEntity(0, destination) as TileEntityPoweredPortal;
        if (tileEntity == null) return Vector3i.zero; // Dead portal
        if (tileEntity.RequiredPower <= 0) return destination;
        if (tileEntity.IsPowered) return destination;
        return Vector3i.zero;
    }

    // public void RemovePosition(Vector3i position)
    // {
    //     PortalMap.Remove(position);
    //     Save();
    // }

    public void Write(BinaryWriter _bw)
    {
        _bw.Write(PortalManager.Version);
        var writeOut = "";
        foreach (var temp in PortalMap)
            writeOut += $"{temp.Key}:{temp.Value};";

        writeOut = writeOut.TrimEnd(';');
        _bw.Write(writeOut);
    }

    public void Read(BinaryReader _br)
    {
        _br.ReadByte();
        var positions = _br.ReadString();
        foreach (var position in positions.Split(';'))
        {
            PortalMap.Add(StringParsers.ParseVector3i(position.Split(':')[0]), position.Split(':')[1]);
        }
    }

    private int saveDataThreaded(ThreadManager.ThreadInfo _threadInfo)
    {
        PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
        string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
        if (!Directory.Exists(GameIO.GetSaveGameDir()))
        {
            return -1;
        }

        if (File.Exists(text))
        {
            File.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak"), true);
        }

        pooledExpandableMemoryStream.Position = 0L;
        StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
        MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
        return -1;
    }

    public void Save()
    {
        if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_PortalDataSave"))
        {
            PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
            using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
            {
                pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
                this.Write(pooledBinaryWriter);
            }

            this.dataSaveThreadInfo = ThreadManager.StartThread("silent_PortalDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.saveDataThreaded), null, pooledExpandableMemoryStream, null, false);
        }
    }

    public void Load(ref ModEvents.SGameStartDoneData data)
    {
        Log.Out("Reading Portal Data...");
        PortalMap.Clear();
        string path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
        if (Directory.Exists(GameIO.GetSaveGameDir()) && File.Exists(path))
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                    {
                        pooledBinaryReader.SetBaseStream(fileStream);
                        this.Read(pooledBinaryReader);
                    }
                }
            }
            catch (Exception)
            {
                path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak");
                if (File.Exists(path))
                {
                    using (FileStream fileStream2 = File.OpenRead(path))
                    {
                        using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
                        {
                            pooledBinaryReader2.SetBaseStream(fileStream2);
                            this.Read(pooledBinaryReader2);
                        }
                    }
                }
            }
        }
    }
}