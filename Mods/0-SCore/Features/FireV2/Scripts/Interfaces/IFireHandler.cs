using System.Collections;
using System.Collections.Generic;

public interface IFireHandler
{

    Dictionary<Vector3i, FireBlockData> GetFireMap();
    void AddFire(Vector3i position, int entityId = -1, bool trusted = false);
    void RemoveFire(Vector3i position, int entityId = -1, bool showSmoke = true);
    void SpreadFire(Vector3i sourcePosition);
    bool IsFlammable(Vector3i position);
    bool IsBurning(Vector3i position);
    void UpdateFires();
    void SaveState();
    void LoadState();
    bool IsPositionCloseToFire(Vector3i vector3I, int maxRange);
    void Reset();
    void ForceStop();
    int CloseFires(Vector3i position, int maxRange);

    bool IsAnyFireBurning();
    public void DisplayStatus(double elapsedTime);
    public bool IsProcessing();
    public int GetFiresPerFrame();

    public void FinalizeBatchProcessing();
    void UpdateExtinguishedPositions();
    int RestorePendingFires();
    List<Vector3i> GetBurningPositions();
}