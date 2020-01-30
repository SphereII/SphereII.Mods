using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

static class SphereII_DMTEAIUtilities
{
    private static bool blDisplayLog = false;
    public static void DisplayLog(String strMessage)
    {
        if(blDisplayLog)
            UnityEngine.Debug.Log( strMessage);
    }

    public static bool CheckFactionForEnemy(EntityAlive theEntity, EntityAlive Entity)
    {
        if(theEntity.factionId == Entity.factionId)
            return false;

        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(theEntity, Entity);
        DisplayLog(" CheckFactionForEnemy: " + myRelationship.ToString());
        if(myRelationship == FactionManager.Relationship.Hate)
        {
            DisplayLog(" I hate this entity: " + Entity.ToString());
            return true;
        }
        else
            DisplayLog(" My relationship with this " + Entity.ToString() + " is: " + myRelationship.ToString());
        return false;
    }

    public static bool CheckSurroundingEntities( EntityAlive theEntity)
    {

           List<Entity> NearbyEntities = new List<Entity>();
        List<Entity> NearbyEnemies = new List<Entity>();
    EntityAlive leader = EntityUtilities.GetLeaderOrOwner(theEntity.entityId) as EntityAlive;


        float originalView = theEntity.GetMaxViewAngle();
        theEntity.SetMaxViewAngle(250f);

        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(theEntity.position, new Vector3(20f, 20f, 20f));
        theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, NearbyEntities);
        DisplayLog(" Nearby Entities: " + NearbyEntities.Count);
        for(int i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)NearbyEntities[i];
            if(x is EntityVehicle)
                continue;

            if(x != theEntity && x.IsAlive())
            {
                if(leader != null && x == leader)
                    continue;

                if(x.CanSee(theEntity.position))
                {
                    DisplayLog(" I can be seen by an enemy.");
                    if(!theEntity.CanSee(x.position))
                    {
                        DisplayLog(" I know an entity is there, but I can't see it: " + x.EntityName);
                        continue;
                    }

                }
                DisplayLog("Nearby Entity: " + x.EntityName);
                if(SphereII_DMTEAIUtilities.CheckFactionForEnemy(theEntity, x))
                    NearbyEnemies.Add(x);
            }
        }

        theEntity.SetMaxViewAngle(originalView);
        return NearestEnemy(theEntity, NearbyEnemies);

    }

    public static bool NearestEnemy(EntityAlive theEntity, List<Entity> NearbyEnemies)
    {
        if(NearbyEnemies.Count == 0)
            return false;

        // Finds the closet block we matched with.
        EntityAlive closeEnemy = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = theEntity.position;
        foreach(EntityAlive enemy in NearbyEnemies)
        {
            float dist = Vector3.Distance(enemy.position, currentPos);
            DisplayLog(" Entity: " + enemy.EntityName + "'s distance is: " + dist);
            if(dist < minDist)
            {
                closeEnemy = enemy;
                minDist = dist;
            }
        }

        if(closeEnemy != null)
        {
            DisplayLog(" Closes Enemy: " + closeEnemy.ToString());
            theEntity.SetRevengeTarget(closeEnemy);
            return true;
        }
        return false;
    }
}

