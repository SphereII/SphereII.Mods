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
            if (this.theEntity.Buffs.HasCustomVar("Leader") &&  (int)this.theEntity.Buffs.GetCustomVar("Leader") == this.theEntity.GetRevengeTarget().entityId )
            {
                DisplayLog(" My Revenge Target is my leader. Ignoring this for now...");
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

