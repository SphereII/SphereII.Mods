using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIMaslowLevel1SDX_v2 : EAIApproachSpot
{

 
    List<String> lstHomeBlocks = new List<String>();

    List<String> lstSanitation = new List<String>();
    List<String> lstSanitationBuffs = new List<String>();

    List<String> lstBeds = new List<String>();
    List<String> lstProductionBuffs = new List<string>();

    List<ProductionItem> lstProductionItem = new List<ProductionItem>();

    String strSanitationBlock = "";
    String strProductionFinishedBuff = "";

    int MaxDistance = 20;
    public int investigateTicks;
    // List<Vector3> lstWaterBlocks = new List<Vector3>();

    public bool hadPath;
    private bool blDisplayLog = false;
    private Vector3 investigatePos;
    private Vector3 seekPos;
    private int pathRecalculateTicks;

    public void DisplayLog(String strMessage)
    {
        if(blDisplayLog)
            Debug.Log(theEntity.EntityName + ": " + theEntity.entityId + ": " + strMessage);
    }


    public struct ProductionItem
    {
        public ItemValue item;
        public int Count;
        public String cvar;
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        MutexBits = 3;
        executeDelay = 0.5f;

        // There is too many values that we need to read in from the entity, so we'll read them directly from the entityclass
        EntityClass entityClass = EntityClass.list[_theEntity.entityClass];

     

        lstHomeBlocks = ConfigureEntityClass("HomeBlocks", entityClass);

        lstSanitation = ConfigureEntityClass("ToiletBlocks", entityClass);
        lstSanitationBuffs = ConfigureEntityClass("SanitationBuffs", entityClass);

        if(entityClass.Properties.Values.ContainsKey("SanitationBlock"))
            strSanitationBlock = entityClass.Properties.Values["SanitationBlock"];

        lstBeds = ConfigureEntityClass("Beds", entityClass);
        lstProductionBuffs = ConfigureEntityClass("ProductionFinishedBuff", entityClass);


        if(entityClass.Properties.Classes.ContainsKey("ProductionItems"))
        {
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["ProductionItems"];
            foreach(KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                ProductionItem item = new ProductionItem();
                item.item = ItemClass.GetItem(keyValuePair.Key, false);
                item.Count = int.Parse(dynamicProperties3.Values[keyValuePair.Key]);


                String strCvar = "Nothing";
                if(dynamicProperties3.Params1.TryGetValue(keyValuePair.Key, out strCvar))
                    item.cvar = strCvar;

                lstProductionItem.Add(item);
                DisplayLog("Adding Production Item: " + keyValuePair.Key + " with a count of: " + item.Count + " and will reset: " + strCvar);
            }
        }
    }

    // helper Method to read the entity class and return a list of values based on the key
    // Example: <property name="WaterBins" value="water,waterMoving,waterStaticBucket,waterMovingBucket,terrWaterPOI" />
    public List<String> ConfigureEntityClass(String strKey, EntityClass entityClass)
    {
        List<String> TempList = new List<String>();
        if(entityClass.Properties.Values.ContainsKey(strKey))
        {
            string strTemp = entityClass.Properties.Values[strKey].ToString();
            string[] array = strTemp.Split(new char[]
            {
                ','
            });
            for(int i = 0; i < array.Length; i++)
            {
                if(TempList.Contains(array[i].ToString()))
                    continue;
                TempList.Add(array[i].ToString());
            }

        }
        return TempList;

    }

    // Determines if this AI task can even start. This is based on the thirsty and hunger buffs
    public override bool CanExecute()
    {
        bool result = false;

        if(theEntity.IsSleeping)
            return false;

        if(!CanContinue())
        {
            theEntity.SetInvestigatePosition(Vector3.zero, 0);
            return false;
        }
    
        if(!theEntity.HasInvestigatePosition)
        {
            if ( EntityUtilities.isEntityHungry( this.theEntity.entityId) &&
               ModGeneralUtilities.CheckForBin(this.theEntity.entityId, "Food"))
                result = true;

            if (EntityUtilities.isEntityThirsty(this.theEntity.entityId) &&
               ModGeneralUtilities.CheckForBin(this.theEntity.entityId, "Water"))
                result = true;

            if (EntityUtilities.isEntityHurt(this.theEntity.entityId) &&
                ModGeneralUtilities.CheckForBin(this.theEntity.entityId, "Health"))
                result = true;
        
            else if(CheckForProductionBuff())
                result = true;
            else if(CheckForShelter()) // Check for shelder.
                result = true;
            else
            {
                // check and see if you have a home block.
                CheckForHomeBlock();
                theEntity.SetInvestigatePosition(Vector3.zero, 0);
                result = false;
            }


        }

        // If We can continue, that means we've triggered the hunger or thirst buff. Investigate Position are set int he CheckFor.. methods
        // If we still don't have a position, then there's no food or water near by that satisfies our needs.
        if(result && theEntity.HasInvestigatePosition)
        {
            DisplayLog(" Investigate Pos: " + investigatePos + " Current Seek Time: " + investigateTicks + " Max Seek Time: " + theEntity.GetInvestigatePositionTicks() + " Seek Position: " + seekPos + " Target Block: " + theEntity.world.GetBlock(new Vector3i(investigatePos)).Block.GetBlockName());
            investigatePos = theEntity.InvestigatePosition;
            seekPos = theEntity.world.FindSupportingBlockPos(investigatePos);
            return true;
        }
        return false;

    }

    public override void Start()
    {
        hadPath = false;
        updatePath();
    }
    public override void Reset()
    {
        theEntity.navigator.clearPath();
        theEntity.SetLookPosition(Vector3.zero);
        manager.lookTime = 5f + UnityEngine.Random.value * 3f;
        manager.interestDistance = 2f;
    }
    public override void Update()
    {
        PathEntity path = theEntity.navigator.getPath();
        if(path != null)
        {
            hadPath = true;
            theEntity.moveHelper.CalcIfUnreachablePos(seekPos);
        }
        Vector3 lookPosition = investigatePos;
        lookPosition.y += 0.8f;
        theEntity.SetLookPosition(lookPosition);

        // if the entity is blocked by anything, switch it around and tell it to find a new path.
        if(theEntity.moveHelper.BlockedTime > 1f)
        {
            pathRecalculateTicks = 0;
            theEntity.SetLookPosition(lookPosition - Vector3.back);
        }
        if(--pathRecalculateTicks <= 0)
        {
            updatePath();
        }
    }

    private void updatePath()
    {
        if(PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
            return;

        pathRecalculateTicks = 40 + theEntity.GetRandom().Next(20);
        PathFinderThread.Instance.FindPath(theEntity, seekPos, theEntity.GetMoveSpeed(), false, this);
    }

    // Checks a list of buffs to see if there's an incentive for it to execute.
    public virtual bool CheckIncentive(List<String> Incentives)
    {
        foreach(String strIncentive in Incentives)
        {
            if(theEntity.Buffs.HasBuff(strIncentive))
                return true;
        }
        return false;
    }

    public bool CanContinue()
    {
        // if they are already healing for their water or health, don't try to add anymore.
        if (theEntity.Buffs.HasBuff("buffhealwatermax") || theEntity.Buffs.HasBuff("buffhealstaminamax"))
            return false;


        if (EntityUtilities.isEntityThirsty(this.theEntity.entityId))
            return true;

        if(EntityUtilities.isEntityHungry(this.theEntity.entityId))
            return true;

        //if(EntityUtilities.CheckIncentive(theEntity.entityId, lstSanitationBuffs, null))
        //    return true;

        if(EntityUtilities.CheckIncentive(theEntity.entityId, lstProductionBuffs, null))
            return true;

     
        return false;
    }
    public override bool Continue()
    {

        if(!CanContinue())
        {
            theEntity.SetInvestigatePosition(Vector3.zero, 0);
            return false;
        }
        PathNavigate navigator = theEntity.navigator;
        PathEntity path = navigator.getPath();
        if(hadPath && path == null)
            return false;

        if(++investigateTicks > 40)
        {
            investigateTicks = 0;
            return false;
        }

        //float sqrMagnitude2 = (seekPos - theEntity.position).sqrMagnitude;
        //if(sqrMagnitude2 <= 4f || (path != null && path.isFinished()))
        //{
        //    return PerformAction();
        //}
        return true;
    }

 

    public virtual bool CheckForSanitation()
    {
        if(!CheckIncentive(lstSanitationBuffs))
            return false;

        if(lstSanitation.Count > 0)
        {
            Vector3 TargetBlock = ModGeneralUtilities.ScanForBlockInList(this.theEntity.position, lstSanitation, 20);
            if(TargetBlock == Vector3.zero)
                return false;

            theEntity.SetInvestigatePosition(TargetBlock, 120);
        }
        return true;
    }

    public virtual bool CheckForProductionBuff()
    {
        if(!CheckIncentive(lstProductionBuffs))
            return false;

        // If it's an egg producing entity, scan for a bed to hatch.
        if(theEntity.Buffs.HasBuff(strProductionFinishedBuff))
        {
            Vector3 TargetBlock = ModGeneralUtilities.ScanForBlockInList(this.theEntity.position, lstBeds, 20);
            if(TargetBlock != Vector3.zero)
            {
                theEntity.SetInvestigatePosition(TargetBlock, 120);
                return true;
            }
        }
        return false;
    }
  




    // Scans for the water block in the area.
    public virtual bool CheckForHomeBlock()
    {
        if(lstHomeBlocks.Count == 0)
            return false;

        Vector3 TargetBlock = ModGeneralUtilities.ScanForBlockInList(this.theEntity.position, lstHomeBlocks, 20);

        if(TargetBlock == Vector3.zero)
            return false;

        Vector3i position;
        position.x = Utils.Fastfloor(TargetBlock.x);
        position.z = Utils.Fastfloor(TargetBlock.z);
        position.y = Utils.Fastfloor(TargetBlock.y);
        theEntity.setHomeArea(position, MaxDistance);
        return true;
    }
  

    public virtual bool IsSheltered(Vector3i position)
    {
        // We only want to check air positions.
        if(theEntity.world.GetBlock(position).type != 0)
            return false;

        float num = 1f;
        int x = position.x;
        int y = position.y;
        int z = position.z;
        IChunk chunkFromWorldPos = theEntity.world.GetChunkFromWorldPos(x, y, z);

        num = Mathf.Max(chunkFromWorldPos.GetLight(x, y, z, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos.GetLight(x, y + 1, z, Chunk.LIGHT_TYPE.SUN));
        num /= 15f;
        return (1f - num > 0.3f);

    }

    public virtual bool CheckIfShelterNeeded()
    {

        return false;
        bool results = false;

        // Night time, go hide!
        if(theEntity.world.IsDaytime() == false)
            results = true;

        // if the entity is already sheltered, then no need to go anywhere.
        if(theEntity.Buffs.HasCustomVar("_sheltered"))
            if(theEntity.Buffs.GetCustomVar("_sheltered") < WeatherParams.EnclosureDetectionThreshold) // below sheltered level.
                results = true;
            else
                return false;



        return results;
    }

    public virtual bool CheckForShelter()
    {
        if(!CheckIfShelterNeeded())
            return false;

        DisplayLog(" Shelter is needed: ");
        List<Vector3> ShelteredBlocks = new List<Vector3>();

        Vector3i blockPosition = World.worldToBlockPos(theEntity.GetPosition());
        IChunk chunkFromWorldPos = theEntity.world.GetChunkFromWorldPos(blockPosition);
        Vector3i currentBlock = new Vector3i();

        for(var x = blockPosition.x - MaxDistance; x <= blockPosition.x + MaxDistance; x++)
        {
            for(var z = blockPosition.z - MaxDistance; z <= blockPosition.z + MaxDistance; z++)
            {
                for(var y = blockPosition.y - 5; y <= blockPosition.y + 5; y++)
                {
                    currentBlock.x = x;
                    currentBlock.y = y;
                    currentBlock.z = z;
                    if(chunkFromWorldPos != null && y >= 0 && y < 255)
                    {
                        // Before adding the sheltered block to a valid list, let's make sure it's sheltered all around it.
                        // It's air checked, we don't really care about the other blocks around it.
                        if(IsSheltered(currentBlock)
                            && IsSheltered(currentBlock + Vector3i.back)
                            && IsSheltered(currentBlock + Vector3i.forward)
                            && IsSheltered(currentBlock + Vector3i.left)
                            && IsSheltered(currentBlock + Vector3i.right)
                            && IsSheltered(currentBlock + Vector3i.up))
                        {
                            ShelteredBlocks.Add(currentBlock.ToVector3());
                        }
                    }
                }
            }
        }



        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = theEntity.position;
        foreach(Vector3 block in ShelteredBlocks)
        {
            float dist = Vector3.Distance(block, currentPos);
            if(dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        if(tMin != Vector3.zero)
        {
            DisplayLog(" Close shelter is: " + tMin.ToString());
            theEntity.SetInvestigatePosition(tMin, 100);
            theEntity.setHomeArea(new Vector3i(tMin), 50);
            return true;
        }

        return false;
    }

}

