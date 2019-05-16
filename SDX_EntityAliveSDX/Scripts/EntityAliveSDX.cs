/*
 * Class: EntityAliveSDX
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base entityAlive. This is meant to be a base class, where other classes can extend
 *      from, giving them the ability to accept quests and buffs.
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features. 
 *
 *      <property name="Class" value="EntityAliveSDX, Mods" />
 */
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntityAliveSDX : EntityNPC
{
    public QuestJournal QuestJournal = new QuestJournal();
    public List<String> lstQuests = new List<String>();

    List<String> lstHungryBuffs = new List<String>();
    List<String> lstThirstyBuffs = new List<String>();

    public Orders currentOrder = Orders.Wander;
    public List<Vector3> PatrolCoordinates = new List<Vector3>();
    public int HireCost = 1000;
    public ItemValue HireCurrency = ItemClass.GetItem("casinoCoin", false);
    int DefaultTraderID = 0;

    public Vector3 GuardPosition = Vector3.zero;
    public Vector3 GuardLookPosition = Vector3.zero;
    String strSoundAccept = "";
    String strSoundReject = "";

    private float flEyeHeight = -1f;
    private bool bWentThroughDoor = false;

    // Update Time for NPC's onUpdateLive(). If the time is greater than update time, it'll do a trader area check, opening and closing. Something we don't want.
    private float updateTime = Time.time - 2f;

    // Default name
    String strMyName = "Bob";
    String strTitle;

    public System.Random random = new System.Random();

    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if(blDisplayLog && !IsDead())
            Debug.Log(entityName + ": " + strMessage);
    }

    // Used for maslow
    public virtual bool CheckIncentive(List<String> Incentives)
    {
        foreach(String strIncentive in Incentives)
        {
            if(Buffs.HasBuff(strIncentive))
                return true;
        }
        return false;
    }
    public virtual bool CheckIncentive(List<String> lstIncentives, EntityAlive entity)
    {
        bool result = false;
        foreach(String strIncentive in lstIncentives)
        {
            DisplayLog(" Checking Incentive: " + strIncentive);
            // Check if the entity that is looking at us has the right buff for us to follow.
            if(Buffs.HasBuff(strIncentive))
                result = true;

            // Check if there's a cvar for that incentive, such as $Mother or $Leader.
            if(Buffs.HasCustomVar(strIncentive))
            {
                // DisplayLog(" Incentive: " + strIncentive + " Value: " + this.Buffs.GetCustomVar(strIncentive));
                if((int)Buffs.GetCustomVar(strIncentive) == entity.entityId)
                    result = true;
            }

            // Then we check if the control mechanism is an item being held.
            if(entity.inventory.holdingItem.Name == strIncentive)
                result = true;

            // if we are true here, it means we found a match to our entity.
            if(result)
                break;
        }
        return result;
    }

    public float GetFloatValue(String strProperty)
    {
        float result = 0f;
        EntityClass entityClass = EntityClass.list[this.entityClass];
        if(entityClass.Properties.Values.ContainsKey(strProperty))
            entityClass.Properties.ParseFloat(EntityClass.PropMoveSpeed, ref result);
        return result;
    }

    public String GetStringValue(String strProperty)
    {
        string result = String.Empty;
        EntityClass entityClass = EntityClass.list[this.entityClass];
        if(entityClass.Properties.Values.ContainsKey(strProperty))
            return entityClass.Properties.Values[strProperty];
        return result;
    }

    public Entity GetLeader()
    {
        if(Buffs.HasCustomVar("Leader"))
        {
            Entity leader = world.GetEntity((int)Buffs.GetCustomVar("Leader"));
            return leader;
        }

        return null;
    }

    public bool CanExecuteTask(Orders order)
    {
        DisplayLog("CanExecuteTask():  Current Order:" + Buffs.GetCustomVar("CurrentOrder") + " : Order To Be Checked: " + order.ToString());

        // If we don't match our current order, don't execute
        if(Buffs.HasCustomVar("CurrentOrder"))
        {
            if(Buffs.GetCustomVar("CurrentOrder") != (float)order)
            {
                DisplayLog("CanExecuteTask(): Current Order does not match this order: Current Order:" + Buffs.GetCustomVar("CurrentOrder") + " : Order To Be Checked: " + (float)order);
                return false;
            }
        }

        EntityPlayerLocal leader = GetLeader() as EntityPlayerLocal;
        float Distance = 0f;
        if(leader)
            Distance = GetDistance(leader);

        // If we have an attack or revenge target, don't execute task
        if(GetAttackTarget() != null && GetAttackTarget().IsAlive())
        {
            // If we have a leader and they are far away, abandon the fight and go after your leader.
            DisplayLog("CanExecuteTask(): I have an attack target and a leader. My leader is this far away: " + Distance);
            if(Distance > 20)
            {
                DisplayLog("CanExecuteTask(): Leaving my attack target.");
                SetAttackTarget(null, 0);
                return true;
            }
            DisplayLog("CanExecuteTask():  There is an attack target set. Not executing Order: " + order.ToString());
            DisplayLog(" Attack Target: " + GetAttackTarget().ToString());
            return false;

        }

        if(GetRevengeTarget() != null && GetRevengeTarget().IsAlive())
        {
            DisplayLog("CanExecuteTask(): I have an Revenge target and a leader. My leader is this far away: " + Distance);
            if(Distance > 20)
            {
                DisplayLog("CanExecuteTask(): Leaving my Revenge target.");
                SetRevengeTarget(null);
                return true;
            }

            DisplayLog("CanExecuteTask():  There is an Revenge target set. Not executing Order: " + order.ToString());
            DisplayLog("Revenge Target: " + GetRevengeTarget().ToString());
            return false;

        }


        if(sleepingOrWakingUp || bodyDamage.CurrentStun != EnumEntityStunType.None || Jumping)
            return false;

        return true;
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


    public override float GetEyeHeight()
    {
        if(flEyeHeight == -1f)
            return base.GetEyeHeight();

        return flEyeHeight;
    }

    // Over-ride for CopyProperties to allow it to read in StartingQuests.
    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];

        if(entityClass.Properties.Values.ContainsKey("EyeHeight"))
            flEyeHeight = GetFloatValue("EyeHeight");

        // Read in a list of names then pick one at random.
        if(entityClass.Properties.Values.ContainsKey("Names"))
        {
            string text = entityClass.Properties.Values["Names"];
            string[] Names = text.Split(',');
            int index = random.Next(0, Names.Length);
            strMyName = Names[index];
        }

        if(entityClass.Properties.Values.ContainsKey("Titles"))
        {
            string text = entityClass.Properties.Values["Titles"];
            string[] Names = text.Split(',');
            int index = random.Next(0, Names.Length);
            strTitle = Names[index];
        }
        if(entityClass.Properties.Values.ContainsKey("HireCost"))
            HireCost = int.Parse(entityClass.Properties.Values["HireCost"]);

        if(entityClass.Properties.Values.ContainsKey("HireCurrency"))
        {
            HireCurrency = ItemClass.GetItem(entityClass.Properties.Values["HireCurrency"], false);
            if(HireCurrency.IsEmpty())
                HireCurrency = ItemClass.GetItem("casinoCoin", false);
        }

        lstHungryBuffs = ConfigureEntityClass("HungryBuffs", entityClass);
        lstThirstyBuffs = ConfigureEntityClass("ThirstyBuffs", entityClass);

        if(entityClass.Properties.Classes.ContainsKey("Boundary"))
        {
            DisplayLog(" Found Bandary Settings");
            String strBoundaryBox = "0,0,0";
            String strCenter = "0,0,0";
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["Boundary"];
            foreach(KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                DisplayLog("Key: " + keyValuePair.Key);
                if(keyValuePair.Key == "BoundaryBox")
                {
                    DisplayLog(" Found a Boundary Box");
                    strBoundaryBox = dynamicProperties3.Values[keyValuePair.Key];
                    continue;
                }

                if(keyValuePair.Key == "Center")
                {
                    DisplayLog(" Found a Center");
                    strCenter = dynamicProperties3.Values[keyValuePair.Key];
                    continue;
                }
            }

            Vector3 Box = StringParsers.ParseVector3(strBoundaryBox, 0, -1);
            Vector3 Center = StringParsers.ParseVector3(strCenter, 0, -1);
            ConfigureBounaryBox(Box, Center);
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
    public void ConfigureBounaryBox(Vector3 newSize, Vector3 center)
    {
        BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
        if(component)
        {
            DisplayLog(" Box Collider: " + component.size.ToCultureInvariantString());
            DisplayLog(" Current Boundary Box: " + boundingBox.ToCultureInvariantString());
            // Re-adjusting the box collider     
            component.size = newSize;

            scaledExtent = new Vector3(component.size.x / 2f * base.transform.localScale.x, component.size.y / 2f * base.transform.localScale.y, component.size.z / 2f * base.transform.localScale.z);
            Vector3 vector = new Vector3(component.center.x * base.transform.localScale.x, component.center.y * base.transform.localScale.y, component.center.z * base.transform.localScale.z);
            boundingBox = global::BoundsUtils.BoundsForMinMax(-scaledExtent.x, -scaledExtent.y, -scaledExtent.z, scaledExtent.x, scaledExtent.y, scaledExtent.z);

            boundingBox.center = boundingBox.center + vector;

            ////  this.nativeCollider = component;
            //if (this.isDetailedHeadBodyColliders())
            //    component.enabled = false;

            if(center != Vector3.zero)
                boundingBox.center = center;

            DisplayLog(" After BoundaryBox: " + boundingBox.ToCultureInvariantString());
        }

    }
    Vector3i lastDoorOpen;
    private float nextCheck = 0;
    public float CheckDelay = 5f;

    public bool OpenDoor()
    {
        if(nextCheck < Time.time)
        {
            nextCheck = Time.time + CheckDelay;
            Vector3i blockPosition = GetBlockPosition();
            Vector3i TargetBlockPosition = new Vector3i();

            int MaxDistance = 2;
            for(var x = blockPosition.x - MaxDistance; x <= blockPosition.x + MaxDistance; x++)
            {
                for(var z = blockPosition.z - MaxDistance; z <= blockPosition.z + MaxDistance; z++)
                {
                    TargetBlockPosition.x = x;
                    TargetBlockPosition.y = Utils.Fastfloor(position.y);
                    TargetBlockPosition.z = z;

                    // DisplayLog(" Target Block: " + TargetBlockPosition + " Block: " + this.world.GetBlock(TargetBlockPosition).Block.GetBlockName() + " My Position: " + this.GetBlockPosition());
                    BlockValue blockValue = world.GetBlock(TargetBlockPosition);
                    if(Block.list[blockValue.type].HasTag(BlockTags.Door) && !Block.list[blockValue.type].HasTag(BlockTags.Window))
                    {
                        DisplayLog(" I found a door.");

                        lastDoorOpen = TargetBlockPosition;
                        BlockDoor targetDoor = (Block.list[blockValue.type] as BlockDoor);


                        bool isDoorOpen = BlockDoor.IsDoorOpen(blockValue.meta);
                        if(isDoorOpen)
                            lastDoorOpen = Vector3i.zero;
                        else
                            lastDoorOpen = TargetBlockPosition;

                        emodel.avatarController.SetTrigger("OpenDoor");
                        DisplayLog(" Is Door Open? " + BlockDoor.IsDoorOpen(blockValue.meta));

                        Chunk chunk = (Chunk)world.GetChunkFromWorldPos(TargetBlockPosition);
                        TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)world.GetTileEntity(0, TargetBlockPosition);
                        if(tileEntitySecureDoor == null)
                        {
                            DisplayLog(" Not a door.");
                            return false;
                        }
                        if(tileEntitySecureDoor.IsLocked())
                        {
                            DisplayLog(" The Door is locked.");
                            return false;
                        }
                        targetDoor.OnBlockActivated(world, 0, TargetBlockPosition, blockValue, null);
                        return true;

                    }
                }
            }
        }
        return false;
    }

    public void RestoreSpeed()
    {
        // Reset the movement speed when an attack target is set
        moveSpeed = GetFloatValue("MoveSpeed");

        Vector2 vector;
        vector.x = moveSpeed;
        vector.y = moveSpeed;
        EntityClass entityClass = EntityClass.list[this.entityClass];
        entityClass.Properties.ParseVec(EntityClass.PropMoveSpeedAggro, ref vector);
        moveSpeedAggro = vector.x;
        moveSpeedAggroMax = vector.y;

    }

    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
    {
        // Don't allow you to interact with it when its dead.
        if(IsDead() || NPCInfo == null)
            return new EntityActivationCommand[0];

        return new EntityActivationCommand[]
        {
            new EntityActivationCommand("Greet " + EntityName, "talk" , true)
        };


    }

    protected override void Awake()
    {
        base.Awake();
     //   this.moveHelper = new EntityMoveHelperSDX(this);
        // So they don't step over each other.
        // this.stepHeight = 0.005f;

    }
    public override bool Attack(bool _bAttackReleased)
    {
        if(this.attackTarget == null)
        {
            OpenDoor();
            return false;
        }

        return base.Attack(_bAttackReleased);
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        // set the IsBusy flag, so it won't wander away when you are talking to it.
        emodel.avatarController.SetBool("IsBusy", true);

        // Look at the entity that is talking to you.
        SetLookPosition(_entityFocusing.getHeadPosition());

        _entityFocusing.Buffs.SetCustomVar("CurrentNPC", entityId, true);
        base.OnEntityActivated(_indexInBlockActivationCommands, _tePos, _entityFocusing);

        return true;
    }

    public virtual bool ExecuteCMD(String strCommand, EntityPlayer player)
    {
        Debug.Log(GetType().ToString() + " : Command: " + strCommand);
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);

        // Restore it's walk speed to default.
        RestoreSpeed();

        switch(strCommand)
        {
            case "ShowMe":
                GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, ToString() + "\n\n\n\n\n", "ui_denied");
                break;
            case "ShowAffection":
                GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, "You gentle scratch and stroke the side of the animal.", "");
                break;
            case "FollowMe":
                Buffs.SetCustomVar("Leader", player.entityId, true);
                Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Follow, false);
                moveSpeed = player.moveSpeed;
                moveSpeedAggro = player.moveSpeedAggro;

                break;
            case "StayHere":
                Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.None, false);
                GuardPosition = position;
                moveHelper.Stop();
                break;
            case "GuardHere":
                Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Stay, false);
                SetLookPosition(player.GetLookVector());
                GuardPosition = position;
                moveHelper.Stop();
                GuardLookPosition = player.GetLookVector();
                break;
            case "Wander":
                Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Wander, false);
                break;
            case "SetPatrol":
                Buffs.SetCustomVar("Leader", player.entityId, true);
                Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.SetPatrolPoint, false);
                moveSpeed = player.moveSpeed;
                moveSpeedAggro = player.moveSpeedAggro;
                PatrolCoordinates.Clear(); // Clear the existing point.
                break;
            case "Patrol":
                Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Patrol, false);
                break;
            case "Hire":
                bool result = Hire(player as EntityPlayerLocal);
                break;
            case "OpenInventory":
                GameManager.Instance.TELockServer(0, GetBlockPosition(), entityId, player.entityId);
                uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
                if(lootContainer == null)
                    DisplayLog(" Loot Container is null");

                DisplayLog(" Get Open Time");
                DisplayLog("Loot Container: " + lootContainer.ToString());
                lootContainer.lootListIndex = 62;
                DisplayLog(" Loot List: " + lootContainer.lootListIndex);

                DisplayLog(lootContainer.GetOpenTime().ToString());
                lootContainerOpened(lootContainer, uiforPlayer, player.entityId);

                break;
            case "Loot":
                Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Loot, false);
                Buffs.RemoveCustomVar("Leader");
                break;

        }

        if(Buffs.HasCustomVar("CurrentOrder"))
        {
            currentOrder = (Orders)Buffs.GetCustomVar("CurrentOrder");
            DisplayLog(" Setting Current Order: " + currentOrder);
        }
        return true;

    }

    private void lootContainerOpened(TileEntityLootContainer _te, LocalPlayerUI _playerUI, int _entityIdThatOpenedIt)
    {
        if(_playerUI != null)
        {
            bool flag = true;
            string lootContainerName = string.Empty;
            if(_te.entityId != -1)
            {
                Entity entity = world.GetEntity(_te.entityId);
                if(entity != null)
                {
                    lootContainerName = Localization.Get(EntityClass.list[entity.entityClass].entityClassName, "");
                    if(entity is EntityVehicle)
                    {
                        flag = false;
                    }
                }
            }
            else
            {
                BlockValue block = world.GetBlock(_te.ToWorldPos());
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

    public virtual bool isTame(EntityAlive _player)
    {
        if(Buffs.HasCustomVar("Leader") && Buffs.GetCustomVar("Leader") == _player.entityId)
            return true;
        if(Buffs.HasCustomVar("Owner") && Buffs.GetCustomVar("Owner") == _player.entityId)
            return true;

        return false;
    }

    public int GetHireCost()
    {
        return HireCost;
    }
    public ItemValue GetHireCurrency()
    {
        return HireCurrency;
    }

    public void SetOwner(EntityPlayerLocal player)
    {
        Buffs.SetCustomVar("Owner", player.entityId, true);
        Buffs.SetCustomVar("Leader", player.entityId, true);
        Buffs.SetCustomVar("CurrentOrder", (float)Orders.Follow, true);

        //this.factionId = player.factionId;

        // Match the player's speed if its set to follow
        moveSpeed = player.moveSpeed;
        moveSpeedAggro = player.moveSpeedAggro;
        SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
    }

    public virtual bool Hire(EntityPlayerLocal _player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
        if(null != uiforPlayer)
        {
            if(uiforPlayer.xui.PlayerInventory.GetItemCount(HireCurrency) >= HireCost)
            {
                // Create the stack of currency
                ItemStack stack = new ItemStack(HireCurrency, HireCost);
                uiforPlayer.xui.PlayerInventory.RemoveItems(new ItemStack[] { stack }, 1);

                // Add the stack of currency to the NPC, and set its orders.
                bag.AddItem(stack);
                SetOwner(_player);
                return true;
            }
            else
            {
                GameManager.ShowTooltipWithAlert(_player, "You cannot afford me. I want " + HireCost + " " + HireCurrency, "ui_denied");
            }
        }
        return false;
    }

    public override string EntityName
    {
        get
        {
            if(strMyName == "Bob")
                return entityName;
            else
                return strMyName + " the " + base.EntityName;
        }
        set
        {
            if(!value.Equals(entityName))
            {
                entityName = value;
                bPlayerStatsChanged |= !isEntityRemote;
            }
        }
    }

    public override bool CanBePushed()
    {
        return true;
    }

    public override void PostInit()
    {
        base.PostInit();

        // disable god mode, since that's enabled by default in the NPC
        IsGodMode.Value = false;

        if(NPCInfo != null)
            DefaultTraderID = NPCInfo.TraderID;

        InvokeRepeating("DisplayStats", 0f, 60f);

        // Check if there's a loot container or not already attached to store its stuff.
        DisplayLog(" Checking Entity's Loot Container");
        if(lootContainer == null)
        {
            DisplayLog(" Entity does not have a loot container. Creating one.");
            int lootList = GetLootList();
            DisplayLog(" Loot list is: " + lootList);
            lootContainer = new TileEntityLootContainer(null);
            lootContainer.entityId = entityId;
            lootContainer.SetContainerSize(new Vector2i(8, 6), true);

            // If the loot list is available, set the container to that size.
            if(lootList != 0)
                lootContainer.SetContainerSize(LootContainer.lootList[lootList].size, true);
        }
    }

    // We use a tempList to store the patrol coordinates of each vector, but centered over the block. This allows us to check to make sure each
    // vector we are storing is on a new block, and not just  10.2 and 10.4. This helps smooth out the entity's walk. However, we do want accurate patrol points,
    // so we store the accurate patrol positions for the entity.
    List<Vector3> tempList = new List<Vector3>();
    public virtual void UpdatePatrolPoints(Vector3 position)
    {
        // Center the x and z values of the passed in blocks for a unique check.
        Vector3 temp = position;
        temp.x = 0.5f + Utils.Fastfloor(position.x);
        temp.z = 0.5f + Utils.Fastfloor(position.z);
        temp.y = Utils.Fastfloor(position.y);

        if(!tempList.Contains(temp))
        {
            tempList.Add(temp);
            if(!PatrolCoordinates.Contains(position))
                PatrolCoordinates.Add(position);
        }
    }


    // Reads the buff and quest information
    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        strMyName = _br.ReadString();
        Buffs.Read(_br);
        QuestJournal = new QuestJournal();
        QuestJournal.Read(_br);
        PatrolCoordinates.Clear();
        String strPatrol = _br.ReadString();
        foreach(String strPatrolPoint in strPatrol.Split(';'))
        {
            Vector3 temp = StringToVector3(strPatrolPoint);
            if(temp != Vector3.zero)
                PatrolCoordinates.Add(temp);
        }

        //if (this.PatrolCoordinates.Count > 0)
        //    this.Buffs.AddCustomVar("CurrentOrder", (float)Orders.Patrol);

        String strGuardPosition = _br.ReadString();
        GuardPosition = StringToVector3(strGuardPosition);
        factionId = _br.ReadByte();
        GuardLookPosition = StringToVector3(_br.ReadString());
    }

    public Vector3 StringToVector3(string sVector)
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

    // Saves the buff and quest information
    public override void Write(BinaryWriter _bw)
    {
        base.Write(_bw);
        _bw.Write(strMyName);
        Buffs.Write(_bw, false);
        QuestJournal.Write(_bw);
        String strPatrolCoordinates = "";
        foreach(Vector3 temp in PatrolCoordinates)
            strPatrolCoordinates += ";" + temp;

        _bw.Write(strPatrolCoordinates);
        _bw.Write(GuardPosition.ToString());
        _bw.Write(factionId);
        _bw.Write(GuardLookPosition.ToString());

    }

    public void DisplayStats()
    {
        DisplayLog(ToString());
    }

    public override string ToString()
    {
        String FoodAmount = ((float)Mathf.RoundToInt(Stats.Stamina.ModifiedMax + Stats.Entity.Buffs.GetCustomVar("foodAmount"))).ToString();
        String WaterAmount = ((float)Mathf.RoundToInt(Stats.Water.Value + Stats.Entity.Buffs.GetCustomVar("waterAmount"))).ToString();
        String strSanitation = "Disabled.";
        if(Buffs.HasCustomVar("solidWasteAmount"))
            strSanitation = Buffs.GetCustomVar("solidWasteAmount").ToString();

        string strOutput = strMyName + " The " + entityName + " - ID: " + entityId + " Health: " + Stats.Health.Value;
        strOutput += " Stamina: " + Stats.Stamina.Value + " Thirst: " + Stats.Water.Value + " Food: " + FoodAmount + " Water: " + WaterAmount;
        strOutput += " Sanitation: " + strSanitation;

        // Read the Food items configured.
        String strFoodItems = GetStringValue("FoodItems");
        if(strFoodItems == String.Empty)
            strFoodItems = "All Food Items";
        strOutput += "\n Food Items: " + strFoodItems;

        // Read the Water Items
        String strWaterItems = GetStringValue("WaterItems");
        if(strWaterItems == String.Empty)
            strWaterItems = "All Water Items";
        strOutput += "\n Water Items: " + strWaterItems;

        strOutput += "\n Food Bins: " + GetStringValue("FoodBins");
        strOutput += "\n Water Bins: " + GetStringValue("WaterBins");

        if(Buffs.HasCustomVar("CurrentOrder"))
            strOutput += "\n Current Order: " + (Orders)(int)Buffs.GetCustomVar("CurrentOrder");

        if(Buffs.HasCustomVar("Leader"))
            strOutput += "\n Current Leader: " + (Orders)(int)Buffs.GetCustomVar("Leader");

        strOutput += "\n Active Buffs: ";
        foreach(BuffValue buff in Buffs.ActiveBuffs)
            strOutput += "\n\t" + buff.BuffName + " ( Seconds: " + buff.DurationInSeconds + " Ticks: " + buff.DurationInTicks + " )";

        strOutput += "\n Active Quests: ";
        foreach(Quest quest in QuestJournal.quests)
            strOutput += "\n\t" + quest.ID + " Current State: " + quest.CurrentState + " Current Phase: " + quest.CurrentPhase;

        strOutput += "\n Patrol Points: ";
        foreach(Vector3 vec in PatrolCoordinates)
            strOutput += "\n\t" + vec.ToString();

        strOutput += "\n\nCurrency: " + HireCurrency + " Faction: " + factionId;
        return strOutput;
    }

    public void GiveQuest(String strQuest)
    {
        // Don't give duplicate quests.
        foreach(Quest quest in QuestJournal.quests)
        {
            if(quest.ID == strQuest.ToLower())
                return;
        }

        // Make sure the quest is valid
        Quest NewQuest = QuestClass.CreateQuest(strQuest);
        if(NewQuest == null)
            return;

        // If there's no shared owner, it tries to read the PlayerLocal's entity ID. This entity doesn't have that.
        NewQuest.SharedOwnerID = entityId;
        NewQuest.QuestGiverID = -1;
        QuestJournal.AddQuest(NewQuest);
    }


    public override void OnUpdateLive()
    {

        if(lastDoorOpen != Vector3i.zero)
            OpenDoor();


        // Non-player entities don't fire all the buffs or stats, so we'll manually fire the water tick,
        Stats.Water.Tick(0.5f, 0, false);

        // then fire the updatestats over time, which is protected from a IsPlayer check in the base onUpdateLive().
        Stats.UpdateStatsOverTime(0.5f);

        // Check the state to see if the controller IsBusy or not. If it's not, then let it walk.
        bool isBusy = false;
        emodel.avatarController.TryGetBool("IsBusy", out isBusy);
        if(isBusy || currentOrder == Orders.None)
        {
            moveDirection = Vector3.zero;
            moveHelper.Stop();
        }

        updateTime = Time.time - 2f;
        base.OnUpdateLive();

        // Make the entity sensitive to the environment.
        // this.Stats.UpdateWeatherStats(0.5f, this.world.worldTime, false);


        // Check if there's a player within 10 meters of us. If not, resume wandering.
        emodel.avatarController.SetBool("IsBusy", false);

        if(GetAttackTarget() == null || GetRevengeTarget() == null)
        {
            List<global::Entity> entitiesInBounds = global::GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(position, Vector3.one * 5f));
            if(entitiesInBounds.Count > 0)
            {
                for(int i = 0; i < entitiesInBounds.Count; i++)
                {
                    if(entitiesInBounds[i] is EntityPlayer)
                    {
                        emodel.avatarController.SetBool("IsBusy", true);
                        SetLookPosition(entitiesInBounds[i].getHeadPosition());
                        RotateTo(entitiesInBounds[i], 30f, 30f);
                        moveHelper.Stop();
                        break;
                    }
                }
            }
        }
    }

    public bool IsInParty(int entityID)
    {
        DisplayLog(" Checking if I am in a Party with: " + entityId);
        // This is the entity that is trying to attack me.
        Entity entityTarget = world.GetEntity(entityID);
        if(entityTarget == null)
        {
            DisplayLog("IsInParty(): Entity Is Null");
            return false;
        }

        // Find my master / leader
        EntityPlayerLocal localPlayer;
        if(Buffs.HasCustomVar("Leader"))
        {
            DisplayLog(" Checking my Leader");
            // Find out who the leader is.
            int PlayerID = (int)Buffs.GetCustomVar("Leader");
            DisplayLog(" My Leader ID is: " + PlayerID);
            localPlayer = world.GetEntity(PlayerID) as EntityPlayerLocal;
            if(localPlayer == null)
            {
                DisplayLog("IsInParty(): I have a leader, but that leader is not an EntityPlayerLocal");
                return false;
            }

            if(PlayerID == entityID)
            {
                DisplayLog("My leader hurt me. Forgiving.");
                return true;
            }
        }
        else
        {
            DisplayLog("IsInParty(): No leader.");
            // no leader? You are on your own.
            return false;
        }

        DisplayLog(" The Target entity is: " + entityTarget.ToString());
        // Let's check if a player is being hurt.
        if(entityTarget is EntityPlayerLocal)
        {
            DisplayLog(" Target Entity is a Player.");
            if(localPlayer == null)
            {
                DisplayLog(" Local Player is null.");
            }


            // If another player, who is part of my leader's party hurts me, ignore it.
            if(localPlayer.Party.ContainsMember(entityTarget as EntityPlayerLocal))
            {
                DisplayLog("IsInParty():  Enemy that attacked me is a player, but is party of my leader's party. Forgiving friendly fire.");
                return true;
            }
        }
        else if(entityTarget is EntityAliveSDX) // Check if its a non-player, and see if their leader is in my party.
        {
            if((entityTarget as EntityAliveSDX).Buffs.HasCustomVar("Leader"))
            {
                DisplayLog("IsInParty(): The attacking entity has a leader. Checking Party status...");
                int leader = (int)(entityTarget as EntityAliveSDX).Buffs.GetCustomVar("Leader");
                if(leader == localPlayer.entityId)
                {
                    DisplayLog("IsInParty(): We have the same leader. Forgiving.");
                    return true;
                }

                EntityPlayerLocal entLeader = world.GetEntity(leader) as EntityPlayerLocal;
                if(localPlayer.Party.ContainsMember(entLeader))
                {
                    DisplayLog("IsInParty(): The attacking entity has a leader and is party of my leader's party. Forgiving.");
                    return true;
                }
            }
        }
        return false;

    }
    public void ToggleTraderID(bool Restore)
    {
        if(NPCInfo == null)
            return;

        // Check if we are restoring the default trader ID.
        if(Restore)
            NPCInfo.TraderID = DefaultTraderID;
        else
            NPCInfo.TraderID = 0;
    }
    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
    {
        // If the attacking entity is connected to a party, then don't accept the damage.
        //   if (IsInParty(_damageSource.getEntityId()))
        //        return 0;

        // If we are being attacked, let the state machine know it can fight back
        emodel.avatarController.SetBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        int Damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        ToggleTraderID(true);
        return Damage;
    }

    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
    {
        // If we are being attacked, let the state machine know it can fight back
        emodel.avatarController.SetBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        base.ProcessDamageResponseLocal(_dmResponse);
        ToggleTraderID(true);
    }

    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if(isEntityRemote && _partialTicks > 1f)
        {
            _dist /= _partialTicks;
        }
        speedForward *= 0.5f;
        speedStrafe *= 0.5f;
        speedVertical *= 0.5f;
        if(Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            float num = Mathf.Sin(-rotation.y * 3.14159274f / 180f);
            float num2 = Mathf.Cos(-rotation.y * 3.14159274f / 180f);
            speedForward += num2 * _dist.z - num * _dist.x;
            speedStrafe += num2 * _dist.x + num * _dist.z;
        }
        if(Mathf.Abs(_dist.y) > 0.001f)
        {
            speedVertical += _dist.y;
        }
        SetMovementState();
    }
}
