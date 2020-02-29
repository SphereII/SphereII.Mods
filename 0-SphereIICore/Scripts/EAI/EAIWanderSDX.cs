using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIWanderSDX : EAIWander
{
    public Vector3 position;

    public float time;

    private bool blDisplayLog = false;
    private bool blShowPathFindingBlocks = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }

    public override void Update()
    {
        this.time += 0.05f;
        if (!EntityUtilities.CheckProperty(this.theEntity.entityId, "PathingBlocks"))
           return;
            
            EntityMoveHelper moveHelper = this.theEntity.moveHelper;

        //If we are close, be done with it.
        float dist = Vector3.Distance(this.position, this.theEntity.position);
        if (dist < 0.1f)
        {
            DisplayLog("I am within 1f of the block: " + dist);
            BlockValue block = GameManager.Instance.World.GetBlock( new Vector3i(this.position));
            if (block.type != BlockValue.Air.type || block.Block.GetBlockName() != "PathingCube")
            {
                DisplayLog("I am close enough to this block: " + block.Block.GetBlockName());
//                if (block.Block.GetBlockName() == "bedroll")
//                {
//                    Debug.Log("At a bedroll");
//                    if (this.theEntity.emodel != null && this.theEntity.emodel.avatarController != null)
//                    {
//                        DisplayLog("Turning into crawler");
//                        int sleeperPose = (int)this.theEntity.rand.RandomRange(0, 9);
//                        this.theEntity.emodel.avatarController.TriggerSleeperPose(sleeperPose);
//                       // this.theEntity.emodel.avatarController.TurnIntoCrawler(true);
////                        this.theEntity.emodel.avatarController.GetAnimator().enabled =false;
//                    }

//                }
                moveHelper.Stop();
                this.theEntity.navigator.clearPath();
                this.time = 40f;
                return;
            }
     
        }

          //Check if we are blocked, which may indicate that we are at a door that we want to open.
        //if (moveHelper.IsBlocked && moveHelper.BlockedTime > 0.09)
        //{
        //    DisplayLog("I am blocked, and I've been blocked for more than 0.010 seconds. I cannot keep going.");
        //    moveHelper.Stop();
        //    this.time = 40f;
        //    return;
        //}

    }

    public override void Start()
    {
        // if no pathing blocks, just randomly pick something.

        Vector3 newPosition = EntityUtilities.GetNewPositon(this.theEntity.entityId);
        if ( newPosition == Vector3.zero)
            this.position = RandomPositionGenerator.CalcAway(this.theEntity, 10, 30, 10, this.theEntity.position);

        //Give them more time to path find.The CanContinue() stops at 30f, so we'll set it at -90, rather than 0.
        this.time = -90f;

        // Path finding has to be set for Breaking Blocks so it can path through doors
        PathFinderThread.Instance.FindPath(this.theEntity, this.position, this.theEntity.GetMoveSpeed(), true, this);
        return;
    }

    public override bool CanExecute()
    {
        if (!EntityUtilities.CheckProperty(this.theEntity.entityId, "PathingBlocks"))
            return base.CanExecute();

        if (EntityUtilities.GetAttackOrReventTarget( this.theEntity.entityId) != null)
            return false;

        Vector3 newPosition = EntityUtilities.GetNewPositon(this.theEntity.entityId);
        if (newPosition != Vector3.zero)
        {
             String strParticleName = "#@modfolder(0-SphereIICore):Resources/PathSmoke.unity3d?P_PathSmoke_X";
            //String strParticleName = "forge";
           // ParticleEffect.RegisterBundleParticleEffect(strParticleName);
            BlockValue myBlock = GameManager.Instance.World.GetBlock(new Vector3i(newPosition));
            DisplayLog(" I have a new position I can path too.");

            //  For testing, change the target to this block, so we can see where the NPC intends to go.
            if (blShowPathFindingBlocks)
            {
                DisplayLog(" I have highlighted where I am going: " + newPosition);

                Vector3 supportBlock = GameManager.Instance.World.FindSupportingBlockPos(newPosition);
                BlockUtilitiesSDX.addParticles(strParticleName, new Vector3i(supportBlock));
            }
            if (SphereCache.LastBlock.ContainsKey(this.theEntity.entityId))
            {
                if (blShowPathFindingBlocks)
                {
                    DisplayLog("I am changing the block back to the pathing block");
                    Vector3 supportBlock = GameManager.Instance.World.FindSupportingBlockPos(this.position);
                    BlockUtilitiesSDX.removeParticles( new Vector3i(supportBlock));
                }
                SphereCache.LastBlock[this.theEntity.entityId] = newPosition;
            }
            else
            {
                // Store the LastBlock position here, so we know what we can remove next time.
                SphereCache.LastBlock.Add(this.theEntity.entityId, newPosition);
            }
            this.position = newPosition;


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

}

