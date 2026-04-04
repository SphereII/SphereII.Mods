/*
 * Class:       EntityAliveSDXV4
 * Author:      sphereii
 * Category:    Entity
 * Description:
 *      Refactored successor to EntityAliveSDX.  Behaviour is identical but the internal
 *      architecture applies several targeted performance improvements:
 *
 *      - Per-tick frame cache (NPCFrameCache) populated once at the top of OnUpdateLive.
 *        Every expensive lookup (GetLeaderOrOwner, GetAttackOrRevengeTarget) happens exactly
 *        once per tick regardless of how many methods consume that data.
 *
 *      - Feature components (NPCLeaderComponent, NPCPatrolComponent, NPCCombatComponent,
 *        NPCEffectsComponent) each own a single concern, carry their own state, and are
 *        updated via the shared frame cache.
 *
 *      - Patrol dedup uses a HashSet<Vector3> (O(1)) instead of List.Contains (O(n)).
 *
 *      - Block-radius effects are throttled to once every two seconds instead of every frame.
 *
 *      - Proximity/collision check is throttled to ProximityCheckInterval instead of every frame.
 *
 *      - Companion list management no longer calls IndexOf twice for the same element.
 *
 *      - Task string is lowercased once in SetupAutoPathingBlocks instead of four times.
 *
 *      - Patrol coordinate serialization uses string.Join instead of O(n²) concatenation.
 *
 *      - EntityName caches the localized display string; invalidated when the name changes.
 *
 * Usage:
 *      <property name="Class" value="EntityAliveSDXV4, SCore" />
 */

using Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UAI;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

// ReSharper disable once CheckNamespace
public partial class EntityAliveSDXV4 : EntityTrader, IEntityOrderReceiverSDX, IEntityAliveSDX
{
    // =========================================================================
    // Components & frame cache
    // =========================================================================

    private NPCLeaderComponent  _leaderComp;
    private NPCPatrolComponent  _patrolComp;
    private NPCCombatComponent  _combatComp;
    private NPCEffectsComponent _effectsComp;

    /// <summary>Flat array used for the component update loop — avoids virtual dispatch overhead.</summary>
    private INPCComponent[] _components;

    /// <summary>Populated once per OnUpdateLive tick; passed by ref to each component.</summary>
    private NPCFrameCache _frameCache;

    private void InitializeComponents()
    {
        _leaderComp  = new NPCLeaderComponent();
        _patrolComp  = new NPCPatrolComponent();
        _combatComp  = new NPCCombatComponent();
        _effectsComp = new NPCEffectsComponent();

        _leaderComp .Initialize(this);
        _patrolComp .Initialize(this);
        _combatComp .Initialize(this);
        _effectsComp.Initialize(this);

        _components = new INPCComponent[] { _leaderComp, _patrolComp, _combatComp, _effectsComp };
    }

    // =========================================================================
    // Public fields / state  (mirrors EntityAliveSDX public API)
    // =========================================================================

    public List<string> lstQuests   = new List<string>();
    public bool         isAlwaysAwake;

    // Patrol / guard — backed by NPCPatrolComponent
    [FormerlySerializedAs("PatrolCoordinates")]
    private List<Vector3> _patrolCoordinatesLegacy; // only used during Read() before components exist

    [FormerlySerializedAs("GuardPosition")]
    private Vector3 _guardPositionLegacy;

    [FormerlySerializedAs("GuardLookPosition")]
    private Vector3 _guardLookPositionLegacy;

    public bool   canCollideWithLeader = true;
    public bool   canJump              = true;
    public bool   isTeleporting        = false;
    public bool   isHirable            = true;
    public bool   IsHirable            => isHirable;   // IEntityAliveSDX
    public bool IsSleeping { get; set; }
    public bool   isQuestGiver         = true;
    public bool   bWentThroughDoor;
    public float  flEyeHeight          = -1f;

    public EntityAlive Owner;

    public bool AddNPCToCompanion =
        Configuration.CheckFeatureStatus("AdvancedNPCFeatures", "DisplayCompanions");

    public string _currentWeapon = "";

    public QuestJournal questJournal = new QuestJournal();
    public string DialogWindow { get; set; }

    // =========================================================================
    // IEntityOrderReceiverSDX
    // =========================================================================

    /// <inheritdoc/>
    public List<Vector3> PatrolCoordinates => _patrolComp?.PatrolCoordinates ?? _patrolCoordinatesLegacy;

    /// <inheritdoc/>
    public Vector3 GuardPosition
    {
        get => _patrolComp != null ? _patrolComp.GuardPosition     : _guardPositionLegacy;
        set { if (_patrolComp != null) _patrolComp.GuardPosition     = value; else _guardPositionLegacy     = value; }
    }

    /// <inheritdoc/>
    public Vector3 GuardLookPosition
    {
        get => _patrolComp != null ? _patrolComp.GuardLookPosition  : _guardLookPositionLegacy;
        set { if (_patrolComp != null) _patrolComp.GuardLookPosition = value; else _guardLookPositionLegacy = value; }
    }

    /// <inheritdoc/>
    public Vector3 Position => position;

    // =========================================================================
    // Private fields
    // =========================================================================

    private string _strMyName       = string.Empty;
    private string _strTitle;
    private string _cachedDisplayName;
    private string _cachedDisplayNameKey;

    private string _defaultWeapon   = "";
    private int    _defaultTraderID;
    private float  _enemyDistanceToTalk = 10f;

    private Vector3 _scale;
    private ChunkCluster.OnChunkVisibleDelegate _chunkClusterVisibleDelegate;

    private TileEntityTrader _tileEntityTrader;
    private TraderArea       _traderArea;

    private string _particleOnDestroy;
    private BlockValue _corpseBlockValue;
    private float  _corpseBlockChance;

    private readonly bool _blDisplayLog = false;
    private float _fallTime;
    private float _fallThresholdTime;
    private bool  _bLastAttackReleased;
    private int   _stuckCheckCounter;

    // =========================================================================
    // EntityName — cached to avoid repeated Localization.Get + string allocation
    // =========================================================================

    public override string EntityName
    {
        get
        {
            if (string.IsNullOrEmpty(_strMyName))
                return entityName;

            // Cache the localized display string; invalidate when the name key changes.
            var key = _strMyName + "\x00" + _strTitle;
            if (_cachedDisplayNameKey != key)
            {
                _cachedDisplayNameKey = key;
                _cachedDisplayName = string.IsNullOrEmpty(_strTitle)
                    ? Localization.Get(_strMyName)
                    : Localization.Get(_strMyName) + " the " + Localization.Get(_strTitle);
            }
            return _cachedDisplayName;
        }
    }

    public string FirstName
    {
        get => Localization.Get(_strMyName);
        set { _strMyName = value; _cachedDisplayNameKey = null; }
    }

    public string Title
    {
        get => Localization.Get(_strTitle);
        set { _strTitle = value; _cachedDisplayNameKey = null; }
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    public void DisplayLog(string msg)
    {
        if (_blDisplayLog && !IsDead())
            Debug.Log(entityName + ": " + msg);
    }

    /// <summary>
    /// Logs a warning for any AIPackage name declared on this entity class that is not
    /// registered in <see cref="UAI.UAIBase.AIPackages"/>.  Called once from
    /// <see cref="PostInit"/> so problems are visible immediately on spawn rather than
    /// silently failing every UAI tick.
    /// </summary>
    private void LogMissingAIPackages()
    {
        if (AIPackages == null || AIPackages.Count == 0)
        {
            Log.Warning($"[SCore] {EntityName} ({entityId}): AIPackages list is empty. Add an AIPackages property to the entity class in entityclasses.xml.");
            return;
        }

        var missing = new System.Collections.Generic.List<string>();
        var found   = new System.Collections.Generic.List<string>();
        foreach (var pkg in AIPackages)
        {
            if (UAI.UAIBase.AIPackages.ContainsKey(pkg))
                found.Add(pkg);
            else
                missing.Add(pkg);
        }

        if (missing.Count > 0)
            Log.Warning($"[SCore] {EntityName} ({entityId}): {missing.Count} AIPackage(s) not registered – UAI tasks will be skipped for them: {string.Join(", ", missing)}");

        Log.Out($"[SCore] {EntityName} ({entityId}): AIPackages loaded ({found.Count}/{AIPackages.Count}): {string.Join(", ", found)}");
    }

    public override string ToString() => entityName;

    public override bool CanEntityJump() => canJump;

    public bool IsAvailable() => !IsOnMission();

    public bool IsOnMission()
    {
        return Buffs.HasCustomVar("onMission") && Buffs.GetCustomVar("onMission") == 1f;
    }

    // =========================================================================
    // Initialization
    // =========================================================================

    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        var ec = EntityClass.list[entityClass];

        if (ec.Properties.Values.ContainsKey("Hirable"))
            isHirable = StringParsers.ParseBool(ec.Properties.Values["Hirable"], 0, -1, true);

        if (ec.Properties.Values.ContainsKey("IsQuestGiver"))
            isQuestGiver = StringParsers.ParseBool(ec.Properties.Values["IsQuestGiver"], 0, -1, true);

        flEyeHeight = EntityUtilities.GetFloatValue(entityId, "EyeHeight");

        if (ec.Properties.Values.ContainsKey("Names"))
        {
            var names = ec.Properties.Values["Names"].Split(',');
            _strMyName = names[UnityEngine.Random.Range(0, names.Length)];
            _cachedDisplayNameKey = null;
        }

        DialogWindow = "dialog";
        if (ec.Properties.Values.ContainsKey("dialogWindow"))
            DialogWindow = ec.Properties.Values["dialogWindow"];

        if (ec.Properties.Values.ContainsKey("CanCollideWithLeader"))
            canCollideWithLeader = StringParsers.ParseBool(ec.Properties.Values["CanCollideWithLeader"], 0, -1, true);

        isAlwaysAwake = false;
        if (ec.Properties.Values.ContainsKey("SleeperInstantAwake"))
            isAlwaysAwake = StringParsers.ParseBool(ec.Properties.Values["SleeperInstantAwake"], 0, -1, true);
        if (ec.Properties.Values.ContainsKey("IsAlwaysAwake"))
            isAlwaysAwake = StringParsers.ParseBool(ec.Properties.Values["IsAlwaysAwake"], 0, -1, true);

        if (ec.Properties.Values.ContainsKey("Titles"))
        {
            var titles = ec.Properties.Values["Titles"].Split(',');
            _strTitle = titles[UnityEngine.Random.Range(0, titles.Length)];
            _cachedDisplayNameKey = null;
        }

        var collider = gameObject.GetComponent<BoxCollider>();
        if (ec.Properties.Classes.ContainsKey("Boundary"))
        {
            var strBox    = "0,0,0";
            var strCenter = "0,0,0";
            var dynProps  = ec.Properties.Classes["Boundary"];
            foreach (var kvp in dynProps.Values.Dict)
            {
                if (kvp.Key == "BoundaryBox") strBox    = dynProps.Values[kvp.Key];
                if (kvp.Key == "Center")      strCenter = dynProps.Values[kvp.Key];
            }
            ConfigureBoundaryBox(StringParsers.ParseVector3(strBox), StringParsers.ParseVector3(strCenter));
        }

        if (ec.Properties.Values.ContainsKey("BagItems"))
        {
            foreach (var item in ec.Properties.Values["BagItems"].Split(","))
            {
                var name  = item;
                var count = 1;
                if (item.Contains("="))
                {
                    var parts = item.Split('=');
                    name  = parts[0];
                    count = StringParsers.ParseSInt32(parts[1]);
                }
                var iv = ItemClass.GetItem(name);
                if (!iv.Equals(ItemValue.None))
                    bag.AddItem(new ItemStack(iv, count));
            }
        }
    }

    public override void PostInit()
    {
        base.PostInit();

        InitializeComponents();

        IsGodMode.Value = false;

        if (NPCInfo != null)
            _defaultTraderID = NPCInfo.TraderID;

        if (lootContainer == null)
        {
            Chunk chunk = null;
            lootContainer = new TileEntityLootContainer(chunk) { entityId = entityId };
            lootContainer.SetContainerSize(string.IsNullOrEmpty(GetLootList())
                ? new Vector2i(8, 6)
                : LootContainer.GetLootContainer(GetLootList()).size);
        }

        // Flush any patrol data read before components were ready.
        if (_patrolCoordinatesLegacy != null)
        {
            foreach (var pt in _patrolCoordinatesLegacy)
                _patrolComp.AddPatrolPoint(pt);
            _patrolCoordinatesLegacy = null;
        }
        _patrolComp.GuardPosition     = _guardPositionLegacy;
        _patrolComp.GuardLookPosition = _guardLookPositionLegacy;

        Buffs.SetCustomVar("$waterStaminaRegenAmount", 0, false);
        SetupStartingItems();
        if (!string.IsNullOrEmpty(_currentWeapon))
            UpdateWeapon(_currentWeapon);

        SetupAutoPathingBlocks();
        LogMissingAIPackages();

        _scale = transform.localScale;
        PhysicsTransform.gameObject.SetActive(true);
        SetSpawnerSource(EnumSpawnerSource.Biome);
        _enemyDistanceToTalk = StringParsers.ParseFloat(
            Configuration.GetPropertyValue("AdvancedNPCFeatures", "EnemyDistanceToTalk"));

        SanitizeTraderData();
    }

    private void SanitizeTraderData()
    {
        if (TileEntityTrader?.TraderData == null) return;
        var inv = TileEntityTrader.TraderData.PrimaryInventory;
        if (inv == null || inv.Count > 200)
        {
            Log.Warning($"[0-SCore] Corrupt TraderData on {EntityName} ({entityId}). Resetting.");
            Chunk chunk = null;
            TileEntityTrader = new TileEntityTrader(chunk);
            TileEntityTrader.entityId = entityId;
            TileEntityTrader.TraderData.TraderID = NPCInfo != null ? NPCInfo.TraderID : 1;
        }
    }

    public override void OnAddedToWorld()
    {
        if (isAlwaysAwake)
            IsSleeping = false;

        _chunkClusterVisibleDelegate = OnChunkDisplayed;
        GameManager.Instance.World.ChunkClusters[0].OnChunkVisibleDelegates += _chunkClusterVisibleDelegate;
        base.OnAddedToWorld();
        AddToInventory();
    }

    public void OnChunkDisplayed(long _key, bool _bDisplayed)
    {
        if (emodel == null) return;
        var modelTransform = emodel.GetModelTransform();
        if (modelTransform == null) return;
        foreach (var rb in modelTransform.GetComponentsInChildren<Rigidbody>())
            rb.useGravity = _bDisplayed;
    }

    public override void InitLocation(Vector3 _pos, Vector3 _rot)
    {
        base.InitLocation(_pos, _rot);
        PhysicsTransform.gameObject.SetActive(true);
    }

    // =========================================================================
    // OnUpdateLive — the hot path
    // =========================================================================

    public override void OnUpdateLive()
    {
        // ── 1. Populate the frame cache once ─────────────────────────────────
        _frameCache.Populate(this);

        // ── 2. Tick all components via the shared cache ───────────────────────
        for (int i = 0; i < _components.Length; i++)
            _components[i].OnUpdateLive(ref _frameCache);

        // ── 3. Always-awake wake-up ────────────────────────────────────────────
        if (!sleepingOrWakingUp && isAlwaysAwake)
        {
            IsSleeping = true;
            ConditionalTriggerSleeperWakeUp();
        }

        Buffs.RemoveBuff("buffnewbiecoat", false);
        Stats.Health.MaxModifier = Stats.Health.Max;

        // ── 4. Animation state ────────────────────────────────────────────────
        if (emodel != null && emodel.avatarController != null)
        {
            emodel.avatarController.UpdateBool("CanFall",
                !emodel.IsRagdollActive && bodyDamage.CurrentStun == EnumEntityStunType.None && !isSwimming);
            emodel.avatarController.UpdateBool("IsOnGround", onGround || isSwimming);
        }

        // ── 5. Base update ─────────────────────────────────────────────────────
        try
        {
            base.OnUpdateLive();
            if (IsDead()) return;
            if (bodyDamage.CurrentStun is EnumEntityStunType.Getup or EnumEntityStunType.Prone)
                SetHeight(physicsBaseHeight);
        }
        catch (Exception ex)
        {
            Debug.Log($"Entity Exception {entityId}: {ex}");
        }

        // ── 6. Stuck check ─────────────────────────────────────────────────────
        CheckStuck();

        // ── 7. TileEntityTrader lazy init ──────────────────────────────────────
        if (NPCInfo == null) return;

        if (_tileEntityTrader == null)
        {
            Chunk chunk = null;
            _tileEntityTrader = new TileEntityTrader(chunk);
            _tileEntityTrader.entityId = entityId;
            _tileEntityTrader.TraderData.TraderID = NPCInfo.TraderID;
        }

        if (isEntityRemote) return;
        if (!emodel)        return;

        var avatarController = emodel.avatarController;
        if (!avatarController) return;

        // ── 8. Fall state ──────────────────────────────────────────────────────
        var onStableGround = onGround || isSwimming || bInElevator;
        if (onStableGround)
        {
            _fallTime = 0f;
            _fallThresholdTime = 0f;
            if (bInElevator) _fallThresholdTime = 0.6f;
        }
        else
        {
            if (_fallThresholdTime == 0f)
                _fallThresholdTime = 0.1f + rand.RandomFloat * 0.3f;
            _fallTime += 0.05f;
        }

        var canFall = !emodel.IsRagdollActive
                      && bodyDamage.CurrentStun == EnumEntityStunType.None
                      && !isSwimming && !bInElevator
                      && jumpState == JumpState.Off
                      && !IsDead()
                      && _fallTime > _fallThresholdTime;

        avatarController.SetFallAndGround(canFall, onStableGround);
    }

    // =========================================================================
    // Patrol
    // =========================================================================

    /// <inheritdoc/>
    public virtual void UpdatePatrolPoints(Vector3 position)
    {
        _patrolComp.AddPatrolPoint(position);
    }

    // =========================================================================
    // Auto-pathing setup
    // =========================================================================

    /// <inheritdoc/>
    public void SetupAutoPathingBlocks()
    {
        if (Buffs.HasCustomVar("PathingCode"))
        {
            var pc = Buffs.GetCustomVar("PathingCode");
            if (pc < 0 || pc > 0) return;
        }

        var blocks = EntityUtilities.ConfigureEntityClass(entityId, "PathingBlocks");
        if (blocks.Count == 0)
            blocks = new List<string> { "PathingCube", "PathingCube2" };

        var pathingVectors = ModGeneralUtilities.ScanAutoConfigurationBlocks(position, blocks, 2);
        if (pathingVectors == null || pathingVectors.Count == 0) return;

        var target = ModGeneralUtilities.FindNearestBlock(position, pathingVectors);
        var sign   = GameManager.Instance.World.GetTileEntity(0, new Vector3i(target)) as TileEntitySign;
        if (sign == null) return;

        var text = sign.signText.Text;

        // ── Task ── cache ToLower() once instead of calling it 4 times
        var task = PathingCubeParser.GetValue(text, "task");
        if (!string.IsNullOrEmpty(task))
        {
            Log.Out($"SetupAutoPathingBlocks for: {entityName} ({entityId}) Task: {task}");
            var taskLower = task.ToLower();
            if      (taskLower == "stay")   Buffs.AddBuff("buffOrderStay",   -1, false);
            else if (taskLower == "wander") Buffs.AddBuff("buffOrderWander", -1, false);
            else if (taskLower == "guard")  Buffs.AddBuff("buffOrderGuard",  -1, false);
            else if (taskLower == "follow") Buffs.AddBuff("buffOrderFollow", -1, false);
            else Log.Out($"    Entity: {entityName} ({entityId}) Cannot perform task: {task}");
        }

        foreach (var buff in PathingCubeParser.GetValue(text, "buff").Split(','))
            Buffs.AddBuff(buff, -1, false);

        Buffs.SetCustomVar("PathingCode", -1f);
        var pathingCode = PathingCubeParser.GetValue(text, "pc");
        if (StringParsers.TryParseFloat(pathingCode, out var code))
            Buffs.SetCustomVar("PathingCode", code);
    }
}
