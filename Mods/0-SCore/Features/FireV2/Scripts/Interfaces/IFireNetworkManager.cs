using System.Collections.Generic;

public interface IFireNetworkManager
{
    void SyncAddFire(Vector3i position, int entityId);
    void SyncExtinguishFire(Vector3i position, int entityId);
}