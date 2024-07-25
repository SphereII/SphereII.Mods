using System.Collections.Generic;
using UnityEngine;

internal static class SphereII_DMTEAIUtilities
{
    private static readonly bool blDisplayLog = false;

    public static void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(strMessage);
    }

    public static bool CheckFactionForEnemy(EntityAlive theEntity, EntityAlive Entity)
    {
        if (theEntity.factionId == Entity.factionId)
            return false;

        var myRelationship = FactionManager.Instance.GetRelationshipTier(theEntity, Entity);
        DisplayLog(" CheckFactionForEnemy: " + myRelationship);
        if (myRelationship == FactionManager.Relationship.Hate)
        {
            DisplayLog(" I hate this entity: " + Entity);
            return true;
        }

        DisplayLog(" My relationship with this " + Entity + " is: " + myRelationship);

        return false;
    }

    public static bool CheckSurroundingEntities(EntityAlive theEntity)
    {
        var NearbyEntities = new List<Entity>();
        var NearbyEnemies = new List<Entity>();
        var leader = EntityUtilities.GetLeaderOrOwner(theEntity.entityId) as EntityAlive;


        var originalView = theEntity.GetMaxViewAngle();
        theEntity.SetMaxViewAngle(250f);

        // Search in the bounds are to try to find the most appealing entity to follow.
        var bb = new Bounds(theEntity.position, new Vector3(20f, 20f, 20f));
        theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, NearbyEntities);
        DisplayLog(" Nearby Entities: " + NearbyEntities.Count);
        for (var i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            var x = (EntityAlive)NearbyEntities[i];
            if (x is EntityVehicle)
                continue;

            if (x != theEntity && x.IsAlive())
            {
                if (leader != null && x == leader)
                    continue;

                if (x.CanSee(theEntity.position))
                {
                    DisplayLog(" I can be seen by an enemy.");
                    if (!theEntity.CanSee(x.position))
                    {
                        DisplayLog(" I know an entity is there, but I can't see it: " + x.EntityName);
                        continue;
                    }
                }

                DisplayLog("Nearby Entity: " + x.EntityName);
                if (CheckFactionForEnemy(theEntity, x))
                    NearbyEnemies.Add(x);
            }
        }

        theEntity.SetMaxViewAngle(originalView);
        return NearestEnemy(theEntity, NearbyEnemies);
    }

    public static bool NearestEnemy(EntityAlive theEntity, List<Entity> NearbyEnemies)
    {
        if (NearbyEnemies.Count == 0)
            return false;

        // Finds the closet block we matched with.
        EntityAlive closeEnemy = null;
        var minDist = Mathf.Infinity;
        var currentPos = theEntity.position;
        foreach (EntityAlive enemy in NearbyEnemies)
        {
            var dist = Vector3.Distance(enemy.position, currentPos);
            DisplayLog(" Entity: " + enemy.EntityName + "'s distance is: " + dist);
            if (dist < minDist)
            {
                closeEnemy = enemy;
                minDist = dist;
            }
        }

        if (closeEnemy != null)
        {
            DisplayLog(" Closes Enemy: " + closeEnemy);
            theEntity.SetRevengeTarget(closeEnemy);
            return true;
        }

        return false;
    }
}