using System;
using System.Collections.Generic;
using System.Globalization;
using GamePath;
using UnityEngine;

public class EAIApproachAndFollowTargetSDX : EAIApproachAndAttackTarget
{
    List<String> lstIncentives = new List<String>();
    private List<Entity> NearbyEntities = new List<Entity>();
    private Vector3 entityTargetPos;
    private Vector3 entityTargetVel;
    private int pathCounter;

  //  public EntityAliveSDX entityAliveSDX;

    private bool blDisplayLog = false;
    private EntityAlive entityTarget;
    private bool isTargetToEat;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
       // entityAliveSDX = (_theEntity as EntityAliveSDX);
    }

    // Allow params to be a comma-delimited list of various incentives, such as item name, buff, or cvar.
    public override void SetParams1(string _par1)
    {
        string[] array = _par1.Split(new char[]
        {
                ','
        });
        for (int i = 0; i < array.Length; i++)
        {
            if (this.lstIncentives.Contains(array[i].ToString()))
                continue;
            this.lstIncentives.Add(array[i].ToString());
        }
    }

    public virtual bool ConfigureTargetEntity()
    {
        //if (this.entityTarget != null)
        //    return true;

        this.NearbyEntities.Clear();

        //if (this.entityAliveSDX == null)
        //{
        //    if (this.theEntity is EntityAliveSDX)
        //        this.entityAliveSDX = (this.theEntity as EntityAliveSDX);
        //    else
        //    {
        //        DisplayLog(" Not an EntityAliveSDX");
        //        return false;
        //    }
        //}
  
        DisplayLog(" ConfigureTargetEntity()");
        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(30f, 20f, 30f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity)
            {

                // Check the entity against the incentives
                if (EntityUtilities.CheckIncentive(this.theEntity.entityId, this.lstIncentives, x))
                {
                    DisplayLog(" Found my Target: " + x.EntityName);
                    this.entityTarget = x;

                    Vector3 tempPosition =  (this.theEntity.position - this.entityTarget.position).normalized * 3 + this.entityTarget.position;
                    
                    Vector3 a = this.theEntity.position - tempPosition;
                    DisplayLog(" Distance: " + a);
                    if (a.sqrMagnitude < 2f)
                    {
                        this.entityTarget = null;
                        this.entityTargetPos = Vector3.zero;
                        return false;
                    }
                    this.entityTargetPos = tempPosition;
                    DisplayLog(" my Position: " + this.theEntity.position + " Target Position: " + this.entityTargetPos + " My Leader position: " + this.entityTarget.position);
                    return true;
                }
            }
        }

        
        // We have a leader, but they are not within our range.
        if(this.theEntity.Buffs.HasCustomVar("Leader"))
        {
            EntityAlive entity = this.theEntity.world.GetEntity((int)(this.theEntity.Buffs.GetCustomVar("Leader"))) as EntityAlive;
            if(entity)
            {
                DisplayLog("My leader is out of sight. Teleporting to my leader");
                this.entityTarget = entity;
                SetCloseSpawnPoint();
                return true;

            }
        }
        DisplayLog(" No Entity To be Configured");
        this.entityTarget = null;
        return false;
    }

    public override bool CanExecute()
    {
        DisplayLog("CanExecute() Start");
        bool result = false;
    //    if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Follow);
            DisplayLog("CanExecute() Follow Task? " + result);
            // Since SetPatrol also uses this method, we'll add an extra check.
            if (result == false)
            {
                result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.SetPatrolPoint);
                DisplayLog(" CanExecute() Set Patrol Point? " + result);
            }

            if (!(this.theEntity is EntityAliveFarmingAnimalSDX)  &&  (result == false))
                return false;
        }

        // If there is an entity in bounds, then let this AI Task roceed. Otherwise, don't do anything with it.
        result = ConfigureTargetEntity();
        if (result)
            DisplayLog("CanExecute() Configure Target Result: " + result);

        DisplayLog("CanExecute() End: " + result);
        return result;

    }

    public override void Start()
    {
        this.entityTargetPos = this.entityTarget.position;
        this.entityTargetVel = Vector3.zero;
        this.isTargetToEat = false;
        this.theEntity.IsEating = false;
        this.pathCounter = 0;
    }

    public void SetCloseSpawnPoint()
    {
        Vector3 newPos = entityTarget.GetPosition();
        newPos.x += 2f;
        newPos.z += 2f;
        int x, y, z;
        this.theEntity.world.FindRandomSpawnPointNearPositionUnderground(entityTarget.position, 15, out x, out y, out z, new Vector3(2,2,2));
        //  this.theEntity.SetPosition( newPos, true);
        this.theEntity.SetPosition(new Vector3(x, y, z), true);
    }

  

    public override bool Continue()
    {
        DisplayLog("Continue() Start");

        bool result = false;
     //   if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Follow);
            if (result == false)
            {
                // Since SetPatrol also uses this method, we'll add an extra check.
                result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.SetPatrolPoint);
            }

            if (result == false)
                return false;
        }

        if (this.theEntity.getMoveHelper().BlockedTime > 2)
        {
            DisplayLog(" Blocked Time: " + this.theEntity.getMoveHelper().BlockedTime);

            DisplayLog("Entity is blocked. Resetting its move position");
            // If the npc seems lost, set a generate move to match the leader's position
            this.theEntity.getMoveHelper().SetMoveTo(this.entityTargetPos, false);
            
            return false;

        }
        if (pathCounter == 0) // briefly pause if you are at the end of the path to let other tasks run
            result = false;

        result = ConfigureTargetEntity();


        if(this.entityTarget == null)
        {
            if(this.theEntity.Buffs.HasCustomVar("Leader"))
            {
                DisplayLog(" Checking my Leader");
                // Find out who the leader is.
                int PlayerID = (int)this.theEntity.Buffs.GetCustomVar("Leader");
                float distance = this.theEntity.GetDistance( this.theEntity.world.GetEntity( PlayerID ));
                DisplayLog(" Distance: " + distance);
                if(distance > this.theEntity.GetSeeDistance())
                {
                    DisplayLog("I am too far away from my leader. Teleporting....");
                    SetCloseSpawnPoint();
                    DisplayLog(" my Position: " + this.theEntity.position + " Target Position: " + this.entityTargetPos + " My Leader position: " + this.entityTarget.position);
                }

            }
        }
        DisplayLog("Continue() End: " + result);
        return result;

    }

    public override void Update()
    {
        // No entity, so no need to do anything.
        if(this.entityTarget == null || this.entityTargetPos == null)
        {
            DisplayLog(" Entity Target or EntityTarget Position is null");
            return;
        }

        // Let the entity keep looking at you, otherwise it may just sping around.
        this.theEntity.SetLookPosition(this.entityTargetPos);
        this.theEntity.RotateTo(this.entityTargetPos.x, this.entityTargetPos.y + 2, this.entityTargetPos.z, 30f, 30f);

     //   if (entityAliveSDX)
        {
            if (EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.SetPatrolPoint))
            {
                // Make them a lot closer to you when they are following you.
                (this.theEntity as EntityAliveSDX).UpdatePatrolPoints(this.theEntity.world.FindSupportingBlockPos(this.entityTarget.position));
            }
        }
        Vector3 a = this.theEntity.position - this.entityTargetPos;
        if (a.sqrMagnitude < 2f)
        {
            DisplayLog("Entity is too close. Ending pathing.");
            this.pathCounter = 0;
            return;
        }
        this.theEntity.moveHelper.CalcIfUnreachablePos(this.entityTargetPos);

        // if the entity is not calculating a path, check how many nodes are left, and reset the path counter if its too low.
        if (!PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            PathEntity path = this.theEntity.navigator.getPath();
            if (path != null && path.NodeCountRemaining() <= 2)
                this.pathCounter = 0;
        }

        if (--this.pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            DisplayLog(" Path Counter is 0: Finding new path to " + this.entityTargetPos);
            // If its still not calculating a path, find a new path to the leader
            this.pathCounter = 6 + this.theEntity.GetRandom().Next(10);
            PathFinderThread.Instance.FindPath(this.theEntity, this.entityTargetPos, this.theEntity.GetMoveSpeedAggro(), true, this);
        }

        if (this.theEntity.Climbing)
            return;

        // If there's no path, calculate one.
        if(this.theEntity.navigator.noPathAndNotPlanningOne())
        {
            DisplayLog("No Path and Not Planning One. Searching for new path to : " + this.entityTargetPos);
            PathFinderThread.Instance.FindPath(this.theEntity, this.entityTargetPos, this.theEntity.GetMoveSpeedAggro(), true, this);
        }
    }
}
