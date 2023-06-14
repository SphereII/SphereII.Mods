using GamePath;
using UnityEngine;

internal class EAIWanderSDX : EAIWander
{
    private readonly bool blDisplayLog = false;
    private readonly bool blShowPathFindingBlocks = false;
    public Vector3 position;

    public float time;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(theEntity.EntityName + ": " + strMessage);
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);

        // Delay the wait task to slow them down.
        executeWaitTime = 2f;
    }

    public override void Reset()
    {
        base.Reset();

        // Turn jumping back on, to prevent them from jumping weirdly.
        if (theEntity is EntityAliveSDX)
            (theEntity as EntityAliveSDX).canJump = true;
    }

    public override void Update()
    {
        time += 0.05f;

        //If we are close, be done with it. This is to help prevent the NPC from standing on certain workstations that its supposed to path too.
        var dist = Vector3.Distance(position, theEntity.position);
        if (dist < 2f)
        {
            DisplayLog("I am within 1f of the block: " + dist);
            var block = GameManager.Instance.World.GetBlock(new Vector3i(position));
            if (block.type != BlockValue.Air.type || block.Block.GetBlockName() != "PathingCube")
            {
                DisplayLog("I am close enough to this block: " + block.Block.GetBlockName());
                time = 90f;
            }
        }
    }

    public override bool Continue()
    {
        // Reduces the entity from continuing to walk against a wall
        if (theEntity.moveHelper.BlockedTime >= 0.3f)
        {
            //  Debug.Log("Continuie(): Blocked Time is greater than 0.3: " + theEntity.moveHelper.BlockedTime);
            EntityUtilities.Stop(theEntity.entityId);
            position = Vector3.zero;
            return false;
        }


        //Debug.Log("Blocked Time: " + theEntity.moveHelper.BlockedTime);
        //Debug.Log("Time: " + time);
        //Debug.Log("No Path or not planning one: " + theEntity.navigator.noPathAndNotPlanningOne());
        // calling stop here if we can't continue to clear the path and movement. 
        var result = theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None && theEntity.moveHelper.BlockedTime <= 0.3f && time <= 30f && !theEntity.navigator.noPathAndNotPlanningOne();
        if (!result)
        {
            //   Debug.Log("Continue(): no stunn, and no path.");
            EntityUtilities.Stop(theEntity.entityId);
            position = Vector3.zero;
            return false;
        }

        return result;
    }


    public override void Start()
    {
        // if no pathing blocks, just randomly pick something.
        if (position == Vector3.zero)
        {
            var maxDistance = 30;

            if (theEntity.IsAlert)
                position = RandomPositionGenerator.CalcAway(theEntity, 0, maxDistance, 10, theEntity.LastTargetPos);
            else
                position = RandomPositionGenerator.Calc(theEntity, maxDistance, 10);
        }

        time = 0f;

        EntityUtilities.ChangeHandholdItem(theEntity.entityId, EntityUtilities.Need.Reset);

        // Turn off the entity jumping, and turn on breaking blocks, to allow for better pathing.
        if (theEntity is EntityAliveSDX)
        {
            (theEntity as EntityAliveSDX).canJump = false;
            (theEntity as EntityAliveSDX).moveHelper.CanBreakBlocks = true;
        }


        // Path finding has to be set for Breaking Blocks so it can path through doors
        PathFinderThread.Instance.FindPath(theEntity, position, theEntity.GetMoveSpeed(), true, this);
    }

    public override bool CanExecute()
    {
        // if they are set to IsBusy, don't try to wander around.
        var isBusy = false;
        theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy);

        if (isBusy)
            return false;
        // if you are supposed to stay put, don't wander. 
        if (EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Stay))
            return false;

        // If there's a target to fight, dont wander around. That's lame, sir.
        if (EntityUtilities.GetAttackOrRevengeTarget(theEntity.entityId) != null)
            return false;

        if (theEntity.Buffs.HasCustomVar("PathingCode") && theEntity.Buffs.GetCustomVar("PathingCode") == -1)
            return false;

        // If Pathing blocks does not exist, don't bother trying to do the enhanced wander code
        if (!EntityUtilities.CheckProperty(theEntity.entityId, "PathingBlocks"))
            if (!theEntity.Buffs.HasCustomVar("PathingCode"))
                return base.CanExecute();

        if (theEntity.sleepingOrWakingUp)
            return false;

        if (theEntity.GetTicksNoPlayerAdjacent() >= 120)
            return false;
        if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
            return false;
        var num = (int)(200f * executeWaitTime);
        if (GetRandom(1000) >= num)
            return false;

        if (manager.lookTime > 0f)
            return false;

        var newPosition = EntityUtilities.GetNewPositon(theEntity.entityId);
        if (newPosition == Vector3.zero)
        {
            DisplayLog("I do not have any pathing blocks");
            return base.CanExecute();
        }

        DisplayLog(" I have a new position I can path to: " + newPosition);

        //  For testing, change the target to this block, so we can see where the NPC intends to go.
        if (blShowPathFindingBlocks)
        {
            DisplayLog(" I have highlighted where I am going: " + newPosition);
            var strParticleName = "#@modfolder(0-SphereIICore):Resources/PathSmoke.unity3d?P_PathSmoke_X";
            if (!ParticleEffect.IsAvailable(strParticleName))
                ParticleEffect.LoadAsset(strParticleName);

            var supportBlock = GameManager.Instance.World.FindSupportingBlockPos(newPosition);
            BlockUtilitiesSDX.addParticles(strParticleName, new Vector3i(supportBlock));
        }

        if (SphereCache.LastBlock.ContainsKey(theEntity.entityId))
        {
            if (blShowPathFindingBlocks)
            {
                DisplayLog("I am changing the block back to the pathing block");
                var supportBlock = GameManager.Instance.World.FindSupportingBlockPos(position);
                BlockUtilitiesSDX.removeParticles(new Vector3i(supportBlock));
            }

            SphereCache.LastBlock[theEntity.entityId] = newPosition;
        }
        else
        {
            // Store the LastBlock position here, so we know what we can remove next time.
            SphereCache.LastBlock.Add(theEntity.entityId, newPosition);
        }

        position = newPosition;

        return true;
    }
}