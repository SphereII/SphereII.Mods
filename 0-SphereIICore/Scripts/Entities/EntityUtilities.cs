
// General Purpose Entity Utilities to centralize general checks.
using GamePath;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class EntityUtilities
{
    static bool blDisplayLog = false;
    private static string AdvFeatureClass = "AdvancedNPCFeatures";


    public static void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
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

    // Control the need of the entity to help equip weapons or items to use.
    public enum Need
    {
        Ranged = 0,
        Melee = 1,
        Health = 2,
        Hungry = 3,
        Thirsty = 4,
        Reset = 5
    }

    public static bool IsHuman(int EntityID)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;

        // Read the ConfigBlock to detect what constitutes a human, or rather, what can think
        string[] Tags = Configuration.GetPropertyValue("AdvancedNPCFeatures", "HumanTags").Split(',');
        foreach (String Tag in Tags)
        {
            if (myEntity.HasAnyTags(FastTags.Parse(Tag)))
                return true;
        }

        return false;
    }

    public static void AddBuffToRadius(String strBuff, Vector3 position, int Radius)
    {
        // If there's no radius, pick 30 blocks.
        if (Radius <= 0)
            Radius = 30;

        World world = GameManager.Instance.World;
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(position, Vector3.one * Radius));
        if (entitiesInBounds.Count > 0)
        {
            for (int i = 0; i < entitiesInBounds.Count; i++)
            {
                EntityAlive entity = entitiesInBounds[i] as EntityAlive;
                if (entity != null)
                {
                    if (!entity.Buffs.HasBuff(strBuff))
                        entity.Buffs.AddBuff(strBuff);
                }
            }
        }

    }
    public static void AddQuestToRadius(String strQuest, Vector3 position, int Radius)
    {
        // If there's no radius, pick 30 blocks.
        if (Radius <= 0)
            Radius = 30;

        World world = GameManager.Instance.World;
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(position, Vector3.one * Radius));
        if (entitiesInBounds.Count > 0)
        {
            for (int i = 0; i < entitiesInBounds.Count; i++)
            {
                EntityAliveSDX entity = entitiesInBounds[i] as EntityAliveSDX;
                if (entity != null)
                    entity.QuestJournal.AddQuest(QuestClass.CreateQuest(strQuest));

                EntityPlayerLocal player = entitiesInBounds[i] as EntityPlayerLocal;
                if (player != null)
                    player.QuestJournal.AddQuest(QuestClass.CreateQuest(strQuest));
            }
        }

    }

    public static int ChangeHandholdItem(int EntityID, Need myCurrentNeed, int Preferred = -1)
    {
        int index = 0;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return index;

        if (Preferred == -1)
        {
            switch (myCurrentNeed)
            {
                // Ranged
                case Need.Ranged:
                    index = FindItemWithAction(EntityID, typeof(ItemActionRanged));
                    break;
                // Ranged
                case Need.Melee:
                    index = FindItemWithAction(EntityID, typeof(ItemActionMelee));
                    //if (index == 0)
                    //    index = FindItemWithAction(EntityID, typeof(ItemActionDynamicMelee));
                    break;
            }
        }
        else
            index = Preferred;

       
        // If there's no change, don't do anything.
        if (myEntity.inventory.holdingItemIdx == index)
            return index;

        myEntity.inventory.SetHoldingItemIdxNoHolsterTime(index);

        // Forcing the show items
        myEntity.inventory.ShowHeldItem(false, 0f);
        myEntity.inventory.ShowHeldItem(true);

        //myEntity.inventory.SetHoldingItemIdx(index);
        myEntity.inventory.ForceHoldingItemUpdate();
      //  myEntity.emodel.SwitchModelAndView(myEntity.emodel.IsFPV, myEntity.IsMale);
        return index;

    }

    public static bool CurrentHoldingItemType(int EntityID, Type CheckAction)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;

        ItemClass item = myEntity.inventory.holdingItem;
        if (item == null)
            return false;
        foreach (var action in item.Actions)
        {
            if (action == null)
                continue;
            var checkType = action.GetType();
            if (CheckAction == checkType || CheckAction.IsAssignableFrom(checkType))
                return true;
        }

        return false;
    }
    // Searches the entity, looking for a specific action
    public static int FindItemWithAction(int EntityID, Type findAction)
    {
        int index = 0;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return index;

        int counter = -1;
        foreach (var stack in myEntity.inventory.GetSlots())
        {


            counter++;

            if (stack == ItemStack.Empty)
                continue;
            if (stack.itemValue == null)
                continue;
            if ( stack.itemValue.ItemClass == null )
                continue;
            if (stack.itemValue.ItemClass.Actions == null)
                continue;
            foreach (var action in stack.itemValue.ItemClass.Actions)
            {
                if (action == null)
                    continue;
                var checkType = action.GetType();

                if (findAction == checkType || findAction.IsAssignableFrom(checkType))
                {

                    return counter;
                }
            }
        }
        return index;
    }


    // Returns false if its not within AI Range
    public static bool CheckAIRange(int EntityID, int targetEntity)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;
        EntityAlive myTarget = GameManager.Instance.World.GetEntity(targetEntity) as EntityAlive;
        if (myTarget == null)
            return false;

        // Find the max range for the weapon
        // Example Range:  50
        //   Hold Ground Distance between 20 to 50 
        //   Retreat distance is 20%, so 20
        float MaxRangeForWeapon = EffectManager.GetValue(PassiveEffects.MaxRange, myEntity.inventory.holdingItemItemValue, 60f,myEntity, null, myEntity.inventory.holdingItem.ItemTags, true, true, true, true, 1, true);
        MaxRangeForWeapon = MaxRangeForWeapon * 2;
        float HoldGroundDistance = (float)MaxRangeForWeapon * 0.80f; // minimum range to hold ground 
        float RetreatDistance = (float)MaxRangeForWeapon * 0.30f; // start retreating at this distance.
        float distanceSq = myTarget.GetDistanceSq(myEntity);

        float MinMeleeRange = GetFloatValue(EntityID, "MinimumMeleeRange");
        if (MinMeleeRange == -1)
            MinMeleeRange = 4;

        DisplayLog(myEntity.EntityName  + " Max Range: " + MaxRangeForWeapon + " Hold Ground: " + HoldGroundDistance + " Retreatdistance: " + RetreatDistance + " Entity Distance: " + distanceSq);


        // if they are too close, switch to melee
        if (distanceSq <= MinMeleeRange) 
            return false;

        // Hold your ground
        if(distanceSq > RetreatDistance && distanceSq <= HoldGroundDistance) // distance greater than 20%  of the range of the weapon
            Stop(EntityID);

        // Back away!
        if (distanceSq > MinMeleeRange && distanceSq <= RetreatDistance )
            BackupHelper(EntityID, myTarget.position, 40);

        return true;
    }

    public static void Stop(int EntityID)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return;

        if ( myEntity.moveHelper != null )
            myEntity.moveHelper.Stop();

        if ( myEntity.navigator != null )
            myEntity.navigator.clearPath();

        
        myEntity.speedForward = 0;

    }
    public static void BackupHelper(int EntityID, Vector3 awayFrom, int distance)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return;

        if (myEntity.moveHelper == null)
            return;

        Vector3 dirV = myEntity.position - awayFrom;
        Vector3 vector = Vector3.zero;

        vector = myEntity.position + -new Vector3(awayFrom.x, 0f, awayFrom.z) * distance;

        // If you are blocked, try to go to another side.
        //vector = RandomPositionGenerator.CalcAway(myEntity, distance, distance,distance, awayFrom);
        myEntity.moveHelper.SetMoveTo(vector, false);

        // Move away at a hard coded speed of -4 to make them go backwards
      //  myEntity.speedForward = -4f;// Mathf.SmoothStep(myEntity.speedForward, -0.25f, 2 * Time.deltaTime);

        // Keep them facing the spot
        myEntity.SetLookPosition( awayFrom );
        myEntity.RotateTo(awayFrom.x, awayFrom.y, awayFrom.z, 30f, 30f);
    }

    public static bool CheckProperty(int EntityID, string Property)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;

        EntityClass entityClass = EntityClass.list[myEntity.entityClass];
        if (entityClass.Properties.Values.ContainsKey(Property))
            return true;

        if (entityClass.Properties.Classes.ContainsKey(Property))
            return true;

        return false;
    }


    public static void ProcessConsumables(int EntityID)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return;

        float foodAmount = GetCVarValue(EntityID, "$foodAmountAdd");
        if (foodAmount > 0.5f)
        {
            myEntity.Health += Utils.Fastfloor(foodAmount);
            foodAmount -= 0.5f;
        }
        if (myEntity.Health > myEntity.GetMaxHealth())
            myEntity.Health = myEntity.GetMaxHealth();


    }

    public static bool HasTask(int EntityID, String strTask)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity != null)
        {
            foreach (var task in myEntity.aiManager.GetTasks<EAIBase>())
            {
                if (task.GetTypeName().Contains(strTask))
                    return true;
            }
        }

        return false;
    }
    public static bool Hire(int EntityID, EntityPlayerLocal _player)
    {
        DisplayLog("Hire()");
        bool result = false;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return result;

        DisplayLog("Hire(): I have an entity");

        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
        if (uiforPlayer)
        {
            DisplayLog("Hire(): I have a player.");
            DisplayLog(" The Player wants to hire me for " + GetHireCost(EntityID) + " " + GetHireCurrency(EntityID));
            if (uiforPlayer.xui.PlayerInventory.GetItemCount(GetHireCurrency(EntityID)) >= GetHireCost(EntityID))
            {
                DisplayLog(" The Player has enough currency: " + uiforPlayer.xui.PlayerInventory.GetItemCount(GetHireCurrency(EntityID)));
                // Create the stack of currency
                ItemStack stack = new ItemStack(GetHireCurrency(EntityID), GetHireCost(EntityID));
                DisplayLog(" Removing Item: " + stack.ToString());
                uiforPlayer.xui.PlayerInventory.RemoveItems(new ItemStack[] { stack }, 1);

                // Add the stack of currency to the NPC, and set its orders.
                //myEntity.bag.AddItem(stack);
                SetLeaderAndOwner(EntityID, _player.entityId);
                return true;
            }
            else
            {
                GameManager.ShowTooltipWithAlert(_player, "You cannot afford me. I want " + GetHireCost(EntityID) + " " + GetHireCurrency(EntityID), "ui_denied");
            }
        }
        return false;
    }


    // Returns the leader of the passed in entity ID.
    public static Entity GetLeader(int EntityID)
    {
        Entity leader = null;
        EntityAlive currentEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (currentEntity)
        {

            if (currentEntity.Buffs.HasCustomVar("Leader"))
                leader = GameManager.Instance.World.GetEntity((int)currentEntity.Buffs.GetCustomVar("Leader"));

            // Something happened to our leader.
            if (leader == null)
            {
                currentEntity.Buffs.RemoveCustomVar("Leader");
                leader = currentEntity;
            }
        }

        return leader;
    }

    public static bool isEntityHungry(int EntityID)
    {
        bool result = false;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            List<String> lstBuffs = ConfigureEntityClass(EntityID, "HungryBuffs");
            if (lstBuffs.Count == 0)
                lstBuffs.AddRange(new string[] { "buffStatusHungry2", "buffStatusHungry1" });

            result = CheckIncentive(EntityID, lstBuffs, null);
        }

        if (result)
            DisplayLog(" Is Entity hungry? " + result);
        return result;
    }

    public static bool isEntityHurt(int EntityID)
    {
        bool result = false;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {

            List<String> lstBuffs = new List<string>();
            lstBuffs.Add("buffStatusWounded");
            lstBuffs.Add("buffInjuryBleeding");


            result = CheckIncentive(EntityID, lstBuffs, null);

            if (result == false)
            {
                if (myEntity.Health < (myEntity.GetMaxHealth() * 0.75))
                {
                    DisplayLog(" Entity's Health is less than 35%");
                    result = true;
                }
            }
        }

        if (result)
            DisplayLog(" Is Entity Hurt? ");
        return result;
    }

    // This will search for a mother entity to see it can satisfy its thirst from its mother, rather than a traditional water block.
    public static float GetEntityWater(int EntityID)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            if (myEntity.Buffs.HasCustomVar("Mother"))
            {
                float MotherID = myEntity.Buffs.GetCustomVar("Mother");
                EntityAliveSDX MotherEntity = myEntity.world.GetEntity((int)MotherID) as EntityAliveSDX;
                if (MotherEntity)
                {
                    DisplayLog(" My Mother is: " + MotherEntity.EntityName);
                    if (MotherEntity.Buffs.HasCustomVar("MilkLevel"))
                    {
                        DisplayLog("Heading to mommy");
                        float MilkLevel = MotherEntity.Buffs.GetCustomVar("MilkLevel");
                        myEntity.SetInvestigatePosition(myEntity.world.GetEntity((int)MotherID).position, 60);
                        return MilkLevel;
                    }
                }

            }
        }
        return 0f;
    }
    public static bool isEntityThirsty(int EntityID)
    {
        bool result = false;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            List<String> lstBuffs = ConfigureEntityClass(EntityID, "ThirstyBuffs");
            if (lstBuffs.Count == 0)
                lstBuffs.AddRange(new string[] { "buffStatusThirsty2", "buffStatusThirsty1" });

            result = CheckIncentive(EntityID, lstBuffs, null);
        }

        if (result)  // Really spammy if its false
            DisplayLog(" Is Entity Thirsty? " + result);
        return result;

    }

    // Owner can be different than the current leader
    public static Entity GetOwner(int EntityID)
    {
        Entity leader = null;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            if (leader == null && myEntity.Buffs.HasCustomVar("Owner"))
                leader = GameManager.Instance.World.GetEntity((int)myEntity.Buffs.GetCustomVar("Owner"));
        }

        return leader;
    }

    public static Entity GetLeaderOrOwner(int EntityID)
    {
        Entity leader = GetLeader(EntityID);
        if (leader == null)
            leader = GetOwner(EntityID);

        return leader;
    }

    public static void SetLeader(int EntityID, int LeaderID)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        EntityAlive LeaderEntity = GameManager.Instance.World.GetEntity(LeaderID) as EntityAlive;
        if (myEntity && LeaderEntity)
        {
            myEntity.Buffs.SetCustomVar("Leader", LeaderID, true);
            myEntity.moveSpeed = LeaderEntity.moveSpeed;
            myEntity.moveSpeedAggro = LeaderEntity.moveSpeedAggro;
            myEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

            SetCurrentOrder(EntityID, Orders.Follow);

        }

    }

    public static void SetOwner(int EntityID, int LeaderID)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        EntityAlive LeaderEntity = GameManager.Instance.World.GetEntity(LeaderID) as EntityAlive;
        if (myEntity && LeaderEntity)
            myEntity.Buffs.SetCustomVar("Owner", LeaderID, true);
    }

    public static void SetLeaderAndOwner(int EntityID, int LeaderID)
    {
        SetLeader(EntityID, LeaderID);
        SetOwner(EntityID, LeaderID);
    }

    public static bool IsAnAlly(int EntityID, int AttackingID)
    {
        bool result = false;
        // Let you hurt yourself.
        if (EntityID == AttackingID)
            return false;

        Entity myLeader = GetLeaderOrOwner(EntityID);

        // If my attacker is my leader, forgive him.
        if (myLeader)
            if (AttackingID == myLeader.entityId)
                return true;

        Entity theirLeader = GetLeaderOrOwner(AttackingID);
        if ((myLeader && theirLeader) && (myLeader.entityId == theirLeader.entityId))
            result = true;

        return result;
    }

    public static Entity GetAttackOrReventTarget(int EntityID)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            if (myEntity.GetAttackTarget() != null)
                return myEntity.GetAttackTarget();
            if (myEntity.GetRevengeTarget() == null)
                return myEntity.GetRevengeTarget();
        }
        return null;
    }
    public static bool isTame(int EntityID, EntityAlive _player)
    {
        Entity myLeader = GetLeaderOrOwner(EntityID);
        if (myLeader)
        {
            if (myLeader.entityId == _player.entityId)
                return true;

        }
        return false;
    }

    public static void SetCurrentOrder(int EntityID, Orders order)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            DisplayLog(" Setting Current Order: " + order.ToString());
            myEntity.Buffs.SetCustomVar("CurrentOrder", (float)order, true);
        }
    }

    public static Orders GetCurrentOrder(int EntityID)
    {
        Orders currentOrder = Orders.Wander;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            DisplayLog(" GetCurrentOrder(): This is an Entity AliveSDX");
            if (myEntity.Buffs.HasCustomVar("CurrentOrder"))
            {
                DisplayLog("GetCurrentOrder(): Entity has an Order: " + (Orders)myEntity.Buffs.GetCustomVar("CurrentOrder"));
                currentOrder = (Orders)myEntity.Buffs.GetCustomVar("CurrentOrder");
            }
            else
            {

                DisplayLog("GetCurrentOrder(): This entity has no order.");

            }
        }
        return currentOrder;
    }

    public static bool ExecuteCMD(int EntityID, String strCommand, EntityPlayer player)
    {
        String strDisplay = "ExecuteCMD( " + strCommand + " ) to " + EntityID + " From " + player.DebugNameInfo;
        AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);

        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return false;

        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);

        Vector3 position = myEntity.position;

        // Restore it's walk speed to default.
        myEntity.RestoreSpeed();

        switch (strCommand)
        {
            case "TellMe":
                GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, myEntity.ToString() + "\n\n\n\n\n", "ui_denied");
                AdvLogging.DisplayLog(AdvFeatureClass, "\n\nBuffs:");
                foreach (var Buff in myEntity.Buffs.ActiveBuffs)
                    AdvLogging.DisplayLog(AdvFeatureClass, "\t" + Buff.BuffName);

                AdvLogging.DisplayLog(AdvFeatureClass, myEntity.ToString());
                AdvLogging.DisplayLog(AdvFeatureClass, "Body Damage: ");
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has Right Leg? " + myEntity.bodyDamage.HasRightLeg);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has Left Leg? " + myEntity.bodyDamage.HasLeftLeg);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has Limbs? " + myEntity.bodyDamage.HasLimbs);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Arm or Leg missing? " + myEntity.bodyDamage.IsAnyArmOrLegMissing);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has any leg missing? " + myEntity.bodyDamage.IsAnyLegMissing);
                bool LegDamageTrigger = false;
                myEntity.emodel.avatarController.TryGetTrigger("LegDamageTrigger", out LegDamageTrigger);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Leg Damage Trigger? " + LegDamageTrigger);
                break;
            case "ShowAffection":
                GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, "You gentle scratch and stroke the side of the animal.", "");
                break;
            case "FollowMe":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Leader");
                SetLeader(EntityID, player.entityId);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order");
                SetCurrentOrder(EntityID, Orders.Follow);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Adjusting Speeds");
                myEntity.moveSpeed = player.moveSpeed;
                myEntity.moveSpeedAggro = player.moveSpeedAggro;
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");
                break;
            case "StayHere":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order");
                SetCurrentOrder(EntityID, Orders.Stay);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Position");
                myEntity.GuardPosition = position;
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Stopping Move Helper()");
                if (myEntity.moveHelper != null) // No move helper on the client when on Dedi
                    myEntity.moveHelper.Stop();
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");

                break;
            case "GuardHere":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order");
                SetCurrentOrder(EntityID, Orders.Stay);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Look Direction");

                myEntity.SetLookPosition(player.GetLookVector());
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Position");

                myEntity.GuardPosition = position;
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Stopping Move Helper()");

                if (myEntity.moveHelper != null) // No move helper on the client when on Dedi
                    myEntity.moveHelper.Stop();
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Guard Look");
                myEntity.GuardLookPosition = player.GetLookVector();
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");

                break;
            case "Wander":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order");
                SetCurrentOrder(EntityID, Orders.Wander);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");

                break;
            case "SetPatrol":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order");
                SetLeader(EntityID, player.entityId);
                SetCurrentOrder(EntityID, Orders.SetPatrolPoint);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Move Speed");
                myEntity.moveSpeed = player.moveSpeed;
                myEntity.moveSpeedAggro = player.moveSpeedAggro;
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Resetting Patrol Points");

                myEntity.PatrolCoordinates.Clear(); // Clear the existing point
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");

                break;
            case "Patrol":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order");
                SetCurrentOrder(EntityID, Orders.Patrol);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");

                break;
            case "Hire":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Opening Hire ");
                bool result = Hire(EntityID, player as EntityPlayerLocal);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Hire");

                break;
            case "OpenInventory":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order Open Inventory");

                //      GameManager.Instance.TELockServer(0, myEntity.GetBlockPosition(), EntityID, player.entityId);
                //       uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
                if (myEntity.lootContainer == null)
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Loot Container is null");

                else
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Loot Container: " + myEntity.lootContainer.ToString());
                    myEntity.lootContainer.lootListIndex = 62;
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Opening Loot Container");

                    lootContainerOpened(myEntity.lootContainer, uiforPlayer, player.entityId);
                }
                break;
            case "Loot":
                SetCurrentOrder(EntityID, Orders.Loot);
                myEntity.Buffs.RemoveCustomVar("Leader");
                break;
            case "Dismiss":
                SetCurrentOrder(EntityID, Orders.Wander);
                myEntity.Buffs.RemoveCustomVar("Leader");
                myEntity.Buffs.RemoveCustomVar("Owner");
                break;

        }

        return true;

    }
    public static bool CanExecuteTask(int EntityID, EntityUtilities.Orders order)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            if (GetCurrentOrder(EntityID) != order)
            {
                DisplayLog("CanExecuteTask(): Current Order does not match passed in Order: " + GetCurrentOrder(EntityID));
                return false;
            }

            Entity leader = EntityUtilities.GetLeader(EntityID);
            float Distance = 0f;
            if (leader)
                Distance = myEntity.GetDistance(leader);

            // If we have an attack or revenge target, don't execute task
            if (myEntity.GetAttackTarget() != null && myEntity.GetAttackTarget().IsAlive())
            {
                // If we have a leader and they are far away, abandon the fight and go after your leader.
                if (Distance > 20)
                {
                    DisplayLog("CanExecuteTask(): I have an Attack Target but my Leader is too far away.");
                    myEntity.SetAttackTarget(null, 0);
                    return true;
                }

                // I have an attack target, so don't keep doing your current task
                DisplayLog("CanExecuteTask(): I have an Attack Target: " + myEntity.GetAttackTarget().ToString() );
                return false;

            }

            if (myEntity.GetRevengeTarget() != null && myEntity.GetRevengeTarget().IsAlive())
            {
                if (Distance > 20)
                {
                    DisplayLog("CanExecuteTask(): I have a Revenge Target, but my leader is too far way.");
                    myEntity.SetRevengeTarget(null);
                    return true;
                }
                 DisplayLog("CanExecuteTask(): I have a Revenge Target: " + myEntity.GetRevengeTarget().ToString());
                return false;

            }

            if (GetCurrentOrder(EntityID) == Orders.Follow)
            {
                DisplayLog("My Current Order is Follow");
                if (Distance < 2)
                {
                    if (myEntity.moveHelper != null)
                    {
                        myEntity.moveHelper.Stop();
                           DisplayLog(" Too Close to leader. Moving to Look Vector");
                        myEntity.moveHelper.SetMoveTo((leader as EntityAlive).GetLookVector(), false);

                    }
                    return false;
                }

            }

            return true;
        }
        return true;
    }

    public static Vector3 SetCloseSpawnPoint(int EntityID, Vector3 centralPosition)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            int x, y, z;
            GameManager.Instance.World.FindRandomSpawnPointNearPositionUnderground(centralPosition, 15, out x, out y, out z, new Vector3(3, 3, 3));
            return new Vector3(x, y, z);

        }
        return Vector3.zero;

    }
    public static int GetHireCost(int EntityID)
    {
        int result = -1;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
            result = GetIntValue(EntityID, "HireCost");

        if (result == -1)
            result = 1000;
        return result;
    }

    public static ItemValue GetHireCurrency(int EntityID)
    {
        ItemValue result = ItemClass.GetItem("casinoCoin", false);
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
            result = GetItemValue(EntityID, "HireCurrency");

        if (result.IsEmpty())
            result = ItemClass.GetItem("casinoCoin", true);
        return result;
    }

    public static ItemValue GetItemValue(int EntityID, String strProperty)
    {
        ItemValue result = ItemClass.GetItem("casinoCoin", false);
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = ItemClass.GetItem(entityClass.Properties.Values[strProperty], false);
            if (result.IsEmpty())
                result = ItemClass.GetItem("casinoCoin", false);
        }
        return result;
    }
    public static int GetIntValue(int EntityID, String strProperty)
    {
        int result = -1;

        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = int.Parse(entityClass.Properties.Values[strProperty]);
        }
        return result;
    }

    public static Vector3 GetNewPositon(int EntityID, int maxBlocks = 30)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return Vector3.zero;

        if (!EntityUtilities.CheckProperty(EntityID, "PathingBlocks"))
            return Vector3.zero;

            Vector3 result = Vector3.zero;
        List<Vector3> Paths = SphereCache.GetPaths(EntityID);
        if (Paths == null || Paths.Count == 0)
        {
            //  Grab a list of blocks that are configured for this class.
            //    <property name="PathingBlocks" value="PathingCube" />
            List<string> Blocks = EntityUtilities.ConfigureEntityClass(EntityID, "PathingBlocks");
            if (Blocks.Count == 0)
            {
                // DisplayLog("No Blocks configured. Setting Default", __instance.theEntity);
               // Blocks.Add("PathingCube");
                return result;
            }

            //Scan for the blocks in the area
            List<Vector3> PathingVectors = ModGeneralUtilities.ScanForBlockInListHelper(myEntity.position, Blocks, maxBlocks);
            if (PathingVectors.Count == 0)
                return result;

            //Add to the cache
            SphereCache.AddPaths(EntityID, PathingVectors);
        }

        Vector3 newposition = SphereCache.GetRandomPath(EntityID);
        if (newposition == Vector3.zero)
            return result;

        // Remove it from the cache.
        SphereCache.RemovePath(EntityID, newposition);

        result = GameManager.Instance.World.FindSupportingBlockPos(newposition);
        //Debug.Log("Position: " + result);
        // Center the pathing position.
        result.x = (float)Utils.Fastfloor(result.x) + 0.5f;
        result.y = (float)Utils.Fastfloor(result.y) + 0.5f;
        result.z = (float)Utils.Fastfloor(result.z) + 0.5f;
        return result;
    }
    public static void OpenDoor(int EntityID, Vector3i blockPos)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            BlockValue block = myEntity.world.GetBlock(blockPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
            {
                Chunk chunk = myEntity.world.GetChunkFromWorldPos(blockPos) as Chunk;
                block.Block.OnBlockActivated(myEntity.world, chunk.ClrIdx, blockPos, block, myEntity);
            }
        }
    }
    public static void CloseDoor(int EntityID, Vector3i blockPos)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            BlockValue block = myEntity.world.GetBlock(blockPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
            {
                Chunk chunk = myEntity.world.GetChunkFromWorldPos(blockPos) as Chunk;
                block.Block.OnBlockActivated(myEntity.world, chunk.ClrIdx, blockPos, block, myEntity);
            }
        }
    }

    public static bool GetBoolValue(int EntityID, String strProperty)
    {
        bool result = false;

        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = bool.Parse(entityClass.Properties.Values[strProperty]);
        }
        return result;
    }

    public static float GetFloatValue(int EntityID, String strProperty)
    {
        float result = -1;

        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = StringParsers.ParseFloat(entityClass.Properties.Values[strProperty], 0, -1, NumberStyles.Any);
        }
        return result;
    }
    public static String GetStringValue(int EntityID, String strProperty)
    {
        string result = String.Empty;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            EntityClass entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                return entityClass.Properties.Values[strProperty];
        }
        return result;
    }
    public static float GetCVarValue(int EntityID, string strCvarName)
    {
        float value = 0f;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            if (myEntity.Buffs.HasCustomVar(strCvarName))
                value = myEntity.Buffs.GetCustomVar(strCvarName);
        }
        return value;
    }

    public static List<String> ConfigureEntityClass(int EntityID, String strKey)
    {
        List<String> TempList = new List<String>();
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return TempList;

        EntityClass entityClass = EntityClass.list[myEntity.entityClass];
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

    public static void lootContainerOpened(TileEntityLootContainer _te, LocalPlayerUI _playerUI, int _entityIdThatOpenedIt)
    {
        if (_playerUI != null)
        {
            bool flag = true;
            string lootContainerName = string.Empty;
            if (_te.entityId != -1)
            {
                Entity entity = GameManager.Instance.World.GetEntity(_te.entityId);
                if (entity != null)
                {
                    lootContainerName = Localization.Get(EntityClass.list[entity.entityClass].entityClassName);
                    if (entity is EntityVehicle)
                        flag = false;
                }
            }
            else
            {
                BlockValue block = GameManager.Instance.World.GetBlock(_te.ToWorldPos());
                lootContainerName = Localization.Get(Block.list[block.type].GetBlockName());
            }
            if (flag)
            {
                ((XUiC_LootWindowGroup)((XUiWindowGroup)_playerUI.windowManager.GetWindow("looting")).Controller).SetTileEntityChest(lootContainerName, _te);
                _playerUI.windowManager.Open("looting", true, false, true);
            }
            LootContainer lootContainer = LootContainer.lootList[_te.lootListIndex];
            if (lootContainer != null && _playerUI.entityPlayer != null)
            {
                lootContainer.ExecuteBuffActions(_te.entityId, _playerUI.entityPlayer);
            }
        }
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            GameManager.Instance.lootManager.LootContainerOpened(_te, _entityIdThatOpenedIt, new FastTags());
            _te.bTouched = true;
            _te.SetModified();
        }
    }

    public static bool CheckFaction(int EntityID, EntityAlive entity)
    {
        bool result = false;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return result;

        // same faction
        if (myEntity.factionId == entity.factionId)
            return true;

        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(myEntity, entity);
        DisplayLog(" CheckFactionForEnemy: " + myRelationship.ToString());
        if (myRelationship == FactionManager.Relationship.Hate)
        {
            DisplayLog(" I hate this entity: " + entity.ToString());
            return false;
        }
        else
        {
            DisplayLog(" My relationship with this " + entity.ToString() + " is: " + myRelationship.ToString());
            result = true;
        }
        return false;
    }

    public static bool CheckForBuff(int EntityID, String strBuff)
    {

        bool result = false;
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return result;
        if (myEntity.Buffs.HasBuff(strBuff))
            result = true;

        return result;
    }
    public static bool CheckIncentive(int EntityID, List<String> lstIncentives, EntityAlive entity)
    {
        bool result = false;
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return result;

        foreach (String strIncentive in lstIncentives)
        {
            DisplayLog("CheckIncentive(): Incentive: " + strIncentive);
            // Check if the entity that is looking at us has the right buff for us to follow.
            if (myEntity.Buffs.HasBuff(strIncentive))
                return true;

            if (entity)
            {
                // Check if there's a cvar for that incentive, such as $Mother or $Leader.
                if (myEntity.Buffs.HasCustomVar(strIncentive))
                {
                    DisplayLog(" Incentive: " + strIncentive + " Value: " + myEntity.Buffs.GetCustomVar(strIncentive));
                    if ((int)myEntity.Buffs.GetCustomVar(strIncentive) == entity.entityId)
                        return true;
                }
                // Then we check if the control mechanism is an item being held.
                if (entity.inventory.holdingItem.Name == strIncentive)
                    return true;
            }
        }
        return result;
    }

    public static string DisplayEntityStats(int EntityID)
    {
        EntityAliveSDX myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return "";

        String FoodAmount = Mathf.RoundToInt(myEntity.Stats.Stamina.ModifiedMax + GetCVarValue(EntityID, "foodAmount")).ToString();
        String WaterAmount = Mathf.RoundToInt(myEntity.Stats.Water.Value + GetCVarValue(EntityID, "waterAmount")).ToString();

        // string strOutput = myEntity.EntityName + " - ID: " + myEntity.entityId + " Health: " + myEntity.Stats.Health.Value;
        string strOutput = myEntity.EntityName + " - ID: " + myEntity.entityId + " Health: " + myEntity.Health + "/" + myEntity.GetMaxHealth();
        strOutput += " Stamina: " + myEntity.Stats.Stamina.Value + " Thirst: " + myEntity.Stats.Water.Value + " Food: " + FoodAmount + " Water: " + WaterAmount;
        strOutput += " Sanitation: " + GetCVarValue(EntityID, "solidWasteAmount");
        // Read the Food items configured.
        String strFoodItems = GetStringValue(EntityID, "FoodItems");
        if (strFoodItems == String.Empty)
            strFoodItems = "All Food Items";
        strOutput += "\n Food Items: " + strFoodItems;
        // Read the Water Items
        String strWaterItems = GetStringValue(EntityID, "WaterItems");
        if (strWaterItems == String.Empty)
            strWaterItems = "All Water Items";
        strOutput += "\n Water Items: " + strWaterItems;
        strOutput += "\n Food Bins: " + GetStringValue(EntityID, "FoodBins");
        strOutput += "\n Water Bins: " + GetStringValue(EntityID, "WaterBins");
        if (myEntity.Buffs.HasCustomVar("CurrentOrder"))
            strOutput += "\n Current Order: " + GetCurrentOrder(EntityID).ToString();
        if (myEntity.Buffs.HasCustomVar("Leader"))
        {
            Entity leader = GetLeader(EntityID);
            if (leader)
                strOutput += "\n Current Leader: " + leader.entityId;
        }
        strOutput += "\n Active Buffs: ";
        foreach (BuffValue buff in myEntity.Buffs.ActiveBuffs)
            strOutput += "\n\t" + buff.BuffName + " ( Seconds: " + buff.DurationInSeconds + " Ticks: " + buff.DurationInTicks + " )";
        strOutput += "\n Active CVars: ";
        foreach (KeyValuePair<string, float> myCvar in myEntity.Buffs.CVars)
            strOutput += "\n\t" + myCvar.Key + " : " + myCvar.Value;
        strOutput += "\n Active Quests: ";
        foreach (Quest quest in myEntity.QuestJournal.quests)
            strOutput += "\n\t" + quest.ID + " Current State: " + quest.CurrentState + " Current Phase: " + quest.CurrentPhase;
        strOutput += "\n Patrol Points: ";
        foreach (Vector3 vec in myEntity.PatrolCoordinates)
            strOutput += "\n\t" + vec.ToString();
        strOutput += "\n\nCurrency: " + GetHireCurrency(EntityID) + " Faction: " + myEntity.factionId;

        DisplayLog(strOutput);

        return strOutput;
    }
}

