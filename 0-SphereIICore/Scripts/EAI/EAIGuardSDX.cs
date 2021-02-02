using GamePath;
using System;
using UnityEngine;

class EAIGuardSDX : EAILook
{

    float originalView;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by base class")]
    private int pathRecalculateTicks;

    private readonly bool blDisplayLog = false;
    private int waitTicks;
    private int viewTicks;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(theEntity.EntityName + ": " + strMessage);
    }
    public bool FetchOrders()
    {
        if (!EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Stay))
            return false;

        if (theEntity is EntityAliveSDX)
        {
            EntityAliveSDX temp = theEntity as EntityAliveSDX;
            float sqrMagnitude = (temp.GuardPosition - temp.position).sqrMagnitude;
            DisplayLog(" Magnitude from Guard " + temp.GuardPosition + " and Position " + temp.position + " is " + sqrMagnitude);
            if (sqrMagnitude > 1f)
            {
                DisplayLog(" Moving to my guard position ");
                updatePath(temp.GuardPosition);
                // this.theEntity.moveHelper.SetMoveTo(temp.GuardPosition, false);
                return true;
            }
        }

        originalView = theEntity.GetMaxViewAngle();
        theEntity.SetMaxViewAngle(180f);
        return true;
    }

    public override void Reset()
    {
        theEntity.SetLookPosition(Vector3.zero);
        if (theEntity is EntityAliveSDX)
            theEntity.SetLookPosition((theEntity as EntityAliveSDX).GuardLookPosition);

        // Reset the view angle, and rotate it back to the original look vector.
        theEntity.SetMaxViewAngle(originalView);
        //  this.theEntity.RotateTo(this.theEntity.GetLookVector().x, this.theEntity.GetLookVector().y, this.theEntity.GetLookVector().z, 30f, 30f);

    }
    public override bool Continue()
    {
        if (!FetchOrders())
            return false;

        if (theEntity.bodyDamage.CurrentStun != global::EnumEntityStunType.None)
        {
            return false;
        }
        waitTicks--;
        if (waitTicks <= 0)
        {
            return false;
        }
        viewTicks--;
        if (viewTicks <= 0)
        {
            viewTicks = 40;
            Vector3 headPosition = theEntity.getHeadPosition();
            Vector3 vector = theEntity.GetForwardVector();
            vector = Quaternion.Euler(UnityEngine.Random.value * 60f - 30f, UnityEngine.Random.value * 120f - 60f, 0f) * vector;
            theEntity.SetLookPosition(headPosition + vector);
            return false; // cut it short.
        }
        return true;
    }
    private void updatePath(Vector3 GuardPosition)
    {
        if (PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
        {
            return;
        }
        pathRecalculateTicks = 20 + theEntity.rand.RandomRange(20);
        PathFinderThread.Instance.FindPath(theEntity, GuardPosition, theEntity.GetMoveSpeedAggro(), true, this);
    }
    public override bool CanExecute()
    {
        if (!FetchOrders())
            return false;

        return base.CanExecute();
    }


}

