using GamePath;
using System.Collections.Generic;
using UnityEngine;

public class EAIApproachAndFollowTargetSDX : EAIApproachAndAttackTarget
{
    private readonly bool blDisplayLog = false;
    private readonly List<string> lstIncentives = new List<string>();
    private readonly List<Entity> NearbyEntities = new List<Entity>();
    private EntityAlive entityTarget;
    private Vector3 entityTargetPos;
    private Vector3 entityTargetVel;

    public bool isTargetToEat;
    private int pathCounter;

    private bool Stop;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " :" + theEntity.EntityName + ": " + strMessage);
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        // entityAliveSDX = (_theEntity as EntityAliveSDX);
    }

    // Allow params to be a comma-delimited list of various incentives, such as item name, buff, or cvar.
    //	<property name="AITask-5" value="ApproachAndAttackTarget" data="class=EntityNPC,0,EntityEnemyAnimal,0,EntityPlayer,0"/> <!-- class,maxChaseTime (return home) -->

    public override void SetData(DictionarySave<string, string> data)
    {
        base.SetData(data);
        string text;
        if (data.TryGetValue("incentives", out text))
        {
            DisplayLog("Text: " + text);
            var array = text.Split(',');
            for (var i = 0; i < array.Length; i++)
            {
                DisplayLog("\tIncentive: " + array[i]);
                if (lstIncentives.Contains(array[i]))
                    continue;
                lstIncentives.Add(array[i]);
            }
        }
    }

    public virtual bool ConfigureTargetEntity()
    {
        //if (this.entityTarget != null)
        //    return true;

        NearbyEntities.Clear();


        DisplayLog(" ConfigureTargetEntity()");

        DisplayLog(" Incentivies: " + lstIncentives.ToArray());
        // Search in the bounds are to try to find the most appealing entity to follow.
        var bb = new Bounds(theEntity.position, new Vector3(30f, 20f, 30f));
        theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, NearbyEntities);
        for (var i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            var x = (EntityAlive)NearbyEntities[i];
            DisplayLog("Found entity: " + x.EntityName);
            if (x != theEntity)
                // Check the entity against the incentives
                if (EntityUtilities.CheckIncentive(theEntity.entityId, lstIncentives, x))
                {
                    DisplayLog(" Found my Target: " + x.EntityName);
                    entityTarget = x;

                    // Leader is dead
                    if (!entityTarget.IsAlive())
                        // wait until leader respawns
                        //this.theEntity.MarkToUnload();
                        return false;


                    var tempPosition = (theEntity.position - entityTarget.position).normalized * 3 + entityTarget.position;

                    var a = theEntity.position - tempPosition;

                    //Vector3 temp  = RandomPositionGenerator.CalcNear(this.theEntity, this.entityTarget.position, 4, 2);
                    //Vector3 a = this.theEntity.position - this.entityTarget.position;
                    DisplayLog(" Distance: " + a);
                    if (a.sqrMagnitude < 4f)
                        if (a.sqrMagnitude < 3f)
                        {
                            entityTarget = null;
                            entityTargetPos = Vector3.zero;
                            return false;
                        }

                    entityTargetPos = tempPosition;
                    return true;
                }
        }


        // We have a leader, but they are not within our range.
        if (theEntity.Buffs.HasCustomVar("Leader"))
        {
            var entity = theEntity.world.GetEntity((int)theEntity.Buffs.GetCustomVar("Leader")) as EntityAlive;
            if (entity)
            {
                DisplayLog("My leader is out of sight. Teleporting to my leader");
                if (entityTarget != null)
                {
                    entityTarget.SetAttackTarget(null, 20);
                    entityTarget.SetRevengeTarget(null);
                }

                entityTarget = entity;
                theEntity.Buffs.AddBuff("buffAttackCoolDown");
                SetCloseSpawnPoint();
                return true;
            }
        }

        DisplayLog(" No Entity To be Configured");
        entityTarget = null;
        return false;
    }

    public override bool CanExecute()
    {
        DisplayLog("CanExecute() Start");
        var result = false;
        result = EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Follow);
        DisplayLog("CanExecute() Follow Task? " + result);

        if (!(theEntity is EntityAliveFarmingAnimalSDX) && result == false)
            return false;

        // If there is an entity in bounds, then let this AI Task roceed. Otherwise, don't do anything with it.
        result = ConfigureTargetEntity();
        if (result)
        {
            theEntity.moveSpeed = entityTarget.moveSpeed;
            DisplayLog("CanExecute() Configure Target Result: " + result);
        }

        DisplayLog("CanExecute() End: " + result);


        return result;
    }

    public override void Start()
    {
        entityTargetPos = entityTarget.position;
        entityTargetVel = Vector3.zero;
        isTargetToEat = false;
        theEntity.IsEating = false;
        pathCounter = 0;
        Stop = false;
    }

    public void SetCloseSpawnPoint()
    {
        if (entityTarget == null)
            return;

        theEntity.SetPosition(entityTarget.position + -new Vector3(entityTarget.GetLookVector().x, 0f, entityTarget.GetLookVector().z) * 4);
    }


    public override bool Continue()
    {
        DisplayLog("Continue() Start");
        if (Stop)
            return false;
        var result = false;
        //   if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Follow);
            if (result == false)
                // Since SetPatrol also uses this method, we'll add an extra check.
                result = EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.SetPatrolPoint);

            if (result == false)
                return false;
        }

        if (theEntity.moveHelper.BlockedTime > 2)
        {
            DisplayLog(" Blocked Time: " + theEntity.moveHelper.BlockedTime);

            DisplayLog("Entity is blocked. Resetting its move position");
            // If the npc seems lost, set a generate move to match the leader's position
            theEntity.moveHelper.SetMoveTo(entityTargetPos, false);

            return false;
        }

        if (pathCounter == 0) // briefly pause if you are at the end of the path to let other tasks run
            result = false;

        result = ConfigureTargetEntity();

        // Teleport to the player, even if you have an attack target
        //  if(this.entityTarget == null)
        {
            if (theEntity.Buffs.HasCustomVar("Leader"))
            {
                DisplayLog(" Checking my Leader");
                // Find out who the leader is.
                var PlayerID = (int)theEntity.Buffs.GetCustomVar("Leader");
                var distance = theEntity.GetDistance(theEntity.world.GetEntity(PlayerID));
                DisplayLog(" Distance: " + distance);
                if (distance > theEntity.GetSeeDistance())
                {
                    DisplayLog("I am too far away from my leader. Teleporting....");
                    SetCloseSpawnPoint();
                    if (entityTarget)
                        DisplayLog(" my Position: " + theEntity.position + " Target Position: " + entityTargetPos + " My Leader position: " + entityTarget.position);
                }
            }
        }
        DisplayLog("Continue() End: " + result);
        return result;
    }

    public override void Update()
    {
        // No entity, so no need to do anything.
        if (entityTarget == null)
        {
            DisplayLog(" Entity Target or EntityTarget Position is null");
            return;
        }

        // Let the entity keep looking at you, otherwise it may just sping around.
        theEntity.SetLookPosition(entityTargetPos);
        theEntity.RotateTo(entityTargetPos.x, entityTargetPos.y + 2, entityTargetPos.z, 8f, 8f);

        if (theEntity is IEntityOrderReceiverSDX entityOrderReceiver)
            if (EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.SetPatrolPoint))
                // Make them a lot closer to you when they are following you.
                entityOrderReceiver.UpdatePatrolPoints(theEntity.world.FindSupportingBlockPos(entityTarget.position));
        var a = theEntity.position - entityTargetPos;
        if (a.sqrMagnitude < 2f)
        {
            DisplayLog("Entity is too close. Ending pathing.");
            pathCounter = 0;
            return;
        }

        theEntity.moveHelper.CalcIfUnreachablePos();

        // if the entity is not calculating a path, check how many nodes are left, and reset the path counter if its too low.
        if (!PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
        {
            var path = theEntity.navigator.getPath();
            if (path != null && path.NodeCountRemaining() <= 2)
                pathCounter = 0;
        }

        if (--pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
        {
            DisplayLog(" Path Counter is 0: Finding new path to " + entityTargetPos);
            // If its still not calculating a path, find a new path to the leader
            pathCounter = 6 + theEntity.rand.RandomRange(10);
            PathFinderThread.Instance.FindPath(theEntity, entityTarget.position, theEntity.GetMoveSpeedAggro(), true, this);
        }

        if (theEntity.Climbing)
            return;

        // If there's no path, calculate one.
        if (theEntity.navigator.noPathAndNotPlanningOne())
        {
            DisplayLog("No Path and Not Planning One. Searching for new path to : " + entityTargetPos);
            PathFinderThread.Instance.FindPath(theEntity, entityTarget.position, theEntity.GetMoveSpeedAggro(), true, this);
        }
    }
}