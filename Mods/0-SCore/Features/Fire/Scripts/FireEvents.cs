public class FireEvents
{
    public delegate void OnBlockDestroyedByFire();

    public event OnBlockDestroyedByFire OnDestroyed;

    public delegate void OnFireStart(int entityId);

    public event OnFireStart OnStartFire;

    public delegate void OnFireRefresh(int count);

    public event OnFireRefresh OnFireUpdate;

    public delegate void OnExtinguishFire(int count, int entityId);

    public event OnExtinguishFire OnExtinguish;
}
