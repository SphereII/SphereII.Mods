using System;
using System.Collections.Generic;
using UnityEngine;

class EAISetAsTargetIfLeaderAttackedSDX : EAISetAsTargetIfHurt
{
    private List<Entity> NearbyEntities = new List<Entity>();

    private bool blDisplayLog = false;
    private EntityAlive targetEntity;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        DisplayLog(" CanExecute() ");
        bool result = false; 
        if (CheckSurroundingEntities())
        {
            DisplayLog(" CheckSurroundEntities is true for an enemy");
            result = true;
        }
        return result;
    }

    public bool CheckSurroundingEntities()
    {
        this.NearbyEntities.Clear();
        EntityAlive leader = EntityUtilities.GetLeaderOrOwner( this.theEntity.entityId ) as EntityAlive;
        if(!leader)
            return false;

        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(this.theEntity.GetSeeDistance(), 20f, this.theEntity.GetSeeDistance()));
        //Bounds bb = new Bounds(this.theEntity.position, new Vector3(20f, 20f,20f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        DisplayLog(" Nearby Entities: " + this.NearbyEntities.Count);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity)
            {
                if (x.IsDead())
                    continue;
                    
                DisplayLog("Nearby Entity: " + x.EntityName);
                if (x.GetAttackTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked by " + x.ToString());
                    targetEntity = x;
                    this.theEntity.SetRevengeTarget(targetEntity);
                    return true;
                }

                if (x.GetRevengeTarget() == leader)
                {
                    DisplayLog(" My leader is being avenged by " + x.ToString());
                    targetEntity = x;
                    this.theEntity.SetRevengeTarget(targetEntity);

                    return true;
                }

                if (x.GetDamagedTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked by something that damaged it " + x.ToString());
                    targetEntity = x;
                    this.theEntity.SetRevengeTarget(targetEntity);

                    return true;
                }
            }
        }

        return false;
    }
}

