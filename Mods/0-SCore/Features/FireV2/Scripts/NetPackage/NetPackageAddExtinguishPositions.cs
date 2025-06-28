using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

/// <summary>
/// Distributes the call to all clients to extinguish a block
/// </summary>

[UsedImplicitly]
public class NetPackageAddExtinguishPositions : NetPackage
{
    private List<Vector3i> _positions;
    private int _entityThatCausedIt;

    public NetPackageAddExtinguishPositions Setup(List<Vector3i> positions, int entityThatCausedIt)
    {
        _positions = positions;
        _entityThatCausedIt = entityThatCausedIt;
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

        _entityThatCausedIt = br.ReadInt32();
    }

    public override void write(PooledBinaryWriter bw) {
        base.write(bw);
        var count = _positions.Count;
        bw.Write((short)count);
        for (var i = 0; i < count; i++)
        {
            StreamUtils.Write(bw, _positions[i]);
        }

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
        foreach (var position in _positions)
            FireManager.Instance?.ExtinguishFire(position, _entityThatCausedIt);
    }
}

