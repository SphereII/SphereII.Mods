
public class NetPackageRemoveParticleEffect : NetPackageParticleEffect
{
    private Vector3i position;
    private int entityThatCausedIt;

    public NetPackageRemoveParticleEffect Setup(Vector3i _position, int _entityThatCausedIt)
    {
        position = _position;
        this.entityThatCausedIt = _entityThatCausedIt;
        return this;
    }


    public override void read(PooledBinaryReader _br)
    {
        this.position = StreamUtils.ReadVector3i(_br);
        this.entityThatCausedIt = _br.ReadInt32();
    }


    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        StreamUtils.Write(_bw, this.position);
        _bw.Write(this.entityThatCausedIt);
    }



    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        _world.GetGameManager().RemoveBlockParticleEffect(position);
    }
}

