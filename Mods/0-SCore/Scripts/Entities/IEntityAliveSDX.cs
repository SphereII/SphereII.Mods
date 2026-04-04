/// <summary>
/// Shared capability interface implemented by both EntityAliveSDX and EntityAliveSDXV4.
/// Allows UAI tasks, filters, and Harmony patches to work with either entity class without
/// a hard cast to EntityAliveSDX.
/// </summary>
public interface IEntityAliveSDX
{
    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>Localised display name of this NPC.</summary>
    string EntityName { get; }

    /// <summary>The NPC's first/given name (localization key or raw string).</summary>
    string FirstName { get; set; }

    /// <summary>The NPC's title (localization key or raw string). Read-only; set via entity class config.</summary>
    string Title { get; set; }

    // ── State queries ─────────────────────────────────────────────────────────

    /// <summary>Whether this NPC can be hired by a player.</summary>
    bool IsHirable { get; }

    bool IsSleeping { get; set; }
 
    /// <summary>Returns true if the NPC is currently on a mission (hidden, following a vehicle, etc.).</summary>
    bool IsOnMission();

    // ── Movement ──────────────────────────────────────────────────────────────

    /// <summary>Teleports this NPC to stand near <paramref name="target"/>.</summary>
    void TeleportToPlayer(EntityAlive target, bool randomPosition = false);

    /// <summary>Resets movement speed to the NPC's base value.</summary>
    void RestoreSpeed();

    // ── Combat ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Triggers the held-item action at <paramref name="actionIndex"/>.
    /// Returns false if the action could not start (e.g. entity is stunned or reloading).
    /// </summary>
    bool ExecuteAction(bool bAttackReleased, int actionIndex);

    // ── Inventory / sync ──────────────────────────────────────────────────────

    /// <summary>Equips the weapon stored in the NPC's current-weapon slot.</summary>
    void UpdateWeapon(string name = "");

    /// <summary>Sends an inventory/name sync package to connected clients.</summary>
    void SendSyncData(ushort syncFlags = 1);

    void ConditionalTriggerSleeperWakeUp();
    bool HasAnyTags(FastTags<TagGroup.Global> parse);
}
