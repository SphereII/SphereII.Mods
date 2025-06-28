public interface ILightManager
{
    void AddLight(Vector3i position);
    void RemoveLight(Vector3i position);
    void UpdateLights();
}