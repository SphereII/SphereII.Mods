using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Distributes the entire PortalMap from the server to a client.
/// </summary>
public class NetPackagePortalMapSync : NetPackage
{
    private Dictionary<Vector3i, string> _portalMap;

    public NetPackagePortalMapSync Setup(Dictionary<Vector3i, string> portalMap)
    {
        _portalMap = portalMap;
        return this;
    }

    public override void read(PooledBinaryReader br)
    {
        _portalMap = new Dictionary<Vector3i, string>();
        var count = br.ReadInt32();
        
        for (var i = 0; i < count; i++)
        {
            var position = StreamUtils.ReadVector3i(br);
            var name = br.ReadString();
            _portalMap.Add(position, name);
        }
    }

    public override void write(PooledBinaryWriter bw)
    {
        var count = _portalMap.Count;
        bw.Write(count);
        
        foreach (var entry in _portalMap)
        {
            StreamUtils.Write(bw, entry.Key);
            bw.Write(entry.Value);
        }
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null) return;
        
        Debug.Log($"Setting New Portal Map: {_portalMap.Count}");
        // Clear the client's existing portal map and replace with the server's data.
        PortalManager.Instance.ReplaceMap(_portalMap);
    }

    public override int GetLength()
    {
        // This is a placeholder. The actual length depends on the size of the map.
        // A simple calculation would be:
        // sizeof(int) for count + count * (sizeof(Vector3i) + avg_string_length)
        return 20;
    }
}