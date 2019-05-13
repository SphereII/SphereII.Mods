using System;
using System.Collections.Generic;
using UnityEngine;

class EAIWanderSDX : EAIWander
    {
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }
    public bool FetchOrders( )
    {

        return true;
    }
    public override bool CanExecute()
    {
        DisplayLog("CanExecute() ");

        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && (this.theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Wander))
        {
            DisplayLog("CanExecuteTask(): Current Order does not match this order: Current Order:" + this.theEntity.Buffs.GetCustomVar("CurrentOrder") + " : Order Request: Wander");
            return false;
        }
        else
        {
            DisplayLog("CanExecuteTask(): Order is set for Wander");
        }
        return base.CanExecute();
    }

    public override bool Continue()
    {
        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && (this.theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Wander))
        {
            DisplayLog("Continue(): Current Order does not match this order: Current Order:" + this.theEntity.Buffs.GetCustomVar("CurrentOrder") + " : Order Request: Wander");
            return false;
        }

        // if an entity gets 'stuck' on a block, it just starts attacking it. Kind of aggressive.
        if (this.theEntity.moveHelper.BlockedTime <= 1f)
        {
            DisplayLog("Continue(): I am stuck for more than 1f");
            this.theEntity.navigator.clearPath();
            Vector3 headPosition = this.theEntity.getHeadPosition();
            Vector3 vector = this.theEntity.GetForwardVector();
            vector = Quaternion.Euler(UnityEngine.Random.value * 120f - 30f, UnityEngine.Random.value * 120f - 60f, 0f) * vector;
            this.theEntity.SetLookPosition(headPosition + vector);
            return false;
        }

        return base.Continue();
    }
}

