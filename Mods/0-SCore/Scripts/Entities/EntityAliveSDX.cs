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
 *      <property name="Class" value="EntityAliveSDX, SCore" />
 */

using Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UAI;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

// ReSharper disable once CheckNamespace
public class EntityAliveSDX : EntityTrader, IEntityOrderReceiverSDX {
    public List<string> lstQuests = new List<string>();
    public bool isAlwaysAwake;

    [FormerlySerializedAs("PatrolCoordinates")]
    public List<Vector3> patrolCoordinates = new List<Vector3>();

    [FormerlySerializedAs("GuardPosition")]
    public Vector3 guardPosition = Vector3.zero;

    [FormerlySerializedAs("GuardLookPosition")]
    public Vector3 guardLookPosition = Vector3.zero;

    private string rightHandTransformName;

    private bool canCollideWithLeader = true;
    private float enemyDistanceToTalk = 10f;

    /// <inheritdoc/>
    public List<Vector3> PatrolCoordinates => patrolCoordinates;

    /// <inheritdoc/>
    public Vector3 GuardPosition {
        get => guardPosition;
        set => guardPosition = value;
    }

    /// <inheritdoc/>
    public Vector3 GuardLookPosition {
        get => guardLookPosition;
        set => guardLookPosition = value;
    }

    /// <inheritdoc/>
    public Vector3 Position => position;

    public float flEyeHeight = -1f;
    public bool bWentThroughDoor;

    public EntityAlive Owner;
    public bool isTeleporting = false;

    public bool isHirable = true;
    public bool isQuestGiver = true;

    // Read the configuration to see if the hired NPCs should join the player's group.
    public bool AddNPCToCompanion = Configuration.CheckFeatureStatus("AdvancedNPCFeatures", "DisplayCompanions");


    private string particleOnDestroy;
    private BlockValue corpseBlockValue;
    private float corpseBlockChance;

    private string _currentWeapon = "";
    private string _defaultWeapon = "";

    // if the NPC isn't available, don't return a loot. This disables the "Press <E> to search..."
    public override string GetLootList() {
        return IsAvailable() == false ? "" : base.GetLootList();
    }

    // Check to see if the NPC is available
    public bool IsAvailable() {
        if (this.Buffs.HasCustomVar("onMission") && this.Buffs.GetCustomVar("onMission") == 1f)
            return false;
        return true;
    }

    public bool canJump = true;

    private readonly bool _blDisplayLog = false;

    // We use a tempList to store the patrol coordinates of each vector, but centered over the block. This allows us to check to make sure each
    // vector we are storing is on a new block, and not just  10.2 and 10.4. This helps smooth out the entity's walk. However, we do want accurate patrol points,
    // so we store the accurate patrol positions for the entity.
    private readonly List<Vector3> _tempList = new List<Vector3>();

    private int _defaultTraderID;
    private List<string> _startedThisFrame;

    // Default name
    private string _strMyName = "Bob";
    private string _strTitle;

    private TileEntityTrader _tileEntityTrader;
    private TraderArea _traderArea;

    public QuestJournal questJournal = new QuestJournal();

    // This sets the entity's default scale, so when we re-scale it to make it disappear, everything
    // will still run and work, and we can re-set it.
    private Vector3 scale;

    // Toggle gravity on and off depending on if the chunk is visible.
    private ChunkCluster.OnChunkVisibleDelegate chunkClusterVisibleDelegate;

    public string Title {
        get { return Localization.Get(_strTitle); }
    }

    public string FirstName {
        get { return Localization.Get(_strMyName); }
    }

    public override string EntityName {
        get {
            // No configured name? return the default.
            if (_strMyName == "Bob")
                return entityName;

            if (string.IsNullOrEmpty(_strTitle))
                return Localization.Get(_strMyName);
            return Localization.Get(_strMyName) + " the " + Localization.Get(_strTitle);
        }
    }


    public void DisplayLog(string strMessage) {
        if (_blDisplayLog && !IsDead())
            Debug.Log(entityName + ": " + strMessage);
    }

    public override string ToString() {
        return entityName;
        //return EntityUtilities.DisplayEntityStats(entityId);
    }

    public override bool CanEntityJump() {
        return canJump;
    }


    public bool IsOnMission() {
        return this.Buffs.HasCustomVar("onMission") && this.Buffs.GetCustomVar("onMission") == 1f;
    }

    // SendOnMission will make the NPC disappear and be unavailable
    public void SendOnMission(bool send) {
        if (send)
        {
            var enemy = GetRevengeTarget();
            if (enemy != null)
            {
                SetAttackTarget(null, 0);
                enemy.SetAttackTarget(null, 0);
                enemy.SetRevengeTarget(null);
                enemy.DoRagdoll(new DamageResponse());
                SetRevengeTarget(null);
            }

            // Don't let anything target you
            isIgnoredByAI = true;

            // rescale to make it invisible.
            transform.localScale = new Vector3(0, 0, 0);
            emodel.SetVisible(false, false);
            enabled = false;
            Buffs.AddCustomVar("onMission", 1f);

            // Turn off the compass
            if (this.NavObject != null)
                this.NavObject.IsActive = false;
            // Clear the debug information, usually set from UAI
            this.DebugNameInfo = "";

            // Turn off the display component
            SetupDebugNameHUD(false);
        }
        else
        {
            transform.localScale = scale;

            emodel.SetVisible(true, true);
            enabled = true;
            Buffs.RemoveCustomVar("onMission");
            if (this.NavObject != null)
                this.NavObject.IsActive = true;
            isIgnoredByAI = false;
            // SetupDebugNameHUD(true);
        }
    }

    public override float GetEyeHeight() {
        if (this.walkType == 4)
        {
            return 0.15f;
        }

        if (this.walkType == 8)
        {
            return 0.6f;
        }

        if (!this.IsCrouching)
        {
            return base.height * 0.8f;
        }

        return base.height * 0.5f;
        // return flEyeHeight == -1f ? base.GetEyeHeight() : flEyeHeight;
    }

    public override void SetModelLayer(int _layerId, bool _force = false, string[] excludeTags = null) {
        //Utils.SetLayerRecursively(this.emodel.GetModelTransform().gameObject, _layerId);
    }

    // Over-ride for CopyProperties to allow it to read in StartingQuests.
    public override void CopyPropertiesFromEntityClass() {
        base.CopyPropertiesFromEntityClass();
        var _entityClass = EntityClass.list[entityClass];

        if (_entityClass.Properties.Values.ContainsKey("Hirable"))
            isHirable = StringParsers.ParseBool(_entityClass.Properties.Values["Hirable"], 0, -1, true);

        if (_entityClass.Properties.Values.ContainsKey("IsQuestGiver"))
            isQuestGiver = StringParsers.ParseBool(_entityClass.Properties.Values["IsQuestGiver"], 0, -1, true);

        flEyeHeight = EntityUtilities.GetFloatValue(entityId, "EyeHeight");

        // Read in a list of names then pick one at random.
        if (_entityClass.Properties.Values.ContainsKey("Names"))
        {
            var text = _entityClass.Properties.Values["Names"];
            var names = text.Split(',');

            var index = UnityEngine.Random.Range(0, names.Length);
            _strMyName = names[index];
        }

        if (_entityClass.Properties.Values.ContainsKey("CanCollideWithLeader"))
            canCollideWithLeader =
                StringParsers.ParseBool(_entityClass.Properties.Values["CanCollideWithLeader"], 0, -1, true);

        // By default, make the sleepers to always be awake, this solves the issue where entities in a Passive volume does not wake up fully
        // ie, buffs are not firing, but the uai is.
        isAlwaysAwake = false;
        if (_entityClass.Properties.Values.ContainsKey("SleeperInstantAwake"))
            isAlwaysAwake = StringParsers.ParseBool(_entityClass.Properties.Values["SleeperInstantAwake"], 0, -1, true);

        if (_entityClass.Properties.Values.ContainsKey("IsAlwaysAwake"))
            isAlwaysAwake = StringParsers.ParseBool(_entityClass.Properties.Values["IsAlwaysAwake"], 0, -1, true);


        if (_entityClass.Properties.Values.ContainsKey("Titles"))
        {
            var text = _entityClass.Properties.Values["Titles"];
            var names = text.Split(',');
            var index = UnityEngine.Random.Range(0, names.Length);
            _strTitle = names[index];
        }

        var component = gameObject.GetComponent<BoxCollider>();
        if (component)
        {
            DisplayLog(" Box Collider: " + component.size.ToCultureInvariantString());
            DisplayLog(" Current Boundary Box: " + boundingBox.ToCultureInvariantString());
        }

        if (_entityClass.Properties.Classes.ContainsKey("Boundary"))
        {
            DisplayLog(" Found Boundary Settings");
            var strBoundaryBox = "0,0,0";
            var strCenter = "0,0,0";
            var dynamicProperties3 = _entityClass.Properties.Classes["Boundary"];
            foreach (var keyValuePair in dynamicProperties3.Values.Dict)
            {
                DisplayLog("Key: " + keyValuePair.Key);
                switch (keyValuePair.Key)
                {
                    case "BoundaryBox":
                        DisplayLog(" Found a Boundary Box");
                        strBoundaryBox = dynamicProperties3.Values[keyValuePair.Key];
                        continue;
                    case "Center":
                        DisplayLog(" Found a Center");
                        strCenter = dynamicProperties3.Values[keyValuePair.Key];
                        break;
                }
            }

            var box = StringParsers.ParseVector3(strBoundaryBox);
            var center = StringParsers.ParseVector3(strCenter);
            ConfigureBoundaryBox(box, center);
        }

        if (_entityClass.Properties.Values.ContainsKey("BagItems"))
        {
            var items = _entityClass.Properties.Values["BagItems"];
            foreach (var item in items.Split(","))
            {
                var itemName = item;
                var itemCount = 1;
                if (item.Contains("="))
                {
                    itemName = item.Split("=")[0];
                    itemCount = StringParsers.ParseSInt32(item.Split("=")[1]);
                }

                var itemId = ItemClass.GetItem(itemName);
                if (itemId.Equals(ItemValue.None)) continue;
                var itemStack = new ItemStack(itemId, itemCount);
                bag.AddItem(itemStack);
            }
        }
    }

    public override float getNextStepSoundDistance() {
        return !IsRunning ? 0.5f : 0.25f;
    }

    /// <summary>
    ///     Overrides EntityAlive.OnAddedToWorld().
    ///     When entities are spawned into sleeper volumes, which happens in SleeperVolume.Spawn(),
    ///     several of their properties are set so they are spawned in a sleeping state.
    ///     If the NPC should always be awake, those properties can be reset here.
    /// </summary>
    public override void OnAddedToWorld() {
        if (isAlwaysAwake)
        {
            // Set the current order, defaults to "Wander"
            // EntityUtilities.SetCurrentOrder(entityId, EntityUtilities.GetCurrentOrder(entityId));

            // Set in EntityAlive.TriggerSleeperPose() - resetting here
            IsSleeping = false;
        }

        this.chunkClusterVisibleDelegate = new ChunkCluster.OnChunkVisibleDelegate(this.OnChunkDisplayed);

        GameManager.Instance.World.ChunkClusters[0].OnChunkVisibleDelegates += this.chunkClusterVisibleDelegate;
        base.OnAddedToWorld();
        AddToInventory();
    }

    // This is an attempt at turning off gravity for the entity when chunks are no longer visible. The hope is that it will resolve the disappearing NPCs.
    public void OnChunkDisplayed(long _key, bool _bDisplayed) {
        if (this.emodel == null) return;
        var modelTransform = this.emodel.GetModelTransform();
        if (modelTransform == null) return;
        foreach (var rigid in modelTransform.GetComponentsInChildren<Rigidbody>())
        {
            rigid.useGravity = _bDisplayed;
        }
    }

    public void ConfigureBoundaryBox(Vector3 newSize, Vector3 center) {
        var component = gameObject.GetComponent<BoxCollider>();
        if (!component) return;

        DisplayLog(" Box Collider: " + component.size.ToCultureInvariantString());
        DisplayLog(" Current Boundary Box: " + boundingBox.ToCultureInvariantString());
        // Re-adjusting the box collider     
        component.size = newSize;

        scaledExtent = new Vector3(component.size.x / 2f * transform.localScale.x,
            component.size.y / 2f * transform.localScale.y, component.size.z / 2f * transform.localScale.z);
        var vector = new Vector3(component.center.x * transform.localScale.x,
            component.center.y * transform.localScale.y, component.center.z * transform.localScale.z);
        boundingBox = BoundsUtils.BoundsForMinMax(-scaledExtent.x, -scaledExtent.y, -scaledExtent.z, scaledExtent.x,
            scaledExtent.y, scaledExtent.z);

        boundingBox.center = boundingBox.center + vector;

        if (center != Vector3.zero)
            boundingBox.center = center;

        DisplayLog(" After BoundaryBox: " + boundingBox.ToCultureInvariantString());
    }

    public void RestoreSpeed() {
        // Reset the movement speed when an attack target is set
        moveSpeed = EntityUtilities.GetFloatValue(entityId, "MoveSpeed");

        Vector2 vector;
        vector.x = moveSpeed;
        vector.y = moveSpeed;
        var _entityClass = EntityClass.list[entityClass];
        _entityClass.Properties.ParseVec(EntityClass.PropMoveSpeedAggro, ref vector);
        moveSpeedAggro = vector.x;
        moveSpeedAggroMax = vector.y;
    }

    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing) {
        // Don't allow you to interact with it when its dead.
        if (IsDead() || NPCInfo == null)
        {
            return new[] {
                new EntityActivationCommand("Search", "search", true)
            };
        }

        // Do they even like us enough to talk?
        if (EntityTargetingUtilities.IsEnemy(this, _entityFocusing)) return new EntityActivationCommand[0];

        // If not a human, don't talk to them
        if (!EntityUtilities.IsHuman(entityId)) return new EntityActivationCommand[0];

        // do we have an attack or revenge target? don't have time to talk, bro
        var target = EntityUtilities.GetAttackOrRevengeTarget(entityId);
        if (target != null && EntityTargetingUtilities.CanDamage(this, target)) return new EntityActivationCommand[0];


        return new[] {
            new EntityActivationCommand("Greet " + EntityName, "talk", true)
        };
    }

    public override bool OnEntityActivated(int indexInBlockActivationCommands, Vector3i tePos,
        EntityAlive entityFocusing) {
        var localPlayer = entityFocusing as EntityPlayerLocal;

        if (IsDead())
        {
            GameManager.Instance.TELockServer(0, tePos, this.entityId, entityFocusing.entityId, null);
            return true;
        }

        // Don't allow interaction with a Hated entity
        if (EntityTargetingUtilities.IsEnemy(this, entityFocusing))
        {
            if (localPlayer)
                GameManager.ShowTooltip(localPlayer, Localization.Get("entityaliveSDXEnemy"));
            return false;
        }

        // do we have an attack or revenge target? don't have time to talk, bro
//        var target = EntityUtilities.GetAttackOrRevengeTarget(entityId);
//        if (target != null && EntityTargetingUtilities.CanDamage(this, target)) return false;
        if (enemyDistanceToTalk > 0)
        {
            if (SCoreUtils.IsEnemyNearby(this, enemyDistanceToTalk))
            {
                if (localPlayer)
                    GameManager.ShowTooltip(localPlayer, Localization.Get("entityaliveSDXEnemyNearby"));

                return false;
            }
        }

        Buffs.SetCustomVar("Persist", 1);

        // Look at the entity that is talking to you.
        SetLookPosition(entityFocusing.getHeadPosition());

        // This is used by various dialog boxes to know which EntityID the player is talking too.
        entityFocusing.Buffs.SetCustomVar("CurrentNPC", entityId);
        Buffs.SetCustomVar("CurrentPlayer", entityFocusing.entityId);

        // Copied from EntityTrader
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityFocusing as EntityPlayerLocal);
        uiforPlayer.xui.Dialog.Respondent = this;

        // We don't want the quest system to consider this NPC as interacted with
        if (Buffs.HasCustomVar("NPCInteractedFlag") && Buffs.GetCustomVar("NPCInteractedFlag") == 1)
        {
            return base.OnEntityActivated(indexInBlockActivationCommands, tePos, entityFocusing);
        }

        //if (!isQuestGiver)
        //{
        //    return base.OnEntityActivated(_indexInBlockActivationCommands, _tePos, _entityFocusing);
        //}
        Quest nextCompletedQuest =
            (entityFocusing as EntityPlayerLocal).QuestJournal.GetNextCompletedQuest(null, this.entityId);
        // If the quest giver is not defined, don't let them close out the quest. We only want them to close out their own.

        if (nextCompletedQuest != null && nextCompletedQuest.QuestGiverID != entityId)
            nextCompletedQuest = null;


        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"
                || GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting"
                || GamePrefs.GetString(EnumGamePrefs.GameMode) == "GameModeEditWorld")
            {
                Debug.Log("Skipping activating Quests in Playtesting.");
            }
            else
            {
                this.activeQuests = QuestEventManager.Current.GetQuestList(GameManager.Instance.World, this.entityId,
                    entityFocusing.entityId);
                if (this.activeQuests == null)
                {
                    this.activeQuests = this.PopulateActiveQuests(entityFocusing as EntityPlayer, -1);
                    QuestEventManager.Current.SetupQuestList(this.entityId, entityFocusing.entityId, this.activeQuests);
                }
            }
        }

        if (indexInBlockActivationCommands != 0)
        {
            if (indexInBlockActivationCommands == 1)
            {
                uiforPlayer.xui.Trader.TraderEntity = this;
                if (nextCompletedQuest == null)
                {
                    GameManager.Instance.TELockServer(0, tePos, this.TileEntityTrader.entityId, entityFocusing.entityId,
                        null);
                }
                else
                {
                    if (nextCompletedQuest.QuestGiverID != -1)
                    {
                        QuestEventManager.Current.NPCInteracted(this);
                        uiforPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
                        uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
                        uiforPlayer.xui.Trader.TraderEntity.PlayVoiceSetEntry("questcomplete", localPlayer);
                        uiforPlayer.windowManager.Open("questTurnIn", true, false, true);
                    }
                }
            }
        }
        else
        {
            uiforPlayer.xui.Dialog.Respondent = this;
            if (nextCompletedQuest == null)
            {
                uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
                uiforPlayer.windowManager.Open("dialog", true, false, true);
                return false;
            }

            if (nextCompletedQuest != null && nextCompletedQuest.QuestGiverID != -1)
            {
                QuestEventManager.Current.NPCInteracted(this);
                uiforPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
                uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
                uiforPlayer.xui.Dialog.Respondent.PlayVoiceSetEntry("questcomplete", localPlayer);
                uiforPlayer.windowManager.Open("questTurnIn", true, false, true);
            }
        }


        return true;
    }


    public override bool CanBePushed() {
        return true;
    }

    public override void InitLocation(Vector3 _pos, Vector3 _rot) {
        base.InitLocation(_pos, _rot);
        // Trader class turns that off.
        PhysicsTransform.gameObject.SetActive(true);
    }

    public override void PostInit() {
        base.PostInit();

        // disable god mode, since that's enabled by default in the NPC
        IsGodMode.Value = false;

        if (NPCInfo != null)
            _defaultTraderID = NPCInfo.TraderID;

        // Check if there's a loot container or not already attached to store its stuff.
        DisplayLog(" Checking Entity's Loot Container");
        if (lootContainer == null)
        {
            DisplayLog(" Entity does not have a loot container. Creating one.");
            Chunk chunk = null;
            lootContainer = new TileEntityLootContainer(chunk) { entityId = entityId };
            lootContainer.SetContainerSize(string.IsNullOrEmpty(GetLootList())
                ? new Vector2i(8, 6)
                : LootContainer.GetLootContainer(GetLootList()).size);
        }

        Buffs.SetCustomVar("$waterStaminaRegenAmount", 0, false);
        SetupStartingItems();
        if (!string.IsNullOrEmpty(_currentWeapon))
            UpdateWeapon(_currentWeapon);

        // Does a quick local scan to see what pathing blocks, if any, are nearby. If one is found nearby, then it'll use that code for pathing.
        SetupAutoPathingBlocks();

        scale = transform.localScale;


        // EntityTraders turn off their physics transforms, but we want it on,
        // Otherwise NPCs won't collider with each other.
        this.PhysicsTransform.gameObject.SetActive(true);
        SetSpawnerSource(EnumSpawnerSource.Biome);
        enemyDistanceToTalk =
            StringParsers.ParseFloat(Configuration.GetPropertyValue("AdvancedNPCFeatures", "EnemyDistanceToTalk"));
    }

    /// <inheritdoc/>
    public virtual void UpdatePatrolPoints(Vector3 position) {
        // Center the x and z values of the passed in blocks for a unique check.
        var temp = position;
        temp.x = 0.5f + Utils.Fastfloor(position.x);
        temp.z = 0.5f + Utils.Fastfloor(position.z);
        temp.y = Utils.Fastfloor(position.y);

        if (_tempList.Contains(temp)) return;
        _tempList.Add(temp);
        if (!patrolCoordinates.Contains(position))
            patrolCoordinates.Add(position);
    }

    // Reads the buff and quest information
    public override void Read(byte _version, BinaryReader _br) {
        base.Read(_version, _br);
        _strMyName = _br.ReadString();
        questJournal = new QuestJournal();
        questJournal.Read(_br as PooledBinaryReader);
        patrolCoordinates.Clear();
        var strPatrol = _br.ReadString();
        foreach (var strPatrolPoint in strPatrol.Split(';'))
        {
            var temp = ModGeneralUtilities.StringToVector3(strPatrolPoint);
            if (temp != Vector3.zero)
                patrolCoordinates.Add(temp);
        }

        var strGuardPosition = _br.ReadString();
        guardPosition = ModGeneralUtilities.StringToVector3(strGuardPosition);
        factionId = _br.ReadByte();
        guardLookPosition = ModGeneralUtilities.StringToVector3(_br.ReadString());
        try
        {
            Buffs.Read(_br);

            // Disabled due to Potential Performance issues
            if (Progression != null)
                Progression.Read(_br, this);
        }
        catch (Exception)
        {
            //  fail safe to protect game saves
        }

        _currentWeapon = _br.ReadString();
        UpdateWeapon(_currentWeapon);

        ReadSyncData(_br, 1, -1);
    }

    public ushort GetSyncFlagsReplicated(ushort syncFlags) {
        return syncFlags;
    }

    public void SendSyncData(ushort syncFlags = 1) {
        var primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
        this.SendSyncData(syncFlags, primaryPlayerId);
    }

    private void SendSyncData(ushort syncFlags, int playerId) {
        var package = NetPackageManager.GetPackage<NetPackageEntityAliveSDXDataSync>().Setup(this, playerId, syncFlags);
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
            return;
        }

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
    }

    public void ReadSyncData(BinaryReader _br, ushort syncFlags, int senderId) {
        // Preserve Inventory
        if (lootContainer == null) return;
        var num2 = (int)_br.ReadByte();
        var array = new ItemStack[num2];
        var containerSize = lootContainer.GetContainerSize();
        var totalItems = containerSize.x * containerSize.y;
        for (var j = 0; j < num2; j++)
        {
            var itemStack = new ItemStack();
            array[j] = itemStack.Read(_br);
            if (j > totalItems)
            {
                Log.Out("Loot container is out of range.");
                break;
            }

            lootContainer.UpdateSlot(j, array[j]);
        }

        lootContainer.bPlayerStorage = true;
    }

    /// <inheritdoc/>
    public void SetupAutoPathingBlocks() {
        // If we already have a pathing code, don't re-scan.
        if (Buffs.HasCustomVar("PathingCode") &&
            (Buffs.GetCustomVar("PathingCode") < 0 || Buffs.GetCustomVar("PathingCode") > 0))
            return;

        // Check if pathing blocks are defined.
        var blocks = EntityUtilities.ConfigureEntityClass(entityId, "PathingBlocks");
        if (blocks.Count == 0)
            blocks = new List<string> { "PathingCube", "PathingCube2" };

        //Scan for the blocks in the area
        var pathingVectors = ModGeneralUtilities.ScanAutoConfigurationBlocks(position, blocks, 2);
        if (pathingVectors == null || pathingVectors.Count == 0) return;

        // Find the nearest block, and if its a sign, read its code.
        var target = ModGeneralUtilities.FindNearestBlock(position, pathingVectors);
        var tileEntitySign = GameManager.Instance.World.GetTileEntity(0, new Vector3i(target)) as TileEntitySign;
        if (tileEntitySign == null) return;

        // Since signs can have multiple codes, splite with a ,, parse each one.
        var text = tileEntitySign.signText.Text;

        // We need to apply the buffs during this scan, as the creation of the entity + adding buffs is not really MP safe.
        var Task = PathingCubeParser.GetValue(text, "task");
        if (!string.IsNullOrEmpty(Task))
        {
            Log.Out($"SetupAutoPathingBlocks for: {entityName} ( {entityId} ) : Task: {Task}");
            // We need to apply the buffs during this scan, as the creation of the entity + adding buffs is not really MP safe.
            if (Task.ToLower() == "stay")
                Buffs.AddBuff("buffOrderStay", -1, false);
            else if (Task.ToLower() == "wander")
                Buffs.AddBuff("buffOrderWander", -1, false);
            else if (Task.ToLower() == "guard")
                // Use the buff that issues the "guard" order, not the one that issues the "stay" order
                Buffs.AddBuff("buffOrderGuard", -1, false);
            else if (Task.ToLower() == "follow")
                Buffs.AddBuff("buffOrderFollow", -1, false);
            else
                Log.Out($"    Entity: {entityName} ( {entityId} ) : Cannot perform task: {Task}");
        }

        var Buff = PathingCubeParser.GetValue(text, "buff");
        foreach (var buff in Buff.Split(','))
            Buffs.AddBuff(buff, -1, false);


        // Set up the pathing code.
        Buffs.SetCustomVar("PathingCode", -1f);

        var PathingCode = PathingCubeParser.GetValue(text, "pc");
        if (StringParsers.TryParseFloat(PathingCode, out var pathingCode))
            Buffs.SetCustomVar("PathingCode", pathingCode);

        //foreach (var temp in text.Split(','))
        //{
        //    if (!StringParsers.TryParseFloat(temp, out var code)) continue;

        //    Buffs.AddCustomVar("PathingCode", code);
        //    return;
        //}
    }

    // Saves the buff and quest information
    public override void Write(BinaryWriter _bw, bool bNetworkWrite) {
        base.Write(_bw, bNetworkWrite);
        _bw.Write(_strMyName);
        questJournal.Write(_bw as PooledBinaryWriter);
        var strPatrolCoordinates = "";
        foreach (var temp in patrolCoordinates) strPatrolCoordinates += ";" + temp;

        _bw.Write(strPatrolCoordinates);
        _bw.Write(guardPosition.ToString());
        _bw.Write(factionId);
        _bw.Write(guardLookPosition.ToString());
        try
        {
            Buffs.Write(_bw);
            // Disabled due to Potential Performance issues
            Progression?.Write(_bw);
        }
        catch (Exception)
        {
            // fail safe to protect game saves
        }

        _bw.Write(inventory.holdingItem.GetItemName());

        WriteSyncData(_bw, 1);
    }

    public void WriteSyncData(BinaryWriter _bw, ushort syncFlags) {
        // Inventory
        //var slots = this.bag.GetSlots();
        if (lootContainer == null) return;
        var slots = this.lootContainer.items;
        _bw.Write((byte)slots.Length);
        for (int k = 0; k < slots.Length; k++)
        {
            slots[k].Write(_bw);
        }

        _bw.Write(_currentWeapon);
    }

    public void GiveQuest(string strQuest) {
        // Don't give duplicate quests.
        foreach (var quest in questJournal.quests)
            if (quest.ID == strQuest.ToLower())
                return;

        // Make sure the quest is valid
        var newQuest = QuestClass.CreateQuest(strQuest);
        if (newQuest == null)
            return;

        // If there's no shared owner, it tries to read the PlayerLocal's entity ID. This entity doesn't have that.
        newQuest.SharedOwnerID = entityId;
        newQuest.QuestGiverID = -1;
        questJournal.AddQuest(newQuest);
    }

    public override void UpdateJump() {
        if (this.walkType == 4 && !this.isSwimming)
        {
            base.FaceJumpTo();
            this.jumpState = EntityAlive.JumpState.Climb;
            if (!this.emodel.avatarController || !this.emodel.avatarController.IsAnimationJumpRunning())
            {
                this.Jumping = false;
            }

            if (this.jumpTicks == 0 && this.accumulatedRootMotion.y > 0.005f)
            {
                this.jumpTicks = 30;
            }

            return;
        }

        base.UpdateJump();
        if (this.isSwimming)
        {
            return;
        }

        this.accumulatedRootMotion.y = 0f;
    }

    public override bool CanCollideWith(Entity _other) {
        if (canCollideWithLeader) return true;

        var result = base.CanCollideWith(_other);
        var leader = EntityUtilities.GetLeaderOrOwner(entityId);
        if (leader && leader.entityId == _other.entityId)
        {
            return false;
        }

        return result;
    }

    public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute) {
        // Check the state to see if the controller IsBusy or not. If it's not, then let it walk.
        var isBusy = false;
        if (emodel != null && emodel.avatarController != null)
            emodel.avatarController.TryGetBool("IsBusy", out isBusy);
        if (isBusy)
            return;

        if (this.walkType == 4 && this.Jumping)
        {
            this.motion = this.accumulatedRootMotion;
            this.accumulatedRootMotion = Vector3.zero;
            this.IsRotateToGroundFlat = true;
            if (this.moveHelper != null)
            {
                Vector3 vector = this.moveHelper.JumpToPos - this.position;
                if (Utils.FastAbs(vector.y) < 0.2f)
                {
                    this.motion.y = vector.y * 0.2f;
                }

                if (Utils.FastAbs(vector.x) < 0.3f)
                {
                    this.motion.x = vector.x * 0.2f;
                }

                if (Utils.FastAbs(vector.z) < 0.3f)
                {
                    this.motion.z = vector.z * 0.2f;
                }

                if (vector.sqrMagnitude < 0.010000001f)
                {
                    if (this.emodel && this.emodel.avatarController)
                    {
                        this.emodel.avatarController.StartAnimationJump(AnimJumpMode.Land);
                    }

                    this.Jumping = false;
                }
            }

            this.entityCollision(this.motion);
            return;
        }

        // Base Class for EntityAlive, since EntityTrader does not call the base.MoveEntityHeading, causing the NPCs to stand still.
        if (this.AttachedToEntity != null)
        {
            return;
        }

        if (this.jumpIsMoving)
        {
            this.JumpMove();
            return;
        }

        if (this.RootMotion)
        {
            if (this.isEntityRemote && this.bodyDamage.CurrentStun == EnumEntityStunType.None && !this.IsDead() &&
                (!(this.emodel != null) || !(this.emodel.avatarController != null) ||
                 !this.emodel.avatarController.IsAnimationHitRunning()))
            {
                this.accumulatedRootMotion = Vector3.zero;
                return;
            }

            bool flag = this.emodel && this.emodel.IsRagdollActive;
            if (this.isSwimming && !flag)
            {
                this.motion += this.accumulatedRootMotion * 0.001f;
            }
            else if (this.onGround || this.jumpTicks > 0)
            {
                if (flag)
                {
                    this.motion.x = 0f;
                    this.motion.z = 0f;
                }
                else
                {
                    float y = this.motion.y;
                    this.motion = this.accumulatedRootMotion;
                    this.motion.y = this.motion.y + y;
                }
            }

            this.accumulatedRootMotion = Vector3.zero;
        }

        if (this.IsFlyMode.Value)
        {
            EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            float num = ((primaryPlayer != null) ? primaryPlayer.GodModeSpeedModifier : 1f);
            float num2 = 2f * (this.MovementRunning ? 0.35f : 0.12f) * num;
            if (!this.RootMotion)
            {
                this.Move(_direction, _isDirAbsolute, this.GetPassiveEffectSpeedModifier() * num2,
                    this.GetPassiveEffectSpeedModifier() * num2);
            }

            if (!this.IsNoCollisionMode.Value)
            {
                this.entityCollision(this.motion);
                this.motion *= base.ConditionalScalePhysicsMulConstant(0.546f);
            }
            else
            {
                this.SetPosition(this.position + this.motion, true);
                this.motion = Vector3.zero;
            }
        }
        else
        {
            this.DefaultMoveEntity(_direction, _isDirAbsolute);
        }

        if (!this.isEntityRemote && this.RootMotion)
        {
            float num3 = this.landMovementFactor;
            num3 *= 2.5f;
            if (this.inWaterPercent > 0.3f)
            {
                if (num3 > 0.01f)
                {
                    float num4 = (this.inWaterPercent - 0.3f) * 1.4285715f;
                    num3 = Mathf.Lerp(num3, 0.01f + (num3 - 0.01f) * 0.1f, num4);
                }

                if (this.isSwimming)
                {
                    num3 = this.landMovementFactor * 5f;
                }
            }

            float num5 = _direction.magnitude;
            num5 = Mathf.Max(num5, 1f);
            float num6 = num3 / num5;
            if (this.lerpForwardSpeed)
            {
                this.speedForwardTarget = _direction.z * num6;
            }
            else
            {
                this.speedForward = _direction.z * num6;
            }

            this.speedStrafe = _direction.x * num6;
            this.SetMovementState();
            base.ReplicateSpeeds();
        }
        //  base.MoveEntityHeaded(_direction, _isDirAbsolute);
    }

    public override void HandleNavObject() {
        if (EntityClass.list[this.entityClass].NavObject != "")
        {
            if (this.LocalPlayerIsOwner() && this.Owner != null)
            {
                if (EntityClass.list[this.entityClass].NavObject != "")
                    this.NavObject =
                        NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, this,
                            "");
                else
                    this.NavObject = NavObjectManager.Instance.RegisterNavObject("ally", this, "");

                this.NavObject.UseOverrideColor = true;
                this.NavObject.OverrideColor = Color.cyan;
                this.NavObject.name = EntityName;

                return;
            }

            if (this.NavObject != null)
            {
                NavObjectManager.Instance.UnRegisterNavObject(this.NavObject);
                this.NavObject = null;
            }
        }
    }

    public bool LocalPlayerIsOwner() {
        var leader = EntityUtilities.GetLeaderOrOwner(entityId);
        if (leader != null)
        {
            if (GameManager.Instance.World.IsLocalPlayer(leader.entityId))
                return true;
        }

        return false;
    }


    public override bool IsSavedToFile() {
        // Has a leader cvar set, good enough, as the leader may already be disconnected, so we'll fail a GetLeaderOrOwner()
        if (Buffs.HasCustomVar("Leader")) return true;

        // If they have a cvar persist, keep them around.
        if (Buffs.HasCustomVar("Persist")) return true;

        // If its dynamic spawn, don't let them stay.
        if (GetSpawnerSource() == EnumSpawnerSource.Dynamic) return false;

        // If its biome spawn, don't let them stay.
        if (GetSpawnerSource() == EnumSpawnerSource.Biome) return false;
        return true;
    }


    int expireLeaderCache = 30;

    public void LeaderUpdate() {
        if (IsDead()) return;

        if (Buffs.HasBuff("buffOrderDismiss")) return;
        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
        if (leader == null)
        {
            Owner = null;
            IsEntityUpdatedInUnloadedChunk = false;
            bWillRespawn = false;
            return;
        }


        if (Owner == null)
        {
            Owner = leader;
            //  Owner.AddOwnedEntity(this);
            if (GameManager.Instance.World.IsLocalPlayer(leader.entityId))
            {
                this.HandleNavObject();
            }
        }


        // Recheck the cache to make sure the owner is updated.
        expireLeaderCache--;
        if (expireLeaderCache < 0)
        {
            expireLeaderCache = 30;
            if (SphereCache.LeaderCache.ContainsKey(entityId))
                SphereCache.LeaderCache.Remove(entityId);
        }

        // Force the leader to have the hired entity id
        leader.Buffs.SetCustomVar($"hired_{entityId}", (float)entityId);

        var player = leader as EntityPlayer;
        // If the player doesn't have a party, create one, so we can share exp with our leader.
        if (player && !player.IsInParty())
        {
            //  player.CreateParty();
        }

        switch (EntityUtilities.GetCurrentOrder(entityId))
        {
            case EntityUtilities.Orders.Loot:
            case EntityUtilities.Orders.Follow:
                // if our leader is attached, that means they are attached to a vehicle
                if (leader.AttachedToEntity != null)
                {
                    //   if (!Buffs.HasCustomVar("onMission"))
                    SendOnMission(true);
                    var distanceToLeader2 = GetDistance(leader);
                    if (distanceToLeader2 > 10)
                    {
                        var _position = leader.GetPosition();
                        _position.y += 2;
                        SetPosition(_position);
                    }
                }
                else
                {
                    // No longer attached to the vehicle, but still has the cvar? return the NPC to the player.
                    if (Buffs.HasCustomVar("onMission"))
                        SendOnMission(false);
                }

                // This needs to be set for the entities to be still alive, so the player can teleport them
                IsEntityUpdatedInUnloadedChunk = true;
                bWillRespawn =
                    true; // this needs to be off for entities to despawn after being killed. Handled via SetDead()

                var distanceToLeader = GetDistance(leader);
                if (distanceToLeader > 60)
                    TeleportToPlayer(leader);

                if (player && AddNPCToCompanion && IsAlive())
                {
                    if (player.Companions.IndexOf(this) < 0)
                    {
                        player.Companions.Add(this);
                        int num2 = player.Companions.IndexOf(this);
                        var v = Constants.TrackedFriendColors[num2 % Constants.TrackedFriendColors.Length];
                        if (this.NavObject != null)
                        {
                            this.NavObject.UseOverrideColor = true;
                            this.NavObject.OverrideColor = v;
                            //this.NavObject.DisplayName = EntityName;
                        }
                    }
                }

                break;
            case EntityUtilities.Orders.Patrol:
            case EntityUtilities.Orders.Stay:
            case EntityUtilities.Orders.Wander:
            default:
                // This needs to be set for the entities to be still alive, so the player can teleport them
                IsEntityUpdatedInUnloadedChunk = false;
                bWillRespawn =
                    false; // this needs to be off for entities to despawn after being killed. Handled via SetDead()
                if (player)
                    player.Companions.Remove(this);

                break;
        }
    }

    public override void OnUpdateLive() {
        //CheckNoise();
        if (isHirable)
            LeaderUpdate();

        CheckStuck();

        // SetupAutoPathingBlocks();

        // Wake them up if they are sleeping, since the trigger sleeper makes them go idle again.
        if (!sleepingOrWakingUp && isAlwaysAwake)
        {
            IsSleeping = true;
            ConditionalTriggerSleeperWakeUp();
        }

        Buffs.RemoveBuff("buffnewbiecoat", false);
        Stats.Health.MaxModifier = Stats.Health.Max;

        // Set CanFall and IsOnGround
        if (emodel != null && emodel.avatarController != null)
        {
            emodel.avatarController.UpdateBool("CanFall",
                !emodel.IsRagdollActive && bodyDamage.CurrentStun == EnumEntityStunType.None && !isSwimming);
            emodel.avatarController.UpdateBool("IsOnGround", onGround || isSwimming);
        }

        // To catch a null ref.
        try
        {
            base.OnUpdateLive();
        }
        catch (Exception ex)
        {
            // ignored
        }

        if (IsAlert)
        {
            var target = EntityUtilities.GetAttackOrRevengeTarget(entityId);
            if (target != null && target.IsDead())
            {
                bReplicatedAlertFlag = false;
            }
        }

        // Allow EntityAliveSDX to get buffs from blocks
        if (!isEntityRemote && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            UpdateBlockRadiusEffects();

        // No NPC info, don't continue
        if (NPCInfo == null)
            return;

        // if (isQuestGiver)
        {
            // If the Tile Entity Trader isn't set, set it now. Sometimes this fails, and won't allow interaction.
            if (_tileEntityTrader == null)
            {
                Chunk chunk = null;
                _tileEntityTrader = new TileEntityTrader(chunk);
                _tileEntityTrader.entityId = entityId;
                _tileEntityTrader.TraderData.TraderID = NPCInfo.TraderID;
            }
        }
        if (this.isEntityRemote) return;
        if (!this.emodel) return;

        var avatarController = this.emodel.avatarController;
        if (!avatarController) return;

        var flag = this.onGround || this.isSwimming || this.bInElevator;
        if (flag)
        {
            this.fallTime = 0f;
            this.fallThresholdTime = 0f;
            if (this.bInElevator)
            {
                this.fallThresholdTime = 0.6f;
            }
        }
        else
        {
            if (this.fallThresholdTime == 0f)
            {
                this.fallThresholdTime = 0.1f + this.rand.RandomFloat * 0.3f;
            }

            this.fallTime += 0.05f;
        }

        var canFall = !this.emodel.IsRagdollActive && this.bodyDamage.CurrentStun == EnumEntityStunType.None &&
                      !this.isSwimming && !this.bInElevator && this.jumpState == EntityAlive.JumpState.Off &&
                      !this.IsDead();
        if (this.fallTime <= this.fallThresholdTime)
        {
            canFall = false;
        }

        avatarController.SetFallAndGround(canFall, flag);
    }

    private float fallTime;


    private float fallThresholdTime;

    public override Ray GetLookRay() {
        return new Ray(position + new Vector3(0f, GetEyeHeight() * eyeHeightHackMod, 0f), GetLookVector());
    }

    public void ToggleTraderID(bool Restore) {
        if (NPCInfo == null)
            return;

        // Check if we are restoring the default trader ID.
        if (Restore)
            NPCInfo.TraderID = _defaultTraderID;
        else
            NPCInfo.TraderID = 0;
    }

    public override void ProcessDamageResponse(DamageResponse _dmResponse) {
        if (IsOnMission()) return;
        base.ProcessDamageResponse(_dmResponse);
    }

    public override bool IsImmuneToLegDamage {
        get {
            if (IsOnMission()) return true;
            return base.IsImmuneToLegDamage;
        }
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit,
        float _impulseScale) {
        if (IsOnMission()) return 0;

        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable"))
            return 0;

        if (Buffs.HasBuff("buffInvulnerable"))
            return 0;


        // If we are being attacked, let the state machine know it can fight back
        if (!EntityTargetingUtilities.CanTakeDamage(this, world.GetEntity(_damageSource.getEntityId())))
            return 0;

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        var damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        ToggleTraderID(true);
        return damage;
    }


    public new void SetRevengeTarget(EntityAlive _other) {
        if (IsOnMission())
            return;

        if (_other)
        {
            // Forgive friendly fire, even from explosions.
            var myLeader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
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
        Buffs.AddBuff("buffNotifyTeamAttack");
    }

    public override void OnEntityUnload() {
        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityPlayer;
        if (leader)
        {
            leader.Companions.Remove(this);
        }

        base.OnEntityUnload();
    }

    public override void SetDead() {
        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityPlayerLocal;
        if (leader)
        {
            leader.Buffs.RemoveCustomVar($"hired_{entityId}");
            EntityUtilities.SetLeaderAndOwner(entityId, 0);
            GameManager.ShowTooltip(leader, $"Oh no! {EntityName} has died. :(");

            if (lootContainer != null)
            {
                var isBackpackEmpty = lootContainer.IsEmpty();
                if (!isBackpackEmpty)
                {
                    var bagPosition = new Vector3i(this.position + base.transform.up);

                    var className = "BackpackNPC";
                    var entityClass = EntityClass.GetEntityClass(className.GetHashCode());
                    if (entityClass == null)
                        className = "Backpack";

                    var entityBackpack = EntityFactory.CreateEntity(className.GetHashCode(), bagPosition) as EntityItem;

                    var entityCreationData = new EntityCreationData(entityBackpack) {
                        entityName = Localization.Get(this.EntityName),
                        id = -1,
                        lootContainer = lootContainer
                    };

                    GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
                    if (entityBackpack)
                        entityBackpack.OnEntityUnload();
                    //  this.SetDroppedBackpackPosition(new Vector3i(bagPosition));
                }
            }
        }

        var player = leader as EntityPlayer;
        if (leader)
        {
            player.Companions.Remove(this);
            player.Buffs.RemoveCustomVar($"hired_{entityId}");
        }

        bWillRespawn = false;
        if (this.NavObject != null)
        {
            NavObjectManager.Instance.UnRegisterNavObject(this.NavObject);
            this.NavObject = null;
        }

        SetupDebugNameHUD(false);

        this.lootContainer = null;
        this.lootListAlive = null;
        this.lootListOnDeath = null;
        this.isCollided = false;
        this.nativeCollider = null;
        this.physicsCapsuleCollider = null;

        var component = this.gameObject.GetComponentsInChildren<Collider>();

        foreach (var t in component)
        {
            if (t.name.ToLower() == "gameobject")
            {
                t.enabled = false;
            }
        }

        base.SetDead();
    }


    public new void SetAttackTarget(EntityAlive _attackTarget, int _attackTargetTime) {
        if (_attackTarget != null)
        {
            if (_attackTarget.IsDead())
            {
                return;
            }
        }

        if (IsOnMission())
            return;

        if (_attackTarget == null)
        {
            // Some of the AI tasks resets the attack target when it falls down stunned; this will prevent the NPC from ignoring its stunned opponent.
            if (attackTarget != null && attackTarget.IsAlive())
                return;
        }

        base.SetAttackTarget(_attackTarget, _attackTargetTime);
        // Debug.Log("Adding Buff for Attack Target() ");
        Buffs.AddBuff("buffNotifyTeamAttack");
    }

    public override void OnUpdatePosition(float _partialTicks) {
        if (!isHirable)
        {
            base.OnUpdatePosition(_partialTicks);
            return;
        }

        if (this.position.y <= 0)
        {
            var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
            if (leader)
            {
                TeleportToPlayer(leader);
                return;
            }
        }

        base.OnUpdatePosition(_partialTicks);
    }

    public override bool CanDamageEntity(int _sourceEntityId) {
        var canDamage = EntityTargetingUtilities.CanTakeDamage(this, world.GetEntity(_sourceEntityId));
        return canDamage;
    }

    public override bool IsAttackValid() {
        // If they are on a mission, don't attack. 
        if (IsOnMission()) return false;

        return base.IsAttackValid();
    }

    public void TeleportToPlayer(EntityAlive target, bool randomPosition = false) {
        if (target == null) return;

        if (EntityUtilities.GetCurrentOrder(entityId) == EntityUtilities.Orders.Stay) return;
        if (EntityUtilities.GetCurrentOrder(entityId) == EntityUtilities.Orders.Guard) return;


        var target2i = new Vector2(target.position.x, target.position.z);
        var mine2i = new Vector2(position.x, position.z);
        var distance = Vector2.Distance(target2i, mine2i);
        //var distance = GetDistance(target);
        if (distance < 20) return;

        if (isTeleporting) return;

        var myPosition = target.position + Vector3.back;
        var player = target as EntityPlayer;
        if (player != null)
        {
            myPosition = player.GetBreadcrumbPos(3 * rand.RandomFloat);

            // If my target distance is still way off from the player, teleport randomly. That means the bread crumb isn't accurate
            var distance2 = Vector3.Distance(myPosition, player.position);
            if (distance2 > 40f)
                randomPosition = true;

            if (randomPosition)
            {
                Vector3 dirV = target.position - this.position;
                myPosition = RandomPositionGenerator.CalcPositionInDirection(target, target.position, dirV, 5, 80f);
            }

            //// Find the ground.
            myPosition.y = (int)GameManager.Instance.World.GetHeightAt(myPosition.x, myPosition.z) + 1;
        }

        motion = Vector3.zero;
        navigator?.clearPath();
        SphereCache.RemovePaths(entityId);

        this.SetPosition(myPosition, true);
        StartCoroutine(validateTeleport(target, randomPosition));
    }

    private float getAltitude(Vector3 pos) {
        RaycastHit raycastHit;
        if (Physics.Raycast(pos - Origin.position, Vector3.down, out raycastHit, 1000f, 65536))
        {
            return raycastHit.distance;
        }

        return -1f;
    }

    private IEnumerator validateTeleport(EntityAlive target, bool randomPosition = false) {
        yield return new WaitForSeconds(1f);
        var y = (int)GameManager.Instance.World.GetHeightAt(position.x, position.z);
        if (position.y < y)
        {
            var myPosition = position;

            var player = target as EntityPlayer;
            if (player != null)
                myPosition = player.GetBreadcrumbPos(3 * rand.RandomFloat);

            if (randomPosition)
            {
                Vector3 dirV = target.position - this.position;
                myPosition = RandomPositionGenerator.CalcPositionInDirection(target, target.position, dirV, 5, 80f);
            }

            //// Find the ground.
            myPosition.y = (int)GameManager.Instance.World.GetHeightAt(myPosition.x, myPosition.z) + 2;

            // var myPosition = RandomPositionGenerator.CalcTowards(Owner, 5, 20, 2, Owner.position);

            // Find the ground.

            motion = Vector3.zero;
            navigator?.clearPath();
            this.SetPosition(myPosition, true);
        }

        this.isTeleporting = false;
        yield return null;
        yield break;
    }

    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse) {
        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable"))
            return;

        if (Buffs.HasBuff("buffInvulnerable"))
            return;

        if (!isEntityRemote)
        {
            // If we are being attacked, let the state machine know it can fight back
            emodel.avatarController.UpdateBool("IsBusy", false);
        }

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        base.ProcessDamageResponseLocal(_dmResponse);
        ToggleTraderID(true);
    }


    // Cleaned up this method to try to avoid the disappearing NPCs.
    //Original logic was meant to protect the trader's from unloading NPCs when they entered a trader area.
    public override void MarkToUnload() {
        GameManager.Instance.World.ChunkClusters[0].OnChunkVisibleDelegates -= this.chunkClusterVisibleDelegate;

        //if ( !isHirable)
        //{
        //    base.MarkToUnload();
        //    return;
        //}

        //// Only prevent despawning if owned.
        //var leader = EntityUtilities.GetLeaderOrOwner(entityId);
        //// make sure they are alive first.
        //if (leader != null && IsAlive())
        //{
        //    switch (EntityUtilities.GetCurrentOrder(entityId))
        //    {
        //        case EntityUtilities.Orders.Patrol:
        //        case EntityUtilities.Orders.Stay:
        //            base.MarkToUnload();
        //            return;
        //        default:
        //            break;
        //    }
        // Something asked us to despawn. Check if we are in a trader area. If we are, ignore the request.
        //if (_traderArea == null)
        //    _traderArea = world.GetTraderAreaAt(new Vector3i(position));

        //if (_traderArea != null)
        //{

        //    IsDespawned = false;
        //    return;
        //}
        ////  }

        base.MarkToUnload();
    }

    private void UpdateBlockRadiusEffects() {
        var blockPosition = GetBlockPosition();
        var num = World.toChunkXZ(blockPosition.x);
        var num2 = World.toChunkXZ(blockPosition.z);
        _startedThisFrame = new List<string>();
        for (var i = -1; i < 2; i++)
        for (var j = -1; j < 2; j++)
        {
            var chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
            if (chunk == null) continue;

            var tileEntities = chunk.GetTileEntities();
            for (var k = 0; k < tileEntities.list.Count; k++)
            {
                var tileEntity = tileEntities.list[k];

                if (!tileEntity.IsActive(world)) continue;

                var block = world.GetBlock(tileEntity.ToWorldPos());
                var block2 = Block.list[block.type];
                if (block2.RadiusEffects == null) continue;


                var distanceSq = GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
                for (var l = 0; l < block2.RadiusEffects.Length; l++)
                {
                    var blockRadiusEffect = block2.RadiusEffects[l];
                    if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius &&
                        !Buffs.HasBuff(blockRadiusEffect.variable))
                        Buffs.AddBuff(blockRadiusEffect.variable);
                }
            }
        }
    }

    public override float GetMoveSpeed() {
        var speed = EffectManager.GetValue(PassiveEffects.WalkSpeed, null, this.moveSpeed);
        if (IsCrouching)
            speed = EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, this.moveSpeed);

        return speed;
    }

    public override float GetMoveSpeedAggro() {
        var speed = EffectManager.GetValue(PassiveEffects.RunSpeed, null, this.moveSpeedPanic);
        return speed;
    }

    public new float GetMoveSpeedPanic() {
        var speed = EffectManager.GetValue(PassiveEffects.RunSpeed, null, this.moveSpeedPanic);
        return speed;
    }


    public void AddKillXP(EntityAlive killedEntity, float xpModifier = 1f) {
        var num = EntityClass.list[killedEntity.entityClass].ExperienceValue;
        if (xpModifier is > 1f or < 1f)
        {
            num = (int)(num * xpModifier);
        }

        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityPlayer;
        if (leader)
        {
            if (leader.Party != null)
            {
                // We don't check to see if its in the party, as the NPC isn't' really part of the party.
                num = leader.Party.GetPartyXP(leader, num);
            }
        }


        if (!isEntityRemote)
        {
            if (Progression != null)
            {
                Progression.AddLevelExp(num, "_xpFromKill", Progression.XPTypes.Kill, true);
                bPlayerStatsChanged = true;
            }
        }
        else
        {
            var package = NetPackageManager.GetPackage<NetPackageEntityAddExpClient>()
                .Setup(this.entityId, num, Progression.XPTypes.Kill);
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, this.entityId);
        }

        if (leader == null) return;
        if (leader.Party == null)
        {
            if (GameManager.Instance.World.IsLocalPlayer(leader.entityId))
            {
                GameManager.Instance.SharedKillClient(killedEntity.entityClass, num, null);
            }

            return;
        }

        foreach (var entityPlayer2 in leader.Party.MemberList)
        {
            if (!(Vector3.Distance(leader.position, entityPlayer2.position) <
                  (float)GameStats.GetInt(EnumGameStats.PartySharedKillRange))) continue;
            if (GameManager.Instance.World.IsLocalPlayer(entityPlayer2.entityId))
            {
                GameManager.Instance.SharedKillClient(killedEntity.entityClass, num, null);
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageSharedPartyKill>()
                        .Setup(killedEntity.entityId, entityId), false, entityPlayer2.entityId);
            }
        }
    }

    // General ExecuteAction that takes an action ID.
    private bool bLastAttackReleased;

    public bool ExecuteAction(bool _bAttackReleased, int actionIndex) {
        if (!_bAttackReleased)
        {
            if (this.emodel && this.emodel.avatarController && this.emodel.avatarController.IsAnimationAttackPlaying())
            {
                return false;
            }

            if (!this.IsAttackValid())
            {
                return false;
            }
        }

        if (this.bLastAttackReleased && this.GetSoundAttack() != null)
        {
            this.PlayOneShot(this.GetSoundAttack(), false, true);
        }

        this.bLastAttackReleased = _bAttackReleased;
        this.attackingTime = 60;

        ItemAction itemAction = this.inventory.holdingItem.Actions[actionIndex];
        if (itemAction != null)
        {
            itemAction.ExecuteAction(this.inventory.holdingItemData.actionData[actionIndex], _bAttackReleased);
        }

        return true;
    }

    public override void OnEntityDeath() {
        Log.Out($"{entityName} ({entityId}) has died.");
        Log.Out("Active Buffs:");
        foreach (var buff in Buffs.ActiveBuffs)
        {
            Log.Out($" > {buff.BuffName}");
        }

        base.OnEntityDeath();
    }

    public override void dropItemOnDeath() {
        // Don't drop your toolbelt
        if (this.world.IsDark())
        {
            this.lootDropProb *= 1f;
        }

        if (this.entityThatKilledMe)
        {
            this.lootDropProb = EffectManager.GetValue(PassiveEffects.LootDropProb,
                this.entityThatKilledMe.inventory.holdingItemItemValue, this.lootDropProb, this.entityThatKilledMe);
        }

        if (this.lootDropProb > this.rand.RandomFloat)
        {
            GameManager.Instance.DropContentOfLootContainerServer(BlockValue.Air, new Vector3i(this.position),
                this.entityId);
        }

        return;
    }
    // public override Vector3i dropCorpseBlock()
    // {
    //     var bagPosition = new Vector3i(this.position + base.transform.up);
    //     if (lootContainer == null) return base.dropCorpseBlock();
    //
    //     if (lootContainer.IsEmpty()) return base.dropCorpseBlock();
    //
    //     // Check to see if we have our backpack container.
    //     var className = "BackpackNPC";
    //     EntityClass entityClass = EntityClass.GetEntityClass(className.GetHashCode());
    //     if (entityClass == null)
    //         className = "Backpack";
    //
    //     var entityBackpack = EntityFactory.CreateEntity(className.GetHashCode(), bagPosition) as EntityItem;
    //     EntityCreationData entityCreationData = new EntityCreationData(entityBackpack);
    //     entityCreationData.entityName = Localization.Get(this.EntityName);
    //
    //     entityCreationData.id = -1;
    //     entityCreationData.lootContainer = lootContainer;
    //     GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
    //     entityBackpack.OnEntityUnload();
    //    // this.SetDroppedBackpackPosition(new Vector3i(bagPosition));
    //     return bagPosition;
    //
    // }

    public override void playStepSound(string stepSound) {
        if (IsOnMission()) return;
        if (HasAnyTags(FastTags<TagGroup.Global>.Parse("floating"))) return;

        base.playStepSound(stepSound);
    }

    public void CheckNoise() {
        // if they arn't sleeping, don't bother scanning for players.
        if (!IsSleeping) return;

        float num9 = EAIManager.CalcSenseScale();
        Bounds bb = new Bounds(position, new Vector3(15, 15, 15));
        List<Entity> entityTempList = new List<Entity>();
        world.GetEntitiesInBounds(typeof(EntityPlayer), bb, entityTempList);
        for (int j = 0; j < entityTempList.Count; j++)
        {
            EntityPlayer entityAlive = (EntityPlayer)entityTempList[j];
            var noiseLevel = entityAlive.Buffs.GetCustomVar("_noiseLevel");
            float distance = GetDistance(entityAlive);
            float num11 = noiseLevel * (1f + num9 * aiManager.feralSense);
            num11 /= distance * 0.6f + 0.4f;
            // Stealth walking is between 2 and 3. Walking without stealth is 4+
            if (num11 > 4) // wake up! 
            {
                ConditionalTriggerSleeperWakeUp();
                return;
            }
        }
    }

    public override void AwardKill(EntityAlive killer) {
        if (killer != null && killer != this)
        {
            var entityPlayer = killer as EntityPlayer;
            if (entityPlayer)
            {
                if (!entityPlayer.isEntityRemote)
                    SCoreQuestEventManager.Instance.EntityAliveKilled(EntityClass.list[entityClass].entityClassName);
            }
        }

        base.AwardKill(killer);
    }

    public void Collect(int _playerId) {
        var entityPlayerLocal = world.GetEntity(_playerId) as EntityPlayerLocal;
        if (entityPlayerLocal == null) return;
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        var itemStack = new ItemStack(GetItemValue(), 1);
        foreach (var meta in itemStack.itemValue.Metadata)
        {
            Debug.Log($"Key: {meta.Key}");
            Debug.Log($"Value: {meta.Value.GetValue()}");
        }

        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
        {
            GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId,
                60f, false);
        }
    }

    public void SetItemValue(ItemValue itemValue) {
        var entityName = itemValue.GetMetadata("NPCName") as string;
        SetEntityName(entityName);

        var myName = itemValue.GetMetadata("MyName") as string;
        if (!string.IsNullOrEmpty(myName))
            _strMyName = myName;

        var myTitle = itemValue.GetMetadata("MyTitle") as string;
        if (!string.IsNullOrEmpty(myTitle))
            _strTitle = myTitle;

        belongsPlayerId = (int)itemValue.GetMetadata("BelongsToPlayer");
        Health = (int)itemValue.GetMetadata("health");
        var leaderID = (int)itemValue.GetMetadata("Leader");
        EntityUtilities.SetLeaderAndOwner(entityId, leaderID);
        lootContainer.entityId = entityId;
        lootContainer.lootListName = itemValue.GetMetadata("LootListName") as string;
        var slots = lootContainer.items;
        for (var i = 0; i < slots.Length; i++)
        {
            var key = $"LootContainer-{i}";
            var storage = itemValue.GetMetadata(key)?.ToString();
            if (string.IsNullOrEmpty(storage)) continue;

            var itemId = storage.Split(',')[0];
            var quality = StringParsers.ParseUInt16(storage.Split(',')[1]);
            var itemCount = StringParsers.ParseSInt32(storage.Split(',')[2]);
            var createItem = ItemClass.GetItem(itemId);
            if (storage.Split(',').Length > 3)
            {
                var useTime = StringParsers.ParseFloat(storage.Split(',')[3]);
                createItem.UseTimes = useTime;
            }

            createItem.Quality = quality;
            var stack = new ItemStack(createItem, itemCount);
            lootContainer.AddItem(stack);
        }


        // Tool belt
        slots = inventory.GetSlots();
        for (var i = 0; i < slots.Length; i++)
        {
            var key = $"InventorySlot-{i}";
            var storage = itemValue.GetMetadata(key)?.ToString();
            if (string.IsNullOrEmpty(storage)) continue;

            var itemId = storage.Split(',')[0];
            var quality = StringParsers.ParseUInt16(storage.Split(',')[1]);
            var itemCount = StringParsers.ParseSInt32(storage.Split(',')[2]);
            var createItem = ItemClass.GetItem(itemId);
            createItem.Quality = quality;
            var stack = new ItemStack(createItem, itemCount);
            inventory.SetItem(i, stack);
        }


        var x = (int)itemValue.GetMetadata("TotalBuff");
        for (var i = 0; i < x; i++)
        {
            var buffName = itemValue.GetMetadata($"Buff-{i}")?.ToString();
            Buffs.AddBuff(buffName);
        }

        x = (int)itemValue.GetMetadata("TotalCVar");
        for (var i = 0; i < x; i++)
        {
            var cvarData = itemValue.GetMetadata($"CVar-{i}")?.ToString();
            if (cvarData == null) continue;

            var cvarName = cvarData.Split(':')[0];
            var cvarValue = cvarData.Split(':')[1];
            Buffs.AddCustomVar(cvarName, StringParsers.ParseFloat(cvarValue));
        }

        var weaponType = itemValue.GetMetadata("CurrentWeapon").ToString();
        _defaultWeapon = itemValue.GetMetadata("DefaultWeapon").ToString();
        var item = ItemClass.GetItem(weaponType);
        if (item != null)
        {
            UpdateWeapon(item, true);
        }

        Buffs.SetCustomVar("WeaponTypeNeedsUpdate", 1);
    }

    public ItemValue GetItemValue() {
        var type = 0;

        var targetItemClass = "spherePickUpNPC";
        var currentEntityClass = EntityClass.list[entityClass];
        if (currentEntityClass.Properties.Values.ContainsKey("PickUpItem"))
        {
            targetItemClass = currentEntityClass.Properties.Values["PickUpItem"];
        }

        var itemClass = ItemClass.GetItemClass(targetItemClass, true);
        if (itemClass != null)
            type = itemClass.Id;
        else
        {
            Log.Out($"Invalid PickUpItem: {targetItemClass}");
            return ItemValue.None;
        }

        var itemValue = new ItemValue(type, false);
        itemValue.SetMetadata("NPCName", EntityName, TypedMetadataValue.TypeTag.String);
        if (!string.IsNullOrEmpty(_strMyName))
            itemValue.SetMetadata("MyName", _strMyName, TypedMetadataValue.TypeTag.String);
        if (!string.IsNullOrEmpty(_strTitle))
            itemValue.SetMetadata("MyTitle", _strTitle, TypedMetadataValue.TypeTag.String);
        itemValue.SetMetadata("EntityClassId", entityClass, TypedMetadataValue.TypeTag.Integer);
        itemValue.SetMetadata("BelongsToPlayer", belongsPlayerId, TypedMetadataValue.TypeTag.Integer);
        itemValue.SetMetadata("health", Health, TypedMetadataValue.TypeTag.Integer);
        var leader = EntityUtilities.GetLeaderOrOwner(entityId);
        if (leader)
            itemValue.SetMetadata("Leader", leader.entityId, TypedMetadataValue.TypeTag.Integer);
        itemValue.SetMetadata("CurrentWeapon", inventory.holdingItem.GetItemName(), TypedMetadataValue.TypeTag.String);
        itemValue.SetMetadata("DefaultWeapon", _defaultWeapon, TypedMetadataValue.TypeTag.String);
        itemValue.SetMetadata("RightHandJointName", inventory.holdingItem.Properties.GetString("RightHandJointName"),
            TypedMetadataValue.TypeTag.String);
        if (lootContainer == null) return itemValue;

        if (!string.IsNullOrEmpty(lootContainer.lootListName))
            itemValue.SetMetadata("LootListName", lootContainer.lootListName, TypedMetadataValue.TypeTag.String);
        var slots = lootContainer.items;
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty()) continue;
            var itemId = slots[i].itemValue.ItemClass.GetItemName();
            var quality = slots[i].itemValue.Quality;
            var itemCount = slots[i].count;
            var itemUse = slots[i].itemValue.UseTimes;
            var storage = $"{itemId},{quality},{itemCount},{itemUse}";
            itemValue.SetMetadata($"LootContainer-{i}", storage, TypedMetadataValue.TypeTag.String);
        }

        // Tool belt
        slots = inventory.GetSlots();
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty()) continue;
            var itemId = slots[i].itemValue.ItemClass.GetItemName();
            var quality = slots[i].itemValue.Quality;
            var itemCount = slots[i].count;
            var itemUse = slots[i].itemValue.UseTimes;
            var storage = $"{itemId},{quality},{itemCount},{itemUse}";
            itemValue.SetMetadata($"InventorySlot-{i}", storage, TypedMetadataValue.TypeTag.String);
        }

        var x = 0;
        itemValue.SetMetadata($"TotalBuff", Buffs.ActiveBuffs.Count, TypedMetadataValue.TypeTag.Integer);
        foreach (var buff in Buffs.ActiveBuffs)
        {
            itemValue.SetMetadata($"Buff-{x}", buff.BuffName, TypedMetadataValue.TypeTag.String);
            x++;
        }

        x = 0;
        itemValue.SetMetadata($"TotalCVar", Buffs.CVars.Count, TypedMetadataValue.TypeTag.Integer);
        foreach (var cvar in Buffs.CVars)
        {
            var value = $"{cvar.Key}:{cvar.Value}";
            itemValue.SetMetadata($"CVar-{x}", value, TypedMetadataValue.TypeTag.String);
            x++;
        }

        return itemValue;
    }

    public bool FindWeapon(string weapon) {
        var currentWeapon = ItemClass.GetItem(weapon);
        if (currentWeapon == null) return false;
        if (!currentWeapon.ItemClass.Properties.Contains("CompatibleWeapon")) return false;
        var playerWeapon = currentWeapon.ItemClass.Properties.GetStringValue("CompatibleWeapon");
        if (string.IsNullOrEmpty(playerWeapon)) return false;
        var playerWeaponItem = ItemClass.GetItem(playerWeapon);
        if (playerWeaponItem == null) return false;
        if (lootContainer != null)
        {
            if (lootContainer.HasItem(playerWeaponItem))
                return true;
        }

        // If we don't have it in our loot container, check to see if we had it when we first spawned in.
        for (var i = 0; i < itemsOnEnterGame.Count; i++)
        {
            var itemStack = itemsOnEnterGame[i];
            if (itemStack.itemValue.ItemClass.GetItemName()
                .Equals(weapon, StringComparison.InvariantCultureIgnoreCase)) return true;
        }

        if (GetHandItem().ItemClass.GetItemName().Equals(weapon, StringComparison.InvariantCultureIgnoreCase))
            return true;
        return false;
    }


    public override void SetupStartingItems() {
        for (var i = 0; i < this.itemsOnEnterGame.Count; i++)
        {
            var itemStack = this.itemsOnEnterGame[i];
            var forId = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
            {
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
            }
            else
            {
                itemStack.count = forId.Stacknumber.Value;
            }

            if (i == 0)
                _defaultWeapon = forId.GetItemName();
            inventory.SetItem(i, itemStack);
        }
    }

    private void AddToInventory() {
        // We only want to fire the initial inventory once per entity creation.
        // Let's gate it using a cvar
        if (Buffs.GetCustomVar("InitialInventory") > 0) return;
        Buffs.SetCustomVar("InitialInventory", 1);
        var currentEntityClass = EntityClass.list[entityClass];
        if (currentEntityClass.Properties.Values.ContainsKey("StartingItems"))
        {
            var text = currentEntityClass.Properties.Values["StartingItems"];
            var items = text.Split(',');
            foreach (var item in items)
            {
                var itemStack = ItemStack.FromString(item.Trim());
                if (itemStack.itemValue.IsEmpty()) continue;
                var forId = ItemClass.GetForId(itemStack.itemValue.type);
                if (forId.HasQuality)
                {
                    itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
                }

                lootContainer.AddItem(itemStack);
            }
        }
    }

    public void RefreshWeapon() {
        var item = ItemClass.GetItem(_currentWeapon);
        UpdateWeapon(item);
    }

    public void UpdateWeapon(string itemName = "") {
        if (string.IsNullOrEmpty(itemName))
            itemName = _currentWeapon;

        var item = ItemClass.GetItem(itemName);
        UpdateWeapon(item);
        EntityUtilities.UpdateHandItem(entityId, itemName);
    }

    // Allows the NPC to change their hand items, and update their animator.
    public void UpdateWeapon(ItemValue item, bool force = false) {
        if (item == null) return;
        if (item.GetItemId() < 0) return;
        _currentWeapon = item.ItemClass.GetItemName();

        // Do we have this item?
        if (!FindWeapon(_currentWeapon))
        {
            Debug.Log($"EntityAliveSDX: UpdateWeapon() Item not found: {_currentWeapon}");
            if (string.IsNullOrEmpty(_defaultWeapon))
                return;

            // Switch to default
            item = ItemClass.GetItem(_defaultWeapon);
        }

        if (item.GetItemId() == inventory.holdingItemItemValue.GetItemId() && force == false)
        {
            return;
        }

        _currentWeapon = item.ItemClass.GetItemName();
        Buffs.SetCustomVar("CurrentWeaponID", item.GetItemId());
        inventory.SetItem(0, item, 1);

        foreach (var action in item.ItemClass.Actions)
        {
            if (action is ItemActionRanged ranged)
            {
                ranged.AutoFire = new DataItem<bool>(true);
            }
        }


        // Since we are potentially changing the Hand Transform, we need to set the animator it changed.
        var entityClassType = EntityClass.list[entityClass];
        emodel.avatarController.SwitchModelAndView(entityClassType.mesh.name, emodel.IsFPV, IsMale);

        // Item update has to happen after the SwitchModelAndView, otherwise the weapon will attach to the previous hand position
        inventory.OnUpdate();
        inventory.ForceHoldingItemUpdate();
    }

    // The GetRightHandTransformName() is not virtual in the base class. There's a Harmony patch that redirects the AvatarAnimator's call here.
    // This helps adjust the hand position for various weapons we can add to the NPC.
    public new string GetRightHandTransformName() {
        var currentItemHand = inventory.holdingItem;
        if (currentItemHand.Properties.Contains(EntityClass.PropRightHandJointName))
        {
            currentItemHand.Properties.ParseString(EntityClass.PropRightHandJointName, ref rightHandTransformName);
        }
        else
        {
            rightHandTransformName = "Gunjoint";
            EntityClass.list[entityClass].Properties
                .ParseString(EntityClass.PropRightHandJointName, ref rightHandTransformName);
        }

        return rightHandTransformName;
    }

    public override void PlayOneShot(string clipName, bool sound_in_head = false, bool netsync = true,
        bool isUnique = false) {
        if (IsOnMission()) return;
        base.PlayOneShot(clipName, sound_in_head);
    }

    public override void OnDeathUpdate() {
        if (this.deathUpdateTime < this.timeStayAfterDeath)
        {
            this.deathUpdateTime++;
        }

        int deadBodyHitPoints = EntityClass.list[this.entityClass].DeadBodyHitPoints;
        if (deadBodyHitPoints > 0 && this.DeathHealth <= -deadBodyHitPoints)
        {
            this.deathUpdateTime = this.timeStayAfterDeath;
        }

        if (this.deathUpdateTime != this.timeStayAfterDeath)
        {
            return;
        }

        if (!this.isEntityRemote && !this.markedForUnload)
        {
            this.dropCorpseBlock();
            if (this.particleOnDestroy != null && this.particleOnDestroy.Length > 0)
            {
                float lightBrightness = this.world.GetLightBrightness(base.GetBlockPosition());
                this.world.GetGameManager().SpawnParticleEffectServer(
                    new ParticleEffect(this.particleOnDestroy, this.getHeadPosition(), lightBrightness, Color.white,
                        null, null, false), this.entityId);
            }
        }
    }

    public override Vector3i dropCorpseBlock() {
        if (lootContainer != null && lootContainer.IsUserAccessing())
        {
            return Vector3i.zero;
        }

        if (corpseBlockValue.isair)
        {
            return Vector3i.zero;
        }

        if (rand.RandomFloat > corpseBlockChance)
        {
            return Vector3i.zero;
        }

        var vector3I = World.worldToBlockPos(this.position);
        while (vector3I.y < 254 && (float)vector3I.y - this.position.y < 3f &&
               !this.corpseBlockValue.Block.CanPlaceBlockAt(this.world, 0, vector3I, this.corpseBlockValue, false))
        {
            vector3I += Vector3i.up;
        }

        if (vector3I.y >= 254)
        {
            return Vector3i.zero;
        }

        if ((float)vector3I.y - this.position.y >= 2.1f)

        {
            return Vector3i.zero;
        }

        this.world.SetBlockRPC(vector3I, this.corpseBlockValue);

        if (vector3I == Vector3i.zero)
        {
            return Vector3i.zero;
        }

        if (world.GetTileEntity(0, vector3I) is not TileEntityLootContainer tileEntityLootContainer)
        {
            return Vector3i.zero;
        }

        if (lootContainer != null)
        {
            tileEntityLootContainer.CopyLootContainerDataFromOther(lootContainer);
        }
        else
        {
            tileEntityLootContainer.lootListName = this.lootListOnDeath;
            tileEntityLootContainer.SetContainerSize(LootContainer.GetLootContainer(lootListOnDeath).size,
                true);
        }

        tileEntityLootContainer.SetModified();
        return vector3I;
    }

    public override void updateStepSound(float distX, float distZ) {
        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityAlive;
        if (leader == null)
        {
            base.updateStepSound(distX, distZ);
            return;
        }

        if (leader.Buffs.GetCustomVar("quietNPC") > 0) return;
        //if (leader.Buffs.HasCustomVar("quietNPC")) return;

        // Mute the foot steps when crouching.
        if (IsCrouching) return;

        base.updateStepSound(distX, distZ);
    }

    private bool ShouldPushOutOfBlock(int _x, int _y, int _z, bool pushOutOfTerrain) {
        var shape = world.GetBlock(_x, _y, _z).Block.shape;
        if (shape.IsSolidSpace && !shape.IsTerrain())
        {
            return true;
        }

        if (!pushOutOfTerrain || !shape.IsSolidSpace || !shape.IsTerrain()) return false;
        var shape2 = this.world.GetBlock(_x, _y + 1, _z).Block.shape;
        return shape2.IsSolidSpace && shape2.IsTerrain();
    }

    private bool PushOutOfBlocks(float _x, float _y, float _z) {
        var num = Utils.Fastfloor(_x);
        var num2 = Utils.Fastfloor(_y);
        var num3 = Utils.Fastfloor(_z);
        var num4 = _x - (float)num;
        var num5 = _z - (float)num3;
        var result = false;
        if (!this.ShouldPushOutOfBlock(num, num2, num3, false) &&
            (!this.ShouldPushOutOfBlock(num, num2 + 1, num3, false))) return false;
        var flag2 = !this.ShouldPushOutOfBlock(num - 1, num2, num3, true) &&
                    !this.ShouldPushOutOfBlock(num - 1, num2 + 1, num3, true);
        var flag3 = !this.ShouldPushOutOfBlock(num + 1, num2, num3, true) &&
                    !this.ShouldPushOutOfBlock(num + 1, num2 + 1, num3, true);
        var flag4 = !this.ShouldPushOutOfBlock(num, num2, num3 - 1, true) &&
                    !this.ShouldPushOutOfBlock(num, num2 + 1, num3 - 1, true);
        var flag5 = !this.ShouldPushOutOfBlock(num, num2, num3 + 1, true) &&
                    !this.ShouldPushOutOfBlock(num, num2 + 1, num3 + 1, true);
        var b = byte.MaxValue;
        var num6 = 9999f;
        if (flag2 && num4 < num6)
        {
            num6 = num4;
            b = 0;
        }

        if (flag3 && 1.0 - (double)num4 < (double)num6)
        {
            num6 = 1f - num4;
            b = 1;
        }

        if (flag4 && num5 < num6)
        {
            num6 = num5;
            b = 4;
        }

        if (flag5 && 1f - num5 < num6)
        {
            b = 5;
        }

        var num7 = 0.1f;
        switch (b)
        {
            case 0:
                this.motion.x = -num7;
                break;
            case 1:
                this.motion.x = num7;
                break;
            case 4:
                this.motion.z = -num7;
                break;
            case 5:
                this.motion.z = num7;
                break;
        }

        if (b != 255)
        {
            result = true;
        }

        return result;
    }

    private bool CheckNonSolidVertical(Vector3i blockPos, int maxY, int verticalSpace) {
        for (int i = 0; i < maxY; i++)
        {
            if (!this.world.GetBlock(blockPos.x, blockPos.y + i + 1, blockPos.z).Block.shape.IsSolidSpace)
            {
                bool flag = true;
                for (int j = 1; j < verticalSpace; j++)
                {
                    if (this.world.GetBlock(blockPos.x, blockPos.y + i + 1 + j, blockPos.z).Block.shape.IsSolidSpace)
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    return true;
                }
            }
        }

        return false;
    }


    public virtual void CheckStuck() {
        IsStuck = false;
        if (IsFlyMode.Value) return;
        var num = boundingBox.min.y + 0.5f;
        IsStuck = PushOutOfBlocks(position.x - width * 0.3f, num, position.z + depth * 0.3f);
        IsStuck = (PushOutOfBlocks(position.x - width * 0.3f, num, position.z - depth * 0.3f) || IsStuck);
        IsStuck = (PushOutOfBlocks(position.x + width * 0.3f, num, position.z - depth * 0.3f) || IsStuck);
        IsStuck = (PushOutOfBlocks(position.x + width * 0.3f, num, position.z + depth * 0.3f) || IsStuck);
        if (IsStuck) return;

        var x = Utils.Fastfloor(position.x);
        var num2 = Utils.Fastfloor(num);
        var z = Utils.Fastfloor(position.z);
        if (!ShouldPushOutOfBlock(x, num2, z, true) ||
            !CheckNonSolidVertical(new Vector3i(x, num2 + 1, z), 4, 2)) return;
        IsStuck = true;
        motion = new Vector3(0f, 1.6f, 0f);
        Log.Warning($"{EntityName} ({entityId}) is stuck. Unsticking.");
    }
}