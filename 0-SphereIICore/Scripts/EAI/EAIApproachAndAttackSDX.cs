using System;
using UnityEngine;

//<property name="AITask-5" value="ApproachAndAttackSDX, Mods" param1="" param2=""  /> <!-- param1 not used -->
// Disables the Eating animation
class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    private bool isTargetToEat = false;

    private bool blDisplayLog = false;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }
    public override bool CanExecute()
    {
        bool result = base.CanExecute();

        if (result && this.entityTarget != null)
        {
            if (EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Stay))
                return false;

            this.theEntity.SetLookPosition(this.entityTarget.getHeadPosition());
            this.theEntity.RotateTo(this.entityTarget, 30f, 30f);

            DisplayLog(" Has Task: " + EntityUtilities.HasTask(this.theEntity.entityId, "Ranged"));

            // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
            if (EntityUtilities.HasTask(this.theEntity.entityId, "Ranged"))
            {
                if (result)
                    result = !EntityUtilities.CheckAIRange(theEntity.entityId, entityTarget.entityId);

            }

            if ( result )
                EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Melee);
        }
        return result;
    }

    public override bool Continue()
    {
        bool result = base.Continue();

        // Non zombies should continue to attack
        if (this.entityTarget.IsDead())
        {
            this.theEntity.IsEating = false;
            this.theEntity.SetAttackTarget(null, 0);
            this.theEntity.SetRevengeTarget(null);
            EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Reset);

            return false;
        }

        if (result)
        {
            // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
            if (EntityUtilities.HasTask(this.theEntity.entityId, "Ranged"))
                    return !EntityUtilities.CheckAIRange(theEntity.entityId, entityTarget.entityId);
        }

        return result;
    }


}

