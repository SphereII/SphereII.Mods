using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDeployNPCSDX : NetPackage
{
    private int entityClassId;
    private Vector3 pos;
    private Vector3 rot;
    private ItemValue itemValue;
    private int entityThatPlaced;

    public NetPackageDeployNPCSDX Setup(int _entityClassId, Vector3 _pos, Vector3 _rot, ItemValue _itemValue, int _entityThatPlaced)
    {
        this.entityClassId = _entityClassId;
        this.pos = _pos;
        this.rot = _rot;
        this.itemValue = _itemValue;
        this.entityThatPlaced = _entityThatPlaced;
        return this;
    }

    public override void read(PooledBinaryReader _reader)
    {
        this.entityClassId = _reader.ReadInt32();
        this.pos = StreamUtils.ReadVector3(_reader);
        this.rot = StreamUtils.ReadVector3(_reader);
        this.itemValue = new ItemValue();
        this.itemValue.Read(_reader);
        this.entityThatPlaced = _reader.ReadInt32();
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);
        _writer.Write(this.entityClassId);
        StreamUtils.Write(_writer, this.pos);
        StreamUtils.Write(_writer, this.rot);
        this.itemValue.Write(_writer);
        _writer.Write(this.entityThatPlaced);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        // Verify the sender is allowed to spawn things for this player ID
        if (!base.ValidEntityIdForSender(this.entityThatPlaced, false))
        {
            return;
        }

        // SERVER LOGIC
        if (!_world.IsRemote())
        {
            // 1. Create the Entity (In Memory)
            Entity entity = EntityFactory.CreateEntity(this.entityClassId, this.pos, this.rot);
            var entityAlive = entity as EntityAlive;
            var iEntity     = entityAlive as IEntityAliveSDX;

            if (entityAlive != null && iEntity != null)
            {
                // 2. Persistence Setup
                entityAlive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

                // Block fresh-item generation during PostInit for restored NPCs.
                bool isFreshSpawn = !this.itemValue.HasMetadata("EntityClassId");
                if (!isFreshSpawn)
                    entityAlive.Buffs.SetCustomVar("InitialInventory", 1);

                // 3. Spawn first — EntityTrader.PostInit() (called inside SpawnEntityInWorld) creates
                // a fresh TileEntityTrader and assigns it to lootContainer.  Hydrating before spawn
                // would be overwritten.  We hydrate AFTER so we write into the initialized containers.
                _world.SpawnEntityInWorld(entityAlive);

                // 4. Hydrate Data — unpack ItemValue metadata into the entity.
                EntitySyncUtils.SetNPCItemValue(entityAlive, this.itemValue);

                // 5. Force Sync — vanilla spawn packets don't carry SDX-specific data (name, title, weapon).
                iEntity.SendSyncData();
            }
        }
    }

    public override int GetLength()
    {
        // CRITICAL: Return 0. 
        // Unlike NetPackageTurretSpawn which returns 20, our ItemValue contains variable-length metadata strings.
        // Returning 0 forces the network layer to calculate the exact packet size header dynamically.
        return 0;
    }
}