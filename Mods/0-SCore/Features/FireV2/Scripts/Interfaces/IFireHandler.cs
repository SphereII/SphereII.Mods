public interface IFireHandler
{
    void AddFire(Vector3i position, int entityId = -1);
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
}