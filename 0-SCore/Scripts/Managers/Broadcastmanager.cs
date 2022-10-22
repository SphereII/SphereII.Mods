using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections.Concurrent;

// Stripped down Firemanager
public class Broadcastmanager
{
    private static readonly string AdvFeatureClass = "AdvancedRecipes";

    private static Broadcastmanager instance = null;
    private static ConcurrentDictionary<Vector3i, BlockValue> Broadcastmap = new ConcurrentDictionary<Vector3i, BlockValue>();

    private const string saveFile = "Broadcastmanager.dat";
    private ThreadManager.ThreadInfo dataSaveThreadInfo;

    public static bool HasInstance => instance != null;

    //public bool Enabled { private set; get; }
    public static Broadcastmanager Instance
    {
        get
        {
            return instance;
        }
    }
    public static void Init()
    {

        var option = Configuration.GetPropertyValue(AdvFeatureClass, "BroadcastManage");
        if (!StringParsers.ParseBool(option))
        {
            Log.Out("Broadcast Manager is disabled.");
            //Broadcastmanager.Instance.Enabled = false;
            return;
        }
        //Broadcastmanager.Instance.Enabled = true;
        Broadcastmanager.instance = new Broadcastmanager();
        Log.Out("Starting Broadcast Manager");

        // Read the Broadcastmanager
        Broadcastmanager.Instance.Load();
    }

    // Save the lootcontainer location.
    public void Write(BinaryWriter _bw)
    {
        _bw.Write("V1");
        foreach (var temp in Broadcastmap)
            _bw.Write(temp.Key.ToString());
    }

    // Read lootcontainer location.
    public void Read(BinaryReader _br)
    {
        var version = _br.ReadString();
        if (version == "V1")
        {
            while (_br.BaseStream.Position != _br.BaseStream.Length)
            {
                string container = _br.ReadString();
                add(StringParsers.ParseVector3i(container));
            }
        }
        else 
        {
            foreach (var position in version.Split(';'))
                while (_br.BaseStream.Position != _br.BaseStream.Length)
                {
                    if (string.IsNullOrEmpty(position)) continue;
                    var vector = StringParsers.ParseVector3i(position);
                    add(vector);
                    add(StringParsers.ParseVector3i(_br.ReadString()));
                }
        }
    }
    // check if lootcontainer exists in dictionary
    public bool Check(Vector3i _blockPos)
    {
        return Broadcastmap.TryGetValue(_blockPos, out _);
    }

    public void Add(Vector3i _blockPos, int entityID = -1)
    {
        if (!GameManager.IsDedicatedServer)
            add(_blockPos);

        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageAddBroadcastPosition>().Setup(_blockPos, entityID), false);
            return;
        }
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAddBroadcastPosition>().Setup(_blockPos, entityID), false, -1, -1, -1, -1);
    }
    public void Remove(Vector3i _blockPos, int entityID = -1)
    {
        if (!GameManager.IsDedicatedServer)
            remove(_blockPos);
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRemoveBroadcastPosition>().Setup(_blockPos, entityID), false);
            return;
        }
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageRemoveBroadcastPosition>().Setup(_blockPos, entityID), false, -1, -1, -1, -1);
    }

    // Remove lootcontainer from dictionary
    public void remove(Vector3i _blockPos)
    {
        if (!Broadcastmap.ContainsKey(_blockPos)) return;
        Broadcastmap.TryRemove(_blockPos, out var block);
    }
    // Add lootcontainer to dictionary
    public void add(Vector3i _blockPos)
    {
        var block = GameManager.Instance.World.GetBlock(_blockPos);
        Broadcastmap.TryAdd(_blockPos, block);
    }

    private int saveDataThreaded(ThreadManager.ThreadInfo _threadInfo)
    {
        PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
        string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
        if (!Directory.Exists(GameIO.GetSaveGameDir()))
        {
            Directory.CreateDirectory(GameIO.GetSaveGameDir());
        }
        if (File.Exists(text))
        {
            File.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak"), true);
        }
        pooledExpandableMemoryStream.Position = 0L;
        StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
        MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
        Log.Out($"Broadcast Manager {text} Saving: {Broadcastmap.Count}");

        return -1;
    }

    public void Save()
    {
        if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_BroadcastDataSave"))
        {
            PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
            using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
            {
                pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
                this.Write(pooledBinaryWriter);
            }


            this.dataSaveThreadInfo = ThreadManager.StartThread("silent_BroadcastDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.saveDataThreaded), null, System.Threading.ThreadPriority.Normal, pooledExpandableMemoryStream, null, false);
        }
    }

    public void Load()
    {
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

            Log.Out($"Broadcast Manager {path} Loaded: {Broadcastmap.Count}");
        }
    }

    private void WaitOnSave()
    {
        if (this.dataSaveThreadInfo != null)
        {
            this.dataSaveThreadInfo.WaitForEnd();
            this.dataSaveThreadInfo = null;
        }
    }

    public static void Cleanup()
    {
        if (instance != null)
        {
            instance.SaveAndClear();
        }
    }

    private void SaveAndClear()
    {
        WaitOnSave();
        Save();
        WaitOnSave();
        Broadcastmap.Clear();
        instance = null;
        Log.Out("Broadcastmanager stopped");
    }
}
