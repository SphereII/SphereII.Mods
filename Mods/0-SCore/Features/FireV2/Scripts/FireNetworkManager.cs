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
        if (!_connectionManager.IsServer)
        {
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackageAddFirePositions>().Setup(positions, entityId));
            return;
        }

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageAddFirePositions>().Setup(positions, entityId));
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


    public void SyncExtinguishFireBatch(List<Vector3i> positions, int entityId = -1)
    {
        if (!_connectionManager.IsServer)
        {
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackageAddExtinguishPositions>().Setup(positions, entityId));
            return;
        }

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageAddExtinguishPositions>().Setup(positions, entityId));
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
        if (!_connectionManager.IsServer)
        {
            _connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackageRemoveFirePositions>().Setup(positions, entityId));
            return;
        }

        _connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageRemoveFirePositions>().Setup(positions, entityId));
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