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
 *      <property name="Class" value="EntitySwimingSDX, Mods" />
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

using System;
using UnityEngine;

class EntitySwimingSDX : EntityZombieFlyingSDX
{
    //public static System.Random random = new System.Random();
    //public DynamicProperties Properties = new DynamicProperties();

    //public override void Init(int _entityClass)
    //{
    //    base.Init(_entityClass);
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        this.emodel.SetVisible(true, true);
        if (base.getNavigator() == null)
            return;

        base.getNavigator().setCanDrown(false);
        //base.getNavigator().setInWater(true);
    }
    //}

    public override void OnAddedToWorld()
    {
        base.OnAddedToWorld();
       // Debug.Log("Position: " + this.position.ToString());
        //Debug.Log("Spawning Fish: " + this.entityName);
    }


    // While the fish are birds, we do want to adjust the way point settings, so they are not attracted to the air, but rather the water.
    public override void AdjustWayPoint()
    {

        int num = 255;
        Vector3i localWaypoint = new Vector3i((Waypoint));
        // if waypoint is in the air, keep dropping it until it's out of the air, and into the water.
        while (world.GetBlock(localWaypoint).type == BlockValue.Air.type && num > 0)
        {
            maxHeight = Utils.Fastfloor(localWaypoint.y) - 2;
            localWaypoint.y = localWaypoint.y - 1;
            num--;
        }

        // Attempt to get rid of the vector zero errors.
        Waypoint = localWaypoint.ToVector3();
    
    }


    // Over-riding the SetRotation to get around the look vector zero error
    public override void SetRotation(Vector3 _rot)
    {
        if ( _rot != Vector3.zero)
            this.rotation = _rot;
   
    }

}


