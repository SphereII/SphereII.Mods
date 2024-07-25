using GamePath;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

internal class EAIGuardSDX : EAILook
{
    private readonly bool blDisplayLog = false;

    private float originalView;

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by base class")]
    private int pathRecalculateTicks;

    private int viewTicks;
    private int waitTicks;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(theEntity.EntityName + ": " + strMessage);
    }

    public bool FetchOrders()
    {
        if (!EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Stay))
            return false;

        if (theEntity is IEntityOrderReceiverSDX temp)
        {
            var sqrMagnitude = (temp.GuardPosition - temp.Position).sqrMagnitude;
            DisplayLog(" Magnitude from Guard " + temp.GuardPosition + " and Position " + temp.Position + " is " + sqrMagnitude);
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
            theEntity.SetLookPosition((theEntity as EntityAliveSDX).guardLookPosition);

        // Reset the view angle, and rotate it back to the original look vector.
        theEntity.SetMaxViewAngle(originalView);
        //  this.theEntity.RotateTo(this.theEntity.GetLookVector().x, this.theEntity.GetLookVector().y, this.theEntity.GetLookVector().z, 30f, 30f);
    }

    public override bool Continue()
    {
        if (!FetchOrders())
            return false;

        if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None) return false;
        waitTicks--;
        if (waitTicks <= 0) return false;
        viewTicks--;
        if (viewTicks <= 0)
        {
            viewTicks = 40;
            var headPosition = theEntity.getHeadPosition();
            var vector = theEntity.GetForwardVector();
            vector = Quaternion.Euler(UnityEngine.Random.value * 60f - 30f, UnityEngine.Random.value * 120f - 60f, 0f) * vector;
            theEntity.SetLookPosition(headPosition + vector);
            return false; // cut it short.
        }

        return true;
    }

    private void updatePath(Vector3 GuardPosition)
    {
        if (PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId)) return;
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