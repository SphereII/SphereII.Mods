using UnityEngine;

public class NetPackagePickUpNPCSDX : global::NetPackage
{
    private int _entityId;
    private int _playerId;

    public NetPackagePickUpNPCSDX Setup(int entityId, int playerId)
    {
        _entityId = entityId;
        _playerId = playerId;
        return this;
    }

    public override void read(PooledBinaryReader br)
    {
        _entityId = br.ReadInt32();
        _playerId = br.ReadInt32();
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        bw.Write(_entityId);
        bw.Write(_playerId);
    }

    public override int GetLength()
    {
        return 8;
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null)
        {
            return;
        }

        if (!world.IsRemote())
        {
            EntityUtilities.CollectEntityServer(_entityId, _playerId);
            return;
        }

        EntityUtilities.CollectEntityClient(_entityId, _playerId);
    }

 
}