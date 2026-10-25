using JetBrains.Annotations;

/// <summary>
/// Client to server: put out every fire within a cube around a position. The server checks its own
/// fire map, so this works even when the client never heard about a fire, then tells every client
/// which blocks went out.
/// </summary>
[UsedImplicitly]
public class NetPackageExtinguishArea : NetPackage
{
    private Vector3i _center;
    private int _range;
    private int _entityThatCausedIt;

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

    public NetPackageExtinguishArea Setup(Vector3i center, int range, int entityThatCausedIt)
    {
        _center = center;
        _range = range;
        _entityThatCausedIt = entityThatCausedIt;
        return this;
    }

    public override void read(PooledBinaryReader br)
    {
        _center = StreamUtils.ReadVector3i(br);
        _range = br.ReadInt32();
        _entityThatCausedIt = br.ReadInt32();
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        StreamUtils.Write(bw, _center);
        bw.Write(_range);
        bw.Write(_entityThatCausedIt);
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null) return;
        FireManager.Instance?.ExtinguishArea(_center, _range, _entityThatCausedIt);
    }
}
