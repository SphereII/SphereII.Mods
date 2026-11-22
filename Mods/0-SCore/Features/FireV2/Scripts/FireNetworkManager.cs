using System;
using System.Collections.Generic;
using UnityEngine;

public class FireNetworkManager : IFireNetworkManager
{
    private readonly ConnectionManager _connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;

    public void SyncAddFire(Vector3i blockPos, int entityId = -1)
    {
        if (!_connectionManager.IsServer)
        {
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackageAddFirePosition>().Setup(blockPos, entityId));
            return;
        }

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageAddFirePosition>().Setup(blockPos, entityId));
    }

    public void SyncAddFireBatch(List<Vector3i> positions, int entityId = -1)
    {
        foreach (var chunk in SplitIntoChunks(positions))
            Send(NetPackageManager.GetPackage<NetPackageAddFirePositions>().Setup(chunk, entityId));
    }

    /// <summary>Server to one client: everything that is burning. Used when a player joins.</summary>
    public void SendFiresToClient(ClientInfo client, List<Vector3i> positions)
    {
        if (client == null || !_connectionManager.IsServer) return;

        foreach (var chunk in SplitIntoChunks(positions))
            client.SendPackage(NetPackageManager.GetPackage<NetPackageAddFirePositions>().Setup(chunk, -1));
    }

    private void Send(NetPackage package)
    {
        if (_connectionManager.IsServer)
            _connectionManager.SendPackage(package);
        else
            _connectionManager.SendToServer(package);
    }

    // The batch packets store their count as a short, so large batches go out in chunks. Each chunk
    // is its own copy: a packet is written some time after it is queued, and the caller is free to
    // reuse its list in the meantime.
    private static IEnumerable<List<Vector3i>> SplitIntoChunks(List<Vector3i> positions)
    {
        if (positions == null) yield break;

        for (var start = 0; start < positions.Count; start += MaxPositionsPerPackage)
            yield return positions.GetRange(start, Math.Min(MaxPositionsPerPackage, positions.Count - start));
    }

    public void SyncExtinguishFire(Vector3i blockPos, int entityId = -1)
    {
        if (!_connectionManager.IsServer)
        {
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackageAddExtinguishPosition>().Setup(blockPos, entityId));
            return;
        }

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageAddExtinguishPosition>().Setup(blockPos, entityId));
    }


    private const int MaxPositionsPerPackage = 1000;

    public void SyncExtinguishFireBatch(List<Vector3i> positions, int entityId = -1)
    {
        foreach (var chunk in SplitIntoChunks(positions))
            Send(NetPackageManager.GetPackage<NetPackageAddExtinguishPositions>().Setup(chunk, entityId));
    }

    public void SyncExtinguishArea(Vector3i center, int range, int entityId = -1)
    {
        // Only clients ask; the server works the area out itself.
        if (_connectionManager.IsServer) return;
        _connectionManager.SendToServer(
            NetPackageManager.GetPackage<NetPackageExtinguishArea>().Setup(center, range, entityId));
    }

    public void SyncRemoveFire(Vector3i blockPos, int entityId = -1)
    {
        if (!_connectionManager.IsServer)
        {
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackageRemoveFirePosition>().Setup(blockPos, entityId));
            return;
        }

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageRemoveFirePosition>().Setup(blockPos, entityId));
    }

    public void SyncRemoveFireBatch(List<Vector3i> positions, int entityId = -1)
    {
        foreach (var chunk in SplitIntoChunks(positions))
            Send(NetPackageManager.GetPackage<NetPackageRemoveFirePositions>().Setup(chunk, entityId));
    }

    public void SyncFireUpdate(int fireCount)
    {
        if (!_connectionManager.IsServer)
            return;

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageFireUpdate>().Setup(fireCount));
    }

    public void SyncBlockDestroyedByFire(int count)
    {
        if (!_connectionManager.IsServer)
        {
            return;
        }

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageBlockDestroyedByFire>().Setup(count));
    }
}