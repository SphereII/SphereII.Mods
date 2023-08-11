using UnityEngine;

public class NetPackagePlaceNPCSDX : global::NetPackage
{
    private int _entityType;
    private Vector3 _pos;
    private Vector3 _rot;
    private ItemValue _itemValue;

    private int entityThatPlaced;
    public NetPackagePlaceNPCSDX Setup(int entityType, Vector3 pos, Vector3 rot, ItemValue itemValue, int entityThatPlaced = -1)
    {
        _entityType = entityType;
        _pos = pos;
        _rot = rot;
        _itemValue = itemValue;
        entityThatPlaced = entityThatPlaced;
        return this;
    }
    public override void read(PooledBinaryReader reader)
    {
        _entityType = reader.ReadInt32();
        _pos = StreamUtils.ReadVector3(reader);
        _rot = StreamUtils.ReadVector3(reader);
        _itemValue = new ItemValue();
        _itemValue.Read(reader);
        entityThatPlaced = reader.ReadInt32();
    }

    public override void write(PooledBinaryWriter writer)
    {
        base.write(writer);
        writer.Write(_entityType);
        StreamUtils.Write(writer, _pos);
        StreamUtils.Write(writer, _rot);
        _itemValue.Write(writer);
        writer.Write(entityThatPlaced);
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null)
        {
            return;
        }
        var entityAliveSdx = (EntityAliveSDX)EntityFactory.CreateEntity(_entityType, _pos, _rot);
        entityAliveSdx.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
        entityAliveSdx.SetItemValue(_itemValue.Clone());
        world.SpawnEntityInWorld(entityAliveSdx);
    }

    public override int GetLength()
    {
        return 20;
    }
}