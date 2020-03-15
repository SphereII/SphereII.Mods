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

    public override string ToString()
    {
        return EntityUtilities.DisplayEntityStats(entityId);
    }



    public override float GetEyeHeight()
    {
        if(flEyeHeight == -1f)
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
        if(entityClass.Properties.Values.ContainsKey("Names"))
        {
            string text = entityClass.Properties.Values["Names"];
            string[] Names = text.Split(',');

            int index = UnityEngine.Random.Range(0, Names.Length);
            strMyName = Names[index];
        }

        if(entityClass.Properties.Values.ContainsKey("SleeperInstantAwake"))
        {
            isAlwaysAwake = true;
        }
        if(entityClass.Properties.Values.ContainsKey("Titles"))
        {
            string text = entityClass.Properties.Values["Titles"];
            string[] Names = text.Split(',');
            int index = UnityEngine.Random.Range(0, Names.Length);
            strTitle = Names[index];
        }


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

    public override void SetSleeper()
    {
        // if configured as a sleeper, this should wake them up
        if(isAlwaysAwake)
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
        if(isAlwaysAwake)
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

            if(center != Vector3.zero)
                boundingBox.center = center;

            DisplayLog(" After BoundaryBox: " + boundingBox.ToCultureInvariantString());
        }

    }

    // Reduce the block time threshold to see if the entity can jump or not.
    //public override bool CanEntityJump()
    //{
    //    bool result = base.CanEntityJump();

    //    if (EntityUtilities.GetAttackOrReventTarget(this.entityId) != null)
    //        return false;

    //    if (this.moveHelper.BlockedEntity)
    //        return false;

    //    if (this.moveHelper.BlockedTime < 0.2)
    //        return false;

    //    return result;
    //}

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
        if(IsDead() || NPCInfo == null)
            return new EntityActivationCommand[0];

        return new EntityActivationCommand[]
        {
            new EntityActivationCommand("Greet " + EntityName, "talk" , true)
        };


    }

    public override bool Attack(bool _bAttackReleased)
    {
        if(attackTarget == null)
        {
            EntityUtilities.ChangeHandholdItem(entityId, EntityUtilities.Need.Ranged, 0);
            return false;
        }

        return base.Attack(_bAttackReleased);
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        // Don't allow interaction with a Hated entity
        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(this, _entityFocusing);
        if(myRelationship == FactionManager.Relationship.Hate)
            return false;


        // If they have attack targets, don't interrupt them.
        if(GetAttackTarget() != null || GetRevengeTarget() != null)
            return false;

        // set the IsBusy flag, so it won't wander away when you are talking to it.
        emodel.avatarController.SetBool("IsBusy", true);
        // Look at the entity that is talking to you.
        SetLookPosition(_entityFocusing.getHeadPosition());

        _entityFocusing.Buffs.SetCustomVar("CurrentNPC", entityId, true);
        base.OnEntityActivated(_indexInBlockActivationCommands, _tePos, _entityFocusing);

        SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

        return true;
    }

    public override string EntityName
    {
        get
        {
            if(strMyName == "Bob")
                return entityName;

            if(String.IsNullOrEmpty(strTitle))
                return strMyName + " the " + base.EntityName;
            else
                return strMyName + " the " + strTitle;
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

        Buffs.SetCustomVar("$waterStaminaRegenAmount", 0, false);

        SetupStartingItems();
        inventory.SetHoldingItemIdx(0);
    }

    // We use a tempList to store the patrol coordinates of each vector, but centered over the block. This allows us to check to make sure each
    // vector we are storing is on a new block, and not just  10.2 and 10.4. This helps smooth out the entity's walk. However, we do want accurate patrol points,
    // so we store the accurate patrol positions for the entity.
    List<Vector3> tempList = new List<Vector3>();
    private int waitTicks;

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
            Vector3 temp = ModGeneralUtilities.StringToVector3(strPatrolPoint);
            if(temp != Vector3.zero)
                PatrolCoordinates.Add(temp);
        }

        String strGuardPosition = _br.ReadString();
        GuardPosition = ModGeneralUtilities.StringToVector3(strGuardPosition);
        factionId = _br.ReadByte();
        GuardLookPosition = ModGeneralUtilities.StringToVector3(_br.ReadString());



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

    public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
    {
        // Check the state to see if the controller IsBusy or not. If it's not, then let it walk.
        bool isBusy = false;
        if(emodel != null && emodel.avatarController != null)
            emodel.avatarController.TryGetBool("IsBusy", out isBusy);
        if(isBusy)
            return;

        base.MoveEntityHeaded(_direction, _isDirAbsolute);
    }
    public override void OnUpdateLive()
    {
        //If blocked, check to see if its a door.
        if(moveHelper.IsBlocked)
        {
            Vector3i blockPos = moveHelper.HitInfo.hit.blockPos;
            BlockValue block = world.GetBlock(blockPos);
            if(Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
            {
                bool canOpenDoor = true;
                TileEntitySecureDoor tileEntitySecureDoor = GameManager.Instance.World.GetTileEntity(0, blockPos) as TileEntitySecureDoor;
                if(tileEntitySecureDoor != null)
                {
                    if(tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.GetOwner() == "")
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


        // Check to see if we've opened a door, and close it behind you.
        Vector3i doorPos = SphereCache.GetDoor(entityId);
        if(doorPos != Vector3i.zero)
        {
            DisplayLog("I've opened a door recently. I'll see if I can close it.");
            BlockValue block = world.GetBlock(doorPos);
            if(Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
            {
                float CloseDistance = 3;
                // If it's a multidim, increase tha radius a bit
                if(Block.list[block.type].isMultiBlock)
                {
                    Vector3i vector3i = StringParsers.ParseVector3i(Block.list[block.type].Properties.Values["MultiBlockDim"], 0, -1, false);
                    if(CloseDistance > vector3i.x)
                        CloseDistance = vector3i.x + 1;

                }
                if((GetDistanceSq(doorPos.ToVector3()) > CloseDistance))
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
            SetLookPosition(attackTarget.position);
            RotateTo(attackTarget, 45, 45);
        }
        Buffs.RemoveBuff("buffnewbiecoat", false);
        Stats.Health.MaxModifier = Stats.Health.Max;


        // Non-player entities don't fire all the buffs or stats, so we'll manually fire the water tick,
        Stats.Water.Tick(0.5f, 0, false);

        // then fire the updatestats over time, which is protected from a IsPlayer check in the base onUpdateLive().
        Stats.UpdateStatsOverTime(0.5f);


        updateTime = Time.time - 2f;
        base.OnUpdateLive();

        // No NPC info, don't continue
        if(NPCInfo == null)
            return;

        // If the Tile Entity Trader isn't set, set it now. Sometimes this fails, and won't allow interaction.
        if(TileEntityTrader == null)
        {
            TileEntityTrader = new TileEntityTrader(null);
            TileEntityTrader.entityId = entityId;
            TileEntityTrader.TraderData.TraderID = NPCInfo.TraderID;
        }

        // Check if there's a player within 10 meters of us. If not, resume wandering.
        emodel.avatarController.SetBool("IsBusy", false);

        if(!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        if(target == null)
        {
            if(this is EntityAliveFarmingAnimalSDX)
                return;

            List<global::Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(position, Vector3.one * 5f));
            if(entitiesInBounds.Count > 0)
            {
                for(int i = 0; i < entitiesInBounds.Count; i++)
                {
                    if(entitiesInBounds[i] is EntityPlayerLocal)
                    {

                        // Check your faction relation. If you hate each other, don't stop and talk.
                        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(this, entitiesInBounds[i] as EntityPlayerLocal);
                        if(myRelationship == FactionManager.Relationship.Hate)
                            break;

                        if(GetDistance(entitiesInBounds[i]) < 2)
                        {
                            DisplayLog("The entity is too close to me. Moving away: " + entitiesInBounds[i].ToString());
                            EntityUtilities.BackupHelper(entityId, entitiesInBounds[i].position, 5);
                            //moveHelper.SetMoveTo((entitiesInBounds[i] as EntityPlayerLocal).GetLookVector(), false);
                            break;
                        }


                        // Turn to face the player, and stop the movement.
                        emodel.avatarController.SetBool("IsBusy", true);

                        SetLookPosition(entitiesInBounds[i].getHeadPosition());
                        RotateTo(entitiesInBounds[i], 90f, 90f);
                        navigator.clearPath();
                        moveHelper.Stop();
                        break;
                    }
                }
            }
        }
    }

    public override Ray GetLookRay()
    {
        return new Ray(this.position + new Vector3(0f, this.GetEyeHeight() * this.eyeHeightHackMod, 0f), this.GetLookVector());
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

        if(EntityUtilities.IsAnAlly(entityId, _damageSource.getEntityId()))
            return 0;

        if(EntityUtilities.GetBoolValue(entityId, "Invulnerable"))
            return 0;

        if(Buffs.HasBuff("buffInvulnerable"))
            return 0;

        // If we are being attacked, let the state machine know it can fight back
        emodel.avatarController.SetBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        int Damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        ToggleTraderID(true);
        return Damage;
    }


    public override void SetRevengeTarget(EntityAlive _other)
    {
        if(_other)
        {
            // Forgive friendly fire, even from explosions.
            EntityAlive myLeader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
            if(myLeader)
                if(myLeader.entityId == _other.entityId)
                    return;
            if(EntityUtilities.IsAnAlly(entityId, _other.entityId))
                return;

            if(_other.IsDead())
                return;
        }

        if (_other == null)
        {
            // Reset the hand held back to their preferred item 0
            EntityUtilities.ChangeHandholdItem(this.entityId, EntityUtilities.Need.Ranged, 0);
        }

        base.SetRevengeTarget(_other);
        //  Debug.Log("Adding Buff for RevengeTarget() ");
        Buffs.AddBuff("buffNotifyTeamAttack", -1, true);

    }

    public override void SetAttackTarget(EntityAlive _attackTarget, int _attackTargetTime)
    {
        if(_attackTarget != null)
            if(_attackTarget.IsDead())
                return;


        if (_attackTarget == null)
        {
            // Some of the AI tasks resets the attack target when it falls down stunned; this will prevent the NPC from ignoring its stunned opponent.
            if (attackTarget != null && attackTarget.IsAlive())
                return;

            // Reset the hand held back to their preferred item 0
            EntityUtilities.ChangeHandholdItem(this.entityId, EntityUtilities.Need.Ranged, 0);
        }

        base.SetAttackTarget(_attackTarget, _attackTargetTime);
        // Debug.Log("Adding Buff for Attack Target() ");
        Buffs.AddBuff("buffNotifyTeamAttack", -1, true);
    }
    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
    {
        if(EntityUtilities.GetBoolValue(entityId, "Invulnerable"))
            return;

        if(Buffs.HasBuff("buffInvulnerable"))
            return;

        if(!isEntityRemote)
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
        if(traderArea == null)
            traderArea = world.GetTraderAreaAt(new Vector3i(position));

        if(traderArea != null)
        {
            IsDespawned = false;
            return;
        }


        base.MarkToUnload();
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
