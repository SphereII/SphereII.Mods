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

public class EntitySwimmingSDX : EntitySwimingSDX {
}

public class EntitySwimingSDX : EntityZombieFlyingSDX
{
    public List<Vector3i> WaterBlocks = new List<Vector3i>();

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        emodel.SetVisible(true, true);
        if (getNavigator() == null)
            return;

        getNavigator().setCanDrown(false);
       
    }

    public override bool IsAirBorne()
    {
        return false;
    }
    public override void OnAddedToWorld()
    {
        base.OnAddedToWorld();
        RefreshWaterBlocks();
    }

    public override void OnUpdateLive() {
        base.OnUpdateLive();
        if (!IsGoingToWater())
            AdjustWayPoint();
        if (!IsInWater())
            MarkToUnload();
    }

    public bool IsGoingToWater() {
        var vector = new Vector3i(Waypoint);
        return WaterBlocks.Contains(vector);
    }
    public void RefreshWaterBlocks(int range = 10, int maxY = 5) {
        var blockPos = Vector3i.zero;
        var vector = new Vector3i(position);
        for (var x = vector.x - range; x < vector.x + range; x++)
        {
            for (var z = vector.z - range; z < vector.z + range; z++)
            {
                for (var y = vector.y - maxY; y < vector.y + maxY; y++)
                {
                    blockPos.x = x;
                    blockPos.z = z;
                    blockPos.y = y;
                    var waterPercent = GameManager.Instance.World.GetWaterPercent(blockPos);
                    if (waterPercent < 0.1) continue;
                    if (WaterBlocks.Contains(blockPos)) continue;
                    WaterBlocks.Add(blockPos);
                }
            }
        }
        // If there's not enough water, then just despawn.
        if ( WaterBlocks.Count < 20)
            MarkToUnload();
    }

    private Vector3i GetRandomPosition() {
        if (WaterBlocks.Count == 0)
            RefreshWaterBlocks();

        if (WaterBlocks.Count == 0)
            return Vector3i.zero;

        var index = rand.RandomRange(0, WaterBlocks.Count);
        return WaterBlocks[index];
    }

    public void AdjustWayPoint() {
        Waypoint = GetRandomPosition();
    }


    // Over-riding the SetRotation to get around the look vector zero error
    public override void SetRotation(Vector3 _rot)
    {
        if (_rot != Vector3.zero)
            rotation = _rot;
    }
}