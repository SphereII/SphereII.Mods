﻿// General Purpose Entity Utilities to centralize general checks.

using System;
using System.Collections.Generic;
using UnityEngine;

public static class EntityUtilities
{
    private static readonly bool blDisplayLog = false;
    private static readonly string AdvFeatureClass = "AdvancedNPCFeatures";

    // Control the need of the entity to help equip weapons or items to use.
    public enum Need
    {
        None = 0,
        Melee = 1,
        Health = 2,
        Hungry = 3,
        Thirsty = 4,
        Reset = 5,
        Ranged = 6
    }

    // These are the orders, used in cvars for the EAI Tasks. They are casted as floats.
    public enum Orders
    {
        None = 0,
        Follow = 1,
        Stay = 2,
        Wander = 3,
        SetPatrolPoint = 4,
        Patrol = 5,
        Hire = 6,
        Loot = 7,
        Task = 8
    }


    public static void Traverse(GameObject obj)
    {
        var advFeatureClass = "AdvancedTroubleshootingFeatures";
        var feature = "ComponentMapper";
        if (obj == null)
        {
            AdvLogging.DisplayLog(advFeatureClass, feature, "Traversing a null object is not allowed.");
            AdvLogging.DisplayLog(advFeatureClass, feature, Environment.StackTrace);
            return;
        }

        AdvLogging.DisplayLog(advFeatureClass, feature, $"\t GameObject: {obj.name} Tag: {obj.tag}");
        foreach (Transform child in obj.transform)
        {
            foreach (Component component in child.GetComponents<Component>())
            {
             //   Debug.Log( $"\t\tComponent: {component} Tag: {component.tag}");
                AdvLogging.DisplayLog(advFeatureClass, feature, $"\t\tComponent: {component} Tag: {component.tag}");
            }

            foreach (var component in child.GetComponents<MonoBehaviour>())
            {
               // Debug.Log($"\t\tMonoBehaviour: {component} Tag: {component.tag}");
                AdvLogging.DisplayLog(advFeatureClass, feature, $"\t\tMonoBehaviour: {component} Tag: {component.tag}");
            }

            Traverse(child.gameObject);
        }
    }

    public static void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(strMessage);
    }


    public static Vector3 CenterPosition(Vector3 position)
    {
        var temp = position;
        temp.x = 0.5f + Utils.Fastfloor(position.x);
        temp.z = 0.5f + Utils.Fastfloor(position.z);
        temp.y = Utils.Fastfloor(position.y);
        return temp;
    }

    public static bool IsHuman(int EntityID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;

        // Read the ConfigBlock to detect what constitutes a human, or rather, what can think
        var Tags = Configuration.GetPropertyValue("AdvancedNPCFeatures", "HumanTags").Split(',');
        foreach (var Tag in Tags)
        {
            if (myEntity.HasAnyTags(FastTags.Parse(Tag)))
                return true;
        }

        return false;
    }

    public static void AddBuffToRadius(string strBuff, Vector3 position, int Radius)
    {
        // If there's no radius, pick 30 blocks.
        if (Radius <= 0)
            Radius = 30;

        var world = GameManager.Instance.World;
        var entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(position, Vector3.one * Radius));
        if (entitiesInBounds.Count > 0)
            for (var i = 0; i < entitiesInBounds.Count; i++)
            {
                var entity = entitiesInBounds[i] as EntityAlive;
                if (entity != null)
                    if (!entity.Buffs.HasBuff(strBuff))
                        entity.Buffs.AddBuff(strBuff);
            }
    }

    public static void AddQuestToRadius(string strQuest, Vector3 position, int Radius)
    {
        // If there's no radius, pick 30 blocks.
        if (Radius <= 0)
            Radius = 30;

        var world = GameManager.Instance.World;
        var entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(position, Vector3.one * Radius));
        if (entitiesInBounds.Count > 0)
            for (var i = 0; i < entitiesInBounds.Count; i++)
            {
                var entity = entitiesInBounds[i] as EntityAliveSDX;
                if (entity != null)
                    entity.questJournal.AddQuest(QuestClass.CreateQuest(strQuest));

                var player = entitiesInBounds[i] as EntityPlayerLocal;
                if (player != null)
                    player.QuestJournal.AddQuest(QuestClass.CreateQuest(strQuest));
            }
    }

    public static int ChangeHandholdItem(int EntityID, Need myCurrentNeed, int Preferred = -1)
    {
        var index = 0;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return index;
        if (Preferred == -1)
            switch (myCurrentNeed)
            {
                // Ranged
                case Need.Ranged:
                    index = FindItemWithAction(EntityID, typeof(ItemActionRanged));
                    break;
                // Ranged
                case Need.Melee:
                    index = FindItemWithAction(EntityID, typeof(ItemActionMelee));
                    if (index == 0)
                        index = FindItemWithAction(EntityID, typeof(ItemActionDynamicMelee));
                    break;
                case Need.Health:
                    index = FindItemWithTag(EntityID, "medical");
                    break;
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
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;

        var item = myEntity.inventory.holdingItem;
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

    public static bool SameValue(float f1, float f2)
    {
        if (Math.Abs(f1 - f2) < 0.1)
            return true;
        return false;
    }

    public static ItemStack GetItemStackByProperty(int EntityID, string property)
    {
        var itemStack = ItemStack.Empty;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return itemStack;

        // Check for the items in the tool belt.
        foreach (var stack in myEntity.inventory.GetSlots())
        {
            if (CheckItemStack(stack, property))
                return stack;
        }

        // Check for the items in the inventory.
        foreach (var stack in myEntity.bag.GetSlots())
        {
            if (CheckItemStack(stack, property))
                return stack;
        }

        // if there's no loot container, don't check it.
        if (myEntity.lootContainer == null) return itemStack;

        foreach (var stack in myEntity.lootContainer.items)
        {
            if (CheckItemStack(stack, property))
                return stack;
        }

        return itemStack;
    }
    public static ItemStack GetItemStackByAction(int EntityID, Type findAction)
    {
        var itemStack = ItemStack.Empty;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return itemStack;

        // Check for the items in the tool belt.
        foreach (var stack in myEntity.inventory.GetSlots())
        {
            if (CheckItemStack(stack, findAction))
                return stack;
        }

        // Check for the items in the inventory.
        foreach (var stack in myEntity.bag.GetSlots())
        {
            if (CheckItemStack(stack, findAction))
                return stack;
        }

        // if there's no loot container, don't check it.
        if (myEntity.lootContainer == null) return itemStack;

        foreach (var stack in myEntity.lootContainer.items)
        {
            if (CheckItemStack(stack, findAction))
                return stack;
        }

        return itemStack;
    }

    public static ItemStack GetItemByName(int EntityID, string items)
    {
        var itemStack = ItemStack.Empty;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return itemStack;

        foreach (var ID in items.Split(','))
        {
            // Validate the the string is actually an Item.
            var item = ItemClass.GetItem(ID);
            if (item != null)
            {
                foreach (var stack in myEntity.inventory.GetSlots())
                {
                    if (CheckItemStackByName(stack,ID))
                        return stack;
                }

                foreach (var stack in myEntity.lootContainer?.items)
                {
                    if (CheckItemStackByName(stack, ID))
                        return stack;
                }

                // Check for the items in the inventory.
                foreach (var stack in myEntity.bag.GetSlots())
                {
                    if (CheckItemStackByName(stack, ID))
                        return stack;
                }

            }
        }
        return itemStack;
    }
    public static ItemStack GetItemStackByTag(int EntityID, string Tag)
    {
        var itemStack = ItemStack.Empty;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return itemStack;

        var tag = FastTags.Parse(Tag);
        // Check for the items in the tool belt.
        foreach (var stack in myEntity.inventory.GetSlots())
        {
            if (CheckItemStack(stack, tag))
                return stack;
        }

        // Check for the items in the inventory.
        foreach (var stack in myEntity.bag.GetSlots())
        {
            if (CheckItemStack(stack, tag))
                return stack;
        }

        // if there's no loot container, don't check it.
        if (myEntity.lootContainer == null) return itemStack;

        foreach (var stack in myEntity.lootContainer.items)
        {
            if (CheckItemStack(stack, tag))
                return stack;
        }

        return itemStack;
    }

    public static int FindItemWithTag(int EntityID, string Tag)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return -1;

        var tag = FastTags.Parse(Tag);
        var counter = -1;

        // Check for the items in the tool belt.
        foreach (var stack in myEntity.inventory.GetSlots())
        {
            counter++;
            if (CheckItemStack(stack, tag))
                return counter;
        }

        // Reset counter.
        counter = -1;

        // Check for the items in the inventory.
        foreach (var stack in myEntity.bag.GetSlots())
        {
            counter++;
            if (CheckItemStack(stack, tag))
                return counter;
        }

        // Reset counter.
        counter = -1;

        if (myEntity.lootContainer != null)
        {
            foreach (var stack in myEntity.lootContainer.items)
            {
                counter++;
                if (CheckItemStack(stack, tag))
                    return counter;
            }
        }


        return -1;
    }

    private static bool CheckItemStack(ItemStack stack, Type findAction)
    {
        if (Equals(stack, ItemStack.Empty))
            return false;
        if (stack.itemValue == null)
            return false;

        if (Equals(stack, ItemStack.Empty))
            return false;
        if (stack.itemValue?.ItemClass?.Actions == null)
            return false;
        foreach (var action in stack.itemValue.ItemClass.Actions)
        {
            if (action == null)
                continue;
            var checkType = action.GetType();

            if (findAction == checkType || findAction.IsAssignableFrom(checkType))
            {
                return true;

            }
        }

        return false;
    }

    private static bool CheckItemStack(ItemStack stack, string property)
    {
        if (Equals(stack, ItemStack.Empty))
            return false;
        if (stack.itemValue == null)
            return false;

        if (stack.itemValue.ItemClass.Properties.Contains(property))
            return true;
        return false;
    }

    private static bool CheckItemStackByName(ItemStack stack, string itemName)
    {
        if (Equals(stack, ItemStack.Empty))
            return false;
        if (stack.itemValue == null)
            return false;

        return stack.itemValue.ItemClass != null && stack.itemValue.ItemClass.GetItemName() == itemName;
    }

    private static bool CheckItemStack(ItemStack stack, FastTags tag)
    {
        if (Equals(stack, ItemStack.Empty))
            return false;
        if (stack.itemValue == null)
            return false;

        return stack.itemValue.ItemClass != null && stack.itemValue.ItemClass.HasAnyTags(tag);
    }

    //public static ItemStack GetItemWithAction(int EntityID, ItemAction action, string tags = "")
    //{
    //    var index = -1;
    //    var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
    //    if (myEntity == null)
    //        return null;

    //    index = FindItemWithAction(EntityID, action.GetType(), tags);
    //    if (index < 0)
    //        return null;

    //    return myEntity.inventory.GetItem(index);
    //}

    // Searches the entity, looking for a specific action
    public static int FindItemWithAction(int EntityID, Type findAction, string tags = "")
    {
        var index = -1;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return index;

        var fastTags = FastTags.none;
        if (!string.IsNullOrEmpty(tags))
            fastTags = FastTags.Parse(tags);

        var counter = -1;
        foreach (var stack in myEntity.lootContainer.items)
        {
            counter++;
            if (Equals(stack, ItemStack.Empty))
                continue;
            if (stack.itemValue?.ItemClass?.Actions == null)
                continue;
            foreach (var action in stack.itemValue.ItemClass.Actions)
            {
                if (action == null)
                    continue;
                var checkType = action.GetType();

                if (findAction == checkType || findAction.IsAssignableFrom(checkType))
                {
                    if (string.IsNullOrEmpty(tags))
                        return counter;

                    if (CheckItemStack(stack, fastTags))
                        return counter;
                }
            }
        }

        return index;
    }


    // Returns false if its not within AI Range
    public static bool CheckAIRange(int EntityID, int targetEntity)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;
        var myTarget = GameManager.Instance.World.GetEntity(targetEntity) as EntityAlive;
        if (myTarget == null)
            return false;

        // Find the max range for the weapon
        // Example Range:  50
        //   Hold Ground Distance between 20 to 50 
        //   Retreat distance is 20%, so 20
        var MaxRangeForWeapon = EffectManager.GetValue(PassiveEffects.MaxRange, myEntity.inventory.holdingItemItemValue, 60f, myEntity, null, myEntity.inventory.holdingItem.ItemTags);

        MaxRangeForWeapon = MaxRangeForWeapon * 2;
        var HoldGroundDistance = MaxRangeForWeapon * 0.80f; // minimum range to hold ground 
        var RetreatDistance = MaxRangeForWeapon * 0.30f; // start retreating at this distance.
        var distanceSq = myTarget.GetDistanceSq(myEntity);

        var MinMeleeRange = GetFloatValue(EntityID, "MinimumMeleeRange");
        if (MinMeleeRange == -1)
            MinMeleeRange = 2;

        DisplayLog(myEntity.EntityName + " Max Range: " + MaxRangeForWeapon + " Hold Ground: " + HoldGroundDistance + " Retreatdistance: " + RetreatDistance + " Entity Distance: " + distanceSq);


        // if they are too close, switch to melee
        if (distanceSq <= MinMeleeRange)
            return false;


        // Hold your ground
        if (distanceSq > RetreatDistance && distanceSq <= HoldGroundDistance) // distance greater than 20%  of the range of the weapon
        {
            DisplayLog(myEntity.EntityName + " Stopping: Retreat: " + (distanceSq > RetreatDistance) + " Hold: " + (distanceSq <= HoldGroundDistance));
            Stop(EntityID);
        }

        // Back away!
        if (distanceSq > MinMeleeRange && distanceSq <= RetreatDistance)
            BackupHelper(EntityID, myTarget.position, 40);


        // if we can't see the target, move closer rather than staying at range.
        if (!myEntity.CanSee(myTarget))
        {
            DisplayLog(myEntity.EntityName + " I cannot see my target.");
            return false;
        }


        // if the entity is half the distance away, approach
        if (distanceSq > 200)
            return false;

        return true;
    }

    public static void Stop(int EntityID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return;

        myEntity.navigator?.clearPath();
        myEntity.moveHelper?.Stop();
        myEntity.speedForward = 0;
        myEntity.speedStrafe = 0;
    }

    public static void BackupHelper(int EntityID, Vector3 awayFrom, int distance)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return;

        if (myEntity.moveHelper == null)
            return;

        var dirV = myEntity.position - awayFrom;
        var vector = Vector3.zero;

        vector = myEntity.position + -new Vector3(awayFrom.x, 0f, awayFrom.z) * distance;

        // If you are blocked, try to go to another side.
        //vector = RandomPositionGenerator.CalcAway(myEntity, distance, distance,distance, awayFrom);
        myEntity.moveHelper.SetMoveTo(vector, false);

        myEntity.SetInvestigatePosition(vector, 200);


        // Move away at a hard coded speed of -4 to make them go backwards
        //  myEntity.speedForward = -4f;// Mathf.SmoothStep(myEntity.speedForward, -0.25f, 2 * Time.deltaTime);

        // Keep them facing the spot
        myEntity.SetLookPosition(awayFrom);
        myEntity.RotateTo(awayFrom.x, awayFrom.y, awayFrom.z, 30f, 30f);
    }

    public static bool CheckProperty(int EntityID, string Property)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return false;

        var entityClass = EntityClass.list[myEntity.entityClass];
        if (entityClass.Properties.Values.ContainsKey(Property))
            return true;

        if (entityClass.Properties.Classes.ContainsKey(Property))
            return true;

        return false;
    }


    public static void ProcessConsumables(int EntityID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return;

        var foodAmount = GetCVarValue(EntityID, "$foodAmountAdd");
        if (foodAmount > 0.5f)
        {
            myEntity.Health += Utils.Fastfloor(foodAmount);
            foodAmount -= 0.5f;
        }

        if (myEntity.Health > myEntity.GetMaxHealth())
            myEntity.Health = myEntity.GetMaxHealth();
    }

    public static bool HasTask(int EntityID, string strTask)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity != null)
        {
            if (EntityClass.list[myEntity.entityClass].UseAIPackages)
                return false;

            foreach (var task in myEntity.aiManager.GetTasks<EAIBase>())
                if (task.GetTypeName().Contains(strTask))
                    return true;
        }

        return false;
    }

    public static bool Hire(int EntityID, EntityPlayerLocal _player)
    {
        var result = false;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return result;

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
        if (uiforPlayer)
        {
            if (uiforPlayer.xui.PlayerInventory.GetItemCount(GetHireCurrency(EntityID)) >= GetHireCost(EntityID))
            {
                // Create the stack of currency
                var stack = new ItemStack(GetHireCurrency(EntityID), GetHireCost(EntityID));
                DisplayLog(" Removing Item: " + stack);
                uiforPlayer.xui.PlayerInventory.RemoveItems(new[] { stack });

                // Add the stack of currency to the NPC, and set its orders.
                //myEntity.bag.AddItem(stack);
                SetLeaderAndOwner(EntityID, _player.entityId);
                return true;
            }

            GameManager.ShowTooltip(_player, "You cannot afford me. I want " + GetHireCost(EntityID) + " " + GetHireCurrency(EntityID), "ui_denied");
        }

        return false;
    }


    // Returns the leader of the passed in entity ID.
    public static Entity GetLeader(int EntityID)
    {
        Entity leader = null;
        var currentEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (currentEntity)
        {
            if (currentEntity.Buffs.HasCustomVar("Leader"))
            {
                leader = GameManager.Instance.World.GetEntity((int)currentEntity.Buffs.GetCustomVar("Leader"));

                // Something happened to our leader.
                if (leader == null)
                {
                    currentEntity.Buffs.RemoveCustomVar("Leader");
                    leader = currentEntity;
                }
            }
        }

        return leader;
    }

    public static bool isEntityHungry(int EntityID)
    {
        var result = false;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            var lstBuffs = ConfigureEntityClass(EntityID, "HungryBuffs");
            if (lstBuffs.Count == 0)
                lstBuffs.AddRange(new[] { "buffStatusHungry2", "buffStatusHungry1" });

            result = CheckIncentive(EntityID, lstBuffs, null);
        }

        if (result)
            DisplayLog(" Is Entity hungry? " + result);
        return result;
    }

    public static bool isEntityHurt(int EntityID)
    {
        var result = false;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            var lstBuffs = new List<string>();
            lstBuffs.Add("buffStatusWounded");
            lstBuffs.Add("buffInjuryBleeding");


            result = CheckIncentive(EntityID, lstBuffs, null);

            if (result == false)
                if (myEntity.Health < myEntity.GetMaxHealth() * 0.75)
                {
                    DisplayLog(" Entity's Health is less than 35%");
                    result = true;
                }
        }

        if (result)
            DisplayLog(" Is Entity Hurt? ");
        return result;
    }

    // This will search for a mother entity to see it can satisfy its thirst from its mother, rather than a traditional water block.
    public static float GetEntityWater(int EntityID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
            if (myEntity.Buffs.HasCustomVar("Mother"))
            {
                var MotherID = myEntity.Buffs.GetCustomVar("Mother");
                var MotherEntity = myEntity.world.GetEntity((int)MotherID) as EntityAliveSDX;
                if (MotherEntity)
                {
                    DisplayLog(" My Mother is: " + MotherEntity.EntityName);
                    if (MotherEntity.Buffs.HasCustomVar("MilkLevel"))
                    {
                        DisplayLog("Heading to mommy");
                        var MilkLevel = MotherEntity.Buffs.GetCustomVar("MilkLevel");
                        myEntity.SetInvestigatePosition(myEntity.world.GetEntity((int)MotherID).position, 60);
                        return MilkLevel;
                    }
                }
            }

        return 0f;
    }

    public static bool isEntityThirsty(int EntityID)
    {
        var result = false;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            var lstBuffs = ConfigureEntityClass(EntityID, "ThirstyBuffs");
            if (lstBuffs.Count == 0)
                lstBuffs.AddRange(new[] { "buffStatusThirsty2", "buffStatusThirsty1" });

            result = CheckIncentive(EntityID, lstBuffs, null);
        }

        if (result) // Really spammy if its false
            DisplayLog(" Is Entity Thirsty? " + result);
        return result;
    }

    // Owner can be different than the current leader
    public static Entity GetOwner(int EntityID)
    {
        Entity leader = null;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null) return null;

        if (myEntity.Buffs.HasCustomVar("Owner"))
            leader = GameManager.Instance.World.GetEntity((int)myEntity.Buffs.GetCustomVar("Owner"));

        return leader;
    }

    public static Entity GetLeaderOrOwner(int EntityID)
    {
        var leader = GetLeader(EntityID);
        if (leader == null)
            leader = GetOwner(EntityID);

        return leader;
    }

    public static void SetLeader(int EntityID, int LeaderID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        var leaderEntity = GameManager.Instance.World.GetEntity(LeaderID) as EntityAlive;
        if (myEntity == null || leaderEntity == null) return;

        myEntity.Buffs.SetCustomVar("Leader", LeaderID);
        myEntity.moveSpeed = leaderEntity.moveSpeed;
        myEntity.moveSpeedAggro = leaderEntity.moveSpeedAggro;
        myEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
        SetCurrentOrder(EntityID, Orders.Follow);

        (GameManager.Instance.World.GetEntity(LeaderID) as EntityAlive).AddOwnedEntity(myEntity);

    }

    public static void SetOwner(int EntityID, int LeaderID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        var leaderEntity = GameManager.Instance.World.GetEntity(LeaderID) as EntityAlive;
        if (myEntity != null && leaderEntity != null)
            myEntity.Buffs.SetCustomVar("Owner", LeaderID);
    }

    public static void SetLeaderAndOwner(int EntityID, int LeaderID)
    {
        SetLeader(EntityID, LeaderID);
        SetOwner(EntityID, LeaderID);
    }

    public static bool IsAnAlly(int EntityID, int AttackingID)
    {
        var result = false;

        // Let you hurt yourself.
        if (EntityID == AttackingID)
            return false;

        var myLeader = GetLeaderOrOwner(EntityID);

        // If my attacker is my leader, forgive him.
        if (myLeader)
            if (AttackingID == myLeader.entityId)
                return true;

        var theirLeader = GetLeaderOrOwner(AttackingID);
        if (myLeader && theirLeader && myLeader.entityId == theirLeader.entityId)
            result = true;

        // Don't fight vehicles.
        var entity = GameManager.Instance.World.GetEntity(AttackingID);
        if (entity != null && entity is EntityVehicle)
            return true;

        return result;
    }

    public static Entity GetAttackOrRevengeTarget(int EntityID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity != null)
        {
            if (myEntity.GetAttackTarget() != null)
                return myEntity.GetAttackTarget();
            if (myEntity.GetRevengeTarget() != null)
                return myEntity.GetRevengeTarget();
        }

        return null;
    }

    public static bool isTame(int EntityID, EntityAlive _player)
    {
        var myLeader = GetLeaderOrOwner(EntityID);
        if (myLeader)
            if (myLeader.entityId == _player.entityId)
                return true;
        return false;
    }

    public static void SetCurrentOrder(int EntityID, Orders order)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            DisplayLog(" Setting Current Order: " + order);
            myEntity.Buffs.SetCustomVar("PreviousOrder", (float)GetCurrentOrder(EntityID), false);
            myEntity.Buffs.SetCustomVar("CurrentOrder", (float)order, false);
        }
    }

    public static void RevertPreviousOrder(int EntityID)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            var previous = -1f;
            if (myEntity.Buffs.HasCustomVar("PreviousOrder"))
                previous = myEntity.Buffs.GetCustomVar("PreviousOrder");
            else
                previous = (float)Orders.Wander;

            var current = myEntity.Buffs.GetCustomVar("CurrentOrder");
            myEntity.Buffs.SetCustomVar("PreviousOrder", current);
            myEntity.Buffs.SetCustomVar("CurrentOrder", previous);
        }
    }

    public static Orders GetCurrentOrder(int EntityID)
    {
        var currentOrder = Orders.Wander;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
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

    public static bool ExecuteCMD(int EntityID, string strCommand, EntityPlayer player)
    {
        var strDisplay = "ExecuteCMD( " + strCommand + " ) to " + EntityID + " From " + player.DebugNameInfo;
        AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);

        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return false;

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);

        var position = myEntity.position;

        // Restore it's walk speed to default.
        myEntity.RestoreSpeed();

        switch (strCommand)
        {
            case "TellMe":
                GameManager.ShowTooltip(player as EntityPlayerLocal, myEntity + "\n\n\n\n\n", "ui_denied");
                AdvLogging.DisplayLog(AdvFeatureClass, "\n\nBuffs:");
                foreach (var Buff in myEntity.Buffs.ActiveBuffs)
                {
                    Debug.Log("Buff: " + Buff.BuffName + " ");
                    AdvLogging.DisplayLog(AdvFeatureClass, "\t" + Buff.BuffName);
                }

                var wvar = 0;
                myEntity.emodel.avatarController.TryGetInt("WVar", out wvar);
                Debug.Log("WVar: " + wvar);
                Debug.Log("CVar Human Walk Types: " + myEntity.Buffs.GetCustomVar("HumanWalkTypes"));
                myEntity.emodel.avatarController.TryGetInt("IsHuman", out wvar);
                Debug.Log("IsHuman: " + wvar);

                var temp = 0f;
                myEntity.emodel.avatarController.TryGetFloat("IVar", out temp);
                Debug.Log("Ivar: " + temp);
                AdvLogging.DisplayLog(AdvFeatureClass, myEntity.ToString());
                AdvLogging.DisplayLog(AdvFeatureClass, "Body Damage: ");
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has Right Leg? " + myEntity.bodyDamage.HasRightLeg);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has Left Leg? " + myEntity.bodyDamage.HasLeftLeg);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has Limbs? " + myEntity.bodyDamage.HasLimbs);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Arm or Leg missing? " + myEntity.bodyDamage.IsAnyArmOrLegMissing);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Has any leg missing? " + myEntity.bodyDamage.IsAnyLegMissing);
                var LegDamageTrigger = false;
                myEntity.emodel.avatarController.TryGetTrigger("LegDamageTrigger", out LegDamageTrigger);
                AdvLogging.DisplayLog(AdvFeatureClass, "\t Leg Damage Trigger? " + LegDamageTrigger);
                break;
            case "ShowAffection":
                GameManager.ShowTooltip(player as EntityPlayerLocal, "You gentle scratch and stroke the side of the animal.", "");
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
                myEntity.guardPosition = position;
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

                myEntity.guardPosition = position;
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Stopping Move Helper()");

                if (myEntity.moveHelper != null) // No move helper on the client when on Dedi
                    myEntity.moveHelper.Stop();
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Guard Look");
                myEntity.guardLookPosition = player.GetLookVector();
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

                myEntity.patrolCoordinates.Clear(); // Clear the existing point
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");

                break;
            case "Patrol":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order");
                SetCurrentOrder(EntityID, Orders.Patrol);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Order");

                break;


            case "Hire":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Opening Hire ");
                var result = Hire(EntityID, player as EntityPlayerLocal);
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Done with Hire");

                break;
            case "OpenInventory":
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting Order Open Inventory");

                if (myEntity.lootContainer == null)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Loot Container is null");
                }
                else
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Loot Container: " + myEntity.lootContainer);
                    if (string.IsNullOrEmpty(myEntity.lootContainer.lootListName))
                        myEntity.lootContainer.lootListName = "traderNPC";

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

    public static bool CanExecuteTask(int EntityID, Orders order)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            if (GetCurrentOrder(EntityID) != order)
            {
                DisplayLog("CanExecuteTask(): Current Order does not match passed in Order: " + GetCurrentOrder(EntityID));
                return false;
            }

            var leader = GetLeader(EntityID);
            var Distance = 0f;
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
                DisplayLog("CanExecuteTask(): I have an Attack Target: " + myEntity.GetAttackTarget());
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

                DisplayLog("CanExecuteTask(): I have a Revenge Target: " + myEntity.GetRevengeTarget());
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
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
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
        var result = -1;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
            result = GetIntValue(EntityID, "HireCost");

        if (result == -1)
            result = 1000;
        return result;
    }

    public static ItemValue GetHireCurrency(int EntityID)
    {
        var result = ItemClass.GetItem("casinoCoin");
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
            result = GetItemValue(EntityID, "HireCurrency");

        if (result.IsEmpty())
            result = ItemClass.GetItem("casinoCoin", true);
        return result;
    }

    public static ItemValue GetItemValue(int EntityID, string strProperty)
    {
        var result = ItemClass.GetItem("casinoCoin");
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            var entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = ItemClass.GetItem(entityClass.Properties.Values[strProperty]);
            if (result.IsEmpty())
                result = ItemClass.GetItem("casinoCoin");
        }

        return result;
    }

    public static int GetIntValue(int EntityID, string strProperty)
    {
        var result = -1;

        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity)
        {
            var entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = int.Parse(entityClass.Properties.Values[strProperty]);
        }

        return result;
    }

    public static Vector3 GetNewPositon(int EntityID, bool Random = false)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return Vector3.zero;


        var result = Vector3.zero;
        var Paths = SphereCache.GetPaths(EntityID);
        if (Paths == null || Paths.Count == 0)
        {
            //  Grab a list of blocks that are configured for this class.
            //    <property name="PathingBlocks" value="PathingCube" />
            var Blocks = ConfigureEntityClass(EntityID, "PathingBlocks");
            if (Blocks.Count == 0)
                Blocks.Add("PathingCube");

            //Scan for the blocks in the area
            var PathingVectors = ModGeneralUtilities.ScanForTileEntityInChunksListHelper(myEntity.position, Blocks, EntityID);
            if (PathingVectors == null || PathingVectors.Count == 0)
                return result;

            //Add to the cache
            SphereCache.AddPaths(EntityID, PathingVectors);
        }

        // Finds the closet block we matched with.
        var tMin = new Vector3();
        if (Random)
        {
            tMin = SphereCache.GetRandomPath(EntityID);
        }
        else
        {
            tMin = ModGeneralUtilities.FindNearestBlock(myEntity.position, SphereCache.GetPaths(EntityID));
            if (tMin == Vector3.zero)
                return tMin;
        }

        // Remove it from the cache.
        SphereCache.RemovePath(EntityID, tMin);

        result = GameManager.Instance.World.FindSupportingBlockPos(tMin);
        // Center the pathing position.
        result.x = Utils.Fastfloor(result.x) + 0.5f;
        result.y = Utils.Fastfloor(result.y) + 0.5f;
        result.z = Utils.Fastfloor(result.z) + 0.5f;
        return result;
    }

    public static void OpenDoor(int EntityID, Vector3i blockPos)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            var block = myEntity.world.GetBlock(blockPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
            {
                var chunk = myEntity.world.GetChunkFromWorldPos(blockPos) as Chunk;
                block.Block.OnBlockActivated(myEntity.world, chunk.ClrIdx, blockPos, block, myEntity);
            }
        }
    }

    public static void CloseDoor(int EntityID, Vector3i blockPos)
    {
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            var block = myEntity.world.GetBlock(blockPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
            {
                var chunk = myEntity.world.GetChunkFromWorldPos(blockPos) as Chunk;
                if (chunk == null)
                    return;
                /*
                var flag = !BlockDoor.IsDoorOpen(block.meta);
                ChunkCluster chunkCluster = myEntity.world.ChunkClusters[chunk.ClrIdx];
                if (chunkCluster == null)
                {
                    return;
                }
                block.meta = (byte)((flag ? 1 : 0) | ((int)block.meta & -2)); 
                myEntity.world.SetBlockRPC(chunk.ClrIdx, blockPos, block);
                */
                block.Block.OnBlockActivated(myEntity.world, chunk.ClrIdx, blockPos, block, null);
            }
        }
    }


    public static bool GetBoolValue(int EntityID, string strProperty)
    {
        var result = false;

        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            var entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = bool.Parse(entityClass.Properties.Values[strProperty]);
        }

        return result;
    }

    public static float GetFloatValue(int EntityID, string strProperty)
    {
        float result = -1;

        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            var entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                result = StringParsers.ParseFloat(entityClass.Properties.Values[strProperty]);
        }

        return result;
    }

    public static string GetStringValue(int EntityID, string strProperty)
    {
        var result = string.Empty;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
        {
            var entityClass = EntityClass.list[myEntity.entityClass];
            if (entityClass.Properties.Values.ContainsKey(strProperty))
                return entityClass.Properties.Values[strProperty];
        }

        return result;
    }

    public static float GetCVarValue(int EntityID, string strCvarName)
    {
        var value = 0f;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity)
            if (myEntity.Buffs.HasCustomVar(strCvarName))
                value = myEntity.Buffs.GetCustomVar(strCvarName);
        return value;
    }

    public static List<string> ConfigureEntityClass(int EntityID, string strKey)
    {
        var TempList = new List<string>();
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return TempList;

        var entityClass = EntityClass.list[myEntity.entityClass];
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

    public static void lootContainerOpened(TileEntityLootContainer _te, LocalPlayerUI _playerUI, int _entityIdThatOpenedIt)
    {
        if (_playerUI != null)
        {
            var flag = true;
            var lootContainerName = string.Empty;
            if (_te.entityId != -1)
            {
                var entity = GameManager.Instance.World.GetEntity(_te.entityId);
                if (entity != null)
                {
                    lootContainerName = Localization.Get(EntityClass.list[entity.entityClass].entityClassName);
                    if (entity is EntityVehicle)
                        flag = false;
                }
            }
            else
            {
                var block = GameManager.Instance.World.GetBlock(_te.ToWorldPos());
                lootContainerName = Localization.Get(Block.list[block.type].GetBlockName());
            }

            if (flag)
            {
                ((XUiC_LootWindowGroup)((XUiWindowGroup)_playerUI.windowManager.GetWindow("looting")).Controller).SetTileEntityChest(lootContainerName, _te);
                _playerUI.windowManager.Open("looting", true);
            }

            var lootContainer = LootContainer.GetLootContainer(_te.lootListName);
            if (lootContainer != null && _playerUI.entityPlayer != null) lootContainer.ExecuteBuffActions(_te.entityId, _playerUI.entityPlayer);
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
        var result = false;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return result;

        // same faction
        if (myEntity.factionId == entity.factionId)
            return true;

        var revengeTarget = myEntity.GetRevengeTarget();
        if (revengeTarget != null)
        {
            return false;
        }

        var myRelationship = FactionManager.Instance.GetRelationshipTier(myEntity, entity);
        DisplayLog(" CheckFactionForEnemy: " + myRelationship);
        if (myRelationship == FactionManager.Relationship.Hate)
        {
            DisplayLog(" I hate this entity: " + entity);
            return false;
        }

        DisplayLog(" My relationship with this " + entity + " is: " + myRelationship);
        return true;
    }

    public static bool CheckForBuff(int EntityID, string strBuff)
    {
        var result = false;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return result;
        if (myEntity.Buffs.HasBuff(strBuff))
            result = true;

        return result;
    }

    public static bool CheckIncentive(int EntityID, List<string> lstIncentives, EntityAlive entity)
    {
        var result = false;
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAlive;
        if (myEntity == null)
            return result;

        foreach (var strIncentive in lstIncentives)
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
        var myEntity = GameManager.Instance.World.GetEntity(EntityID) as EntityAliveSDX;
        if (myEntity == null)
            return "";

        var FoodAmount = Mathf.RoundToInt(myEntity.Stats.Stamina.ModifiedMax + GetCVarValue(EntityID, "foodAmount")).ToString();
        var WaterAmount = Mathf.RoundToInt(myEntity.Stats.Water.Value + GetCVarValue(EntityID, "waterAmount")).ToString();

        // string strOutput = myEntity.EntityName + " - ID: " + myEntity.entityId + " Health: " + myEntity.Stats.Health.Value;
        var strOutput = myEntity.EntityName + " - ID: " + myEntity.entityId + " Health: " + myEntity.Health + "/" + myEntity.GetMaxHealth();
        strOutput += " Stamina: " + myEntity.Stats.Stamina.Value + " Thirst: " + myEntity.Stats.Water.Value + " Food: " + FoodAmount + " Water: " + WaterAmount;
        strOutput += " Sanitation: " + GetCVarValue(EntityID, "solidWasteAmount");
        // Read the Food items configured.
        var strFoodItems = GetStringValue(EntityID, "FoodItems");
        if (strFoodItems == string.Empty)
            strFoodItems = "All Food Items";
        strOutput += "\n Food Items: " + strFoodItems;
        // Read the Water Items
        var strWaterItems = GetStringValue(EntityID, "WaterItems");
        if (strWaterItems == string.Empty)
            strWaterItems = "All Water Items";
        strOutput += "\n Water Items: " + strWaterItems;
        strOutput += "\n Food Bins: " + GetStringValue(EntityID, "FoodBins");
        strOutput += "\n Water Bins: " + GetStringValue(EntityID, "WaterBins");
        if (myEntity.Buffs.HasCustomVar("CurrentOrder"))
            strOutput += "\n Current Order: " + GetCurrentOrder(EntityID);
        if (myEntity.Buffs.HasCustomVar("Leader"))
        {
            var leader = GetLeader(EntityID);
            if (leader)
                strOutput += "\n Current Leader: " + leader.entityId;
        }

        strOutput += "\n Active Buffs: ";
        foreach (var buff in myEntity.Buffs.ActiveBuffs)
            strOutput += "\n\t" + buff.BuffName + " ( Seconds: " + buff.DurationInSeconds + " Ticks: " + buff.DurationInTicks + " )";
        strOutput += "\n Active CVars: ";
        foreach (var myCvar in myEntity.Buffs.CVars)
            strOutput += "\n\t" + myCvar.Key + " : " + myCvar.Value;
        strOutput += "\n Active Quests: ";
        foreach (var quest in myEntity.questJournal.quests)
            strOutput += "\n\t" + quest.ID + " Current State: " + quest.CurrentState + " Current Phase: " + quest.CurrentPhase;
        strOutput += "\n Patrol Points: ";
        foreach (var vec in myEntity.patrolCoordinates)
            strOutput += "\n\t" + vec;
        strOutput += "\n\nCurrency: " + GetHireCurrency(EntityID) + " Faction: " + myEntity.factionId;

        DisplayLog(strOutput);

        return strOutput;
    }
}