using GamePath;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/****************************
 * 
 * This class is functional, in that it'll help you to loot locations, but its not well tuned. the AI will take the zombie around unique paths to get otherwise
 * close loot boxes. As such this class isn't enabled by default.
 */
internal class EAILootLocationSDX : EAIApproachSpot
{
    private readonly bool blDisplayLog = false;
    private readonly List<TileEntityLootContainer> lstTileContainers = new List<TileEntityLootContainer>();

    private readonly Dictionary<int, PrefabInstance> prefabsAroundFar = new Dictionary<int, PrefabInstance>();
    private readonly Dictionary<int, PrefabInstance> prefabsAroundNear = new Dictionary<int, PrefabInstance>();
    private EntityAliveSDX entityAliveSDX;
    private bool hadPath;
    private Vector3 investigatePos;
    private int investigateTicks;
    private int pathCounter;
    private int pathRecalculateTicks;

    private PrefabInstance prefab;

    private Vector3 seekPos;
    private int taskTimeOut;

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " :" + theEntity.EntityName + ": " + strMessage);
    }


    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        entityAliveSDX = _theEntity as EntityAliveSDX;
    }

    public override bool CanExecute()
    {
        //  return false;
        DisplayLog("CanExecute()");
        var result = false;
        if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Loot);
            DisplayLog("CanExecute() Loot Task? " + result);
            if (result == false)
                return false;
        }

        if (FindBoundsOfPrefab())
        {
            DisplayLog(" Within the Bounds of a Prefab");
            ScanForTileEntityInList();
            DisplayLog(" Searching for closes container: " + lstTileContainers.Count);
            result = FindNearestContainer();
        }

        DisplayLog("CanExecute() End: " + result);
        return result;
    }


    public override bool Continue()
    {
        DisplayLog("CanContinue()");
        var result = false;
        if (entityAliveSDX)
        {
            result = EntityUtilities.CanExecuteTask(theEntity.entityId, EntityUtilities.Orders.Loot);
            DisplayLog("CanContinue() Loot Task? " + result);
            if (result == false)
                return false;
        }

        if (++taskTimeOut > 40)
        {
            taskTimeOut = 0;
            return false;
        }

        if (theEntity.Buffs.HasCustomVar("Owner"))
        {
            DisplayLog(" Checking my Leader");
            // Find out who the leader is.
            var PlayerID = (int)theEntity.Buffs.GetCustomVar("Owner");
            var myLeader = theEntity.world.GetEntity(PlayerID);
            var distance = theEntity.GetDistance(myLeader);
            DisplayLog(" Distance: " + distance);
            if (distance > theEntity.GetSeeDistance())
            {
                DisplayLog("I am too far away from my leader. Teleporting....");
                theEntity.SetPosition(myLeader.position);
                EntityUtilities.ExecuteCMD(theEntity.entityId, "FollowMe", myLeader as EntityPlayer);
            }
        }

        var navigator = theEntity.navigator;
        var path = navigator.getPath();
        if (hadPath && path == null)
        {
            DisplayLog(" Not Path to continue looting.");
            return false;
        }

        if (++investigateTicks > 40)
        {
            investigateTicks = 0;
            if (!theEntity.HasInvestigatePosition)
                result = FindNearestContainer();

            //float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
            //if (sqrMagnitude > 4f)
            //{
            //    DisplayLog(" Too far away from my investigate Position: " + sqrMagnitude);
            //    return false;
            //}
        }

        var sqrMagnitude2 = (seekPos - theEntity.position).sqrMagnitude;
        DisplayLog(" Seek Position: " + seekPos + " My Location: " + theEntity.position + " Magnitude: " + sqrMagnitude2);
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
        theEntity.SetLookPosition(seekPos);

        var lookRay = new Ray(theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
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
        DisplayLog(" Looking at: " + seekPos + " My position is: " + theEntity.position);
        var tileEntityLootContainer = theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
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

        DisplayLog("Did not loot the container.");


        return false;
    }

    private IEnumerator Loot()
    {
        DisplayLog(" Starting Looting...");
        theEntity.emodel.avatarController.UpdateBool("IsLooting", true);
        yield return new WaitForSeconds(10);
        DisplayLog(" End Looting...");
        theEntity.emodel.avatarController.UpdateBool("IsLooting", false);
    }

    public PrefabInstance FindPrefabsNear()
    {
        var pos = theEntity.position;

        EntityPlayer player = null;
        if (theEntity.Buffs.HasCustomVar("Owner"))
        {
            player = theEntity.world.GetEntity((int)theEntity.Buffs.GetCustomVar("Owner")) as EntityPlayerLocal;
        }
        else
        {
            DisplayLog("I do not have a leader.");
            return null;
        }

        if (player)
        {
            var dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
            if (dynamicPrefabDecorator == null)
            {
                DisplayLog("FindPrefabsNear(): No Prefab Decorator found");
                return null;
            }

            var position = player.position;
            var num = player.ChunkObserver == null ? GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) : player.ChunkObserver.viewDim;
            num = (num - 1) * 16;
            if (!player.isEntityRemote)
            {
                prefabsAroundFar.Clear();
                prefabsAroundNear.Clear();
                DisplayLog(" Entity is not remote. Grabbing prefab lists.");
                dynamicPrefabDecorator.GetPrefabsAround(position, num, 1000f, prefabsAroundFar, prefabsAroundNear);
                GameManager.Instance.prefabLODManager.UpdatePrefabsAround(prefabsAroundFar, prefabsAroundNear);
            }

            DisplayLog(" Checking Boundary Box");
            return prefabsAroundNear.Values.FirstOrDefault(d =>
                pos.x >= d.boundingBoxPosition.x && pos.x < d.boundingBoxPosition.x + d.boundingBoxSize.x && pos.z >= d.boundingBoxPosition.z && pos.z < d.boundingBoxPosition.z + d.boundingBoxSize.z);
        }

        DisplayLog(" No Prefabs");
        return null;
    }

    public bool FindBoundsOfPrefab()
    {
        var pos = theEntity.position;
        FindPrefabsNear();
        prefab = FindPrefabsNear();
        if (prefab == null)
        {
            DisplayLog(" I am not in a prefab. Returning false.");
            return false;
        }

        if (prefab.CheckForAnyPlayerHome(theEntity.world) != GameUtils.EPlayerHomeType.None)
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
        lstTileContainers.Clear();
        var blockPosition = theEntity.GetBlockPosition();

        var minX = prefab.boundingBoxPosition.x;
        var maxX = prefab.boundingBoxPosition.x + prefab.boundingBoxSize.x - 1;

        var minZ = prefab.boundingBoxPosition.z;
        var maxZ = prefab.boundingBoxPosition.z + prefab.boundingBoxSize.z - 1;

        var num = World.toChunkXZ(blockPosition.x);
        var num2 = World.toChunkXZ(blockPosition.z);
        for (var i = -1; i < 2; i++)
            for (var j = -1; j < 2; j++)
            {
                var chunk = (Chunk)theEntity.world.GetChunkSync(num + j, num2 + i);
                if (chunk != null)
                {
                    var chunkPos = chunk.GetWorldPos();
                    var worldX = chunkPos.x + i;
                    var worldZ = chunkPos.z + j;

                    // Out of bounds
                    if (worldX < minX || worldX > maxX || worldZ < minZ || worldZ > maxZ)
                        continue;

                    // Grab all the Tile Entities in the chunk
                    var tileEntities = chunk.GetTileEntities();
                    for (var k = 0; k < tileEntities.list.Count; k++)
                    {
                        var tileEntity = tileEntities.list[k] as TileEntityLootContainer;
                        if (tileEntity != null)
                        {
                            var block = theEntity.world.GetBlock(tileEntity.ToWorldPos());
                            if (tileEntity.bTouched)
                            {
                                DisplayLog(" This tile Entity has already been touched: " + tileEntities);
                                continue;
                            }

                            if (Block.list[block.type].HasTag(BlockTags.Door))
                            {
                                DisplayLog(" This tile entity is a door. ignoring.");
                                continue;
                            }

                            DisplayLog(" Loot Container: " + tileEntity + " Distance: " + Vector3.Distance(tileEntity.ToWorldPos().ToVector3(), theEntity.position));
                            lstTileContainers.Add(tileEntity);
                        }
                    }
                }
            }
    }


    // Grab a single item from the storage box, and remmove it.
    public void GetItemFromContainer(TileEntityLootContainer tileLootContainer)
    {
        var lookRay = new Ray(theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
            return; // Not seeing the target.

        if (!Voxel.voxelRayHitInfo.bHitValid)
            return; // Missed the target. Overlooking?

        var blockPos = tileLootContainer.ToWorldPos();
        lstTileContainers.Remove(tileLootContainer);

        DisplayLog(" Loot List: " + tileLootContainer.lootListName);
        if (string.IsNullOrEmpty(tileLootContainer.lootListName))
            return;
        if (tileLootContainer.bTouched)
            return;

        tileLootContainer.bTouched = true;
        tileLootContainer.bWasTouched = true;

        DisplayLog("Checking Loot Container: " + tileLootContainer);
        if (tileLootContainer.items != null)
        {
            var block = theEntity.world.GetBlock(blockPos);
            var lootContainerName = Localization.Get(Block.list[block.type].GetBlockName());
            theEntity.SetLookPosition(blockPos.ToVector3());

            DisplayLog(" Loot container is: " + lootContainerName);
            DisplayLog(" Loot Container has this many Slots: " + tileLootContainer.items.Length);

            EntityPlayer player = null;
            if (theEntity.Buffs.HasCustomVar("Owner"))
                player = theEntity.world.GetEntity((int)theEntity.Buffs.GetCustomVar("Owner")) as EntityPlayerLocal;

            if (!player)
                return;

            theEntity.MinEventContext.TileEntity = tileLootContainer;
            theEntity.FireEvent(MinEventTypes.onSelfOpenLootContainer);
            var state = UnityEngine.Random.state;
            UnityEngine.Random.InitState((int)(GameManager.Instance.World.worldTime % 2147483647UL));
            var array = LootContainer.GetLootContainer(tileLootContainer.lootListName).Spawn(Random, tileLootContainer.items.Length,
               player.Progression.GetLevel(), 0f, player, new FastTags(), false, false);
            UnityEngine.Random.state = state;
            for (var i = 0; i < array.Count; i++)
                if (theEntity.lootContainer.AddItem(array[i].Clone()))
                {
                    DisplayLog("Removing item from loot container: " + array[i].itemValue.ItemClass.Name);
                }
                else
                {
                    DisplayLog(" Could Not add Item to NPC inventory. " + tileLootContainer.items[i].itemValue);
                    if (theEntity is EntityAliveSDX)
                    {
                        EntityUtilities.ExecuteCMD(theEntity.entityId, "FollowMe", player);
                        return;
                    }
                }

            theEntity.FireEvent(MinEventTypes.onSelfLootContainer);
        }
    }

    public bool FindNearestContainer()
    {
        // Finds the closet block we matched with.
        var tMin = new Vector3();
        tMin = Vector3.zero;
        var minDist = Mathf.Infinity;
        var currentPos = theEntity.position;
        foreach (var block in lstTileContainers)
        {
            if (Vector3.Distance(block.ToWorldPos().ToVector3(), seekPos) < 2) continue;
            var dist = Vector3.Distance(block.ToWorldPos().ToVector3(), currentPos);
            DisplayLog(" FindNearestContainer(): " + theEntity.world.GetBlock(block.ToWorldPos()).Block.GetBlockName() + " Distance: " + dist);
            if (dist < minDist)
            {
                tMin = block.ToWorldPos().ToVector3();
                minDist = dist;
            }
        }

        if (tMin == Vector3.zero)
            return false;


        // this.theEntity.SetInvestigatePosition(tMin, 60);
        investigatePos = tMin;
        seekPos = tMin; // this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
        investigateTicks = 60;
        //        this.theEntity.moveHelper.SetMoveTo( tMin, false);
        //PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeedAggro(), true, this);
        DisplayLog(" Investigate Pos: " + investigatePos + " Current Seek Time: " + investigateTicks + " Max Seek Time: " + theEntity.GetInvestigatePositionTicks() + " Seek Position: " + seekPos +
                   " Target Block: " + theEntity.world.GetBlock(new Vector3i(investigatePos)).Block.GetBlockName());
        return true;
    }

    public virtual Vector3 GetMoveToLocation(float maxDist)
    {
        var vector = seekPos;
        // vector = this.theEntity.world.FindSupportingBlockPos(vector);
        if (maxDist > 0f)
        {
            var vector2 = new Vector3(theEntity.position.x, theEntity.position.y, theEntity.position.z);
            var vector3 = vector - vector2;
            var magnitude = vector3.magnitude;
            if (magnitude < 3f)
            {
                if (magnitude <= maxDist)
                {
                    var num = vector.y - theEntity.position.y;
                    if (num > 1.5f) return vector;
                    return vector2;
                }

                vector3 *= maxDist / magnitude;
                var vector4 = vector - vector3;
                vector4.y += 0.51f;
                var pos = World.worldToBlockPos(vector4);
                var type = theEntity.world.GetBlock(pos).type;
                var block = Block.list[type];
                //  if (!block.IsPathSolid && Physics.Raycast(vector4, Vector3.down, 1.02f, 1082195968)) return vector4;
            }
        }

        return vector;
    }

    public override void Update()
    {
        var path = theEntity.navigator.getPath();
        if (path != null)
        {
            hadPath = true;
            theEntity.moveHelper.CalcIfUnreachablePos();
        }

        var lookPosition = investigatePos;
        lookPosition.y += 0.8f;
        theEntity.SetLookPosition(lookPosition);
        if (--pathRecalculateTicks <= 0) updatePath();
    }

    private void updatePath()
    {
        if (!PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
        {
            var path = theEntity.navigator.getPath();
            if (path != null && path.NodeCountRemaining() <= 2) pathCounter = 0;
        }

        if (--pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
        {
            pathCounter = 6 + theEntity.rand.RandomRange(10);
            PathFinderThread.Instance.FindPath(theEntity, seekPos, theEntity.GetMoveSpeedAggro(), true, this);
        }
    }
}