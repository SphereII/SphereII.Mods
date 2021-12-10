using System.Collections.Generic;
using UnityEngine;

internal class EAISetAsTargetIfLeaderAttackedSDX : EAISetAsTargetIfHurt
{
    private readonly bool blDisplayLog = true;
    private readonly List<Entity> NearbyEntities = new List<Entity>();
    private EntityAlive targetEntity;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + theEntity.EntityName + ": " + theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        DisplayLog(" CanExecute() ");
        var result = false;
        if (CheckSurroundingEntities())
        {
            DisplayLog(" CheckSurroundEntities is true for an enemy");
            result = true;
        }

        return result;
    }

    public bool CheckSurroundingEntities()
    {
        NearbyEntities.Clear();

        // If we don't have a leader, we don't need to scan.
        var leader = EntityUtilities.GetLeaderOrOwner(theEntity.entityId) as EntityAlive;
        if (!leader)
            return false;

        // Search in the bounds are to try to find the most appealing entity to follow.
        var bb = new Bounds(theEntity.position, new Vector3(theEntity.GetSeeDistance(), 20f, theEntity.GetSeeDistance()));

        theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, NearbyEntities);
        DisplayLog(" Nearby Entities: " + NearbyEntities.Count);
        for (var i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            var x = (EntityAlive)NearbyEntities[i];
            if (x != theEntity)
            {
                if (x.IsDead())
                    continue;

                DisplayLog("Nearby Entity: " + x.EntityName);
                if (x.GetAttackTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked by " + x);
                    targetEntity = x;
                    theEntity.SetRevengeTarget(targetEntity);
                    return true;
                }

                if (x.GetRevengeTarget() == leader)
                {
                    DisplayLog(" My leader is being avenged by " + x);
                    targetEntity = x;
                    theEntity.SetRevengeTarget(targetEntity);

                    return true;
                }

                if (x.GetDamagedTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked by something that damaged it " + x);
                    targetEntity = x;
                    theEntity.SetRevengeTarget(targetEntity);

                    return true;
                }
            }
        }

        return false;
    }
}