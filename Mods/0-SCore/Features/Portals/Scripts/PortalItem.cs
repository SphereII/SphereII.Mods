using System.Linq;

public class PortalItem
{
    public Vector3i Position;
    public string Source;
    public string Destination;
    public string Prefab;
    public PortalItem(Vector3i position, string signData)
    {
        Position = position;
        Source = signData;
        Destination = signData;
        foreach (var config in signData.Split(','))
        {
            if (config.StartsWith("source="))
                Source = config.Split('=')[1];
            if (config.StartsWith("destination="))
                Destination = config.Split('=')[1];
            if (config.StartsWith("prefab="))
                Prefab = config.Split('=')[1];
        }

    }
    public PrefabInstance GetPrefabInstance()
    {
        if (string.IsNullOrEmpty(Prefab)) return null;

        foreach (var prefabInstance in GameManager.Instance.GetDynamicPrefabDecorator().allPrefabs.Where(n => n.name.StartsWith(Prefab)))
            return prefabInstance;

        return null;
    }
}