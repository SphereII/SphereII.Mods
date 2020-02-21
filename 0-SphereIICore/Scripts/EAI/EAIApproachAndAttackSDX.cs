using System;
using UnityEngine;

//<property name="AITask-5" value="ApproachAndAttackTargetSDX, Mods" param1="" param2=""  /> <!-- param1 not used -->
// Disables the Eating animation
class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    private bool isTargetToEat = false;
  // // private Vector3 entityTargetPos;
  // // public EntityAlive entityTarget;
  ////  private Vector3 entityTargetVel;
  //  private bool isGoingHome;

  //  private float homeTimeout;
  //  private bool hasHome;
  //  private bool isEating;
  //  private int pathCounter;
  //  private int attackTimeout;
  //  private int relocateTicks;
  //  private float maxChaseTime;

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
                // If the entity is dead, don't hover over it.
                // this.theEntity.inventory.SetHoldingItemIdx(0);
                return false;
            }

            EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Melee);
        }

        
        return result;
    }

 
    // If the entity is closer than 4 blocks, return true, allowing the approach and attack target to start.
    private bool InRange()
    {
        float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
        return distanceSq < 2f;
    }
 
}

