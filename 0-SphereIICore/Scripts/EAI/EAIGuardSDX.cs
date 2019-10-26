using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIGuardSDX : EAILook
    {

    float originalView;
    private bool hadPath;
    private int pathRecalculateTicks;

    private bool blDisplayLog = false;
    private int waitTicks;
    private int viewTicks;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }
    public bool FetchOrders( )
    {
        if(EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Stay))
            return false;

        if ( this.theEntity is EntityAliveSDX )
        {
            EntityAliveSDX temp = this.theEntity as EntityAliveSDX;
            float sqrMagnitude = (temp.GuardPosition - temp.position).sqrMagnitude;
            DisplayLog(" Magnitude from Guard " + temp.GuardPosition + " and Position " + temp.position + " is " + sqrMagnitude);
            if (sqrMagnitude > 1f)
            {
                DisplayLog(" Moving to my guard position ");
                this.updatePath( temp.GuardPosition);
               // this.theEntity.moveHelper.SetMoveTo(temp.GuardPosition, false);
                return true;
            }
        }

        originalView = this.theEntity.GetMaxViewAngle();
        this.theEntity.SetMaxViewAngle(180f);
        return true;
    }

    public override void Reset()
    {
        this.theEntity.SetLookPosition(Vector3.zero);
        if ( this.theEntity is EntityAliveSDX )
            this.theEntity.SetLookPosition((this.theEntity as EntityAliveSDX).GuardLookPosition);

        // Reset the view angle, and rotate it back to the original look vector.
        this.theEntity.SetMaxViewAngle(this.originalView);
      //  this.theEntity.RotateTo(this.theEntity.GetLookVector().x, this.theEntity.GetLookVector().y, this.theEntity.GetLookVector().z, 30f, 30f);

    }
    public override bool Continue()
    {
        if (!FetchOrders())
            return false;

        if (this.theEntity.bodyDamage.CurrentStun != global::EnumEntityStunType.None)
        {
            return false;
        }
        this.waitTicks--;
        if (this.waitTicks <= 0)
        {
            return false;
        }
        this.viewTicks--;
        if (this.viewTicks <= 0)
        {
            this.viewTicks = 40;
            Vector3 headPosition = this.theEntity.getHeadPosition();
            Vector3 vector = this.theEntity.GetForwardVector();
            vector = Quaternion.Euler(UnityEngine.Random.value * 60f - 30f, UnityEngine.Random.value * 120f - 60f, 0f) * vector;
            this.theEntity.SetLookPosition(headPosition + vector);
            return false; // cut it short.
        }
        return true;
    }
    private void updatePath( Vector3 GuardPosition)
    {
        if (PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            return;
        }
        this.pathRecalculateTicks = 20 + this.theEntity.rand.RandomRange(20);
        PathFinderThread.Instance.FindPath(this.theEntity, GuardPosition, this.theEntity.GetMoveSpeedAggro(), true, this);
    }
    public override bool CanExecute()
    {
        if (!FetchOrders())
            return false;

        return base.CanExecute();
    }

  
}

