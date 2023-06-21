using JetBrains.Annotations;
/// <summary>
/// Distributes the call to remove a particle effect to all clients. This is used by the Fire management system.
/// </summary>
/// <remarks>
/// This allows us to distribute which block is showing a fire particle. 
/// </remarks>
/// 
[UsedImplicitly]
public class NetPackageRemoveParticleEffect : NetPackage
{
    private Vector3i _position;
    private int _entityThatCausedIt;

    public NetPackageRemoveParticleEffect Setup(Vector3i position, int entityThatCausedIt)
    {
        _position = position;
        _entityThatCausedIt = entityThatCausedIt;
        return this;
    }


    public override void read(PooledBinaryReader br)
    {
        _position = new Vector3i(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
        _entityThatCausedIt = br.ReadInt32();
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        bw.Write(_position.x);
        bw.Write(_position.y);
        bw.Write(_position.z); 
        bw.Write(_entityThatCausedIt);
    }

     public override int GetLength()
    {
        return 20;
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null)
        {
            return;
        }

        if (!world.IsRemote())
        {
            return;
        }
        FireManager.Instance.ClearPos(_position);
    }
}

