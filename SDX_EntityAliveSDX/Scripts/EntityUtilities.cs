
// General Purpose Entity Utilities to centralize general checks.
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class EntityUtilities
{

    public static void DisplayLog(string strMessage)
    {
        UnityEngine.Debug.Log(strMessage);
    }
    // These are the orders, used in cvars for the EAI Tasks. They are casted as floats.
    public enum Orders
    {
        Follow = 0,
        Stay = 1,
        Wander = 2,
        None = 3,
        SetPatrolPoint = 4,
        Patrol = 5,
        Hire = 6,
        Loot = 7
    }

    public static bool Hire(int EntityID, EntityPlayerLocal _player)
    {
        bool result = false;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity == null)
            return result;
        
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
        if(null != uiforPlayer)
        {
            if(uiforPlayer.xui.PlayerInventory.GetItemCount(myEntity.HireCurrency) >= myEntity.HireCost)
            {
                // Create the stack of currency
                ItemStack stack = new ItemStack(myEntity.HireCurrency, myEntity.HireCost);
                uiforPlayer.xui.PlayerInventory.RemoveItems(new ItemStack[] { stack }, 1);

                // Add the stack of currency to the NPC, and set its orders.
                myEntity.bag.AddItem(stack);
                SetOwner(EntityID, _player.entityId);
                return true;
            }
            else
            {
                GameManager.ShowTooltipWithAlert(_player, "You cannot afford me. I want " + myEntity.HireCost + " " + myEntity.HireCurrency, "ui_denied");
            }
        }
        return false;
    }

    // Returns the leader of the passed in entity ID.
    public static Entity GetLeader(int EntityID)
    {
        Entity leader = null;
        EntityAliveSDX currentEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(currentEntity)
        {
            if(currentEntity.Buffs.HasCustomVar("Leader"))
                leader = GameManager.Instance.World.GetEntity((int)currentEntity.Buffs.GetCustomVar("Leader"));
        }

        return leader;
    }

    // Owner can be different than the current leader
    public static Entity GetOwner(int EntityID)
    {
        Entity leader = null;
        EntityAliveSDX currentEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(currentEntity)
        {
            if(leader == null && currentEntity.Buffs.HasCustomVar("Owner"))
                leader = GameManager.Instance.World.GetEntity((int)currentEntity.Buffs.GetCustomVar("Owner"));
        }

        return leader;
    }

    public static Entity GetLeaderOrOwner(int EntityID)
    {
        Entity leader = GetLeader(EntityID);
        if(leader == null)
            leader = GetOwner(EntityID);

        return leader;
    }

    public static void SetLeader(int EntityID, int LeaderID)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        EntityAlive LeaderEntity = GameManager.Instance.World.GetEntity(LeaderID) as EntityAlive;
        if(myEntity && LeaderEntity)
        {
            myEntity.Buffs.SetCustomVar("Leader", (float)LeaderID, false);
            myEntity.moveSpeed = LeaderEntity.moveSpeed;
            myEntity.moveSpeedAggro = LeaderEntity.moveSpeedAggro;
            myEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
        }

    }

    public static void SetOwner(int EntityID, int LeaderID)
    {
        EntityAliveSDX currentEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(currentEntity)
            currentEntity.Buffs.SetCustomVar("Owner", (float)LeaderID, false);
    }

    public static void SetLeaderAndOwner(int EntityID, int LeaderID)
    {
        SetLeader(EntityID, LeaderID);
        SetOwner(EntityID, LeaderID);
        SetCurrentOrder(EntityID, Orders.Follow);
    }

    public static bool IsAnAlly(int EntityID, int AttackingID)
    {
        bool result = false;
        Entity myLeader = GetLeaderOrOwner(EntityID);

        // If my attacker is my leader, forgive him.
        if(myLeader)
            if(AttackingID == myLeader.entityId)
                return true;

        Entity theirLeader = GetLeaderOrOwner(AttackingID);
        if ( (myLeader && theirLeader)  && (myLeader.entityId == theirLeader.entityId))
        {
            UnityEngine.Debug.Log("My Leader is the same as their leader. My ID: " + EntityID + " Their ID: " + AttackingID);
            result = true;
        }
        return result;
    }

    public static bool isTame(int EntityID, EntityAlive _player)
    {
        Entity myLeader = GetLeaderOrOwner(EntityID);
        if(myLeader)
        {
            if(myLeader.entityId == _player.entityId)
                return true;
                 
        }
        return false;
    }

    public static void SetCurrentOrder(int EntityID, Orders order)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
            myEntity.Buffs.SetCustomVar("CurrentOrder", (float)order, false);
    }

    public static Orders GetCurrentOrder( int EntityID )
    {
        Orders currentOrder = Orders.None;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
        {
            if(myEntity.Buffs.HasCustomVar("CurrentOrder"))
                currentOrder = (Orders)myEntity.Buffs.GetCustomVar("CurrentOrder");
        }
        return currentOrder;
    }

    public static bool ExecuteCMD(int EntityID, String strCommand, EntityPlayer player)
    {
        DisplayLog("ExecuteCMD: " + strCommand);
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity == null )
            return false;

        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);

        Vector3 position = myEntity.position;

        // Restore it's walk speed to default.
        myEntity.RestoreSpeed();

        switch(strCommand)
        {
            case "ShowMe":
                GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, myEntity.ToString() + "\n\n\n\n\n", "ui_denied");
                break;
            case "ShowAffection":
                GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, "You gentle scratch and stroke the side of the animal.", "");
                break;
            case "FollowMe":
                myEntity.Buffs.SetCustomVar("Leader", player.entityId, true);
                myEntity.Buffs.SetCustomVar("CurrentOrder", (float)Orders.Follow, false);
                myEntity.moveSpeed = player.moveSpeed;
                myEntity.moveSpeedAggro = player.moveSpeedAggro;

                break;
            case "StayHere":
                myEntity.Buffs.SetCustomVar("CurrentOrder", (float)Orders.None, false);
                myEntity.GuardPosition = position;
                myEntity.moveHelper.Stop();
                break;
            case "GuardHere":
                myEntity.Buffs.SetCustomVar("CurrentOrder", (float)Orders.Stay, false);
                myEntity.SetLookPosition(player.GetLookVector());
                myEntity.GuardPosition = position;
                myEntity.moveHelper.Stop();
                myEntity.GuardLookPosition = player.GetLookVector();
                break;
            case "Wander":
                myEntity.Buffs.SetCustomVar("CurrentOrder", (float)Orders.Wander, false);
                break;
            case "SetPatrol":
                myEntity.Buffs.SetCustomVar("Leader", player.entityId, true);
                myEntity.Buffs.SetCustomVar("CurrentOrder", (float)Orders.SetPatrolPoint, false);
                myEntity.moveSpeed = player.moveSpeed;
                myEntity.moveSpeedAggro = player.moveSpeedAggro;
                myEntity.PatrolCoordinates.Clear(); // Clear the existing point.
                break;
            case "Patrol":
                myEntity.Buffs.SetCustomVar("CurrentOrder", (float)Orders.Patrol, false);
                break;
            case "Hire":
                bool result = Hire(EntityID, player as EntityPlayerLocal);
                break;
            case "OpenInventory":
                GameManager.Instance.TELockServer(0, myEntity.GetBlockPosition(), EntityID, player.entityId);
                uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
                if(myEntity.lootContainer == null)
                    DisplayLog(" Loot Container is null");

                DisplayLog(" Get Open Time");
                DisplayLog("Loot Container: " + myEntity.lootContainer.ToString());
                myEntity.lootContainer.lootListIndex = 62;
                DisplayLog(" Loot List: " + myEntity.lootContainer.lootListIndex);

                DisplayLog(myEntity.lootContainer.GetOpenTime().ToString());
                lootContainerOpened(myEntity.lootContainer, uiforPlayer, player.entityId);

                break;
            case "Loot":
                myEntity.Buffs.SetCustomVar("CurrentOrder", (float)Orders.Loot, false);
                myEntity.Buffs.RemoveCustomVar("Leader");
                break;

        }

        if(myEntity.Buffs.HasCustomVar("CurrentOrder"))
        {
            myEntity.currentOrder = (Orders)myEntity.Buffs.GetCustomVar("CurrentOrder");
            DisplayLog(" Setting Current Order: " + myEntity.currentOrder);
        }
        return true;

    }
    public static bool CanExecuteTask(int EntityID, EntityUtilities.Orders order)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
        {
            if(GetCurrentOrder(EntityID) != order)
                return false;

            EntityPlayerLocal leader = EntityUtilities.GetLeader(EntityID) as EntityPlayerLocal;
            float Distance = 0f;
            if(leader)
                Distance = myEntity.GetDistance(leader);

            // If we have an attack or revenge target, don't execute task
            if(myEntity.GetAttackTarget() != null && myEntity.GetAttackTarget().IsAlive())
            {
                // If we have a leader and they are far away, abandon the fight and go after your leader.
                DisplayLog("CanExecuteTask(): I have an attack target and a leader. My leader is this far away: " + Distance);
                if(Distance > 20)
                {
                    DisplayLog("CanExecuteTask(): Leaving my attack target.");
                    myEntity.SetAttackTarget(null, 0);
                    return true;
                }
                DisplayLog("CanExecuteTask():  There is an attack target set. Not executing Order: " + order.ToString());
                DisplayLog(" Attack Target: " + myEntity.GetAttackTarget().ToString());
                return false;

            }

            if(myEntity.GetRevengeTarget() != null && myEntity.GetRevengeTarget().IsAlive())
            {
                DisplayLog("CanExecuteTask(): I have an Revenge target and a leader. My leader is this far away: " + Distance);
                if(Distance > 20)
                {
                    DisplayLog("CanExecuteTask(): Leaving my Revenge target.");
                    myEntity.SetRevengeTarget(null);
                    return true;
                }

                DisplayLog("CanExecuteTask():  There is an Revenge target set. Not executing Order: " + order.ToString());
                DisplayLog("Revenge Target: " + myEntity.GetRevengeTarget().ToString());
                return false;

            }

            return true;
        }
        return true;
    }

    public static int GetHireCost(int EntityID)
    {
        int result = -1;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
            result = GetIntValue(EntityID, "HireCost");
        return result;
    }

    public static ItemValue GetHireCurrency(int EntityID)
    {
        ItemValue result = ItemClass.GetItem("casinoCoin");
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
            result = GetItemValue(EntityID, "HireCurrency");
        return result;
    }

    public static ItemValue GetItemValue(int EntityID, String strProperty)
    {
        ItemValue result = ItemClass.GetItem("casinoCoin");
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            result = ItemClass.GetItem(entityClass.Properties.Values[ strProperty], false);
            if(result.IsEmpty())
                result = ItemClass.GetItem("casinoCoin");
        }
        return result;
    }
    public static int GetIntValue(int EntityID, String strProperty)
    {
        int result = -1;
        
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if(entityClass.Properties.Values.ContainsKey(strProperty))
                result = int.Parse(entityClass.Properties.Values[ strProperty ]);
        }
        return result;
    }
    public static float GetFloatValue(int EntityID, String strProperty)
    {
        float result = -1;

        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if(entityClass.Properties.Values.ContainsKey(strProperty))
                result  = StringParsers.ParseFloat(entityClass.Properties.Values[strProperty]);
        }
        return result;
    }
    public static String GetStringValue( int EntityID, String strProperty)
    {
        string result = String.Empty;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if(entityClass.Properties.Values.ContainsKey(strProperty))
                return entityClass.Properties.Values[strProperty];
        }
        return result;
    }
    public static float GetCVarValue(int EntityID, string strCvarName)
    {
        float value = 0f;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity)
        {
            if(myEntity.Buffs.HasCustomVar(strCvarName))
                value = myEntity.Buffs.GetCustomVar(strCvarName);
        }
        return value;
    }

    public static List<String> ConfigureEntityClass(int EntityID, String strKey)
    {
        List<String> TempList = new List<String>();
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity == null)
            return TempList;

        EntityClass entityClass = EntityClass.list[myEntity.entityClass];
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

    public static void lootContainerOpened(TileEntityLootContainer _te, LocalPlayerUI _playerUI, int _entityIdThatOpenedIt)
    {
        if(_playerUI != null)
        {
            bool flag = true;
            string lootContainerName = string.Empty;
            if(_te.entityId != -1)
            {
                Entity entity = GameManager.Instance.World.GetEntity(_te.entityId);
                if(entity != null)
                {
                    lootContainerName = Localization.Get(EntityClass.list[entity.entityClass].entityClassName, "");
                    if(entity is EntityVehicle)
                        flag = false;
                }
            }
            else
            {
                BlockValue block = GameManager.Instance.World.GetBlock(_te.ToWorldPos());
                lootContainerName = Localization.Get(Block.list[block.type].GetBlockName(), "");
            }
            if(flag)
            {
                ((XUiC_LootWindowGroup)((XUiWindowGroup)_playerUI.windowManager.GetWindow("looting")).Controller).SetTileEntityChest(lootContainerName, _te);
                _playerUI.windowManager.Open("looting", true, false, true);
            }
            LootContainer lootContainer = LootContainer.lootList[_te.lootListIndex];
            if(lootContainer != null && _playerUI.entityPlayer != null)
            {
                lootContainer.ExecuteBuffActions(_te.entityId, _playerUI.entityPlayer);
            }
        }
        if(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            GameManager.Instance.lootManager.LootContainerOpened(_te, _entityIdThatOpenedIt);
            _te.bTouched = true;
            _te.SetModified();
        }
    }

    public static bool CheckIncentive(int EntityID, List<String> lstIncentives, EntityAlive entity)
    {
        bool result = false;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity == null)
            return result;

        foreach(String strIncentive in lstIncentives)
        {
            DisplayLog(" Checking Incentive: " + strIncentive);
            // Check if the entity that is looking at us has the right buff for us to follow.
            if(myEntity.Buffs.HasBuff(strIncentive))
                result = true;

            // Check if there's a cvar for that incentive, such as $Mother or $Leader.
            if(myEntity.Buffs.HasCustomVar(strIncentive))
            {
                // DisplayLog(" Incentive: " + strIncentive + " Value: " + this.Buffs.GetCustomVar(strIncentive));
                if((int)myEntity.Buffs.GetCustomVar(strIncentive) == entity.entityId)
                    result = true;
            }

            if(entity)
            {
                // Then we check if the control mechanism is an item being held.
                if(entity.inventory.holdingItem.Name == strIncentive)
                    result = true;
            }
            // if we are true here, it means we found a match to our entity.
            if(result)
                break;
        }
        return result;
    }

    public static string DisplayEntityStats(int EntityID )
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if(myEntity == null)
            return " Not An EntityAliveSDX";

        String FoodAmount = Mathf.RoundToInt(myEntity.Stats.Stamina.ModifiedMax + GetCVarValue( EntityID, "foodAmount")).ToString();
        String WaterAmount = Mathf.RoundToInt(myEntity.Stats.Water.Value + GetCVarValue( EntityID, "waterAmount")).ToString();

        string strOutput = myEntity.EntityName + " - ID: " + myEntity.entityId + " Health: " + myEntity.Stats.Health.Value;
        strOutput += " Stamina: " + myEntity.Stats.Stamina.Value + " Thirst: " + myEntity.Stats.Water.Value + " Food: " + FoodAmount + " Water: " + WaterAmount;
        strOutput += " Sanitation: " + GetCVarValue( EntityID, "solidWasteAmount");

        // Read the Food items configured.
        String strFoodItems = GetStringValue(EntityID, "FoodItems");
        if(strFoodItems == String.Empty)
            strFoodItems = "All Food Items";
        strOutput += "\n Food Items: " + strFoodItems;

        // Read the Water Items
        String strWaterItems = GetStringValue(EntityID, "WaterItems");
        if(strWaterItems == String.Empty)
            strWaterItems = "All Water Items";
        strOutput += "\n Water Items: " + strWaterItems;

        strOutput += "\n Food Bins: " + GetStringValue(EntityID, "FoodBins");
        strOutput += "\n Water Bins: " + GetStringValue(EntityID, "WaterBins");

        if(myEntity.Buffs.HasCustomVar("CurrentOrder"))
            strOutput += "\n Current Order: " + GetCurrentOrder(EntityID).ToString();

        if(myEntity.Buffs.HasCustomVar("Leader"))
            strOutput += "\n Current Leader: " + GetLeader(EntityID).ToString();

        strOutput += "\n Active Buffs: ";
        foreach(BuffValue buff in myEntity.Buffs.ActiveBuffs)
            strOutput += "\n\t" + buff.BuffName + " ( Seconds: " + buff.DurationInSeconds + " Ticks: " + buff.DurationInTicks + " )";

        strOutput += "\n Active Quests: ";
        foreach(Quest quest in myEntity.QuestJournal.quests)
            strOutput += "\n\t" + quest.ID + " Current State: " + quest.CurrentState + " Current Phase: " + quest.CurrentPhase;

        strOutput += "\n Patrol Points: ";
        foreach(Vector3 vec in myEntity.PatrolCoordinates)
            strOutput += "\n\t" + vec.ToString();

        strOutput += "\n\nCurrency: " + myEntity.HireCurrency + " Faction: " + myEntity.factionId;
        return strOutput;
    }
}

