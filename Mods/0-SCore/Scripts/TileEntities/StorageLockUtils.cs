/// <summary>
/// Answers "does someone have this container open?" without TileEntity.IsUserAccessing().
///
/// Since 3.4 IsUserAccessing() is a local flag set by this machine's loot window. The server's copy
/// never sees it for remote players, and it is due to be fully replaced by LockManager. Opening a
/// container takes a LockManager lock on the TEFeatureStorage (channel 0), so that lock is what to ask.
/// </summary>
public static class StorageLockUtils
{
    /// <summary>
    /// On the server (dedicated or host): true if any player holds a lock on the target.
    /// On a client: true only if the local player holds it. Clients can't see other players' locks.
    /// </summary>
    public static bool IsLocked(ILockTarget target, ushort channel = 0)
    {
        if (target == null) return false;

        var lockManager = LockManager.Instance;
        if (lockManager == null) return false;

        var connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        return connectionManager != null && connectionManager.IsServer
            ? lockManager.IsLockedServer(target, channel)
            : lockManager.IsLockedByLocalPlayer(target, channel);
    }

    /// <summary>
    /// Server only: true if a player other than the local one holds a lock on the target.
    /// Always false on a client, which can't see other players' locks.
    /// </summary>
    public static bool IsLockedByOtherPlayer(ILockTarget target, ushort channel = 0)
    {
        if (target == null) return false;

        var connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (connectionManager == null || !connectionManager.IsServer) return false;

        var lockManager = LockManager.Instance;
        if (lockManager == null) return false;

        return lockManager.IsLockedServer(target, channel) && !lockManager.IsLockedByLocalPlayer(target, channel);
    }

    /// <summary>
    /// True if this machine's loot window is showing this container. For storage that isn't in the
    /// world, such as an NPC's SCoreLootContainer: locks are addressed by world position, so
    /// LockManager can't lock those, and SCore opens them straight through OpenLooting.
    /// </summary>
    public static bool IsOpenInLocalLootWindow(TEFeatureStorage storage)
    {
        if (storage == null || GameManager.IsDedicatedServer) return false;

        var ui = LocalPlayerUI.GetUIForPrimaryPlayer();
        if (ui == null || ui.xui == null || ui.windowManager == null) return false;
        if (!ui.windowManager.IsWindowOpen(XUiC_LootWindowGroup.ID)) return false;

        // The group keeps its last container after closing, so the open check above is required.
        return ui.xui.FindWindowGroupByName(XUiC_LootWindowGroup.ID) is XUiC_LootWindowGroup group
               && group.te == storage;
    }

    /// <summary>
    /// True if anyone this machine can know about has the storage open: a LockManager lock, or the
    /// local loot window for storage that can't be locked. A null storage counts as in use, so
    /// callers leave it alone.
    /// </summary>
    public static bool IsInUse(TEFeatureStorage storage)
    {
        if (storage == null) return true;
        return IsLocked(storage) || IsOpenInLocalLootWindow(storage);
    }
}
