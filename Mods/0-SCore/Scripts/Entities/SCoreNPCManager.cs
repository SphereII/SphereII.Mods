using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SCoreNPCManager
{
    private static SCoreNPCManager instance = null;
    private static readonly string SaveFileName = "SCoreNPCData.bin";
    
    // Maps UniqueID -> Binary Data
    private Dictionary<float, byte[]> npcDataCache = new Dictionary<float, byte[]>();

    public static SCoreNPCManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SCoreNPCManager();
                instance.Load();
            }
            return instance;
        }
    }

    public long SaveNPC(EntityAliveSDX entity)
    {
        if (entity == null) return -1;
        long uniqueID = DateTime.Now.Ticks + entity.entityId;

        using (MemoryStream ms = new MemoryStream())
        {
            using (PooledBinaryWriter writer = MemoryPools.poolBinaryWriter.AllocSync(false))
            {
                writer.SetBaseStream(ms);
                // REMOVED: EntityCreationData (Source of corruption)
                
                // Write only the Entity Data
                entity.Write(writer, false);
            }
            
            if (npcDataCache.ContainsKey(uniqueID))
                npcDataCache[uniqueID] = ms.ToArray();
            else
                npcDataCache.Add(uniqueID, ms.ToArray());
        }
        Save();
        return uniqueID;
    }

    public void ApplyDataToEntity(long uniqueID, EntityAliveSDX freshEntity)
    {
        if (!npcDataCache.TryGetValue(uniqueID, out var data))
        {
            Log.Warning($"[0-SCore] SCoreNPCManager: No saved data found for NPC ID: {uniqueID}");
            return;
        }

        int runtimeEntityID = freshEntity.entityId;

        using (MemoryStream ms = new MemoryStream(data))
        {
            using (PooledBinaryReader reader = MemoryPools.poolBinaryReader.AllocSync(false))
            {
                reader.SetBaseStream(ms);
            
                // We removed EntityCreationData usage to simplify alignment.
            
                // CRITICAL FIX: Pass a high version number (60) instead of 0.
                // This ensures Entity.Read and EntityAlive.Read actually consume their bytes.
                // If you pass 0, they skip their reads, and EntityNPC reads garbage data.
                freshEntity.Read(60, reader);
            }
        }

        freshEntity.entityId = runtimeEntityID;

        if (freshEntity.lootContainer != null)
        {
            freshEntity.lootContainer.entityId = runtimeEntityID;
            freshEntity.lootContainer.SetModified();
        }
    }
    public void Save()
    {
        string saveDir = GameIO.GetSaveGameDir();
        string path = Path.Combine(saveDir, SaveFileName);

        try
        {
            using (Stream stream = SdFile.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (PooledBinaryWriter writer = MemoryPools.poolBinaryWriter.AllocSync(false))
                {
                    writer.SetBaseStream(stream);

                    writer.Write(npcDataCache.Count);
                    foreach (var kvp in npcDataCache)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value.Length);
                        writer.Write(kvp.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[0-SCore] SCoreNPCManager Save Error: {ex.Message}");
        }
    }

    public void Load()
    {
        string saveDir = GameIO.GetSaveGameDir();
        string path = Path.Combine(saveDir, SaveFileName);

        if (!SdFile.Exists(path)) return;

        try
        {
            using (Stream stream = SdFile.OpenRead(path))
            {
                using (PooledBinaryReader reader = MemoryPools.poolBinaryReader.AllocSync(false))
                {
                    reader.SetBaseStream(stream);
                    int count = reader.ReadInt32();
                    npcDataCache.Clear();

                    for (int i = 0; i < count; i++)
                    {
                        long id = reader.ReadInt64();
                        int length = reader.ReadInt32();
                        byte[] data = reader.ReadBytes(length);
                        npcDataCache.Add(id, data);
                    }
                }
            }
            Log.Out($"[0-SCore] SCoreNPCManager Loaded {npcDataCache.Count} NPCs.");
        }
        catch (Exception ex)
        {
            Log.Error($"[0-SCore] SCoreNPCManager Load Error: {ex.Message}");
        }
    }
}