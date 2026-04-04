using System;

/// <summary>
/// Network sync package for EntityAliveSDXV4.
/// Serialises inventory, current weapon, and entity name from the server to all clients.
/// On the server, replicated flags are forwarded to all connected clients except the sender.
/// </summary>
public class NetPackageEntityAliveSDXV4DataSync : NetPackage
{
    // -------------------------------------------------------------------------
    // Setup (server → clients)
    // -------------------------------------------------------------------------

    public NetPackageEntityAliveSDXV4DataSync Setup(EntityAliveSDXV4 entity, int _senderId, ushort _syncFlags)
    {
        senderId  = _senderId;
        entityId  = entity.entityId;
        syncFlags = _syncFlags;

        using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
        {
            pooledBinaryWriter.SetBaseStream(entityData);
            entity.WriteSyncData(pooledBinaryWriter, _syncFlags);
        }

        return this;
    }

    // -------------------------------------------------------------------------
    // Destructor — returns pooled stream
    // -------------------------------------------------------------------------

    ~NetPackageEntityAliveSDXV4DataSync()
    {
        MemoryPools.poolMemoryStream.FreeSync(entityData);
    }

    // -------------------------------------------------------------------------
    // Serialisation
    // -------------------------------------------------------------------------

    public override void read(PooledBinaryReader _br)
    {
        senderId  = _br.ReadInt32();
        entityId  = _br.ReadInt32();
        syncFlags = _br.ReadUInt16();
        int length = (int)_br.ReadUInt16();
        StreamUtils.StreamCopy(_br.BaseStream, entityData, length, null, true);
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(senderId);
        _bw.Write(entityId);
        _bw.Write(syncFlags);
        _bw.Write((ushort)entityData.Length);
        entityData.WriteTo(_bw.BaseStream);
    }

    // -------------------------------------------------------------------------
    // Processing
    // -------------------------------------------------------------------------

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null) return;

        var entity = _world.GetEntity(entityId) as EntityAliveSDXV4;
        if (entity == null) return;

        if (entityData.Length > 0L)
        {
            PooledExpandableMemoryStream stream = entityData;
            lock (stream)
            {
                entityData.Position = 0L;
                try
                {
                    using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                    {
                        pooledBinaryReader.SetBaseStream(entityData);
                        entity.ReadSyncData(pooledBinaryReader, syncFlags, senderId);
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        // Server forwards to all other clients.
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            ushort replicatedFlags = entity.GetSyncFlagsReplicated(syncFlags);
            if (replicatedFlags != 0)
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageEntityAliveSDXV4DataSync>()
                        .Setup(entity, senderId, replicatedFlags),
                    false, -1, senderId);
            }
        }
    }

    public override int GetLength()
    {
        return (int)(12L + entityData.Length);
    }

    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private int    senderId;
    private int    entityId;
    private ushort syncFlags;
    private readonly PooledExpandableMemoryStream entityData = MemoryPools.poolMemoryStream.AllocSync(true);
}
