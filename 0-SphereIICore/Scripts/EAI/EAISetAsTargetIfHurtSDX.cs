using System;
using UnityEngine;

class EAISetAsTargetIfHurtSDX : EAISetAsTargetIfHurt
{
    private readonly bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + theEntity.EntityName + ": " + theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        // If the Revenge Target is your leader, then forgive them?
        if (theEntity.GetRevengeTarget() != null)
        {
            Entity myLeader = EntityUtilities.GetLeaderOrOwner(theEntity.entityId);
            if (myLeader)
            {
                if (theEntity.GetRevengeTarget().entityId == myLeader.entityId)
                    return false;
                if (EntityUtilities.IsAnAlly(theEntity.entityId, theEntity.GetRevengeTarget().entityId))
                    return false;
            }
        }

        if (theEntity.GetAttackTarget() != null)
        {
            Entity myLeader = EntityUtilities.GetLeaderOrOwner(theEntity.entityId);
            if (myLeader)
            {
                if (theEntity.GetAttackTarget().entityId == myLeader.entityId)
                    return false;
                if (EntityUtilities.IsAnAlly(theEntity.entityId, theEntity.GetAttackTarget().entityId))
                    return false;
            }
        }
        return true;
    }

}

