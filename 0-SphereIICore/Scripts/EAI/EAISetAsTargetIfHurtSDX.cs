using System;
using System.Collections.Generic;
using UnityEngine;

class EAISetAsTargetIfHurtSDX : EAISetAsTargetIfHurt
{
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log( this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }
   
    public override bool CanExecute()
    {
        // If the Revenge Target is your leader, then forgive them?
        if (this.theEntity.GetRevengeTarget() != null)
        {
            Entity myLeader = EntityUtilities.GetLeaderOrOwner(this.theEntity.entityId);
            if(myLeader)
            {
                if(this.theEntity.GetRevengeTarget().entityId == myLeader.entityId)
                    return false;
            }
       

            return true;
        }
        else
            return false;
        ////bool result = base.CanExecute();
        //DisplayLog(" Result of CanExecute(): " + result);
        //return result;
    }

}

