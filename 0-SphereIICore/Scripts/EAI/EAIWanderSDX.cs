using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIWanderSDX : EAIWander
{
    public Vector3 position;

    public float time;
    private float throttle = 10f;

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

        //If we are close, be done with it. This is to help prevent the NPC from standing on certain workstations that its supposed to path too.
        float dist = Vector3.Distance(this.position, this.theEntity.position);
       
        if (dist < 2f)
        {
            DisplayLog("I am within 1f of the block: " + dist);
            BlockValue block = GameManager.Instance.World.GetBlock(new Vector3i(this.position));
            if (block.type != BlockValue.Air.type || block.Block.GetBlockName() != "PathingCube")
            {
                DisplayLog("I am close enough to this block: " + block.Block.GetBlockName());
                // Test code here
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

                // Call the stop, and set the to 40, which kills the task.
              //  EntityUtilities.Stop(this.theEntity.entityId);
                this.time = 90f;
                return;
            }

        }
    }

    public override bool Continue()
    {
        // calling stop here if we can't continue to clear the path and movement. 
        bool result = this.theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None && this.theEntity.moveHelper.BlockedTime <= 0.3f && this.time <= 30f && !this.theEntity.navigator.noPathAndNotPlanningOne();
        if (!result)
        {
            float dist = Vector3.Distance(this.position, this.theEntity.position);

          //  Debug.Log("Position: " + this.position + " My Position: " + this.theEntity.position + " Distance: " + dist);

            // calling stop here if we can't continue to clear the path and movement. 
            EntityUtilities.Stop(this.theEntity.entityId);
            this.position = Vector3.zero;
        }

        return result;
    }


    public override void Start()
    {
        // if no pathing blocks, just randomly pick something.
        if (this.position == Vector3.zero)
        {
            Vector3 vector = Vector3.zero;
            int num2 = 30;

            if (this.theEntity.IsAlert)
                vector = RandomPositionGenerator.CalcAway(this.theEntity, 0, num2, num2, this.theEntity.LastTargetPos);
            else
                vector = RandomPositionGenerator.Calc(this.theEntity, num2, num2);

            this.position = vector;
            this.time = 0f;
        }
        else
        {
            this.time = -60f;
        }
        //Vector3 temp = Ent
       //EntityUtilities.GetNewPositon(this.theEntity.entityId);
        //if (temp != Vector3.zero)
        //{
        //    this.position = temp;
        //    //Give them more time to path find.The CanContinue() stops at 30f, so we'll set it at -90, rather than 0.
        //    this.time = 90f;
        //}
     //   EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Melee);
        // Path finding has to be set for Breaking Blocks so it can path through doors
        PathFinderThread.Instance.FindPath(this.theEntity, this.position, this.theEntity.GetMoveSpeed(), true, this);
        return;
    }

    public override bool CanExecute()
    {
        // if you are supposed to stay put, don't wander. 
        if (EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Stay))
            return false;

        // If Pathing blocks does not exist, don't bother trying to do the enhanced wander code
        if (!EntityUtilities.CheckProperty(this.theEntity.entityId, "PathingBlocks"))
             return base.CanExecute();

        // If there's a target to fight, dont wander around. That's lame, sir.
        if (EntityUtilities.GetAttackOrReventTarget( this.theEntity.entityId) != null)
            return false;

    
        this.throttle += 0.05f;
        if (this.throttle > 10)
        {
            this.throttle = 0;

            Vector3 newPosition = EntityUtilities.GetNewPositon(this.theEntity.entityId);
            if (newPosition == Vector3.zero)
            {
                DisplayLog("I do not have any pathing blocks");
                //result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Wander);
                //if (result == false)
                //    return false;
                //else
                //    DisplayLog("CanExecuteTask(): Order is set for Wander");
                return base.CanExecute();
            }
            else
            {
                String strParticleName = "#@modfolder(0-SphereIICore):Resources/PathSmoke.unity3d?P_PathSmoke_X";
                if (!ParticleEffect.IsAvailable(strParticleName))
                    ParticleEffect.RegisterBundleParticleEffect(strParticleName);

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
                        BlockUtilitiesSDX.removeParticles(new Vector3i(supportBlock));
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
        }
        return false;
    }

}

