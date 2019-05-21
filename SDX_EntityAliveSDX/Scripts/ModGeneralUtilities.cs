
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModGeneralUtilities
{

    static bool blDisplayLog = false;

    public static void DisplayLog(string strMessage)
    {
        if(blDisplayLog)
            UnityEngine.Debug.Log(strMessage);
    }
    public static Vector3 StringToVector3(string sVector)
    {
        if(String.IsNullOrEmpty(sVector))
            return Vector3.zero;

        // Remove the parentheses
        if(sVector.StartsWith("(") && sVector.EndsWith(")"))
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
    public static bool CheckForBin(int EntityID, String StatType)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(!myEntity)
            return false;

        // There is too many values that we need to read in from the entity, so we'll read them directly from the entityclass
        EntityClass entityClass = EntityClass.list[myEntity.entityClass];

        List<String> lstContainers = new List<String>();
        List<String> lstItems = new List<String>();

        switch( StatType )
        {
            case "Food":
                lstContainers = ConfigureEntityClass("FoodBins", entityClass);
                lstItems = ConfigureEntityClass("FoodItems", entityClass);
                break;
            case "Water":
                lstContainers = ConfigureEntityClass("WaterBins", entityClass);
                lstItems = ConfigureEntityClass("WaterItems", entityClass);
                break;
            case "Health":
                DisplayLog("CheckForBin(): Health Items not implemented");
                return false;
            default:
                DisplayLog("CheckForBin(): Default is not implemented");
                return false;

        };

        ItemClass original = myEntity.inventory.holdingItem;
        ItemValue item = CheckContents(myEntity.lootContainer, lstItems, StatType);
        if(item != null)
        {
            DisplayLog(" Found " + StatType + " in the backpack: " + item.ItemClass.GetItemName());
            // Hold the food item.
            myEntity.inventory.SetBareHandItem(item);
            ItemAction itemAction = myEntity.inventory.holdingItem.Actions[0];
            if(itemAction != null)
            {
                DisplayLog("CheckForBin(): Hold Item has Action0. Executing..");
                itemAction.ExecuteAction(myEntity.inventory.holdingItemData.actionData[0], true);

                //// We want to consume the food, but the consumption of food isn't supported on the non-players, so just fire off the buff 
                ///
                DisplayLog("CheckForBin(): Trigger Events");
                myEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
                myEntity.FireEvent(MinEventTypes.onSelfHealedSelf);
            }

            DisplayLog(" CheckForBin(): Restoring hand item");
            myEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));

        }

     
        DisplayLog(" Checking For " + StatType );
        Vector3 TargetBlock = ScanForBlockInList(myEntity.position, lstContainers, Utils.Fastfloor(myEntity.GetSeeDistance()));
        if(TargetBlock == Vector3.zero)
            return false;

        myEntity.SetInvestigatePosition(TargetBlock, 120);
        return true;
    }

    // This will check if the food item actually exists in the container, before making the trip to it.
    public static ItemValue CheckContents(TileEntityLootContainer tileLootContainer, List<String> lstContents, String strSearchType)
    {
        DisplayLog(" Check Contents of Container: " + tileLootContainer.ToString());
        DisplayLog(" TileEntity: " + tileLootContainer.items.Length);

        if(tileLootContainer.items != null)
        {
            ItemStack[] array = tileLootContainer.GetItems();
            for(int i = 0; i < array.Length; i++)
            {
                if(array[i].IsEmpty())
                {
                    continue;
                }
                DisplayLog(" Not Empty: " + array[i].itemValue.ItemClass.Name);
                // The animals will only eat the food they like best.
                if(lstContents.Contains(array[i].itemValue.ItemClass.Name))
                {
                    DisplayLog(" Found food item: " + array[i].itemValue.ItemClass.Name);
                    return array[i].itemValue;
                }

                DisplayLog(" Contents Count: " + lstContents.Count);
                // If there's no items to compare again, such as food items or water items, then do a action check if its food.
                if(lstContents.Count == 0)
                {
                    DisplayLog(" No Filtered list. Checking if its edible.");
                    if(IsConsumable(array[i].itemValue, strSearchType) != null)
                        return array[i].itemValue;
                }

            }
        }

        return null;
    }

    // Loops around an item to reach in the triggered effects, to see if it can satisfy food and water requirements.
    public static ItemValue IsConsumable(ItemValue item, String strSearchType)
    {
        DisplayLog(" IsConsumable() " + item.ItemClass.Name);
        DisplayLog(" Checking for : " + strSearchType);
        foreach(var Action in item.ItemClass.Actions)
        {
            if(Action is ItemActionEat)
            {
                DisplayLog(" Action Is Eat");
                foreach(var EffectGroup in item.ItemClass.Effects.EffectGroups)
                {
                    foreach(var TriggeredEffects in EffectGroup.TriggeredEffects)
                    {
                        MinEventActionModifyCVar effect = TriggeredEffects as MinEventActionModifyCVar;
                        if(effect == null)
                            continue;

                        DisplayLog(" Checking Effects");
                        if(strSearchType == "Food")
                        {
                            if((effect.cvarName == "$foodAmountAdd") || (effect.cvarName == "foodHealthAmount"))
                                return item;
                        }
                        else if(strSearchType == "Water")
                        {
                            if((effect.cvarName == "$waterAmountAdd") || (effect.cvarName == "waterHealthAmount"))
                                return item;
                        }

                    }
                }
            }
        }

        return null;
    }

    // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
    public static Vector3 ScanForBlockInList(Vector3 centerPosition, List<String> lstBlocks, int MaxDistance)
    {
        if(lstBlocks.Count == 0)
            return Vector3.zero;

        List<Vector3> localLists = new List<Vector3>();

        Vector3i TargetBlockPosition = new Vector3i();

        for(var x = (int)centerPosition.x - MaxDistance; x <= centerPosition.x + MaxDistance; x++)
        {
            for(var z = (int)centerPosition.z - MaxDistance; z <= centerPosition.z + MaxDistance; z++)
            {
                for(var y = (int)centerPosition.y - 5; y <= centerPosition.y + 5; y++)
                {
                    TargetBlockPosition.x = x;
                    TargetBlockPosition.y = y;
                    TargetBlockPosition.z = z;

                    BlockValue block = GameManager.Instance.World.GetBlock(TargetBlockPosition);
                    // if its not a listed block, then keep searching.
                    if(!lstBlocks.Contains(block.Block.GetBlockName()))
                        continue;

                    localLists.Add(TargetBlockPosition.ToVector3());
                }
            }
        }

        return FindNearestBlock(centerPosition, localLists);
   
    }


    public static Vector3 FindNearestBlock(Vector3 fromPosition, List<Vector3> lstOfPositions)
    {
        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        foreach(Vector3 block in lstOfPositions)
        {
            float dist = Vector3.Distance(block, fromPosition);
            if(dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        if(tMin != Vector3.zero)
        {

            return tMin;

        }
        return Vector3.zero;
    
}
}

