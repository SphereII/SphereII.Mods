using System;

public class NetPackageEntityAliveSDXDataSync : NetPackage
{
    public NetPackageEntityAliveSDXDataSync Setup(EntityAliveSDX _entityAliveSdx, int _senderId, ushort _syncFlags)
    {
        senderId = _senderId;
        vehicleId = _entityAliveSdx.entityId;
        syncFlags = _syncFlags;
        using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
        {
            pooledBinaryWriter.SetBaseStream(this.entityData);
            _entityAliveSdx.WriteSyncData(pooledBinaryWriter, _syncFlags);
        }

        return this;
    }

    ~NetPackageEntityAliveSDXDataSync()
    {
        MemoryPools.poolMemoryStream.FreeSync(this.entityData);
    }

    public override void read(PooledBinaryReader _br)
    {
        this.senderId = _br.ReadInt32();
        this.vehicleId = _br.ReadInt32();
        this.syncFlags = _br.ReadUInt16();
        int length = (int) _br.ReadUInt16();
        StreamUtils.StreamCopy(_br.BaseStream, this.entityData, length, null, true);
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(this.senderId);
        _bw.Write(this.vehicleId);
        _bw.Write(this.syncFlags);
        _bw.Write((ushort) this.entityData.Length);
        this.entityData.WriteTo(_bw.BaseStream);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        var entityAliveSdx = GameManager.Instance.World.GetEntity(vehicleId) as EntityAliveSDX;
        if (entityAliveSdx == null)
        {
            return;
        }

        if (this.entityData.Length > 0L)
        {
            PooledExpandableMemoryStream obj = this.entityData;
            lock (obj)
            {
                this.entityData.Position = 0L;
                try
                {
                    using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                    {
                        pooledBinaryReader.SetBaseStream(this.entityData);
                        entityAliveSdx.ReadSyncData(pooledBinaryReader, this.syncFlags, this.senderId);
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            ushort syncFlagsReplicated = entityAliveSdx.GetSyncFlagsReplicated(this.syncFlags);
            if (syncFlagsReplicated != 0)
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageEntityAliveSDXDataSync>()
                        .Setup(entityAliveSdx, this.senderId, syncFlagsReplicated), false, -1, this.senderId, -1, -1);
            }
        }
    }

    public override int GetLength()
    {
        return (int) (12L + this.entityData.Length);
    }

    private int senderId;

    private int vehicleId;

    private ushort syncFlags;

    private PooledExpandableMemoryStream entityData = MemoryPools.poolMemoryStream.AllocSync(true);
}