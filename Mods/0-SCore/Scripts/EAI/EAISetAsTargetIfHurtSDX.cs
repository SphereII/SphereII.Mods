using UnityEngine;

internal class EAISetAsTargetIfHurtSDX : EAISetAsTargetIfHurt
{
    private readonly bool blDisplayLog = true;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + theEntity.EntityName + ": " + theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {

        // Check if we have a target.
        var entity = EntityUtilities.GetAttackOrRevengeTarget(theEntity.entityId);
        if (entity == null)
            return false;

        // Check if our target is our leader, or part of our team.
        var myLeader = EntityUtilities.GetLeaderOrOwner(theEntity.entityId);
        if (myLeader)
        {
            if (entity.entityId == myLeader.entityId)
                return false;
            if (EntityUtilities.IsAnAlly(theEntity.entityId, entity.entityId))
                return false;
        }


        // We are good to go. We can fight back.
        return true;
    }
}