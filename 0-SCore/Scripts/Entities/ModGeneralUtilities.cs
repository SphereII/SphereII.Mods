using System.Collections.Generic;
using UnityEngine;

public static class ModGeneralUtilities
{
    private static readonly bool blDisplayLog = false;

    public static void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(strMessage);
    }

    public static Vector3 StringToVector3(string sVector)
    {
        if (string.IsNullOrEmpty(sVector))
            return Vector3.zero;

        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            sVector = sVector.Substring(1, sVector.Length - 2);

        // split the items
        var sArray = sVector.Split(',');

        // store as a Vector3
        var result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    // helper Method to read the entity class and return a list of values based on the key
    // Example: <property name="WaterBins" value="water,waterMoving,waterStaticBucket,waterMovingBucket,terrWaterPOI" />
    public static List<string> ConfigureEntityClass(string strKey, EntityClass entityClass)
    {
        var TempList = new List<string>();
        if (entityClass.Properties.Values.ContainsKey(strKey))
        {
            var strTemp = entityClass.Properties.Values[strKey];
            var array = strTemp.Split(',');
            for (var i = 0; i < array.Length; i++)
            {
                if (TempList.Contains(array[i]))
                    continue;
                TempList.Add(array[i]);
            }
        }

        return TempList;
    }

    public static bool CheckForBin(int EntityID, string StatType)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (!myEntity)
            return false;


        DisplayLog(myEntity.entityId + " CheckForBin() " + StatType);
        // There is too many values that we need to read in from the entity, so we'll read them directly from the entityclass
        var entityClass = EntityClass.list[myEntity.entityClass];

        var lstContainers = new List<string>();
        var lstItems = new List<string>();

        switch (StatType)
        {
            case "Food":
                // If it isn't hungry, don't look for food.
                if (!EntityUtilities.isEntityHungry(EntityID))
                    return false;

                lstContainers = ConfigureEntityClass("FoodBins", entityClass);
                lstItems = ConfigureEntityClass("FoodItems", entityClass);
                break;
            case "Water":

                if (!EntityUtilities.isEntityThirsty(EntityID))
                    return false;

                lstContainers = ConfigureEntityClass("WaterBins", entityClass);
                lstItems = ConfigureEntityClass("WaterItems", entityClass);
                break;
            case "Health":
                if (!EntityUtilities.isEntityHurt(EntityID))
                    return false;

                DisplayLog("CheckForBin(): Health Items not implemented");
                return false;
            default:
                DisplayLog("CheckForBin(): Default is not implemented");
                return false;
        }

        ;

        // Checks the Entity's backpack to see if it can meet its needs there.
        var item = CheckContents(myEntity.lootContainer, lstItems, StatType);
        var result = ConsumeProduct(EntityID, item);
        if (result)
        {
            DisplayLog("CheckForBin(): Found Item in my Back pack: " + item.ItemClass.GetItemName());
            return false; // If we found something to consume, don't bother looking further.
        }

        // If the entity already has an investigative position, check to see if we are close enough for it.
        if (myEntity.HasInvestigatePosition)
        {
            DisplayLog(" CheckForBin(): Has Investigative position. Checking distance to bin");
            var sqrMagnitude2 = (myEntity.InvestigatePosition - myEntity.position).sqrMagnitude;
            if (sqrMagnitude2 <= 4f)
            {
                DisplayLog(" CheckForBin(): I am close to a bin.");
                var blockLocation = new Vector3i(myEntity.InvestigatePosition.x, myEntity.InvestigatePosition.y, myEntity.InvestigatePosition.z);
                var checkBlock = myEntity.world.GetBlock(blockLocation);
                DisplayLog(" CheckForBin(): Target Block is: " + checkBlock);
                var myTile = myEntity.world.GetTileEntity(0, blockLocation) as TileEntityLootContainer;
                if (myTile != null)
                {
                    item = CheckContents(myTile, lstItems, StatType);
                    if (item != null)
                        DisplayLog("CheckForBin() I retrieved: " + item.ItemClass.GetItemName());
                    result = ConsumeProduct(EntityID, item);
                    DisplayLog(" Did I consume? " + result);
                    return result;
                }
            }

            return false;
        }

        DisplayLog(" Scanning For " + StatType);
        var TargetBlock = ScanForBlockInList(myEntity.position, lstContainers, Utils.Fastfloor(myEntity.GetSeeDistance()));
        if (TargetBlock == Vector3.zero)
            return false;

        DisplayLog(" Setting Target:" + GameManager.Instance.World.GetBlock(new Vector3i(TargetBlock)).Block.GetBlockName());
        myEntity.SetInvestigatePosition(TargetBlock, 120);
        return true;
    }

    public static bool ConsumeProduct(int EntityID, ItemValue item)
    {
        var result = false;

        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (!myEntity)
            return false;

        // No Item, no consumption
        if (item == null)
            return false;


        DisplayLog(" ConsumeProduct() " + item.ItemClass.GetItemName());
        var original = myEntity.inventory.holdingItem;
        myEntity.inventory.SetBareHandItem(item);
        var itemAction = myEntity.inventory.holdingItem.Actions[0];
        if (itemAction != null)
        {
            myEntity.Attack(true);
            DisplayLog("ConsumeProduct(): Hold Item has Action0. Executing..");
            itemAction.ExecuteAction(myEntity.inventory.holdingItemData.actionData[0], true);

            //// We want to consume the food, but the consumption of food isn't supported on the non-players, so just fire off the buff 
            ///
            DisplayLog("ConsumeProduct(): Trigger Events");
            myEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
            myEntity.FireEvent(MinEventTypes.onSelfHealedSelf);


            myEntity.SetInvestigatePosition(Vector3.zero, 0);
        }

        DisplayLog(" ConsumeProduct(): Restoring hand item");
        myEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name));

        return result;
    }

    // This will check if the food item actually exists in the container, before making the trip to it.
    public static ItemValue CheckContents(TileEntityLootContainer tileLootContainer, List<string> lstContents, string strSearchType)
    {
        DisplayLog(" Check Contents of Container: " + tileLootContainer);
        DisplayLog(" TileEntity: " + tileLootContainer.items.Length);
        ItemValue myItem = null;
        if (tileLootContainer.items != null)
        {
            var array = tileLootContainer.GetItems();
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i].IsEmpty())
                    continue;

                DisplayLog(" Not Empty: " + array[i].itemValue.ItemClass.Name);
                // The animals will only eat the food they like best.
                if (lstContents.Contains(array[i].itemValue.ItemClass.Name))
                {
                    if (IsConsumable(array[i].itemValue, strSearchType) != null)
                        myItem = array[i].itemValue;
                }
                else if (lstContents.Count == 0)
                {
                    DisplayLog(" No Filtered list. Checking if its edible.");
                    if (IsConsumable(array[i].itemValue, strSearchType) != null)
                        myItem = array[i].itemValue;
                }

                if (myItem != null)
                    if (IsConsumable(myItem, strSearchType) != null)
                    {
                        DisplayLog(" My Item is consumable: " + myItem.ItemClass.GetItemName());
                        // if there's only one left, remove the entire item; otherwise, decrease it.
                        if (array[i].count == 1)
                            tileLootContainer.RemoveItem(array[i].itemValue);
                        else
                            array[i].count--;

                        tileLootContainer.UpdateSlot(i, array[i]);

                        return myItem;
                    }
            }
        }

        DisplayLog("CheckContents(): No Items found.");
        return null;
    }

    // This help method returns the effect flags used on food items, so we can tell which ones are food, which ones affect thirst, which ones can heal, etc.
    // <triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" cvar="$waterAmountAdd" operation="add" value="20"/>
    //	<triggered_effect trigger = "onSelfPrimaryActionEnd" action="ModifyCVar" cvar="$foodAmountAdd" operation="add" value="50"/>
    //	<triggered_effect trigger = "onSelfPrimaryActionEnd" action="ModifyCVar" cvar="foodHealthAmount" operation="add" value="25"/>
    public static List<string> GetFoodEffects()
    {
        var effects = new List<string>();
        effects.Add("$foodAmountAdd");
        effects.Add("foodHealthAmount");
        return effects;
    }

    // This help method returns the effect flags used on food items, so we can tell which ones are food, which ones affect thirst, which ones can heal, etc.
    // 		<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" cvar="$waterAmountAdd" operation="add" value="24"/>
    public static List<string> GetWaterEffects()
    {
        var effects = new List<string>();
        effects.Add("$waterAmountAdd");
        effects.Add("waterHealthAmount");
        return effects;
    }

    // This help method returns the effect flags used on food items, so we can tell which ones are food, which ones affect thirst, which ones can heal, etc.
    // 		<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" target="self" cvar="medicalHealthAmount" operation="add" value="180"/> <!-- X -->
    public static List<string> GetHealingEffects()
    {
        var effects = new List<string>();
        effects.Add("medicalHealthAmount");
        effects.Add("maxHealthAmount");
        return effects;
    }

    public static bool CheckItemForConsumable(MinEffectGroup group, List<string> cvars)
    {
        var result = false;
        foreach (var TriggeredEffects in group.TriggeredEffects)
        {
            var effect = TriggeredEffects as MinEventActionModifyCVar;
            if (effect == null)
                continue;

            DisplayLog(" Checking Effects: " + effect.cvarName + " " + effect.GetValueForDisplay());
            if (cvars.Contains(effect.cvarName))
                return true;
        }

        return result;
    }

    // Loops around an item to reach in the triggered effects, to see if it can satisfy food and water requirements.
    public static ItemValue IsConsumable(ItemValue item, string strSearchType)
    {
        if (item == null || item.ItemClass == null)
            return null;
        DisplayLog(" IsConsumable() " + item.ItemClass.Name);
        DisplayLog(" Checking for : " + strSearchType);

        // Since we don't really know what determines a food or drink item, we will look for the effects that each item gives, and compare it to a list of known
        // effects. 
        var cvars = new List<string>();
        if (strSearchType == "Food")
            cvars = GetFoodEffects();
        else if (strSearchType == "Water")
            cvars = GetWaterEffects();
        else if (strSearchType == "Health")
            cvars = GetHealingEffects();
        else
            return null;

        foreach (var Action in item.ItemClass.Actions)
            if (Action is ItemActionEat)
            {
                DisplayLog(" Action Is Eat");
                foreach (var EffectGroup in item.ItemClass.Effects.EffectGroups)
                    foreach (var TriggeredEffects in EffectGroup.TriggeredEffects)
                        if (CheckItemForConsumable(EffectGroup, cvars))
                            return item;
            }

        return null;
    }


    // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
    public static List<Vector3> ScanForTileEntityInChunksListHelper(Vector3 centerPosition, List<string> lstBlocks, int EntityID = -1, float maxDistance = -1)
    {
        if (lstBlocks == null || lstBlocks.Count == 0)
            lstBlocks = new List<string> { "PathingCube" };

        if (EntityID < 0)
            return null;

        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return null;

        // Check if the entity has a PathingCode already. If it doesn't, use a default of 0.
        var code = 0f;
        if (myEntity.Buffs.HasCustomVar("PathingCode"))
            code = EntityUtilities.GetCVarValue(EntityID, "PathingCode");

        var localLists = new List<Vector3>();
        Vector3i TargetBlockPosition;
        var blockPosition = new Vector3i(centerPosition);

        var num = World.toChunkXZ(blockPosition.x);
        var num2 = World.toChunkXZ(blockPosition.z);
        for (var i = -1; i < 2; i++)
            for (var j = -1; j < 2; j++)
            {
                var chunk = (Chunk)GameManager.Instance.World.GetChunkSync(num + j, num2 + i);
                if (chunk != null)
                {
                    var tileEntities = chunk.GetTileEntities();
                    for (var k = 0; k < tileEntities.list.Count; k++)
                    {
                        var tileEntity = tileEntities.list[k];
                        TargetBlockPosition = tileEntity.ToWorldPos();

                        if (maxDistance > 0)
                        {
                            if (myEntity.GetDistanceSq(TargetBlockPosition) > maxDistance)
                                continue;
                        }
                        // If it's a sign, check to see if there's a code on it.
                        var tileEntitySign = tileEntity as TileEntitySign;
                        if (tileEntitySign != null)
                        {
                            // If the sign is empty and the code is 0, then accept it as a path
                            var text = tileEntitySign.GetText();
                            if (string.IsNullOrEmpty(text) && code == 0)
                            {
                                localLists.Add(TargetBlockPosition.ToVector3());
                                continue;
                            }

                            foreach (var temp in text.Split(','))
                                if (code.ToString() == temp || code == 0)
                                {
                                    localLists.Add(TargetBlockPosition.ToVector3());
                                    break;
                                }
                        }
                        //else
                        //    continue;


                        //// if its not a listed block, then keep searching.
                        //if (!lstBlocks.Contains(block.Block.GetBlockName()))
                        //    continue;
                        //localLists.Add(TargetBlockPosition.ToVector3());
                    }
                }
            }

        return localLists;
    }

    public static List<Vector3> ScanAutoConfigurationBlocks(Vector3 centerPosition, List<string> lstBlocks, int MaxDistance = 4)
    {
        if (lstBlocks.Count == 0)
            return null;

        var localLists = new List<Vector3>();

        var TargetBlockPosition = new Vector3i();

        for (var x = (int)centerPosition.x - MaxDistance; x <= centerPosition.x + MaxDistance; x++)
        {
            for (var z = (int)centerPosition.z - MaxDistance; z <= centerPosition.z + MaxDistance; z++)
            {
                for (var y = (int)centerPosition.y - MaxDistance; y <= centerPosition.y + MaxDistance; y++)
                {
                    TargetBlockPosition.x = x;
                    TargetBlockPosition.y = y;
                    TargetBlockPosition.z = z;

                    var block = GameManager.Instance.World.GetBlock(TargetBlockPosition);
                    if (block.ischild)
                        continue;

                    // if its not a listed block, then keep searching.
                    if (!lstBlocks.Contains(block.Block.GetBlockName()))
                        continue;

                    if (GameManager.Instance.World.GetTileEntity(0, TargetBlockPosition) is TileEntitySign tileEntitySign)
                        localLists.Add(TargetBlockPosition.ToVector3());
                }
            }
        }

        return localLists;
    }

    // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
    public static List<Vector3> ScanForBlockInListHelper(Vector3 centerPosition, List<string> lstBlocks, int MaxDistance, int EntityID = -1)
    {
        if (lstBlocks.Count == 0)
            return null;

        var localLists = new List<Vector3>();

        var TargetBlockPosition = new Vector3i();
        if (EntityID < 0)
            return null;

        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return null;

        // Check if the entity has a PathingCode already. If it doesn't, use a default of 0.
        float code = 0;
        if (myEntity.Buffs.HasCustomVar("PathingCode"))
            code = EntityUtilities.GetCVarValue(EntityID, "PathingCode");
        else
            myEntity.Buffs.AddCustomVar("PathingCode", 0f);

        for (var x = (int)centerPosition.x - MaxDistance; x <= centerPosition.x + MaxDistance; x++)
            for (var z = (int)centerPosition.z - MaxDistance; z <= centerPosition.z + MaxDistance; z++)
                for (var y = (int)centerPosition.y - MaxDistance; y <= centerPosition.y + MaxDistance; y++)
                {
                    TargetBlockPosition.x = x;
                    TargetBlockPosition.y = y;
                    TargetBlockPosition.z = z;

                    var block = GameManager.Instance.World.GetBlock(TargetBlockPosition);
                    if (block.ischild)
                        continue;

                    // if its not a listed block, then keep searching.
                    if (!lstBlocks.Contains(block.Block.GetBlockName()))
                        continue;

                    var tileEntitySign = GameManager.Instance.World.GetTileEntity(0, TargetBlockPosition) as TileEntitySign;
                    if (tileEntitySign != null)
                    {
                        // If the sign is empty and the code is 0, then accept it as a path
                        var text = tileEntitySign.GetText();
                        if (string.IsNullOrEmpty(text) && code == 0)
                        {
                            localLists.Add(TargetBlockPosition.ToVector3() + Vector3.up);
                            continue;
                        }

                        foreach (var temp in text.Split(','))
                            if (code.ToString() == temp || code == 0)
                            {
                                localLists.Add(TargetBlockPosition.ToVector3() + Vector3.up);
                                break;
                            }

                        continue;
                    }

                    localLists.Add(TargetBlockPosition.ToVector3() + Vector3.up);
                }

        return localLists;
    }

    // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
    public static Vector3 ScanForBlockInList(Vector3 centerPosition, List<string> lstBlocks, int MaxDistance)
    {
        var localLists = ScanForBlockInListHelper(centerPosition, lstBlocks, MaxDistance);
        return FindNearestBlock(centerPosition, localLists);
    }


    public static Vector3 FindNearestBlock(Vector3 fromPosition, List<Vector3> lstOfPositions)
    {
        if (lstOfPositions == null)
            return Vector3.zero;

        // Finds the closet block we matched with.
        var tMin = new Vector3();
        tMin = Vector3.zero;
        var minDist = 1000f;
        foreach (var block in lstOfPositions)
        {
            if (block == Vector3.zero)
                continue;
            var dist = Vector3.Distance(block, fromPosition);
            if (dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        if (tMin != Vector3.zero)
            return tMin;
        return Vector3.zero;
    }
}