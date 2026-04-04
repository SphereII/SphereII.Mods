using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns patrol-coordinate and guard-position state.
///
/// Key improvement over EntityAliveSDX:
///   - Duplicate detection uses a HashSet of block-centre positions (O(1)) instead of
///     List.Contains on the raw coordinate list (O(n)).
/// </summary>
public class NPCPatrolComponent : INPCComponent
{
    // -----------------------------------------------------------------------
    // State
    // -----------------------------------------------------------------------

    /// <summary>Accurate world-space patrol positions used for NPC movement.</summary>
    public readonly List<Vector3> PatrolCoordinates = new List<Vector3>();

    /// <summary>
    /// Block-centred dedup set.  Keyed on (floor(x)+0.5, floor(y), floor(z)+0.5) so that
    /// positions within the same block are treated as duplicates, matching the original intent.
    /// </summary>
    private readonly HashSet<Vector3> _blockCenterSet = new HashSet<Vector3>();

    public Vector3 GuardPosition     { get; set; } = Vector3.zero;
    public Vector3 GuardLookPosition { get; set; } = Vector3.zero;

    // -----------------------------------------------------------------------
    // INPCComponent — patrol logic is event-driven, not per-tick
    // -----------------------------------------------------------------------

    public void Initialize(EntityAliveSDXV4 entity) { }
    public void OnUpdateLive(ref NPCFrameCache cache) { }
    public void OnDead()   { }
    public void OnUnload() { }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Adds a world-space position as a patrol waypoint, ignoring duplicates at block
    /// granularity.  O(1) duplicate check via the internal HashSet.
    /// </summary>
    public void AddPatrolPoint(Vector3 position)
    {
        var blockCenter = new Vector3(
            0.5f + Utils.Fastfloor(position.x),
            Utils.Fastfloor(position.y),
            0.5f + Utils.Fastfloor(position.z));

        if (!_blockCenterSet.Add(blockCenter))
            return; // duplicate block — skip

        PatrolCoordinates.Add(position);
    }

    /// <summary>Clears all patrol data.</summary>
    public void Clear()
    {
        PatrolCoordinates.Clear();
        _blockCenterSet.Clear();
    }
}
