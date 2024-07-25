using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// This interface signifies that the entity can receiver orders.
/// </para>
/// 
/// <para>
/// The orders do not have to come from a player. For example, they could come from a pathing cube.
/// </para>
/// 
/// <para>
/// The entity does not have to <em>obey</em> the orders. It is only assumed that they will adjust
/// their behavior upon receiving at least one of the possible orders as specified in
/// <see cref="EntityUtilities.Orders"/>.
/// </para>
/// 
/// <para>
/// Entities that implement this interface should also descend from <see cref="EntityAlive"/>,
/// or one of its descendants.
/// </para>
/// </summary>
public interface IEntityOrderReceiverSDX
{
    /// <summary>
    /// If an entity is guarding a location, this is its look position.
    /// </summary>
    Vector3 GuardLookPosition { get; set; }

    /// <summary>
    /// If the entity is guarding a location, this is the position where it is guarding.
    /// </summary>
    Vector3 GuardPosition { get; set; }

    /// <summary>
    /// If the entity is patrolling, this is the list of coordinates on its route.
    /// </summary>
    List<Vector3> PatrolCoordinates { get; }

    /// <summary>
    /// The entity's current position.
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// When this method is called, the entity will scan the area for auto pathing blocks.
    /// If it is capable of carrying out the orders on those blocks, it will do so.
    /// </summary>
    void SetupAutoPathingBlocks();

    /// <summary>
    /// Updates the list of patrol coordinates with the point at the given position.
    /// </summary>
    /// <param name="position"></param>
    void UpdatePatrolPoints(Vector3 position);
}