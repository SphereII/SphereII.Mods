using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIMaslowLevel1SDX : EAIApproachSpot
{

    private static readonly string AdvFeatureClass = "AdvancedNPCFeatures";


    List<String> lstFoodItems = new List<String>();
    List<String> lstFoodBins = new List<String>();

    List<String> lstWaterItems = new List<String>();
    List<String> lstWaterBins = new List<String>();
    List<String> lstHomeBlocks = new List<String>();

    List<String> lstHungryBuffs = new List<String>();
    List<String> lstThirstyBuffs = new List<String>();

    List<String> lstSanitation = new List<String>();
    List<String> lstSanitationBuffs = new List<String>();

    List<String> lstBeds = new List<String>();
    List<String> lstProductionBuffs = new List<string>();
    readonly List<ProductionItem> lstProductionItem = new List<ProductionItem>();

    String strSanitationBlock = "";
    readonly String strProductionFinishedBuff = "";
    readonly int MaxDistance = 20;
    public int investigateTicks;
    // List<Vector3> lstWaterBlocks = new List<Vector3>();

    public bool hadPath;
    private readonly bool blDisplayLog = false;
    private Vector3 investigatePos;
    private Vector3 seekPos;
    private int pathRecalculateTicks;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
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

        lstFoodBins = ConfigureEntityClass("FoodBins", entityClass);
        lstFoodItems = ConfigureEntityClass("FoodItems", entityClass);
        lstWaterBins = ConfigureEntityClass("WaterBins", entityClass);
        lstWaterItems = ConfigureEntityClass("WaterItems", entityClass);


        lstHomeBlocks = ConfigureEntityClass("HomeBlocks", entityClass);
        lstHungryBuffs = ConfigureEntityClass("HungryBuffs", entityClass);
        lstThirstyBuffs = ConfigureEntityClass("ThirstyBuffs", entityClass);

        lstSanitation = ConfigureEntityClass("ToiletBlocks", entityClass);
        lstSanitationBuffs = ConfigureEntityClass("SanitationBuffs", entityClass);

        if (entityClass.Properties.Values.ContainsKey("SanitationBlock"))
            strSanitationBlock = entityClass.Properties.Values["SanitationBlock"];

        lstBeds = ConfigureEntityClass("Beds", entityClass);
        lstProductionBuffs = ConfigureEntityClass("ProductionFinishedBuff", entityClass);


        if (entityClass.Properties.Classes.ContainsKey("ProductionItems"))
        {
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["ProductionItems"];
            foreach (KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                ProductionItem item = new ProductionItem();
                item.item = ItemClass.GetItem(keyValuePair.Key, false);
                item.Count = int.Parse(dynamicProperties3.Values[keyValuePair.Key]);


                String strCvar = "Nothing";
                if (dynamicProperties3.Params1.TryGetValue(keyValuePair.Key, out strCvar))
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
        if (entityClass.Properties.Values.ContainsKey(strKey))
        {
            string strTemp = entityClass.Properties.Values[strKey].ToString();
            string[] array = strTemp.Split(new char[]
            {
                ','
            });
            for (int i = 0; i < array.Length; i++)
            {
                if (TempList.Contains(array[i].ToString()))
                    continue;
                TempList.Add(array[i].ToString());
            }

        }
        return TempList;

    }

    // Determines if this AI task can even start. This is based on the thirsty and hunger buffs
    public override bool CanExecute()
    {
        // Disable maslow for animals.
        if (theEntity is EntityAliveFarmingAnimalSDX)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "AnimalMaslow"))
                return false;
        }
        // disable maslow for NPCs

        else if (theEntity is EntityAliveSDX)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "NPCMaslow"))
                return false;
        }
        bool result = false;

        if (theEntity.IsSleeping)
            return false;


        if (!CanContinue())
        {
            theEntity.SetInvestigatePosition(Vector3.zero, 0);
            return false;
        }
        //// If there's no buff incentive, don't execute.
        //if (!CheckIncentive(this.lstThirstyBuffs)
        //    && !CheckIncentive(this.lstHungryBuffs)
        //    && !CheckIncentive(this.lstSanitationBuffs)
        //    && !CheckIncentive(this.lstBedTimeBuffs)
        //    //&& !CheckIfShelterNeeded()
        //    )
        //{

        //}
        if (!theEntity.HasInvestigatePosition)
        {
            if (CheckForFoodBin())  // Search for food if its hungry.
                result = true;
            else if (CheckForWaterBlock()) // Search for water.
                result = true;
            //else if (CheckForSanitation())  // Potty time?
            //    result = true;
            else if (CheckForProductionBuff())
                result = true;
            else if (CheckForShelter()) // Check for shelder.
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
        if (result && theEntity.HasInvestigatePosition)
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
        if (path != null)
        {
            hadPath = true;
            theEntity.moveHelper.CalcIfUnreachablePos();
        }
        Vector3 lookPosition = investigatePos;
        lookPosition.y += 0.8f;
        theEntity.SetLookPosition(lookPosition);

        // if the entity is blocked by anything, switch it around and tell it to find a new path.
        if (theEntity.moveHelper.BlockedTime > 1f)
        {
            pathRecalculateTicks = 0;
            theEntity.SetLookPosition(lookPosition - Vector3.back);
        }
        if (--pathRecalculateTicks <= 0)
        {
            updatePath();
        }
    }

    private void updatePath()
    {
        if (PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
            return;

        pathRecalculateTicks = 40 + theEntity.rand.RandomRange(20);
        PathFinderThread.Instance.FindPath(theEntity, seekPos, theEntity.GetMoveSpeed(), false, this);
    }

    // Checks a list of buffs to see if there's an incentive for it to execute.
    public virtual bool CheckIncentive(List<String> Incentives)
    {
        foreach (String strIncentive in Incentives)
        {
            if (theEntity.Buffs.HasBuff(strIncentive))
                return true;
        }
        return false;
    }

    public bool CanContinue()
    {
        // If there's no buff incentive, or no nearby water block, don't bother looking for water.
        if (!CheckIncentive(lstThirstyBuffs) && !CheckIncentive(lstHungryBuffs) && !CheckIncentive(lstSanitationBuffs) && !CheckIfShelterNeeded() && !CheckIncentive(lstProductionBuffs))
            return false;

        // if they are already healing for their water or health, don't try to add anymore.
        if (theEntity.Buffs.HasBuff("buffhealwatermax") || theEntity.Buffs.HasBuff("buffhealstaminamax"))
            return false;

        return true;
    }
    public override bool Continue()
    {

        if (!CanContinue())
        {
            theEntity.SetInvestigatePosition(Vector3.zero, 0);
            return false;
        }
        PathNavigate navigator = theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (hadPath && path == null)
            return false;

        if (++investigateTicks > 40)
        {
            investigateTicks = 0;
            return false;
            //if (!this.theEntity.HasInvestigatePosition)
            //    return false; // no invesitgative position

            //float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
            //if (sqrMagnitude >= 4f)
            //{
            //    return false; // not close enough.
            //}
        }

        float sqrMagnitude2 = (seekPos - theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 <= 4f || (path != null && path.isFinished()))
        {
            return PerformAction();
        }
        return true;
    }

    // Virtual methods to overload, so we can choose what kind of action to take.
    public virtual bool PerformAction()
    {
        DisplayLog("PerformAction() ");
        // Look at the target.

        //if (this.investigatePos != Vector3.zero)
        //{
        theEntity.SetLookPosition(seekPos);

        //    Ray lookRay = new Ray(this.theEntity.position, theEntity.GetLookVector());
        //    if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
        //        return false; // Not seeing the target.

        //    if (!Voxel.voxelRayHitInfo.bHitValid)
        //        return false; // Missed the target. Overlooking?
        //}
        DisplayLog("Before: " + theEntity.ToString());

        BlockValue checkBlock = theEntity.world.GetBlock(new Vector3i(seekPos.x, seekPos.y, seekPos.z));

        // Original hand item.
        ItemClass original = theEntity.inventory.holdingItem;

        // Look at the water, then execute the action on the empty jar.
        theEntity.SetLookPosition(seekPos);

        // Execute the drinking process
        if (CheckIncentive(lstThirstyBuffs))
        {
            DisplayLog("Thirsty Check Block: " + checkBlock.Block.GetBlockName());

            ItemValue item = null;


            // Is it a water block?
            if (checkBlock.Block.blockMaterial.IsLiquid)
            {
                // This is the actual item we want to drink out of. The above is just to deplete the water source.
                theEntity.inventory.SetBareHandItem(ItemClass.GetItem("drinkJarEmpty", false));
                theEntity.Use(true);
                theEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));

                return true;
            }
            else if (lstWaterBins.Contains(checkBlock.Block.GetBlockName()))  // If the water bins are configured, then look inside for something to drink. This is for NPCs, rather than cows.
            {
                DisplayLog(" Checking water Bin: " + checkBlock.Block.GetBlockName());
                TileEntityLootContainer tileEntityLootContainer = theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
                if (tileEntityLootContainer == null)
                    return false; // it's not a loot container.


                // Check if it has any water in it.
                if (CheckContents(tileEntityLootContainer, lstWaterItems, "Water") != null)
                {
                    DisplayLog(" Found a water item");
                    item = GetItemFromContainer(tileEntityLootContainer, lstWaterItems, "Water");
                }
            }

            // Check the back pack
            else if (CheckContents(theEntity.lootContainer, lstWaterItems, "Water") != null)
            {
                DisplayLog(" Checking NPCs backpack ");
                item = GetItemFromContainer(theEntity.lootContainer, lstWaterItems, "Water");
            }

            if (item != null)
            {

                DisplayLog(" Drinking: " + item.ItemClass.GetItemName());
                // Hold the food item.
                theEntity.inventory.SetBareHandItem(item);
                theEntity.Attack(true);
                // We want to consume the food, but the consumption of food isn't supported on the non-players, so just fire off the buff 
                theEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
                theEntity.FireEvent(MinEventTypes.onSelfHealedSelf);

                DisplayLog(" Drinking");
                // restore the hand item.
                theEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));

                return true;
            }



            // see if the block is an entity, rather than a watering hold. 
            float milkLevel = GetEntityWater();
            if (milkLevel > 0)
            {
                if (theEntity.Buffs.HasCustomVar("Mother"))
                {
                    DisplayLog("Checking For mother");
                    int MotherID = (int)theEntity.Buffs.GetCustomVar("Mother");
                    EntityAliveSDX MotherEntity = theEntity.world.GetEntity(MotherID) as EntityAliveSDX;
                    if (MotherEntity)
                    {
                        DisplayLog(" Draining Mommy of milk");
                        MotherEntity.Buffs.SetCustomVar("MilkLevel", 0f, true);
                        theEntity.Buffs.SetCustomVar("$foodAmountAdd", 50f, true);
                        theEntity.Buffs.SetCustomVar("$waterAmountAdd", 50f, true);
                    }
                }
            }
            // This is the actual item we want to drink out of. The above is just to deplete the water source.
            theEntity.inventory.SetBareHandItem(ItemClass.GetItem("drinkJarBoiledWater", false));
            theEntity.Attack(true);
            // Then we want to fire off the event on the water we are drinking.
            theEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);

            DisplayLog(" Drinking");
            // restore the hand item.
            theEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));

        }

        if (CheckIncentive(lstHungryBuffs))
        {
            DisplayLog("Hunger Check Block: " + checkBlock.Block.GetBlockName());
            ItemValue item = null;

            if (lstFoodBins.Contains(checkBlock.Block.GetBlockName()))
            {
                TileEntityLootContainer tileEntityLootContainer = theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
                if (tileEntityLootContainer == null)
                    return false; // it's not a loot container.


                // Check if it has any food on it.
                if (CheckContents(tileEntityLootContainer, lstFoodItems, "Food") != null)
                {
                    DisplayLog(" Found Food in food bin.");
                    item = GetItemFromContainer(tileEntityLootContainer, lstFoodItems, "Food");
                }
            }

            // Check the back pack
            else if (CheckContents(theEntity.lootContainer, lstFoodItems, "Food") != null)
            {
                DisplayLog(" Found Food in the backpack");
                item = GetItemFromContainer(theEntity.lootContainer, lstFoodItems, "Food");
            }

            if (item != null)
            {
                DisplayLog(" entity is eating: " + item.ItemClass.GetItemName());
                // Hold the food item.
                theEntity.inventory.SetBareHandItem(item);
                theEntity.Attack(true);
                // We want to consume the food, but the consumption of food isn't supported on the non-players, so just fire off the buff 
                theEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
                theEntity.FireEvent(MinEventTypes.onSelfHealedSelf);

                DisplayLog(" Eating");
                // restore the hand item.
                theEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));
            }
        }

        if (CheckIncentive(lstSanitationBuffs))
        {
            if (lstSanitation.Contains(checkBlock.Block.GetBlockName()))
                theEntity.Buffs.CVars["$solidWasteAmount"] = 0;

            // No toilets.
            if (lstSanitation.Count == 0)
            {
                // No Sanitation location? Let it go where you are.
                theEntity.Buffs.CVars["$solidWasteAmount"] = 0;

                // if there's no block, don't do anything.
                if (!String.IsNullOrEmpty(strSanitationBlock))
                {
                    Vector3i sanitationBlock = new Vector3i(theEntity.position);
                    theEntity.world.SetBlockRPC(sanitationBlock, Block.GetBlockValue(strSanitationBlock, false));
                }
            }
        }

        if (CheckIncentive(lstProductionBuffs))
        {
            if (lstBeds.Contains(checkBlock.Block.GetBlockName()))
            {
                DisplayLog(" My target block is in my approved list. ");
                TileEntityLootContainer tileEntityLootContainer = theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
                if (tileEntityLootContainer != null)
                {
                    DisplayLog(" It's a TileEntity. That's good.");
                    foreach (ProductionItem item in lstProductionItem)
                    {
                        DisplayLog(" Adding " + item.item.GetItemId());
                        // Add the item to the loot container, and reset the cvar, if it's available.
                        tileEntityLootContainer.AddItem(new ItemStack(item.item, item.Count));
                        if (!String.IsNullOrEmpty(item.cvar) && theEntity.Buffs.HasBuff(item.cvar))
                        {
                            theEntity.Buffs.CVars[item.cvar] = 0;
                            theEntity.Buffs.RemoveBuff(strProductionFinishedBuff, true);
                        }
                    }
                }
                else
                    DisplayLog(" Not a tile entity.");
            }
            else
                DisplayLog(" Not an approved block: " + checkBlock.Block.GetBlockName());
        }
        else
        {
            DisplayLog(" No Bed Time buff incentive");
        }

        DisplayLog("After: " + theEntity.ToString());
        theEntity.SetInvestigatePosition(Vector3.zero, 0);
        theEntity.Buffs.AddBuff("buffMaslowCoolDown", -1, true);
        return false;
    }


    // Grab a single item from the storage box, and remmove it.
    public ItemValue GetItemFromContainer(TileEntityLootContainer tileLootContainer, List<String> lstContents, String strSearchType)
    {

        ItemValue item = CheckContents(tileLootContainer, lstContents, strSearchType);
        if (item != null)
        {
            DisplayLog("GetItemFromContainer() Searching for item: " + item.ItemClass.Name);
            if (tileLootContainer.items != null)
            {
                ItemStack[] array = tileLootContainer.items;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].IsEmpty()) // nothing in the slot
                        continue;

                    // The animals will only eat the food they like best.
                    if (array[i].itemValue.ItemClass.Name == item.ItemClass.Name)
                    {
                        DisplayLog(" Found item to remove.");
                        // if there's only one left, remove the entire item; otherwise, decrease it.
                        if (array[i].count == 1)
                            tileLootContainer.RemoveItem(array[i].itemValue);
                        else
                            array[i].count--;

                        tileLootContainer.UpdateSlot(i, array[i]);
                        return array[i].itemValue;
                    }

                    // If there's no specific food items specified, then check for all water / food sources that the player can use.
                    if (lstContents.Count == 0)
                    {
                        if (IsConsumable(array[i].itemValue, strSearchType) != null)
                            return array[i].itemValue;
                    }

                }
            }
        }
        return null;
    }

    // Loops around an item to reach in the triggered effects, to see if it can satisfy food and water requirements.
    public ItemValue IsConsumable(ItemValue item, String strSearchType)
    {
        DisplayLog(" IsConsumable() " + item.ItemClass.Name);
        DisplayLog(" Checking for : " + strSearchType);
        foreach (var Action in item.ItemClass.Actions)
        {
            if (Action is ItemActionEat)
            {
                DisplayLog(" Action Is Eat");
                foreach (var EffectGroup in item.ItemClass.Effects.EffectGroups)
                {
                    foreach (var TriggeredEffects in EffectGroup.TriggeredEffects)
                    {
                        MinEventActionModifyCVar effect = TriggeredEffects as MinEventActionModifyCVar;
                        if (effect == null)
                            continue;

                        DisplayLog(" Checking Effects");
                        if (strSearchType == "Food")
                        {
                            if ((effect.cvarName == "$foodAmountAdd") || (effect.cvarName == "foodHealthAmount"))
                                return item;
                        }
                        else if (strSearchType == "Water")
                        {
                            if ((effect.cvarName == "$waterAmountAdd") || (effect.cvarName == "waterHealthAmount"))
                                return item;
                        }

                    }
                }
            }
        }

        return null;
    }


    // This will check if the food item actually exists in the container, before making the trip to it.
    public ItemValue CheckContents(TileEntityLootContainer tileLootContainer, List<String> lstContents, String strSearchType)
    {
        DisplayLog(" Check Contents of Container: " + tileLootContainer.ToString());
        DisplayLog(" TileEntity: " + tileLootContainer.items.Length);

        if (tileLootContainer.items != null)
        {
            ItemStack[] array = tileLootContainer.GetItems();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].IsEmpty())
                {
                    //DisplayLog(" Empty Slot");
                    continue;
                }
                DisplayLog(" Not Empty: " + array[i].itemValue.ItemClass.Name);
                // The animals will only eat the food they like best.
                if (lstContents.Contains(array[i].itemValue.ItemClass.Name))
                {
                    DisplayLog(" Found food item: " + array[i].itemValue.ItemClass.Name);
                    return array[i].itemValue;
                }

                DisplayLog(" Contents Count: " + lstContents.Count);
                // If there's no items to compare again, such as food items or water items, then do a action check if its food.
                if (lstContents.Count == 0)
                {
                    DisplayLog(" No Filtered list. Checking if its edible.");
                    if (IsConsumable(array[i].itemValue, strSearchType) != null)
                        return array[i].itemValue;
                }

            }
        }

        return null;
    }

    // Check if the entity needs to poop, and where it should go.
    public virtual bool CheckForSanitation()
    {
        if (!CheckIncentive(lstSanitationBuffs))
            return false;

        if (lstSanitation.Count > 0)
        {
            Vector3 TargetBlock = ScanForTileEntityInList(lstSanitation, new List<string>());
            if (TargetBlock == Vector3.zero)
                return false;

            theEntity.SetInvestigatePosition(TargetBlock, 120);
        }
        return true;
    }

    public virtual bool CheckForProductionBuff()
    {
        if (!CheckIncentive(lstProductionBuffs))
            return false;

        // If it's an egg producing entity, scan for a bed to hatch.
        if (theEntity.Buffs.HasBuff(strProductionFinishedBuff))
        {
            Vector3 TargetBlock = ScanForTileEntityInList(lstBeds, new List<String>());
            if (TargetBlock != Vector3.zero)
            {
                theEntity.SetInvestigatePosition(TargetBlock, 120);
                return true;
            }
        }
        return false;
    }
    // Check if the entity is hungry and if there's a food bin nearby.
    public virtual bool CheckForFoodBin()
    {
        if (!CheckIncentive(lstHungryBuffs))
            return false;

        DisplayLog(" Checking for food in inventory:");
        if (CheckContents(theEntity.lootContainer, lstFoodItems, "Food") != null)
        {
            DisplayLog(" Found Food in the backpack");
            // this.theEntity.SetInvestigatePosition(this.theEntity.position, 120);
            PerformAction();
            return true;
        }
        DisplayLog(" Checking For Food");
        Vector3 TargetBlock = ScanForTileEntityInList(lstFoodBins, lstFoodItems);
        if (TargetBlock == Vector3.zero)
            return false;

        theEntity.SetInvestigatePosition(TargetBlock, 120);
        return true;
    }

    // Scans for the water block in the area.
    public virtual bool CheckForWaterBlock()
    {
        if (!CheckIncentive(lstThirstyBuffs))
            return false;

        // This check is if we are a baby, and are seeking the mother to satisfy thirst.
        if (GetEntityWater() > 0f)
            return true;

        if (CheckContents(theEntity.lootContainer, lstWaterItems, "Water") != null)
        {
            DisplayLog(" Found Water in the backpack");
            //  this.theEntity.SetInvestigatePosition(this.theEntity.position, 120);
            PerformAction();
            return true;
        }
        Vector3 TargetBlock = ScanForBlockInList(lstWaterBins);
        if (TargetBlock == Vector3.zero)
            return false;

        theEntity.SetInvestigatePosition(TargetBlock, 120);
        return true;
    }
    public virtual Vector3 ScanForTileEntityInList(List<String> lstBlocks, List<String> lstContents)
    {
        // If there's no blocks to look for, don't do anything.
        if (lstBlocks.Count == 0)
            return Vector3.zero;

        DisplayLog("Scanning For Tile Entities: " + string.Join(", ", lstBlocks.ToArray()));
        DisplayLog(" Contents: " + string.Join(", ", lstContents.ToArray()));
        List<Vector3> localLists = new List<Vector3>();


        // Otherwise, search for your new home.
        Vector3i blockPosition = theEntity.GetBlockPosition();
        int num = World.toChunkXZ(blockPosition.x);
        int num2 = World.toChunkXZ(blockPosition.z);
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Chunk chunk = (Chunk)theEntity.world.GetChunkSync(num + j, num2 + i);
                if (chunk != null)
                {
                    // Grab all the Tile Entities in the chunk
                    DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
                    for (int k = 0; k < tileEntities.list.Count; k++)
                    {
                        TileEntityLootContainer tileEntity = tileEntities.list[k] as TileEntityLootContainer;
                        if (tileEntity != null)
                        {
                            BlockValue block = theEntity.world.GetBlock(tileEntity.ToWorldPos());
                            DisplayLog(" Found Block: " + block.Block.GetBlockName());
                            // if its not a listed block, then keep searching.
                            if (!lstBlocks.Contains(block.Block.GetBlockName()))
                                continue;

                            DisplayLog(" Tile Entity is in my Filtered list: " + block.Block.GetBlockName());
                            if (lstContents.Count > 0)
                            {
                                DisplayLog(" My Content List is Empty. Searcing for regular food items.");
                                if (CheckContents(tileEntity, lstContents, "Food") != null)
                                {
                                    DisplayLog(" Box has food contents: " + tileEntities.ToString());
                                    localLists.Add(tileEntity.ToWorldPos().ToVector3());

                                }
                                else
                                    DisplayLog(" Empty Container: " + tileEntities.ToString());
                            }
                            else
                                localLists.Add(tileEntity.ToWorldPos().ToVector3());

                        }
                    }
                }
            }
        }

        // DisplayLog(" Local List: " + string.Join(", ", localLists.ToArray()));

        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = theEntity.position;
        foreach (Vector3 block in localLists)
        {
            float dist = Vector3.Distance(block, currentPos);
            if (dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        return tMin;
    }

    // This will search for a mother entity to see it can satisfy its thirst from its mother, rather than a traditional water block.
    public virtual float GetEntityWater()
    {
        if (theEntity.Buffs.HasCustomVar("Mother"))
        {
            float MotherID = theEntity.Buffs.GetCustomVar("Mother");
            EntityAliveSDX MotherEntity = theEntity.world.GetEntity((int)MotherID) as EntityAliveSDX;
            if (MotherEntity)
            {
                DisplayLog(" My Mother is: " + MotherEntity.EntityName);
                if (MotherEntity.Buffs.HasCustomVar("MilkLevel"))
                {
                    DisplayLog("Heading to mommy");
                    float MilkLevel = MotherEntity.Buffs.GetCustomVar("MilkLevel");
                    theEntity.SetInvestigatePosition(theEntity.world.GetEntity((int)MotherID).position, 60);
                    return MilkLevel;
                }
            }

        }
        return 0f;
    }



    // Scans for the water block in the area.
    public virtual bool CheckForHomeBlock()
    {
        if (lstHomeBlocks.Count == 0)
            return false;

        Vector3 TargetBlock = ScanForBlockInList(lstHomeBlocks);
        if (TargetBlock == Vector3.zero)
            return false;

        Vector3i position;
        position.x = Utils.Fastfloor(TargetBlock.x);
        position.z = Utils.Fastfloor(TargetBlock.z);
        position.y = Utils.Fastfloor(TargetBlock.y);
        theEntity.setHomeArea(position, MaxDistance);
        return true;
    }
    // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
    public virtual Vector3 ScanForBlockInList(List<String> lstBlocks)
    {
        if (lstBlocks.Count == 0)
            return Vector3.zero;

        //if (!CheckIncentive(this.lstThirstyBuffs))
        //    return Vector3.zero;

        List<Vector3> localLists = new List<Vector3>();

        Vector3i blockPosition = theEntity.GetBlockPosition();
        Vector3i TargetBlockPosition = new Vector3i();

        for (var x = blockPosition.x - MaxDistance; x <= blockPosition.x + MaxDistance; x++)
        {
            for (var z = blockPosition.z - MaxDistance; z <= blockPosition.z + MaxDistance; z++)
            {
                for (var y = blockPosition.y - 5; y <= blockPosition.y + 5; y++)
                {
                    TargetBlockPosition.x = x;
                    TargetBlockPosition.y = y;
                    TargetBlockPosition.z = z;

                    BlockValue block = theEntity.world.GetBlock(TargetBlockPosition);
                    // if its not a listed block, then keep searching.
                    if (!lstBlocks.Contains(block.Block.GetBlockName()))
                        continue;

                    localLists.Add(TargetBlockPosition.ToVector3());
                }
            }
        }

        // Finds the closet block we matched with.
        Vector3 TargetBlock = new Vector3();
        TargetBlock = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = theEntity.position;
        foreach (Vector3 block in localLists)
        {
            float dist = Vector3.Distance(block, currentPos);
            if (dist < minDist)
            {
                TargetBlock = block;
                minDist = dist;
            }
        }

        return TargetBlock;
    }


    public virtual bool IsSheltered(Vector3i position)
    {
        // We only want to check air positions.
        if (theEntity.world.GetBlock(position).type != 0)
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
        //bool results = false;

        //// Night time, go hide!
        //if (this.theEntity.world.IsDaytime() == false)
        //    results = true;

        //// if the entity is already sheltered, then no need to go anywhere.
        //if (this.theEntity.Buffs.HasCustomVar("_sheltered"))
        //    if (this.theEntity.Buffs.GetCustomVar("_sheltered") < WeatherParams.EnclosureDetectionThreshold) // below sheltered level.
        //        results = true;
        //    else
        //        return false;



        //return results;
    }

    public virtual bool CheckForShelter()
    {
        if (!CheckIfShelterNeeded())
            return false;

        DisplayLog(" Shelter is needed: ");
        List<Vector3> ShelteredBlocks = new List<Vector3>();

        Vector3i blockPosition = World.worldToBlockPos(theEntity.GetPosition());
        IChunk chunkFromWorldPos = theEntity.world.GetChunkFromWorldPos(blockPosition);
        Vector3i currentBlock = new Vector3i();

        for (var x = blockPosition.x - MaxDistance; x <= blockPosition.x + MaxDistance; x++)
        {
            for (var z = blockPosition.z - MaxDistance; z <= blockPosition.z + MaxDistance; z++)
            {
                for (var y = blockPosition.y - 5; y <= blockPosition.y + 5; y++)
                {
                    currentBlock.x = x;
                    currentBlock.y = y;
                    currentBlock.z = z;
                    if (chunkFromWorldPos != null && y >= 0 && y < 255)
                    {
                        // Before adding the sheltered block to a valid list, let's make sure it's sheltered all around it.
                        // It's air checked, we don't really care about the other blocks around it.
                        if (IsSheltered(currentBlock)
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
        foreach (Vector3 block in ShelteredBlocks)
        {
            float dist = Vector3.Distance(block, currentPos);
            if (dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        if (tMin != Vector3.zero)
        {
            DisplayLog(" Close shelter is: " + tMin.ToString());
            theEntity.SetInvestigatePosition(tMin, 100);
            theEntity.setHomeArea(new Vector3i(tMin), 50);
            return true;
        }

        return false;
    }

}

