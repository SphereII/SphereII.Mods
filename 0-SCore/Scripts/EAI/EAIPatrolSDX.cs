using GamePath;
using System.Collections.Generic;
using UnityEngine;

internal class EAIPatrolSDX : EAIApproachSpot
{
    private readonly bool blDisplayLog = false;

    private bool blReverse = true;

    private EntityAliveSDX entityAliveSDX;
    private List<Vector3> lstPatrolPoints = new List<Vector3>();
    private int pathRecalculateTicks;
    private int PatrolPointsCounter;

    // Controls the delay in between movements.
    private float PatrolSpeed = 2f;

    private Vector3 seekPos;


    private int taskTimeOut;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + theEntity.EntityName + ": " + strMessage);
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        var entityClass = EntityClass.list[_theEntity.entityClass];
        if (entityClass.Properties.Values.ContainsKey("PatrolSpeed"))
            PatrolSpeed = float.Parse(entityClass.Properties.Values["PatrolSpeed"]);

        entityAliveSDX = _theEntity as EntityAliveSDX;
    }


    public void SetPatrolVectors()
    {
        // this.PatrolPointsCounter = 0;
        DisplayLog(" Setting Up Patrol Vectors");
        if (entityAliveSDX)
        {
            if (lstPatrolPoints == entityAliveSDX.patrolCoordinates)
                return;

            DisplayLog(" Patrol Counters: " + entityAliveSDX.patrolCoordinates.Count);
            if (entityAliveSDX.patrolCoordinates.Count > 0)
            {
                DisplayLog(" Setting up Patrol Coordinates");
                lstPatrolPoints = entityAliveSDX.patrolCoordinates;
                PatrolPointsCounter = lstPatrolPoints.Count - 1;
                seekPos = lstPatrolPoints[PatrolPointsCounter];
            }
        }
    }

    public override bool CanExecute()
    {
        DisplayLog("CanExecute() Start");
        var result = false;
        if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Patrol);
            DisplayLog("CanExecute() Follow Task? " + result);
            if (result == false)
                return false;
        }

        // if The entity is busy, don't continue patrolling.
        var isBusy = false;
        if (theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return true;

        //if (!FetchOrders())
        //    result = false;

        SetPatrolVectors();

        if (entityAliveSDX.patrolCoordinates.Count > 0)
        {
            if (PatrolPointsCounter > entityAliveSDX.patrolCoordinates.Count - 1)
                PatrolPointsCounter = entityAliveSDX.patrolCoordinates.Count - 1;
        }
        else
        {
            return false;
        }

        theEntity.SetInvestigatePosition(lstPatrolPoints[PatrolPointsCounter], 1200);
        if (theEntity.HasInvestigatePosition)
        {
            DisplayLog(" I have an intesgation Position. Starting to Patrol");
            theEntity.emodel.avatarController.TriggerEvent("IsPatrolling");
            result = true;
        }

        DisplayLog("CanExecute() End: " + result);
        return result;
    }

    public override bool Continue()
    {
        if (++taskTimeOut > 40)
        {
            taskTimeOut = 0;
            return false;
        }

        // No order and no patrol. Do reverse ( != checks on these, rather than == as it can leave the entity imprecise.
        var result = false;
        if (entityAliveSDX)
            result = EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Patrol);

        if (lstPatrolPoints.Count <= 0)
        {
            DisplayLog(" Patrol Point Count is too low.");
            result = false;
        }

        // if The entity is busy, don't continue patrolling.
        var isBusy = false;
        if (theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return false;


        DisplayLog(" Continueing to Patrol");
        return result;
    }

    public void GetNextPosition()
    {
        if (PatrolPointsCounter >= lstPatrolPoints.Count - 1)
        {
            PatrolPointsCounter = lstPatrolPoints.Count - 1;
            blReverse = true;
        }

        if (PatrolPointsCounter == 0)
            blReverse = false;

        if (blReverse)
            PatrolPointsCounter--;
        else
            PatrolPointsCounter++;
        //this.PatrolPointsCounter = (this.PatrolPointsCounter + 1) % this.lstPatrolPoints.Count;


        DisplayLog(" Patrol Points Counter: " + PatrolPointsCounter + " Patrol Points Count: " + lstPatrolPoints.Count);
        DisplayLog(" Vector: " + lstPatrolPoints[PatrolPointsCounter]);

        seekPos = theEntity.world.FindSupportingBlockPos(lstPatrolPoints[PatrolPointsCounter]);
        theEntity.SetLookPosition(seekPos);

        theEntity.moveHelper.SetMoveTo(lstPatrolPoints[PatrolPointsCounter], false);
    }

    public override void Update()
    {
        var sqrMagnitude2 = (seekPos - theEntity.position).sqrMagnitude;

        if (sqrMagnitude2 <= 3f)
            GetNextPosition();

        updatePath();
    }

    public void updatePath()
    {
        if (theEntity.IsScoutZombie) AstarManager.Instance.AddLocationLine(theEntity.position, seekPos, 32);
        if (PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId)) return;
        pathRecalculateTicks = 40 + theEntity.rand.RandomRange(20);
        PathFinderThread.Instance.FindPath(theEntity, seekPos, theEntity.GetMoveSpeed(), true, this);
    }
}