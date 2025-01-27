using System.Collections.Generic;
using JetBrains.Annotations;

/// <summary>
/// Distributes the call to all clients to add a block that is considered burning.
/// </summary>

[UsedImplicitly]
public class NetPackageFireUpdate : NetPackage
{
    private List<Vector3i> _positions;
    private int _entityThatCausedIt;

    public NetPackageFireUpdate Setup(List<Vector3i> positions)
    {
        _positions = positions;
        return this;
    }

    public override void read(PooledBinaryReader br) {
        var num = (int)br.ReadInt16();
        _positions = new List<Vector3i>();
        for (var i = 0; i < num; i++)
        {
            var position = StreamUtils.ReadVector3i(br);
            _positions.Add(position);
        }
    }

    public override void write(PooledBinaryWriter bw) {
        base.write(bw);
        var count = _positions.Count;
        bw.Write((short)count);
        for (var i = 0; i < count; i++)
        {
            StreamUtils.Write(bw, _positions[i]);
        }
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
        FireManager.Instance?.InvokeFireUpdate();
    }
}

