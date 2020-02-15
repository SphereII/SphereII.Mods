using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIWanderSDX : EAIWander
{
    public Vector3 position;

    public float time;

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }

    public override void Update()
    {
        this.time += 0.05f;
        // If there's no positions, don't both processing the special tasks.
        //Vector3 newposition = SphereCache.GetRandomPath(this.theEntity.entityId);
        //if (newposition == Vector3.zero)
        //    return;

        //Check if we are blocked, which may indicate that we are at a door that we want to open.
        EntityMoveHelper moveHelper = this.theEntity.moveHelper;

        //If blocked, check to see if its a door.
        if (moveHelper.IsBlocked)
        {
            Debug.Log("I am blocked.");
            Vector3i blockPos = this.theEntity.moveHelper.HitInfo.hit.blockPos;
            BlockValue block = this.theEntity.world.GetBlock(blockPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
            {
                DisplayLog("I am blocked by a door. Trying to open...");
                SphereCache.AddDoor(this.theEntity.entityId, blockPos);
                EntityUtilities.OpenDoor(this.theEntity.entityId, blockPos);
                //      We were blocked, so let's clear it.
                moveHelper.ClearBlocked();
            }

            // if we are still blocked, try to re-position and clear the block flag
            if (moveHelper.IsBlocked && moveHelper.BlockedTime > 0.010)
            {
                DisplayLog("I am blocked, and I've been blocked for more than 0.010 seconds. I cannot keep going.");
                this.theEntity.RotateTo(Vector3.back.x, Vector3.back.y, Vector3.zero.z, 180f, 180f);
                moveHelper.SetMoveTo(this.theEntity.position + (Vector3.forward * 3), true);
                this.time = 40f;
                return;
            }

        }

        // Check to see if we've opened a door, and close it behind you.
        Vector3i doorPos = SphereCache.GetDoor(this.theEntity.entityId);
        if (doorPos != Vector3i.zero)
        {
            DisplayLog("I've opened a door recently. I'll see if I can close it.");
            BlockValue block = this.theEntity.world.GetBlock(doorPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
            {
                if ((this.theEntity.GetDistanceSq(doorPos.ToVector3()) > 3f))
                {
                    DisplayLog("I am going to close the door now.");
                    EntityUtilities.CloseDoor(this.theEntity.entityId, doorPos);
                    SphereCache.RemoveDoor(this.theEntity.entityId, doorPos);
                }
            }
        }

    }

    public override void Start()
    {
            //Give them more time to path find.The CanContinue() stops at 30f, so we'll set it at -90, rather than 0.
            this.time = -90f;
            PathFinderThread.Instance.FindPath(this.theEntity, this.position, this.theEntity.GetMoveSpeed(), true, this);
            return;
    }

    public override bool CanExecute()
    {
        Vector3 newPosition = EntityUtilities.GetNewPositon(this.theEntity.entityId);
        if (newPosition != Vector3.zero)
        {
            this.position = newPosition;
            DisplayLog(" I have a new position I can path too.");

            //  For testing, change the target to this block, so we can see where the NPC intends to go.
            if (blDisplayLog)
            {
                DisplayLog(" I have highlighted where I am going.");
                GameManager.Instance.World.SetBlock(0, new Vector3i(newPosition), new BlockValue((uint)Block.GetBlockByName("PathingCube2", false).blockID), true, false);
            }
            if (SphereCache.LastBlock.ContainsKey(this.theEntity.entityId))
            {
                if (blDisplayLog)
                {
                    DisplayLog("I am changing the block back to the pathing block");
                    GameManager.Instance.World.SetBlock(0, new Vector3i(SphereCache.LastBlock[this.theEntity.entityId]), new BlockValue((uint)Block.GetBlockByName("PathingCube", false).blockID), true, false);
                }
                SphereCache.LastBlock[this.theEntity.entityId] = newPosition;
            }
            else
            {
                // Store the LastBlock position here, so we know what we can remove next time.
                SphereCache.LastBlock.Add(this.theEntity.entityId, newPosition);
            }
            return true;
        }
        else
        {
            DisplayLog("I do not have any pathing blocks");
            //result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Wander);
            //if (result == false)
            //    return false;
            //else
            //    DisplayLog("CanExecuteTask(): Order is set for Wander");
            return base.CanExecute();
        }


    }

    public override bool Continue()
    {
        //bool result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Wander);
        //if (result == false)
        //    return false;
        //else
        //    DisplayLog("CanExecuteTask(): Order is set for Wander");

        // if an entity gets 'stuck' on a block, it just starts attacking it. Kind of aggressive.
        //if (this.theEntity.moveHelper.BlockedTime > 0.01f)
        //{
        //    DisplayLog("Continue(): I am stuck for more than 1f");
        //    this.theEntity.navigator.clearPath();
        //    Vector3 headPosition = this.theEntity.getHeadPosition();
        //    Vector3 vector = this.theEntity.GetForwardVector();
        //    vector = Quaternion.Euler(UnityEngine.Random.value * 120f - 30f, UnityEngine.Random.value * 120f - 60f, 0f) * vector;
        //    this.theEntity.SetLookPosition(headPosition + vector);
        //    return false;
        //}

        return base.Continue();
    }
}

