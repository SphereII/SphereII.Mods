using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Distributes the call to all clients to add a block that is considered burning.
/// </summary>

[UsedImplicitly]
public class NetPackageBlockDestroyedByFire : NetPackage
{
    private int _count;
    public NetPackageBlockDestroyedByFire Setup(int count)
    {
        _count = count;
        return this;
    }

    public override void read(PooledBinaryReader br)
    {
        _count = br.ReadInt32();
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);

        bw.Write(_count);
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
        
        FireManager.Instance.InvokeOnBlockDestroyed(_count);
    }
   
}

