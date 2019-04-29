using System;
using System.Collections.Generic;
using System.Globalization;
using GamePath;
using UnityEngine;

public class EAIApproachAndFollowTargetSDX : EAIApproachAndAttackTarget
{
    List<String> lstIncentives = new List<String>();
    private List<Entity> NearbyEntities = new List<Entity>();
    float distanceToEntity = UnityEngine.Random.Range(2f, 5.0f);

    private Vector3 entityTargetPos;
    private Vector3 entityTargetVel;
    private int pathCounter;

    public EntityAliveSDX entityAliveSDX;

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
        entityAliveSDX = (_theEntity as EntityAliveSDX);
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
        if (this.entityTarget != null)
            return true;

        this.NearbyEntities.Clear();

        if (this.entityAliveSDX == null)
        {
            if (this.theEntity is EntityAliveSDX)
                this.entityAliveSDX = (this.theEntity as EntityAliveSDX);
            else
            {
                DisplayLog(" Not an EntityAliveSDX");
                return false;
            }
        }
        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(30f, 20f, 30f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity)
            {

                // Check the entity against the incentives
                if (entityAliveSDX.CheckIncentive(this.lstIncentives, x))
                {
                    this.entityTarget = x;
                    return true;
                }
            }
        }

        this.entityTarget = null;
        return false;
    }

    public override bool CanExecute()
    {
        DisplayLog("CanExecute() Start");
        bool result = false;
        if (entityAliveSDX)
        {
            result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.Follow);
            DisplayLog("CanExecute() Follow Task? " + result);
            // Since SetPatrol also uses this method, we'll add an extra check.
            if (result == false)
            {
                result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.SetPatrolPoint);
                DisplayLog(" CanExecute() Set Patrol Point? " + result);
            }
        }
        if (!this.theEntity.Buffs.HasCustomVar("Leader"))
        {
            DisplayLog("CanExecute() No Leader");
            result = false;
        }
        // Change the distance allowed each time. This will give it more of a variety in how close it can get to you.
        distanceToEntity = UnityEngine.Random.Range(2f, 5.0f);

        // If there is an entity in bounds, then let this AI Task roceed. Otherwise, don't do anything with it.
        if (result)
        {
            result = ConfigureTargetEntity();
            DisplayLog("CanExecute() Configure Target Result: " + result);
        }

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

    public override bool Continue()
    {
        DisplayLog("Continue() Start");
        bool result = false;
        if (entityAliveSDX)
        {
            result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.Follow);
            if (result == false)
            {
                // Since SetPatrol also uses this method, we'll add an extra check.
                result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.SetPatrolPoint);
            }
        }

        if (this.theEntity.getMoveHelper().BlockedTime > 2)
        {
            DisplayLog(" Blocked Time: " + this.theEntity.getMoveHelper().BlockedTime);

            // If the npc seems lost, set a generate move to match the leader's position
            this.theEntity.getMoveHelper().SetMoveTo(this.entityTarget.position, false);
            return false;

        }
        if (pathCounter == 0) // briefly pause if you are at the end of the path to let other tasks run
            result = false;

        if (this.theEntity.Buffs.HasCustomVar("Leader"))
            if ((int)this.theEntity.Buffs.GetCustomVar("Leader") == 0)
                result = false;
            else
                result = false;

        if (result)
            result = ConfigureTargetEntity();

        DisplayLog("Continue() End: " + result);
        return result;

    }

    public float GetTargetXZDistanceSq(int estimatedTicks)
    {
        Vector3 vector = this.entityTarget.position;
        vector += this.entityTargetVel * (float)estimatedTicks;
        if (this.isTargetToEat)
        {
            EModelBase emodel = this.entityTarget.emodel;
            if (emodel && emodel.bipedPelvisTransform)
            {
                vector = emodel.bipedPelvisTransform.position + Origin.position;
            }
        }
        Vector3 vector2 = this.theEntity.position + this.theEntity.motion * (float)estimatedTicks - vector;
        vector2.y = 0f;
        return vector2.sqrMagnitude;
    }

    public override void Update()
    {
        Vector3 position = Vector3.zero;
        float targetXZDistanceSq = 0f;

        // No entity, so no need to do anything.
        if (this.entityTarget == null)
            return;

        // Let the entity keep looking at you, otherwise it may just sping around.
        this.theEntity.SetLookPosition(this.entityTarget.getHeadPosition());
        this.theEntity.RotateTo(this.entityTarget.position.x, this.entityTarget.position.y + 2, this.entityTarget.position.z, 30f, 30f);

        // Find the location of the entity, and figure out where it's at.
        position = this.entityTarget.position;
        targetXZDistanceSq = GetTargetXZDistanceSq(6);

        if (entityAliveSDX)
        {
            if (entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.SetPatrolPoint))
            {
                // Make them a lot closer to you when they are following you.
                this.distanceToEntity = 1f;
                entityAliveSDX.UpdatePatrolPoints(this.theEntity.world.FindSupportingBlockPos(this.entityTarget.position));
            }
        }
        Vector3 a = position - this.entityTargetPos;
        if (a.sqrMagnitude < 1f)
            this.entityTargetVel = this.entityTargetVel * 0.7f + a * 0.3f;

        this.entityTargetPos = position;
        this.theEntity.moveHelper.CalcIfUnreachablePos(position);
        // num is used to determine how close and comfortable the entity approaches you, so let's make sure they respect some personal space
        if (distanceToEntity < 1)
            distanceToEntity = 3;


        // if the entity is not calculating a path, check how many nodes are left, and reset the path counter if its too low.
        if (!PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            PathEntity path = this.theEntity.navigator.getPath();
            if (path != null && path.NodeCountRemaining() <= 2)
                this.pathCounter = 0;
        }

        if (--this.pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            // If its still not calculating a path, find a new path to the leader
            this.pathCounter = 6 + this.theEntity.GetRandom().Next(10);
            PathFinderThread.Instance.FindPath(this.theEntity, this.entityTarget.position, this.theEntity.GetMoveSpeedAggro(), true, this);
        }

        if (this.theEntity.Climbing)
            return;

        // If there's no path, calculate one.
        if (this.theEntity.navigator.noPathAndNotPlanningOne())
            PathFinderThread.Instance.FindPath(this.theEntity, this.entityTarget.position, this.theEntity.GetMoveSpeedAggro(), true, this);
    }
}
