using System;
using UnityEngine;

//<property name="AITask-5" value="ApproachAndAttackTargetSDX, Mods" param1="" param2=""  /> <!-- param1 not used -->
// Disables the Eating animation
class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    private bool isTargetToEat = false;
   // private Vector3 entityTargetPos;
   // public EntityAlive entityTarget;
  //  private Vector3 entityTargetVel;
    private bool isGoingHome;

    private float homeTimeout;
    private bool hasHome;
    private bool isEating;
    private int pathCounter;
    private int attackTimeout;
    private int relocateTicks;
    private float maxChaseTime;

    private bool blDisplayLog = false;
   // private EntityAlive entityTarget;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        bool result = base.CanExecute();
        
        if(result && this.entityTarget != null )
        {
            this.theEntity.SetLookPosition(this.entityTarget.getHeadPosition());
            this.theEntity.RotateTo(this.entityTarget, 30f, 30f);

            DisplayLog(" Has Task: " + EntityUtilities.HasTask(this.theEntity.entityId, "Ranged"));
            DisplayLog(" Is In Range: " + InRange());
            // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
            if(EntityUtilities.HasTask(this.theEntity.entityId, "Ranged") && !InRange())
            {
 
                DisplayLog(" Has Ranged Attack. Not Moving forward.");
                result = false;
            }
        }

        return result;
    }

    // If the entity is closer than 4 blocks, return true, allowing the approach and attack target to start.
    private bool InRange()
    {
        float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
        return distanceSq < 4f;
    }
    //public override void Start()
    //{
    //    this.entityTarget = this.theEntity.GetAttackTarget();
    //    this.entityTargetPos = this.entityTarget.position;
    //    this.entityTargetVel = Vector3.zero;
    //    this.isTargetToEat = false;
    //    this.isEating = false;
    //    this.theEntity.IsEating = false;
    //    this.homeTimeout = ((!this.theEntity.IsSleeper) ? this.maxChaseTime : 90f);
    //    this.hasHome = (this.homeTimeout > 0f);
    //    this.isGoingHome = false;
    //    if (this.theEntity.ChaseReturnLocation == Vector3.zero)
    //        this.theEntity.ChaseReturnLocation = ((!this.theEntity.IsSleeper) ? this.theEntity.position : this.theEntity.SleeperSpawnPosition);

    //    this.pathCounter = 0;
    //    this.relocateTicks = 0;

    //    this.attackTimeout = 5;
    //}

    //public override bool CanExecute()
    //{
    //    DisplayLog("CanExecute()");
    //    this.entityTarget = this.theEntity.GetAttackTarget();
    //    if (this.entityTarget == null )
    //    {
    //        this.entityTarget = this.theEntity.GetRevengeTarget();
    //        if ( this.entityTarget == null )
    //            return false;
    //    }
    //    if (!this.entityTarget.IsAlive())
    //    {
    //        DisplayLog(" My Target Entity is dead.");
    //        return false;
    //    }
    //    return true;// base.CanExecute();
    //}
    public override bool Continue()
    {
        if(this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != global::EnumEntityStunType.None)
        {
            return false;
        }
        EntityAlive attackTarget = this.theEntity.GetAttackTarget();

        if(attackTarget)
            if(!attackTarget.IsAlive())
                return false;

        if(this.isGoingHome)
        {
            return !attackTarget && this.theEntity.ChaseReturnLocation != Vector3.zero;
        }
        return attackTarget && !(attackTarget != this.entityTarget);
    }
}

