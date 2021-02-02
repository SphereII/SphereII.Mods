using System;
using UnityEngine;

//<property name="AITask-5" value="ApproachAndAttackSDX, Mods" param1="" param2=""  /> <!-- param1 not used -->
// Disables the Eating animation
class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    public readonly bool isTargetToEat = false;

    private readonly bool blDisplayLog = false;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + theEntity.EntityName + ": " + theEntity.entityId + ": " + strMessage);
    }


    public override void Start()
    {
        base.Start();
        EntityUtilities.ChangeHandholdItem(theEntity.entityId, EntityUtilities.Need.Melee);

    }
    public override bool CanExecute()
    {
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || (this.theEntity.Jumping && !this.theEntity.isSwimming))
            return false;


        this.entityTarget = EntityUtilities.GetAttackOrReventTarget(this.theEntity.entityId) as EntityAlive;
        if (this.entityTarget == null)
            return false;

        if (EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Stay))
            return false;

        theEntity.SetLookPosition(entityTarget.getHeadPosition());
        theEntity.RotateTo(entityTarget, 30f, 30f);

        DisplayLog(" Has Task: " + EntityUtilities.HasTask(theEntity.entityId, "Ranged"));

        // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
        if (EntityUtilities.HasTask(theEntity.entityId, "Ranged") && EntityUtilities.CheckAIRange(theEntity.entityId, entityTarget.entityId))
            return false;

        return true;
    }

    public override bool Continue()
    {
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
            return false;

        if (entityTarget == null)
            return false;

        // Non zombies should continue to attack
        if (entityTarget.IsDead())
        {
            theEntity.IsEating = false;
            theEntity.SetAttackTarget(null, 0);
            theEntity.SetRevengeTarget(null);
            return false;
        }

        // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
        if (EntityUtilities.HasTask(theEntity.entityId, "Ranged") && EntityUtilities.CheckAIRange(theEntity.entityId, entityTarget.entityId))
            return false;

        EntityUtilities.ChangeHandholdItem(theEntity.entityId, EntityUtilities.Need.Melee);


        return true;
    }


}

