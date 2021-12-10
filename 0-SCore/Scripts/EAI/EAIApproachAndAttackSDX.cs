using UnityEngine;

//<property name="AITask-5" value="ApproachAndAttackSDX, SCore" param1="" param2=""  /> <!-- param1 not used -->
// Disables the Eating animation
internal class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    private readonly bool blDisplayLog = false;

    public void DisplayLog(string strMessage)
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
        // We are ordered to stay at all costs.
        //        if (EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Stay))
        //          return false;

        // Check to see if we have a target.
        var entityTarget = EntityUtilities.GetAttackOrRevengeTarget(theEntity.entityId) as EntityAlive;
        if (entityTarget == null)
            return base.CanExecute();

        // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
        if (EntityUtilities.HasTask(theEntity.entityId, "Ranged") && EntityUtilities.CheckAIRange(theEntity.entityId, entityTarget.entityId))
            return false;

        return base.CanExecute();
    }

    public override bool Continue()
    {
        if (theEntity.sleepingOrWakingUp || theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
            return false;

        var entityTarget = EntityUtilities.GetAttackOrRevengeTarget(theEntity.entityId) as EntityAlive;
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

        return true;
    }
}