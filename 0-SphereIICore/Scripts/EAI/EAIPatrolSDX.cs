using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;
class EAIPatrolSDX : EAIApproachSpot
{
    private List<Vector3> lstPatrolPoints = new List<Vector3>();
    private int PatrolPointsCounter = 0;

    private float nextCheck = 0;
    private Vector3 seekPos;
    private int pathRecalculateTicks;

    private EntityAliveSDX entityAliveSDX;

    // Controls the delay in between movements.
    private float PatrolSpeed = 2f;


    private int taskTimeOut = 0;
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + strMessage);
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        EntityClass entityClass = EntityClass.list[_theEntity.entityClass];
        if (entityClass.Properties.Values.ContainsKey("PatrolSpeed"))
            this.PatrolSpeed = float.Parse(entityClass.Properties.Values["PatrolSpeed"]);

        entityAliveSDX = (_theEntity as EntityAliveSDX);
    }
 

    public void SetPatrolVectors()
    {
        // this.PatrolPointsCounter = 0;
        DisplayLog(" Setting Up Patrol Vectors");
        if (entityAliveSDX)
        {
            if (this.lstPatrolPoints == entityAliveSDX.PatrolCoordinates)
                return;

            DisplayLog(" Patrol Counters: " + entityAliveSDX.PatrolCoordinates.Count);
            if (entityAliveSDX.PatrolCoordinates.Count > 0)
            {
                DisplayLog(" Setting up Patrol Coordinates");
                this.lstPatrolPoints = entityAliveSDX.PatrolCoordinates;
                PatrolPointsCounter = this.lstPatrolPoints.Count - 1;
                this.seekPos = this.lstPatrolPoints[PatrolPointsCounter];
            }
        }
    }

    public override bool CanExecute()
    {
        DisplayLog("CanExecute() Start");
        bool result = false;
        if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Patrol);
            DisplayLog("CanExecute() Follow Task? " + result);
            if (result == false)
                return false;
        }

        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return true;

        //if (!FetchOrders())
        //    result = false;

        SetPatrolVectors();

        if (entityAliveSDX.PatrolCoordinates.Count > 0)
        {

            if (PatrolPointsCounter > entityAliveSDX.PatrolCoordinates.Count - 1)
                PatrolPointsCounter = entityAliveSDX.PatrolCoordinates.Count - 1;
        }
        else
            return false;

        this.theEntity.SetInvestigatePosition(this.lstPatrolPoints[PatrolPointsCounter], 1200);
        if (this.theEntity.HasInvestigatePosition)
        {
            DisplayLog(" I have an intesgation Position. Starting to Patrol");
            this.theEntity.emodel.avatarController.SetTrigger("IsPatrolling");
            result = true;
        }

        DisplayLog("CanExecute() End: " + result);
        return result;
    }

    public override bool Continue()
    {

        if(++this.taskTimeOut > 40)
        {
            this.taskTimeOut = 0;
            return false;
        }
            // No order and no patrol. Do reverse ( != checks on these, rather than == as it can leave the entity imprecise.
            bool result = false;
        if (entityAliveSDX)
            result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Patrol);

        if (this.lstPatrolPoints.Count <= 0)
        {
            DisplayLog(" Patrol Point Count is too low.");
            result = false;
        }
   
        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return false;

      
        DisplayLog(" Continueing to Patrol");
        return result;
    }

    bool blReverse = true;

    public void GetNextPosition()
    {
        if (this.PatrolPointsCounter >= this.lstPatrolPoints.Count - 1)
        {
            this.PatrolPointsCounter = this.lstPatrolPoints.Count - 1;
            blReverse = true;
        }
        if (this.PatrolPointsCounter == 0)
            blReverse = false;

        if (blReverse)
            this.PatrolPointsCounter--;
        else
            this.PatrolPointsCounter++;
        //this.PatrolPointsCounter = (this.PatrolPointsCounter + 1) % this.lstPatrolPoints.Count;

        

        DisplayLog(" Patrol Points Counter: " + PatrolPointsCounter + " Patrol Points Count: " + this.lstPatrolPoints.Count);
        DisplayLog(" Vector: " + this.lstPatrolPoints[PatrolPointsCounter].ToString());

        this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);
        this.theEntity.SetLookPosition(this.seekPos);

        this.theEntity.moveHelper.SetMoveTo(this.lstPatrolPoints[PatrolPointsCounter], false);

    }
    public override void Update()
    {
        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
       
        if (sqrMagnitude2 <= 3f)
            GetNextPosition();

        updatePath();
    }

    public void updatePath()
    {
        if (this.theEntity.IsScoutZombie)
        {
            AstarManager.Instance.AddLocationLine(this.theEntity.position, this.seekPos, 32);
        }
        if (GamePath.PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            return;
        }
        this.pathRecalculateTicks = 40 + this.theEntity.rand.RandomRange(20);
        GamePath.PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeed(), true, this);
    }
}

