
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModGeneralUtilities
{


    static bool blDisplayLog = false;
    public static void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            UnityEngine.Debug.Log(strMessage);
    }
    public static Vector3 StringToVector3(string sVector)
    {
        if (String.IsNullOrEmpty(sVector))
            return Vector3.zero;

        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            sVector = sVector.Substring(1, sVector.Length - 2);

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    // helper Method to read the entity class and return a list of values based on the key
    // Example: <property name="WaterBins" value="water,waterMoving,waterStaticBucket,waterMovingBucket,terrWaterPOI" />
    public static List<String> ConfigureEntityClass(String strKey, EntityClass entityClass)
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

    public static bool CheckForBin(int EntityID, String StatType)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (!myEntity)
            return false;


        DisplayLog(myEntity.entityId + " CheckForBin() " + StatType);
        // There is too many values that we need to read in from the entity, so we'll read them directly from the entityclass
        EntityClass entityClass = EntityClass.list[myEntity.entityClass];

        List<String> lstContainers = new List<String>();
        List<String> lstItems = new List<String>();

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

        };

        // Checks the Entity's backpack to see if it can meet its needs there.
        ItemValue item = CheckContents(myEntity.lootContainer, lstItems, StatType);
        bool result = ConsumeProduct(EntityID, item);
        if (result)
        {
            DisplayLog("CheckForBin(): Found Item in my Back pack: " + item.ItemClass.GetItemName());
            return false;  // If we found something to consume, don't bother looking further.
        }
        // If the entity already has an investigative position, check to see if we are close enough for it.
        if (myEntity.HasInvestigatePosition)
        {
            DisplayLog(" CheckForBin(): Has Investigative position. Checking distance to bin");
            float sqrMagnitude2 = (myEntity.InvestigatePosition - myEntity.position).sqrMagnitude;
            if (sqrMagnitude2 <= 4f)
            {
                DisplayLog(" CheckForBin(): I am close to a bin.");
                Vector3i blockLocation = new Vector3i(myEntity.InvestigatePosition.x, myEntity.InvestigatePosition.y, myEntity.InvestigatePosition.z);
                BlockValue checkBlock = myEntity.world.GetBlock(blockLocation);
                DisplayLog(" CheckForBin(): Target Block is: " + checkBlock);
                TileEntityLootContainer myTile = myEntity.world.GetTileEntity(0, blockLocation) as TileEntityLootContainer;
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
        Vector3 TargetBlock = ScanForBlockInList(myEntity.position, lstContainers, Utils.Fastfloor(myEntity.GetSeeDistance()));
        if (TargetBlock == Vector3.zero)
            return false;

        DisplayLog(" Setting Target:" + GameManager.Instance.World.GetBlock(new Vector3i(TargetBlock)).Block.GetBlockName());
        myEntity.SetInvestigatePosition(TargetBlock, 120);
        return true;
    }

    public static bool ConsumeProduct(int EntityID, ItemValue item)
    {
        bool result = false;

        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (!myEntity)
            return false;

        // No Item, no consumption
        if (item == null)
            return false;


        DisplayLog(" ConsumeProduct() " + item.ItemClass.GetItemName());
        ItemClass original = myEntity.inventory.holdingItem;
        myEntity.inventory.SetBareHandItem(item);
        ItemAction itemAction = myEntity.inventory.holdingItem.Actions[0];
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
        myEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));

        return result;
    }
    // This will check if the food item actually exists in the container, before making the trip to it.
    public static ItemValue CheckContents(TileEntityLootContainer tileLootContainer, List<String> lstContents, String strSearchType)
    {
        DisplayLog(" Check Contents of Container: " + tileLootContainer.ToString());
        DisplayLog(" TileEntity: " + tileLootContainer.items.Length);
        ItemValue myItem = null;
        if (tileLootContainer.items != null)
        {
            ItemStack[] array = tileLootContainer.GetItems();
            for (int i = 0; i < array.Length; i++)
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
                {
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
        }

        DisplayLog("CheckContents(): No Items found.");
        return null;
    }

    // This help method returns the effect flags used on food items, so we can tell which ones are food, which ones affect thirst, which ones can heal, etc.
    // <triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" cvar="$waterAmountAdd" operation="add" value="20"/>
    //	<triggered_effect trigger = "onSelfPrimaryActionEnd" action="ModifyCVar" cvar="$foodAmountAdd" operation="add" value="50"/>
    //	<triggered_effect trigger = "onSelfPrimaryActionEnd" action="ModifyCVar" cvar="foodHealthAmount" operation="add" value="25"/>
    public static List<String> GetFoodEffects()
    {
        List<String> effects = new List<String>();
        effects.Add("$foodAmountAdd");
        effects.Add("foodHealthAmount");
        return effects;
    }

    // This help method returns the effect flags used on food items, so we can tell which ones are food, which ones affect thirst, which ones can heal, etc.
    // 		<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" cvar="$waterAmountAdd" operation="add" value="24"/>
    public static List<String> GetWaterEffects()
    {
        List<String> effects = new List<String>();
        effects.Add("$waterAmountAdd");
        effects.Add("waterHealthAmount");
        return effects;
    }

    // This help method returns the effect flags used on food items, so we can tell which ones are food, which ones affect thirst, which ones can heal, etc.
    // 		<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" target="self" cvar="medicalHealthAmount" operation="add" value="180"/> <!-- X -->
    public static List<String> GetHealingEffects()
    {
        List<String> effects = new List<String>();
        effects.Add("medicalHealthAmount");
        effects.Add("maxHealthAmount");
        return effects;
    }

    public static bool CheckItemForConsumable(MinEffectGroup group, List<String> cvars)
    {
        bool result = false;
        foreach (var TriggeredEffects in group.TriggeredEffects)
        {
            MinEventActionModifyCVar effect = TriggeredEffects as MinEventActionModifyCVar;
            if (effect == null)
                continue;

            DisplayLog(" Checking Effects: " + effect.cvarName + " " + effect.GetValueForDisplay());
            if (cvars.Contains(effect.cvarName))
                return true;
        }

        return result;

    }
    // Loops around an item to reach in the triggered effects, to see if it can satisfy food and water requirements.
    public static ItemValue IsConsumable(ItemValue item, String strSearchType)
    {
        if (item == null || item.ItemClass == null)
            return null;
        DisplayLog(" IsConsumable() " + item.ItemClass.Name);
        DisplayLog(" Checking for : " + strSearchType);

        // Since we don't really know what determines a food or drink item, we will look for the effects that each item gives, and compare it to a list of known
        // effects. 
        List<String> cvars = new List<String>();
        if (strSearchType == "Food")
            cvars = GetFoodEffects();
        else if (strSearchType == "Water")
            cvars = GetWaterEffects();
        else if (strSearchType == "Health")
            cvars = GetHealingEffects();
        else
            return null;

        foreach (var Action in item.ItemClass.Actions)
        {
            if (Action is ItemActionEat)
            {
                DisplayLog(" Action Is Eat");
                foreach (var EffectGroup in item.ItemClass.Effects.EffectGroups)
                {
                    foreach (var TriggeredEffects in EffectGroup.TriggeredEffects)
                    {
                        if (CheckItemForConsumable(EffectGroup, cvars))
                            return item;
                    }
                }
            }
        }

        return null;
    }
  
    // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
    public static List<Vector3> ScanForBlockInListHelper(Vector3 centerPosition, List<String> lstBlocks, int MaxDistance)
    {
        if (lstBlocks.Count == 0)
            return null;

        List<Vector3> localLists = new List<Vector3>();

        Vector3i TargetBlockPosition = new Vector3i();

        for (var x = (int)centerPosition.x - MaxDistance; x <= centerPosition.x + MaxDistance; x++)
        {
            for (var z = (int)centerPosition.z - MaxDistance; z <= centerPosition.z + MaxDistance; z++)
            {
                for (var y = (int)centerPosition.y - MaxDistance; y <= centerPosition.y + MaxDistance; y++)
                {
                    TargetBlockPosition.x = x;
                    TargetBlockPosition.y = y;
                    TargetBlockPosition.z = z;

                    BlockValue block = GameManager.Instance.World.GetBlock(TargetBlockPosition);
                    if (block.ischild)
                        continue;

                    // if its not a listed block, then keep searching.
                    if (!lstBlocks.Contains(block.Block.GetBlockName()))
                        continue;

                    localLists.Add(TargetBlockPosition.ToVector3() + Vector3.up);
                }
            }
        }
        return localLists;
    }

        // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
        public static Vector3 ScanForBlockInList(Vector3 centerPosition, List<String> lstBlocks, int MaxDistance)
    {
        List<Vector3> localLists = ScanForBlockInListHelper(centerPosition, lstBlocks, MaxDistance);
        return FindNearestBlock(centerPosition, localLists);

    }


    public static Vector3 FindNearestBlock(Vector3 fromPosition, List<Vector3> lstOfPositions)
    {
        if (lstOfPositions == null)
        {
            Debug.Log("Lst of Positions is empty");
            return Vector3.zero;
        }

        Debug.Log(" Positions: " + lstOfPositions.ToArray().ToString());
        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = 1f;
        foreach (Vector3 block in lstOfPositions)
        {
            if (block == Vector3.zero)
                continue;
            float dist = Vector3.Distance(block, fromPosition);
            if (dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        Debug.Log("tMin: " + tMin);
        if (tMin != Vector3.zero)
        {

            
            return tMin;

        }
        return Vector3.zero;

    }
}

