using System.Collections.Generic;

public interface IFireNetworkManager
{
    void SyncAddFire(Vector3i position, int entityId);
    void SyncExtinguishFire(Vector3i position, int entityId);
    void SyncAddFireBatch(List<Vector3i> positions, int entityId = -1);
    public void SyncRemoveFireBatch(List<Vector3i> positions, int entityId = -1);
}