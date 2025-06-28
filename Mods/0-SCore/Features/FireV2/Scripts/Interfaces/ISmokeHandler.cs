public interface ISmokeHandler
{
    void AddSmoke(Vector3i position);
    void CheckSmokePositions();
    void RemoveSmoke(Vector3i position);
}