using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using GamePath;
/****************************
 * 
 * This class is functional, in that it'll help you to loot locations, but its not well tuned. the AI will take the zombie around unique paths to get otherwise
 * close loot boxes. As such this class isn't enabled by default.
 */
class EAILootLocationSDX : EAIApproachSpot
{
    private Vector3 investigatePos;

    private Vector3 seekPos;
    private bool hadPath;
    private int investigateTicks;
    private EntityAliveSDX entityAliveSDX;
    
    PrefabInstance prefab;
    List<TileEntityLootContainer> lstTileContainers = new List<TileEntityLootContainer>();
    private bool blDisplayLog = false;
    private int pathRecalculateTicks;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
    }

 
    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        entityAliveSDX = (_theEntity as EntityAliveSDX);
    }
    public override bool CanExecute()
    {
      //  return false;
        DisplayLog("CanExecute()");
        bool result = false;
        if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Loot);
            DisplayLog("CanExecute() Loot Task? " + result);
            if (result == false)
                return false;
        }

        if (FindBoundsOfPrefab())
        {
            DisplayLog(" Within the Bounds of a Prefab");
            ScanForTileEntityInList();
            DisplayLog(" Searching for closes container: " + this.lstTileContainers.Count);
            result = FindNearestContainer();
        }

        DisplayLog("CanExecute() End: " + result);
        return result;
    }


    public override bool Continue()
    {
        DisplayLog("CanContinue()");
        bool result = false;
        if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(this.theEntity.entityId, EntityUtilities.Orders.Loot);
            DisplayLog("CanContinue() Loot Task? " + result);
            if (result == false)
                return false;
        }

        if (++this.taskTimeOut > 40)
        {
            this.taskTimeOut = 0;
            return false;
        }
        if (this.theEntity.Buffs.HasCustomVar("Owner"))
            {
                DisplayLog(" Checking my Leader");
                // Find out who the leader is.
                int PlayerID = (int)this.theEntity.Buffs.GetCustomVar("Owner");
            Entity myLeader = this.theEntity.world.GetEntity(PlayerID);
            float distance = this.theEntity.GetDistance(myLeader);
                DisplayLog(" Distance: " + distance);
                if (distance > this.theEntity.GetSeeDistance())
                {
                    DisplayLog("I am too far away from my leader. Teleporting....");
                this.theEntity.SetPosition(myLeader.position, true);
                EntityUtilities.ExecuteCMD(this.theEntity.entityId, "FollowMe", myLeader as EntityPlayer);
                }

            }
      
        PathNavigate navigator = this.theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (this.hadPath && path == null)
        {
            DisplayLog(" Not Path to continue looting.");
            return false;
        }
        if (++this.investigateTicks > 40)
        {
            this.investigateTicks = 0;
            if (!this.theEntity.HasInvestigatePosition)
                result = FindNearestContainer();

            //float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
            //if (sqrMagnitude > 4f)
            //{
            //    DisplayLog(" Too far away from my investigate Position: " + sqrMagnitude);
            //    return false;
            //}
        }

        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        DisplayLog(" Seek Position: " + this.seekPos + " My Location: " + this.theEntity.position + " Magnitude: " + sqrMagnitude2 );
        if (sqrMagnitude2 < 4f)
        {
            DisplayLog("I'm at the loot container: " + sqrMagnitude2);
            CheckContainer();
            result = FindNearestContainer();
        }
        else if (path != null && path.isFinished())
        {

            result = FindNearestContainer();
        }

        DisplayLog("Continue() End: " + result);
        return result;
    }

    public bool CheckContainer()
    {
        this.theEntity.SetLookPosition(seekPos);

        Ray lookRay = new Ray(this.theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
        {
            DisplayLog(" Ray cast is invalid");
            return false; // Not seeing the target.
        }
        if (!Voxel.voxelRayHitInfo.bHitValid)
        {
            DisplayLog(" Look cast is not valid.");
            return false; // Missed the target. Overlooking?
        }
    //    float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        //if (sqrMagnitude2 > 1f)
        //{
        //    return false; // too far away from it
        //}
        DisplayLog(" Looking at: " + this.seekPos + " My position is: " + this.theEntity.position);
        TileEntityLootContainer tileEntityLootContainer = this.theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
        if (tileEntityLootContainer == null)
        {
            DisplayLog("No Loot container here.");
            return false;

        }

        GetItemFromContainer(tileEntityLootContainer);
        if (tileEntityLootContainer.IsEmpty())
        {
            DisplayLog(" Looted Container.");
            return true;
        }
        else
            DisplayLog("Did not loot the container.");


        return false;

    }

    IEnumerator Loot()
    {
        DisplayLog(" Starting Looting...");
        this.theEntity.emodel.avatarController.SetBool("IsLooting", true);
        yield return new WaitForSeconds(10);
        DisplayLog(" End Looting...");
        this.theEntity.emodel.avatarController.SetBool("IsLooting", false);


    }

    private Dictionary<int, PrefabInstance> prefabsAroundFar = new Dictionary<int, PrefabInstance>();
    private Dictionary<int, PrefabInstance> prefabsAroundNear = new Dictionary<int, PrefabInstance>();
    private int pathCounter;
    private int taskTimeOut;

    public PrefabInstance FindPrefabsNear()
    {
        var pos = this.theEntity.position;

        EntityPlayer player = null;
        if (this.theEntity.Buffs.HasCustomVar("Owner"))
                player = theEntity.world.GetEntity((int)this.theEntity.Buffs.GetCustomVar("Owner")) as EntityPlayerLocal;
        else
        {
            DisplayLog("I do not have a leader.");
            return null;
        }
        if (player)
        {
            DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
            if (dynamicPrefabDecorator == null)
            {
                DisplayLog("FindPrefabsNear(): No Prefab Decorator found");
                return null;
            }
            Vector3 position = player.position;
            int num = (player.ChunkObserver == null) ? GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) : player.ChunkObserver.viewDim;
            num = (num - 1) * 16;
            if (!player.isEntityRemote)
            {
                this.prefabsAroundFar.Clear();
                this.prefabsAroundNear.Clear();
                DisplayLog(" Entity is not remote. Grabbing prefab lists.");
                dynamicPrefabDecorator.GetPrefabsAround(position, (float)num, (float)1000f, this.prefabsAroundFar, this.prefabsAroundNear, true);
                GameManager.Instance.prefabLODManager.UpdatePrefabsAround(this.prefabsAroundFar, this.prefabsAroundNear);
            }

            DisplayLog(" Checking Boundary Box");
            return this.prefabsAroundNear.Values.FirstOrDefault(d => pos.x >= d.boundingBoxPosition.x && pos.x < d.boundingBoxPosition.x + d.boundingBoxSize.x && pos.z >= d.boundingBoxPosition.z && pos.z < d.boundingBoxPosition.z + d.boundingBoxSize.z);
        }

        DisplayLog(" No Prefabs");
        return null;
    }
    public bool FindBoundsOfPrefab()
    {
        var pos = this.theEntity.position;
        FindPrefabsNear();
        prefab = FindPrefabsNear();
        if (prefab == null)
        {
            DisplayLog(" I am not in a prefab. Returning false.");
            return false;
        }

        if (prefab.CheckForAnyPlayerHome(this.theEntity.world) != GameUtils.EPlayerHomeType.None)
        {
            DisplayLog(" This is a player's home. Not looting.");
            return false;
        }

        var prefabBounds = prefab.boundingBoxSize;

        return true;
    }

    public void ScanForTileEntityInList()
    {
        DisplayLog("ScanForTileEntityInList()");
        this.lstTileContainers.Clear();
        Vector3i blockPosition = this.theEntity.GetBlockPosition();

        var minX = prefab.boundingBoxPosition.x;
        var maxX = prefab.boundingBoxPosition.x + prefab.boundingBoxSize.x - 1;

        var minZ = prefab.boundingBoxPosition.z;
        var maxZ = prefab.boundingBoxPosition.z + prefab.boundingBoxSize.z - 1;

        int num = World.toChunkXZ(blockPosition.x);
        int num2 = World.toChunkXZ(blockPosition.z);
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Chunk chunk = (Chunk)theEntity.world.GetChunkSync(num + j, num2 + i);
                 if (chunk != null)
                {
                    var chunkPos = chunk.GetWorldPos();
                    var worldX = chunkPos.x + i;
                    var worldZ = chunkPos.z + j;

                    // Out of bounds
                    if(worldX < minX || worldX > maxX || worldZ < minZ || worldZ > maxZ)
                        continue;

                    // Grab all the Tile Entities in the chunk
                    DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
                    for (int k = 0; k < tileEntities.list.Count; k++)
                    {
                        TileEntityLootContainer tileEntity = tileEntities.list[k] as TileEntityLootContainer;
                        if (tileEntity != null)
                        {
                            BlockValue block = theEntity.world.GetBlock(tileEntity.ToWorldPos());
                            if (tileEntity.bTouched)
                            {
                                DisplayLog(" This tile Entity has already been touched: " + tileEntities.ToString());
                                continue;
                            }

                            if (Block.list[block.type].HasTag(BlockTags.Door))
                            {
                                DisplayLog(" This tile entity is a door. ignoring.");
                                continue;
                            }
                            DisplayLog(" Loot Container: " + tileEntity.ToString() + " Distance: " + Vector3.Distance(tileEntity.ToWorldPos().ToVector3(), this.theEntity.position));
                            this.lstTileContainers.Add(tileEntity);
                        }
                    }
                }
            }
        }
    }



    // Grab a single item from the storage box, and remmove it.
    public void GetItemFromContainer(TileEntityLootContainer tileLootContainer)
    {
        Ray lookRay = new Ray(this.theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
            return;  // Not seeing the target.

        if (!Voxel.voxelRayHitInfo.bHitValid)
            return; // Missed the target. Overlooking?

        Vector3i blockPos = tileLootContainer.ToWorldPos();
        this.lstTileContainers.Remove(tileLootContainer);

        DisplayLog(" Loot List: " + tileLootContainer.lootListIndex);
        if (tileLootContainer.lootListIndex <= 0)
            return;
        if (tileLootContainer.bTouched)
            return;

        tileLootContainer.bTouched = true;
        tileLootContainer.bWasTouched = true;

        DisplayLog("Checking Loot Container: " + tileLootContainer.ToString());
        if (tileLootContainer.items != null)
        {
            BlockValue block = this.theEntity.world.GetBlock(blockPos);
            String lootContainerName = Localization.Get(Block.list[block.type].GetBlockName());
            theEntity.SetLookPosition(blockPos.ToVector3());

            DisplayLog(" Loot container is: " + lootContainerName);
            DisplayLog(" Loot Container has this many Slots: " + tileLootContainer.items.Length);

            EntityPlayer player = null;
            if (this.theEntity.Buffs.HasCustomVar("Owner"))
                player = theEntity.world.GetEntity((int)this.theEntity.Buffs.GetCustomVar("Owner")) as EntityPlayerLocal;

            if (!player)
                return;

            theEntity.MinEventContext.TileEntity = tileLootContainer;
            theEntity.FireEvent(MinEventTypes.onSelfOpenLootContainer);
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState((int)(GameManager.Instance.World.worldTime % 2147483647UL));
            ItemStack[] array = LootContainer.lootList[tileLootContainer.lootListIndex].Spawn(this.Random, tileLootContainer.items.Length, EffectManager.GetValue(PassiveEffects.LootGamestage, null, (float)player.PartyGameStage, player, null, default(FastTags), true, true, true, true, 1, true), 0f, player, new FastTags());
            UnityEngine.Random.state = state;
            for (int i = 0; i < array.Length; i++)
            {
                if (this.theEntity.lootContainer.AddItem(array[i].Clone()))
                {
                    DisplayLog("Removing item from loot container: " + array[i].itemValue.ItemClass.Name);
                }
                else
                {
                    DisplayLog(" Could Not add Item to NPC inventory. " + tileLootContainer.items[i].itemValue.ToString());
                    if (theEntity is EntityAliveSDX)
                    {
                        EntityUtilities.ExecuteCMD(this.theEntity.entityId, "FollowMe", player);
                        return;
                    }

                }

            }
            theEntity.FireEvent(MinEventTypes.onSelfLootContainer);

        }

    }
    public bool FindNearestContainer()
    {

        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (TileEntityLootContainer block in this.lstTileContainers)
        {
            if ( Vector3.Distance( block.ToWorldPos().ToVector3(), this.seekPos ) < 2 )
            {
                continue;
            }
            float dist = Vector3.Distance(block.ToWorldPos().ToVector3(), currentPos);
            DisplayLog(" FindNearestContainer(): " + this.theEntity.world.GetBlock(block.ToWorldPos()).Block.GetBlockName() + " Distance: " + dist );
            if (dist < minDist)
            {
                tMin = block.ToWorldPos().ToVector3();
                minDist = dist;
            }
        }

        if (tMin == Vector3.zero)
            return false;



       // this.theEntity.SetInvestigatePosition(tMin, 60);
        this.investigatePos = tMin;
        this.seekPos = tMin;// this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
        this.investigateTicks = 60;
//        this.theEntity.moveHelper.SetMoveTo( tMin, false);
        //PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeedAggro(), true, this);
        DisplayLog(" Investigate Pos: " + this.investigatePos + " Current Seek Time: " + investigateTicks + " Max Seek Time: " + this.theEntity.GetInvestigatePositionTicks() + " Seek Position: " + this.seekPos + " Target Block: " + this.theEntity.world.GetBlock(new Vector3i(this.investigatePos)).Block.GetBlockName());
        return true;
    }

    public virtual Vector3 GetMoveToLocation(float maxDist)
    {
        Vector3 vector = this.seekPos;
       // vector = this.theEntity.world.FindSupportingBlockPos(vector);
        if (maxDist > 0f)
        {
            Vector3 vector2  = new Vector3( this.theEntity.position.x, this.theEntity.position.y, this.theEntity.position.z);
            Vector3 vector3 = vector - vector2;
            float magnitude = vector3.magnitude;
            if (magnitude < 3f)
            {
                if (magnitude <= maxDist)
                {
                    float num = vector.y - this.theEntity.position.y;
                    if (num > 1.5f)
                    {
                        return vector;
                    }
                    return vector2;
                }
                else
                {
                    vector3 *= maxDist / magnitude;
                    Vector3 vector4 = vector - vector3;
                    vector4.y += 0.51f;
                    global::Vector3i pos = global::World.worldToBlockPos(vector4);
                    int type = this.theEntity.world.GetBlock(pos).type;
                    global::Block block = global::Block.list[type];
                    if (!block.IsPathSolid && Physics.Raycast(vector4, Vector3.down, 1.02f, 1082195968))
                    {
                        return vector4;
                    }
                }
            }
        }
        return vector;
    }
    public override void Update()
    {
        GamePath.PathEntity path = this.theEntity.navigator.getPath();
        if (path != null)
        {
            this.hadPath = true;
            this.theEntity.moveHelper.CalcIfUnreachablePos(this.seekPos);
        }
        Vector3 lookPosition = this.investigatePos;
        lookPosition.y += 0.8f;
        this.theEntity.SetLookPosition(lookPosition);
        if (--this.pathRecalculateTicks <= 0)
        {
            this.updatePath();
        }
    }

    // Token: 0x06002E3B RID: 11835 RVA: 0x001422B4 File Offset: 0x001404B4
    private new void updatePath()
    {
        if (!PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            PathEntity path = this.theEntity.navigator.getPath();
            if (path != null && path.NodeCountRemaining() <= 2)
            {
                this.pathCounter = 0;
            }
        }
        if (--this.pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            this.pathCounter = 6 + this.theEntity.rand.RandomRange(10);
            PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeedAggro(), true, this);
        }
  
    }
}

