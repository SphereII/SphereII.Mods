using System;
using System.Collections.Generic;
using System.Globalization;
using GamePath;
using UnityEngine;

public class EAIApproachAndFollowTargetSDX : EAIApproachAndAttackTarget
{
    List<String> lstIncentives = new List<String>();
    private List<Entity> NearbyEntities = new List<Entity>();
    private Vector3 entityTargetPos;
    private Vector3 entityTargetVel;
    private int pathCounter;
    private bool DeathOnLeaderLoss = false;

    //  public EntityAliveSDX entityAliveSDX;

    private bool blDisplayLog = false;
    private EntityAlive entityTarget;
    private bool isTargetToEat;

    private bool Stop = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
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
            string[] array = text.Split(new char[]
            {
                ','
            });
            for (int i = 0; i < array.Length; i++)
            {
                DisplayLog("\tIncentive: " + array[i].ToString());
                if (this.lstIncentives.Contains(array[i].ToString()))
                    continue;
                this.lstIncentives.Add(array[i].ToString());
            }
        }

        if (data.TryGetValue("Death", out text))
        {
            if (text.ToLower() == "true")
                DeathOnLeaderLoss = true;
        }
    }

    public virtual bool ConfigureTargetEntity()
    {
        //if (this.entityTarget != null)
        //    return true;

        this.NearbyEntities.Clear();


        DisplayLog(" ConfigureTargetEntity()");

        DisplayLog(" Incentivies: " + this.lstIncentives.ToArray().ToString());
        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(30f, 20f, 30f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            DisplayLog("Found entity: " + x.EntityName);
            if (x != this.theEntity)
            {

                // Check the entity against the incentives
                if (EntityUtilities.CheckIncentive(this.theEntity.entityId, this.lstIncentives, x))
                {
                    DisplayLog(" Found my Target: " + x.EntityName);
                    this.entityTarget = x;

                    // Leader is dead
                    if (!this.entityTarget.IsAlive())
                    {
                        // wait until leader respawns
                        //this.theEntity.MarkToUnload();
                        return false;
                    }


                    Vector3 tempPosition = (this.theEntity.position - this.entityTarget.position).normalized * 3 + this.entityTarget.position;

                    Vector3 a = this.theEntity.position - tempPosition;

                    //Vector3 temp  = RandomPositionGenerator.CalcNear(this.theEntity, this.entityTarget.position, 4, 2);
                    //Vector3 a = this.theEntity.position - this.entityTarget.position;
                    DisplayLog(" Distance: " + a);
                    if (a.sqrMagnitude < 4f)
                        if (a.sqrMagnitude < 3f)
                        {
                            this.entityTarget = null;
                            this.entityTargetPos = Vector3.zero;
                            return false;
                        }
                    this.entityTargetPos = tempPosition;
                    return true;
                }
            }
        }


        // We have a leader, but they are not within our range.
        if (this.theEntity.Buffs.HasCustomVar("Leader"))
        {
            EntityAlive entity = this.theEntity.world.GetEntity((int)(this.theEntity.Buffs.GetCustomVar("Leader"))) as EntityAlive;
            if (entity)
            {
                DisplayLog("My leader is out of sight. Teleporting to my leader");
                if (this.entityTarget != null)
                {
                    this.entityTarget.SetAttackTarget(null, 20);
                    this.entityTarget.SetRevengeTarget(null);
                }
                this.entityTarget = entity;
                this.theEntity.Buffs.AddBuff("buffAttackCoolDown");
                SetCloseSpawnPoint();
                return true;

            }
        }
        DisplayLog(" No Entity To be Configured");
        this.entityTarget = null;
        return false;
    }

    public override bool CanExecute()
    {
        DisplayLog("CanExecute() Start");
        bool result = false;
        result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Follow);
        DisplayLog("CanExecute() Follow Task? " + result);

        if (!(this.theEntity is EntityAliveFarmingAnimalSDX) && (result == false))
            return false;

        // If there is an entity in bounds, then let this AI Task roceed. Otherwise, don't do anything with it.
        result = ConfigureTargetEntity();
        if (result)
        {
            this.theEntity.moveSpeed = this.entityTarget.moveSpeed;
            DisplayLog("CanExecute() Configure Target Result: " + result);

        }
        DisplayLog("CanExecute() End: " + result);

    
        return result;

    }

    public override void Start()
    {
        this.entityTargetPos = this.entityTarget.position;
        this.entityTargetVel = Vector3.zero;
        this.isTargetToEat = false;
        this.theEntity.IsEating = false;
        this.pathCounter = 0;
        Stop = false;
    }

    public void SetCloseSpawnPoint()
    {
        if (entityTarget == null)
            return;

        this.theEntity.SetPosition(this.entityTarget.position + -new Vector3(this.entityTarget.GetLookVector().x, 0f, this.entityTarget.GetLookVector().z) * 4, true);

    }



    public override bool Continue()
    {
        DisplayLog("Continue() Start");
        if (Stop)
            return false;
        bool result = false;
        //   if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Follow);
            if (result == false)
            {
                // Since SetPatrol also uses this method, we'll add an extra check.
                result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.SetPatrolPoint);
            }

            if (result == false)
                return false;
        }

        if (this.theEntity.moveHelper.BlockedTime > 2)
        {
            DisplayLog(" Blocked Time: " + this.theEntity.moveHelper.BlockedTime);

            DisplayLog("Entity is blocked. Resetting its move position");
            // If the npc seems lost, set a generate move to match the leader's position
            this.theEntity.moveHelper.SetMoveTo(this.entityTargetPos, false);

            return false;

        }
        if (pathCounter == 0) // briefly pause if you are at the end of the path to let other tasks run
            result = false;

        result = ConfigureTargetEntity();

        // Teleport to the player, even if you have an attack target
        //  if(this.entityTarget == null)
        {
            if (this.theEntity.Buffs.HasCustomVar("Leader"))
            {
                DisplayLog(" Checking my Leader");
                // Find out who the leader is.
                int PlayerID = (int)this.theEntity.Buffs.GetCustomVar("Leader");
                float distance = this.theEntity.GetDistance(this.theEntity.world.GetEntity(PlayerID));
                DisplayLog(" Distance: " + distance);
                if (distance > this.theEntity.GetSeeDistance())
                {
                    DisplayLog("I am too far away from my leader. Teleporting....");
                    SetCloseSpawnPoint();
                    if (this.entityTarget)
                        DisplayLog(" my Position: " + this.theEntity.position + " Target Position: " + this.entityTargetPos + " My Leader position: " + this.entityTarget.position);
                }

            }
        }
        DisplayLog("Continue() End: " + result);
        return result;

    }

    public override void Update()
    {
        // No entity, so no need to do anything.
        if (this.entityTarget == null || this.entityTargetPos == null)
        {
            DisplayLog(" Entity Target or EntityTarget Position is null");
            return;
        }

        // Let the entity keep looking at you, otherwise it may just sping around.
        this.theEntity.SetLookPosition(this.entityTargetPos);
        this.theEntity.RotateTo(this.entityTargetPos.x, this.entityTargetPos.y + 2, this.entityTargetPos.z, 30f, 30f);

        if (this.theEntity is EntityAliveSDX)
        {
            if (EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.SetPatrolPoint))
            {
                // Make them a lot closer to you when they are following you.
                (this.theEntity as EntityAliveSDX).UpdatePatrolPoints(this.theEntity.world.FindSupportingBlockPos(this.entityTarget.position));
            }

        }
        Vector3 a = this.theEntity.position - this.entityTargetPos;
        if (a.sqrMagnitude < 2f)
        {
            DisplayLog("Entity is too close. Ending pathing.");
            this.pathCounter = 0;
            return;
        }

        this.theEntity.moveHelper.CalcIfUnreachablePos(this.entityTargetPos);

        // if the entity is not calculating a path, check how many nodes are left, and reset the path counter if its too low.
        if (!PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            PathEntity path = this.theEntity.navigator.getPath();
            if (path != null && path.NodeCountRemaining() <= 2)
                this.pathCounter = 0;
        }

        if (--this.pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            DisplayLog(" Path Counter is 0: Finding new path to " + this.entityTargetPos);
            // If its still not calculating a path, find a new path to the leader
            this.pathCounter = 6 + this.theEntity.rand.RandomRange(10);
            PathFinderThread.Instance.FindPath(this.theEntity, this.entityTarget.position, this.theEntity.GetMoveSpeedAggro(), true, this);
        }

        if (this.theEntity.Climbing)
            return;

        // If there's no path, calculate one.
        if (this.theEntity.navigator.noPathAndNotPlanningOne())
        {
            DisplayLog("No Path and Not Planning One. Searching for new path to : " + this.entityTargetPos);
            PathFinderThread.Instance.FindPath(this.theEntity, this.entityTarget.position, this.theEntity.GetMoveSpeedAggro(), true, this);
        }
    }
}
