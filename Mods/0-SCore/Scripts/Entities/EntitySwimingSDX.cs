/*
 * Class: EntitySwimingSDX
 * Author:  sphereii
 * Category: Entity
 * Description:
 *      This Mod extends the flying class to give us entities that will swim in the water, providing a more diversifed underwater environment.
 *      If the entity is not in the water, then it will be marked to despawn.
 *
 *      It contains a built-in auto-scaler of spawned entities, by has no impact on harvest amount or speed.
 *
 * Usage:
 *  Add the following class to entities that are meant to swim in the water.
 *
 *      <property name="Class" value="EntitySwimingSDX, SCore" />
 *
 * Features:
 *      This class inherits features from the FlyingSDX class, but the two main ones are highlighted here.
 *
 *  FlockSize:
 *      In order to gain more entities, you can set the flockSize to a value other than 0. This means for every fish that spawn, it'll have a chance to spawn additional
 *      fish of the same type.
 *
 *      Usage:
 *         <property name="FlockSize" value="0" />
 *
 *  IsAgressive:
 *      Fish are naturally passive, but can be turned aggressive by seeing the IsAgressive flag to "true"
 *
 *      Usage:
 *          <property name="IsAgressive" value="False" />
 *
 */

using System.Collections.Generic;
using UnityEngine;

// Correctly-spelled alias — resolves to the same implementation.
public class EntitySwimmingSDX : EntitySwimingSDX { }

public class EntitySwimingSDX : EntityZombieFlyingSDX
{
    // Separate backing stores so both operations are O(1):
    //   _waterBlockSet  — O(1) Contains for the per-tick IsGoingToWater() check.
    //   _waterBlockList — O(1) index access for the random waypoint pick.
    // Both are rebuilt together in RefreshWaterBlocks().
    private readonly HashSet<Vector3i> _waterBlockSet  = new HashSet<Vector3i>();
    private readonly List<Vector3i>    _waterBlockList = new List<Vector3i>();

    // How many OnUpdateLive ticks between water-block rescans.
    // At ~20 ticks/s this is roughly 15 s, giving fish time to swim to a new
    // area before we re-centre the scan on their updated position.
    private const int RefreshInterval = 300;
    private int _refreshCounter = 0;

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        emodel.SetVisible(true, true);
        if (getNavigator() == null)
            return;

        getNavigator().setCanDrown(false);
    }

    public override bool IsAirBorne() => false;

    public override void OnAddedToWorld()
    {
        base.OnAddedToWorld();
        RefreshWaterBlocks();
    }

    public override void OnUpdateLive()
    {
        base.OnUpdateLive();

        // Periodically re-centre the water scan on the entity's current position
        // so fish that have swum away from their spawn point continue to find
        // valid waypoints instead of chasing stale positions.
        if (++_refreshCounter >= RefreshInterval)
        {
            _refreshCounter = 0;
            RefreshWaterBlocks();
        }

        if (!IsGoingToWater())
            AdjustWayPoint();
        if (!IsInWater())
            MarkToUnload();
    }

    // O(1) via HashSet.
    public bool IsGoingToWater()
    {
        return _waterBlockSet.Contains(new Vector3i(Waypoint));
    }

    /// <summary>
    /// Clears and rebuilds the water-block index centred on the entity's current
    /// position. Despawns the entity if fewer than 20 water blocks are found
    /// (i.e. the entity has strayed out of any meaningful body of water).
    /// </summary>
    public void RefreshWaterBlocks(int range = 10, int maxY = 5)
    {
        _waterBlockSet.Clear();
        _waterBlockList.Clear();

        var center   = new Vector3i(position);
        var blockPos = Vector3i.zero;

        for (var x = center.x - range; x < center.x + range; x++)
        {
            for (var z = center.z - range; z < center.z + range; z++)
            {
                for (var y = center.y - maxY; y < center.y + maxY; y++)
                {
                    blockPos.x = x;
                    blockPos.y = y;
                    blockPos.z = z;

                    if (GameManager.Instance.World.GetWaterPercent(blockPos) < 0.1f)
                        continue;

                    // HashSet.Add returns false for duplicates, so no explicit
                    // Contains-before-Add guard is needed.
                    if (_waterBlockSet.Add(blockPos))
                        _waterBlockList.Add(blockPos);
                }
            }
        }

        if (_waterBlockSet.Count < 20)
            MarkToUnload();
    }

    // O(1) index access via List.
    private Vector3i GetRandomPosition()
    {
        if (_waterBlockList.Count == 0)
            RefreshWaterBlocks();

        if (_waterBlockList.Count == 0)
            return Vector3i.zero;

        return _waterBlockList[rand.RandomRange(0, _waterBlockList.Count)];
    }

    public void AdjustWayPoint()
    {
        Waypoint = GetRandomPosition();
    }

    // Guards against the "look vector is zero" Unity error.
    public override void SetRotation(Vector3 _rot)
    {
        if (_rot != Vector3.zero)
            rotation = _rot;
    }
}
