/// <summary>
/// Interface implemented by every NPC feature component used by EntityAliveSDXV4.
/// Components own a single concern and are driven by the entity's OnUpdateLive tick.
/// </summary>
public interface INPCComponent
{
    /// <summary>Called once during PostInit, after the entity is fully constructed.</summary>
    void Initialize(EntityAliveSDXV4 entity);

    /// <summary>
    /// Called every OnUpdateLive tick.  Receives the frame cache by reference so all
    /// components share the same pre-populated leader/target data without repeating lookups.
    /// </summary>
    void OnUpdateLive(ref NPCFrameCache cache);

    /// <summary>Called when the entity dies (SetDead).</summary>
    void OnDead();

    /// <summary>Called when the entity is removed from the world (OnEntityUnload).</summary>
    void OnUnload();
}
