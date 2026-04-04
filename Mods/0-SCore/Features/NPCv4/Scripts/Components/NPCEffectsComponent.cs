using UnityEngine;

/// <summary>
/// Throttles the block-radius effects scan so it runs at most once every
/// BlockRadiusInterval seconds instead of every frame.
///
/// Key improvement over EntityAliveSDX:
///   - EntityUtilities.UpdateBlockRadiusEffects is expensive (nearby block scan).
///     Running it every frame for many NPCs is wasteful; 2-second intervals are
///     imperceptible to gameplay while dramatically reducing block-query load.
/// </summary>
public class NPCEffectsComponent : INPCComponent
{
    /// <summary>Seconds between block-radius effect evaluations.</summary>
    private const float BlockRadiusInterval = 2f;

    private EntityAliveSDXV4 _entity;
    private float _nextCheckTime;

    // -----------------------------------------------------------------------
    // INPCComponent
    // -----------------------------------------------------------------------

    public void Initialize(EntityAliveSDXV4 entity) => _entity = entity;

    public void OnUpdateLive(ref NPCFrameCache cache)
    {
        // Remote entities don't need to evaluate local block effects.
        if (_entity.isEntityRemote) return;

        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + BlockRadiusInterval;

        EntityUtilities.UpdateBlockRadiusEffects(_entity);
    }

    public void OnDead()   { }
    public void OnUnload() { }
}
