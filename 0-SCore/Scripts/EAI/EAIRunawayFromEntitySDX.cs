using System.Collections.Generic;
using UnityEngine;


// <property name="AITask-2" value="RunAwayFromEntitySDX, SCore" />
internal class EAIRunAwayFromEntitySDX : EAIRunawayWhenHurt

{
    private readonly bool blDisplayLog = false;
    private readonly List<Entity> NearbyEnemies = new List<Entity>();
    private readonly List<Entity> NearbyEntities = new List<Entity>();
    public int fleeDistance = 10;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + theEntity.EntityName + ": " + theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        var originalView = theEntity.GetMaxViewAngle();
        if (CheckSurroundingEntities())
        {
            theEntity.SetMaxViewAngle(360f);
            if (base.CanExecute())
            {
                theEntity.SetMaxViewAngle(180f);
                return true;
            }
        }

        theEntity.SetMaxViewAngle(180f);

        return false;
    }

    public override void Reset()
    {
        base.Reset();
    }

    public bool CheckFactionForEnemy(EntityAlive Entity)
    {
        var myRelationship = FactionManager.Instance.GetRelationshipTier(theEntity, Entity);
        switch (myRelationship)
        {
            case FactionManager.Relationship.Hate:
                fleeDistance = 40;
                break;
            case FactionManager.Relationship.Dislike:
                fleeDistance = 20;
                break;
            case FactionManager.Relationship.Neutral:
                return false;
            case FactionManager.Relationship.Like:
                return false;
            case FactionManager.Relationship.Love:
                return false;
        }

        return true;
    }

    public bool CheckSurroundingEntities()
    {
        NearbyEntities.Clear();
        NearbyEnemies.Clear();

        EntityAlive leader = null;
        if (theEntity.Buffs.HasCustomVar("Leader"))
        {
            DisplayLog(" leader Detected.");
            var EntityID = (int)theEntity.Buffs.GetCustomVar("Leader");
            leader = theEntity.world.GetEntity(EntityID) as EntityAlive;
        }


        // Search in the bounds are to try to find the most appealing entity to follow.
        var bb = new Bounds(theEntity.position, new Vector3(theEntity.GetSeeDistance(), 20f, theEntity.GetSeeDistance()));
        theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, NearbyEntities);
        DisplayLog(" Nearby Entities: " + NearbyEntities.Count);
        for (var i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            var x = (EntityAlive)NearbyEntities[i];
            if (x != theEntity && x.IsAlive())
            {
                if (x == leader)
                    continue;

                DisplayLog("Nearby Entity: " + x.EntityName);
                if (CheckFactionForEnemy(x))
                    NearbyEnemies.Add(x);
            }

            // if one of our faction died, flee from the spot.
            if (x.factionId == theEntity.factionId)
            {
                if (x.GetRevengeTarget() != null)
                {
                    DisplayLog(" My Faction has a Revenge Target. I am sharing it. ");
                    fleeDistance = 100;
                    theEntity.SetRevengeTarget(x.GetRevengeTarget());
                    return true;
                }

                if (x.IsDead())
                {
                    //DisplayLog(" One of my factions has died. Fleeing, and abandoning the herd");
                    //this.fleeDistance = 100;
                    //if (x.entityThatKilledMe != null)
                    //{
                    //    DisplayLog(" I am fleeing: " + x.entityThatKilledMe.EntityName);
                    //    this.avoidEntity = x.entityThatKilledMe;
                    //}
                    //else
                    //{
                    DisplayLog(" I don not know who killed my friend, so i am running from " + x.EntityName);
                    // }
                    return true;
                }
            }
        }

        return NearestEnemy();
    }

    public bool NearestEnemy()
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
            return true;
        }

        return false;
    }
}