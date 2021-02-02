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
    public bool isAlwaysAwake = false;
    public List<Vector3> PatrolCoordinates = new List<Vector3>();

    int DefaultTraderID = 0;

    public ItemValue MeleeWeapon = ItemClass.GetItem("meleeClubIron");
    public Vector3 GuardPosition = Vector3.zero;
    public Vector3 GuardLookPosition = Vector3.zero;

    public float flEyeHeight = -1f;
    public bool bWentThroughDoor = false;

    public float LostTimer = 0f;

    // Update Time for NPC's onUpdateLive(). If the time is greater than update time, it'll do a trader area check, opening and closing. Something we don't want.
    private float updateTime;

    // Default name
    String strMyName = "Bob";
    String strTitle;

    public System.Random random = new System.Random();

    private readonly bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog && !IsDead())
            Debug.Log(entityName + ": " + strMessage);
    }

    public override string ToString()
    {
        return entityName;
        //return EntityUtilities.DisplayEntityStats(entityId);
    }


    //public override bool IsAlive()
    //{
    //    if (IsAvailable() == false)
    //        return false;

    //    return base.IsAlive();
    //}


    //// if the NPC isn't available, don't return a loot. This disables the "Press <E> to search..."
    //public override int GetLootList()
    //{
    //    if (IsAvailable() == false)
    //        return 0;

    //    return base.GetLootList();
    //}
    //// Check to see if the NPC is available
    //public bool IsAvailable()
    //{
    //    if (this.Buffs.HasCustomVar("onMission") && this.Buffs.GetCustomVar("onMission") == 1f)
    //        return false;
    //    return true;
    //}

    public bool canJump = true;

    public override bool CanEntityJump()
    {
        return canJump;
    }

    // SendOnMission will make the NPC disappear and be unavailable
    public void SendOnMission(bool send)
    {
        if (send)
        {
            Buffs.AddCustomVar("onMission", 1f);
            emodel.avatarController.SetBool("IsBusy", true);
            GetRootTransform().gameObject.SetActive(false);
        }
        else
        {
            Buffs.AddCustomVar("onMission", 0f);
            emodel.avatarController.SetBool("IsBusy", false);
            GetRootTransform().gameObject.SetActive(true);
        }
    }
    public override float GetEyeHeight()
    {
        if (flEyeHeight == -1f)
            return base.GetEyeHeight();

        return flEyeHeight;
    }

    public override void SetModelLayer(int _layerId, bool _force = false)
    {
        //Utils.SetLayerRecursively(this.emodel.GetModelTransform().gameObject, _layerId);
    }
    // Over-ride for CopyProperties to allow it to read in StartingQuests.
    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];

        flEyeHeight = EntityUtilities.GetFloatValue(entityId, "EyeHeight");

        // Read in a list of names then pick one at random.
        if (entityClass.Properties.Values.ContainsKey("Names"))
        {
            string text = entityClass.Properties.Values["Names"];
            string[] Names = text.Split(',');

            int index = UnityEngine.Random.Range(0, Names.Length);
            strMyName = Names[index];
        }

        if (entityClass.Properties.Values.ContainsKey("SleeperInstantAwake"))
            isAlwaysAwake = true;

        if (entityClass.Properties.Values.ContainsKey("Titles"))
        {
            string text = entityClass.Properties.Values["Titles"];
            string[] Names = text.Split(',');
            int index = UnityEngine.Random.Range(0, Names.Length);
            strTitle = Names[index];
        }


        if (entityClass.Properties.Classes.ContainsKey("Boundary"))
        {
            DisplayLog(" Found Bandary Settings");
            String strBoundaryBox = "0,0,0";
            String strCenter = "0,0,0";
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["Boundary"];
            foreach (KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                DisplayLog("Key: " + keyValuePair.Key);
                if (keyValuePair.Key == "BoundaryBox")
                {
                    DisplayLog(" Found a Boundary Box");
                    strBoundaryBox = dynamicProperties3.Values[keyValuePair.Key];
                    continue;
                }

                if (keyValuePair.Key == "Center")
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

    protected override float getNextStepSoundDistance()
    {
        if (!IsRunning)
        {
            return 0.5f;
        }
        return 0.25f;
    }

    public override void SetSleeper()
    {
        // if configured as a sleeper, this should wake them up
        if (isAlwaysAwake)
            return;
        base.SetSleeper();
    }

    /// <summary>
    /// Overrides EntityAlive.OnAddedToWorld().
    /// When entities are spawned into sleeper volumes, which happens in SleeperVolume.Spawn(),
    /// several of their properties are set so they are spawned in a sleeping state.
    /// If the NPC should always be awake, those properties can be reset here.
    /// </summary>
    public override void OnAddedToWorld()
    {
        if (isAlwaysAwake)
        {
            // Set the current order, defaults to "Wander"
            EntityUtilities.SetCurrentOrder(entityId, EntityUtilities.GetCurrentOrder(entityId));

            // Set in EntityAlive.TriggerSleeperPose() - resetting here
            IsSleeping = false;
        }

        base.OnAddedToWorld();
    }

    public void ConfigureBounaryBox(Vector3 newSize, Vector3 center)
    {
        BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
        if (component)
        {
            DisplayLog(" Box Collider: " + component.size.ToCultureInvariantString());
            DisplayLog(" Current Boundary Box: " + boundingBox.ToCultureInvariantString());
            // Re-adjusting the box collider     
            component.size = newSize;

            scaledExtent = new Vector3(component.size.x / 2f * base.transform.localScale.x, component.size.y / 2f * base.transform.localScale.y, component.size.z / 2f * base.transform.localScale.z);
            Vector3 vector = new Vector3(component.center.x * base.transform.localScale.x, component.center.y * base.transform.localScale.y, component.center.z * base.transform.localScale.z);
            boundingBox = global::BoundsUtils.BoundsForMinMax(-scaledExtent.x, -scaledExtent.y, -scaledExtent.z, scaledExtent.x, scaledExtent.y, scaledExtent.z);

            boundingBox.center = boundingBox.center + vector;

            if (center != Vector3.zero)
                boundingBox.center = center;

            DisplayLog(" After BoundaryBox: " + boundingBox.ToCultureInvariantString());
        }

    }

    public void RestoreSpeed()
    {
        // Reset the movement speed when an attack target is set
        moveSpeed = EntityUtilities.GetFloatValue(entityId, "MoveSpeed");

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
        if (IsDead() || NPCInfo == null)
            return new EntityActivationCommand[0];

        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(this, _entityFocusing);
        if (myRelationship == FactionManager.Relationship.Hate)
            return new EntityActivationCommand[0];

        // If not a human, don't talk to them
        if (!EntityUtilities.IsHuman(entityId))
            return new EntityActivationCommand[0];

        return new EntityActivationCommand[]
        {
            new EntityActivationCommand("Greet " + EntityName, "talk" , true)
        };


    }


    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        // Don't allow interaction with a Hated entity
        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(this, _entityFocusing);
        if (myRelationship == FactionManager.Relationship.Hate)
            return false;

        // If they have attack targets, don't interrupt them.
        if (GetAttackTarget() != null || GetRevengeTarget() != null)
            return false;

        // set the IsBusy flag, so it won't wander away when you are talking to it.
        emodel.avatarController.SetBool("IsBusy", true);

        // Look at the entity that is talking to you.
        SetLookPosition(_entityFocusing.getHeadPosition());

        // This is used by various dialog boxes to know which EntityID the player is talking too.
        _entityFocusing.Buffs.SetCustomVar("CurrentNPC", entityId, true);

        base.OnEntityActivated(_indexInBlockActivationCommands, _tePos, _entityFocusing);


        SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

        return true;
    }

    public override string EntityName
    {
        get
        {
            if (strMyName == "Bob")
                return entityName;

            if (String.IsNullOrEmpty(strTitle))
                return Localization.Get(strMyName);
            else
                return Localization.Get( strMyName ) + " the " + Localization.Get( strTitle );
        }
        set
        {
            if (!value.Equals(entityName))
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

        if (NPCInfo != null)
            DefaultTraderID = NPCInfo.TraderID;

        InvokeRepeating("DisplayStats", 0f, 60f);

        // Check if there's a loot container or not already attached to store its stuff.
        DisplayLog(" Checking Entity's Loot Container");
        if (lootContainer == null)
        {
            DisplayLog(" Entity does not have a loot container. Creating one.");
            int lootList = GetLootList();
            DisplayLog(" Loot list is: " + lootList);
            lootContainer = new TileEntityLootContainer(null);
            lootContainer.entityId = entityId;
            lootContainer.SetContainerSize(new Vector2i(8, 6), true);

            // If the loot list is available, set the container to that size.
            if (lootList != 0)
                lootContainer.SetContainerSize(LootContainer.lootList[lootList].size, true);
        }

        Buffs.SetCustomVar("$waterStaminaRegenAmount", 0, false);

        SetupStartingItems();
        inventory.SetHoldingItemIdx(0);

        // Does a quick local scan to see what pathing blocks, if any, are nearby. If one is found nearby, then it'll use that code for pathing.
        SetupAutoPathingBlocks();
    }

    // We use a tempList to store the patrol coordinates of each vector, but centered over the block. This allows us to check to make sure each
    // vector we are storing is on a new block, and not just  10.2 and 10.4. This helps smooth out the entity's walk. However, we do want accurate patrol points,
    // so we store the accurate patrol positions for the entity.
    readonly List<Vector3> tempList = new List<Vector3>();
    private List<string> startedThisFrame;

    public virtual void UpdatePatrolPoints(Vector3 position)
    {
        // Center the x and z values of the passed in blocks for a unique check.
        Vector3 temp = position;
        temp.x = 0.5f + Utils.Fastfloor(position.x);
        temp.z = 0.5f + Utils.Fastfloor(position.z);
        temp.y = Utils.Fastfloor(position.y);

        if (!tempList.Contains(temp))
        {
            tempList.Add(temp);
            if (!PatrolCoordinates.Contains(position))
                PatrolCoordinates.Add(position);
        }
    }

    // Reads the buff and quest information
    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        strMyName = _br.ReadString();
        QuestJournal = new QuestJournal();
        QuestJournal.Read(_br);
        PatrolCoordinates.Clear();
        String strPatrol = _br.ReadString();
        foreach (String strPatrolPoint in strPatrol.Split(';'))
        {
            Vector3 temp = ModGeneralUtilities.StringToVector3(strPatrolPoint);
            if (temp != Vector3.zero)
                PatrolCoordinates.Add(temp);
        }

        String strGuardPosition = _br.ReadString();
        GuardPosition = ModGeneralUtilities.StringToVector3(strGuardPosition);
        factionId = _br.ReadByte();
        GuardLookPosition = ModGeneralUtilities.StringToVector3(_br.ReadString());
        try
        {
            Buffs.Read(_br);
        }
        catch (Exception)
        {
            // fail safe to protect game saves
        }


    }


    public void SetupAutoPathingBlocks()
    {
        if (Buffs.HasCustomVar("PathingCode") && (Buffs.GetCustomVar("PathingCode") < 0 || Buffs.GetCustomVar("PathingCode" ) > 0 ) )
            return;

        // Check if pathing blocks are defined.
        List<string> Blocks = EntityUtilities.ConfigureEntityClass(entityId, "PathingBlocks");
        if (Blocks.Count == 0)
            return;

        //Scan for the blocks in the area
        List<Vector3> PathingVectors = ModGeneralUtilities.ScanForTileEntityInChunksListHelper(position, Blocks, entityId);
        if (PathingVectors == null || PathingVectors.Count == 0)
            return;

     
        // Find the nearest block, and if its a sign, read its code.
        Vector3 target = ModGeneralUtilities.FindNearestBlock(position, PathingVectors);
        TileEntitySign tileEntitySign = GameManager.Instance.World.GetTileEntity(0, new Vector3i(target)) as TileEntitySign;
        if (tileEntitySign == null)
        {

            return;
        }

        // Since signs can have multiple codes, splite with a ,, parse each one.
        String text = tileEntitySign.GetText();
        float code = 0f; // Defined here as DMT compiler doesn't like inlining it.
        foreach (String temp in text.Split(','))
        {
            if (StringParsers.TryParseFloat(temp, out code))
            {
                Buffs.AddCustomVar("PathingCode", code);
                return;
            }
        }
    }

    // Saves the buff and quest information
    public override void Write(BinaryWriter _bw)
    {
        base.Write(_bw);
        _bw.Write(strMyName);
        QuestJournal.Write(_bw);
        String strPatrolCoordinates = "";
        foreach (Vector3 temp in PatrolCoordinates)
            strPatrolCoordinates += ";" + temp;

        _bw.Write(strPatrolCoordinates);
        _bw.Write(GuardPosition.ToString());
        _bw.Write(factionId);
        _bw.Write(GuardLookPosition.ToString());
        try
        {
            Buffs.Write(_bw, false);
        }
        catch (Exception)
        {
            // fail safe to protect game saves
        }
    }


    public void GiveQuest(String strQuest)
    {
        // Don't give duplicate quests.
        foreach (Quest quest in QuestJournal.quests)
        {
            if (quest.ID == strQuest.ToLower())
                return;
        }

        // Make sure the quest is valid
        Quest NewQuest = QuestClass.CreateQuest(strQuest);
        if (NewQuest == null)
            return;

        // If there's no shared owner, it tries to read the PlayerLocal's entity ID. This entity doesn't have that.
        NewQuest.SharedOwnerID = entityId;
        NewQuest.QuestGiverID = -1;
        QuestJournal.AddQuest(NewQuest);
    }

    public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
    {
        // Check the state to see if the controller IsBusy or not. If it's not, then let it walk.
        bool isBusy = false;
        if (emodel != null && emodel.avatarController != null)
            emodel.avatarController.TryGetBool("IsBusy", out isBusy);
        if (isBusy)
            return;

        base.MoveEntityHeaded(_direction, _isDirAbsolute);
    }
    public override void OnUpdateLive()
    {

        SetupAutoPathingBlocks();

        if (Buffs.HasCustomVar("PathingCode") && Buffs.GetCustomVar("PathingCode") == -1)
            EntityUtilities.SetCurrentOrder(this.entityId, EntityUtilities.Orders.Stay);

        // Wake them up if they are sleeping, since the trigger sleeper makes them go idle again.
        if (!sleepingOrWakingUp && isAlwaysAwake)
        {
            IsSleeping = true;
            ConditionalTriggerSleeperWakeUp();
        }

        emodel.avatarController.SetBool("IsBusy", false);

        Buffs.RemoveBuff("buffnewbiecoat", false);
        Stats.Health.MaxModifier = Stats.Health.Max;

        // Set CanFall and IsOnGround
        if (emodel != null && emodel.avatarController != null)
        {
            emodel.avatarController.SetBool("CanFall", !emodel.IsRagdollActive && bodyDamage.CurrentStun == EnumEntityStunType.None && !isSwimming);
            emodel.avatarController.SetBool("IsOnGround", onGround || isSwimming);
        }


        bool isHuman = false;
        if (EntityUtilities.IsHuman(entityId))
            isHuman = true;

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            //If blocked, check to see if its a door.
            if (moveHelper.IsBlocked && isHuman)
            {
                Vector3i blockPos = moveHelper.HitInfo.hit.blockPos;
                BlockValue block = world.GetBlock(blockPos);
                if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
                {
                    bool canOpenDoor = true;
                    TileEntitySecureDoor tileEntitySecureDoor = GameManager.Instance.World.GetTileEntity(0, blockPos) as TileEntitySecureDoor;
                    if (tileEntitySecureDoor != null)
                    {
                        if (tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.GetOwner() == "")
                            canOpenDoor = false;
                    }
                    //TileEntityPowered poweredDoor = GameManager.Instance.World.GetTileEntity(0, blockPos) as TileEntityPowered;
                    //if (poweredDoor != null)
                    //{
                    //    if (poweredDoor.IsLocked() && poweredDoor.GetOwner() == "")
                    //        canOpenDoor = false;

                    //}
                    if (canOpenDoor)
                    {
                        DisplayLog("I am blocked by a door. Trying to open...");
                        SphereCache.AddDoor(entityId, blockPos);
                        EntityUtilities.OpenDoor(entityId, blockPos);
                        //  We were blocked, so let's clear it.
                        moveHelper.ClearBlocked();

                    }
                }
            }
        }

        // Check to see if we've opened a door, and close it behind you.
        Vector3i doorPos = SphereCache.GetDoor(entityId);
        if (doorPos != Vector3i.zero && isHuman)
        {
            DisplayLog("I've opened a door recently. I'll see if I can close it.");
            BlockValue block = world.GetBlock(doorPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
            {
                float CloseDistance = 3;
                // If it's a multidim, increase tha radius a bit
                if (Block.list[block.type].isMultiBlock)
                {
                    Vector3i vector3i = StringParsers.ParseVector3i(Block.list[block.type].Properties.Values["MultiBlockDim"], 0, -1, false);
                    if (CloseDistance > vector3i.x)
                        CloseDistance = vector3i.x + 1;

                }
                if ((GetDistanceSq(doorPos.ToVector3()) > CloseDistance))
                {
                    DisplayLog("I am going to close the door now.");
                    EntityUtilities.CloseDoor(entityId, doorPos);
                    SphereCache.RemoveDoor(entityId, doorPos);
                }
            }
        }

        // Makes the NPC always face its attack or revent target.
        EntityAlive target = EntityUtilities.GetAttackOrReventTarget(entityId) as EntityAlive;
        if (target != null)
        {
            // makes the npc look at its attack target
            if (emodel != null && emodel.avatarController != null)
                emodel.SetLookAt(target.getHeadPosition());

            SetLookPosition(target.getHeadPosition());
            RotateTo(target, 30f, 30f);
            if (EntityUtilities.HasTask(entityId, "Ranged"))
            {
                if (EntityUtilities.CheckAIRange(entityId, target.entityId))
                    EntityUtilities.ChangeHandholdItem(entityId, EntityUtilities.Need.Ranged);
                else
                    EntityUtilities.ChangeHandholdItem(entityId, EntityUtilities.Need.Melee);
            }
            else
            {
                EntityUtilities.ChangeHandholdItem(entityId, EntityUtilities.Need.Melee);
            }
        }


        // Non-player entities don't fire all the buffs or stats, so we'll manually fire the water tick,
        //        Stats.Water.Tick(0.5f, 0, false);

        // then fire the updatestats over time, which is protected from a IsPlayer check in the base onUpdateLive().
        //Stats.UpdateStatsOverTime(0.5f);


        updateTime = Time.time + 2f;
        base.OnUpdateLive();

        // Allow EntityAliveSDX to get buffs from blocks
        if (!isEntityRemote &&  !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            updateBlockRadiusEffects();

        // No NPC info, don't continue
        if (NPCInfo == null)
            return;

        // If the Tile Entity Trader isn't set, set it now. Sometimes this fails, and won't allow interaction.
        if (TileEntityTrader == null)
        {
            TileEntityTrader = new TileEntityTrader(null);
            TileEntityTrader.entityId = entityId;
            TileEntityTrader.TraderData.TraderID = NPCInfo.TraderID;
        }

        // If there is no attack target, don't bother checking this.
        if (target == null)
        {
            if (!EntityUtilities.IsHuman(entityId))
                return;

            List<global::Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(position, Vector3.one * 5f));
            if (entitiesInBounds.Count > 0)
            {
                for (int i = 0; i < entitiesInBounds.Count; i++)
                {
                    if (entitiesInBounds[i] is EntityPlayerLocal || entitiesInBounds[i] is EntityPlayer)
                    {

                        // Check your faction relation. If you hate each other, don't stop and talk.
                        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(this, entitiesInBounds[i] as EntityPlayerLocal);
                        if (myRelationship == FactionManager.Relationship.Hate)
                            break;

                        if (GetDistance(entitiesInBounds[i]) < 1 && moveHelper != null)
                        {
                            DisplayLog("The entity is too close to me. Moving away: " + entitiesInBounds[i].ToString());
                            EntityUtilities.BackupHelper(entityId, entitiesInBounds[i].position, 3);
                            break;
                        }

                        // Turn to face the player, and stop the movement.
                        emodel.avatarController.SetBool("IsBusy", true);
                        SetLookPosition(entitiesInBounds[i].getHeadPosition());
                        RotateTo(entitiesInBounds[i], 90f, 90f);
                        EntityUtilities.Stop(entityId);
                        break;

                    }
                }
            }
        }
    }

    public override Ray GetLookRay()
    {
        return new Ray(position + new Vector3(0f, GetEyeHeight() * eyeHeightHackMod, 0f), GetLookVector());
    }

    public void ToggleTraderID(bool Restore)
    {
        if (NPCInfo == null)
            return;

        // Check if we are restoring the default trader ID.
        if (Restore)
            NPCInfo.TraderID = DefaultTraderID;
        else
            NPCInfo.TraderID = 0;
    }
    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
    {

        if (EntityUtilities.IsAnAlly(entityId, _damageSource.getEntityId()))
            return 0;

        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable"))
            return 0;

        if (Buffs.HasBuff("buffInvulnerable"))
            return 0;

        // If we are being attacked, let the state machine know it can fight back
        emodel.avatarController.SetBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        int Damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        ToggleTraderID(true);
        return Damage;
    }


    public new void SetRevengeTarget(EntityAlive _other)
    {
        if (_other)
        {
            // Forgive friendly fire, even from explosions.
            EntityAlive myLeader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
            if (myLeader)
                if (myLeader.entityId == _other.entityId)
                    return;
            if (EntityUtilities.IsAnAlly(entityId, _other.entityId))
                return;

            if (_other.IsDead())
                return;
        }

        base.SetRevengeTarget(_other);
        //  Debug.Log("Adding Buff for RevengeTarget() ");
        Buffs.AddBuff("buffNotifyTeamAttack", -1, true);

    }


    public new void SetAttackTarget(EntityAlive _attackTarget, int _attackTargetTime)
    {
        if (_attackTarget != null)
            if (_attackTarget.IsDead())
                return;


        if (_attackTarget == null)
        {
            // Some of the AI tasks resets the attack target when it falls down stunned; this will prevent the NPC from ignoring its stunned opponent.
            if (attackTarget != null && attackTarget.IsAlive())
                return;

        }

        base.SetAttackTarget(_attackTarget, _attackTargetTime);
        // Debug.Log("Adding Buff for Attack Target() ");
        Buffs.AddBuff("buffNotifyTeamAttack", -1, true);
    }
    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
    {
        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable"))
            return;

        if (Buffs.HasBuff("buffInvulnerable"))
            return;

        if (!isEntityRemote)
        {
            // If we are being attacked, let the state machine know it can fight back
            emodel.avatarController.SetBool("IsBusy", false);

            // Turn off the trader ID while it deals damage to the entity
            ToggleTraderID(false);
            base.ProcessDamageResponseLocal(_dmResponse);
            ToggleTraderID(true);
        }
    }

    public override void MarkToUnload()
    {
        // Something asked us to despawn. Check if we are in a trader area. If we are, ignore the request.
        if (traderArea == null)
            traderArea = world.GetTraderAreaAt(new Vector3i(position));

        if (traderArea != null)
        {
            IsDespawned = false;
            return;
        }

        base.MarkToUnload();
    }

    private void updateBlockRadiusEffects()
    {
        Vector3i blockPosition = base.GetBlockPosition();
        int num = World.toChunkXZ(blockPosition.x);
        int num2 = World.toChunkXZ(blockPosition.z);
        startedThisFrame = new List<string>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Chunk chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
                if (chunk != null)
                {
                    DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
                    for (int k = 0; k < tileEntities.list.Count; k++)
                    {
                        TileEntity tileEntity = tileEntities.list[k];
                        if (tileEntity.IsActive(world))
                        {
                            BlockValue block = world.GetBlock(tileEntity.ToWorldPos());
                            Block block2 = Block.list[block.type];
                            if (block2.RadiusEffects != null)
                            {
                                float distanceSq = base.GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
                                for (int l = 0; l < block2.RadiusEffects.Length; l++)
                                {
                                    BlockRadiusEffect blockRadiusEffect = block2.RadiusEffects[l];
                                    if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius && !Buffs.HasBuff(blockRadiusEffect.variable))
                                    {
                                        Buffs.AddBuff(blockRadiusEffect.variable, -1, true, false, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (isEntityRemote && _partialTicks > 1f)
        {
            _dist /= _partialTicks;
        }
        speedForward *= 0.5f;
        speedStrafe *= 0.5f;
        speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            float num = Mathf.Sin(-rotation.y * 3.14159274f / 180f);
            float num2 = Mathf.Cos(-rotation.y * 3.14159274f / 180f);
            speedForward += num2 * _dist.z - num * _dist.x;
            speedStrafe += num2 * _dist.x + num * _dist.z;
        }
        if (Mathf.Abs(_dist.y) > 0.001f)
        {
            speedVertical += _dist.y;
        }
        SetMovementState();
    }
}
