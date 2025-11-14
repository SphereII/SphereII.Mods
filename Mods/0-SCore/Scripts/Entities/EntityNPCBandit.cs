using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// <para>
/// This class extends <see cref="EntityBandit"/> and adds NPC features.
/// </para>
/// <para>
/// The primary benefits to extending <see cref="EntityBandit"/> are:
/// <list type="bullet">
/// <item>
/// The NPC can be spawned into wandering hordes or blood moon hordes.
/// </item>
/// <item>
/// The NPC can be targeted by vanilla zombies that use EAI, which targets by C# class.
/// <see cref="EntityHuman"/> is also the parent class of zombie entities, so if this NPC class
/// was a subclass, zombies couldn't target these NPCs without also targeting themselves.
/// </item>
/// </list>
/// </para>
/// </summary>
public class EntityNPCBandit : EntityBandit, IEntityOrderReceiverSDX
{
    public Random random = new();

    #region Interface Backing Fields
    private readonly List<Vector3> _patrolCoordinates = new();
    private Vector3 _guardPosition = Vector3.zero;
    private Vector3 _guardLookPosition = Vector3.zero;
    // We use a temp list to store the patrol coordinates of each vector, but centered over the
    // block. This allows us to check to make sure each vector we are storing is on a new block,
    // and not just 10.2 and 10.4. This helps smooth out the entity's walk. However, we do want
    // accurate patrol points, so we store the accurate patrol positions for the entity.
    private readonly List<Vector3> _tempList = new();
    #endregion

    #region Initialization Tracking
    private bool _isFullyInitialized = false;
    private int _initializationFrameCount = 0;
    #endregion

    /// <summary>
    /// If true, the entity should awaken as soon as it is spawned into a sleeper volume,
    /// regardless of the type of sleeper volume, without being triggered.
    /// </summary>
    public bool IsAlwaysAwake { get; protected set; }

    /// <inheritdoc/>
    public List<Vector3> PatrolCoordinates => _patrolCoordinates;

    /// <inheritdoc/>
    public Vector3 GuardPosition { get => _guardPosition; set => _guardPosition = value; }

    /// <inheritdoc/>
    public Vector3 GuardLookPosition { get => _guardLookPosition; set => _guardLookPosition = value; }

    /// <inheritdoc/>
    public Vector3 Position => position;

    public override float GetMoveSpeed()
    {
        return IsCrouching
            ? EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, moveSpeed, this)
            : EffectManager.GetValue(PassiveEffects.WalkSpeed, null, moveSpeed, this);
    }

    public override float GetMoveSpeedAggro()
    {
        return IsBloodMoon
            ? EffectManager.GetValue(PassiveEffects.RunSpeed, null, moveSpeedAggroMax, this)
            : EffectManager.GetValue(
                // Horde entities use WalkSpeed, but that messes up melee attacking. Luckily when
                // an entity finds an attack target they're no longer considered part of a horde.
                IsHordeZombie ? PassiveEffects.WalkSpeed : PassiveEffects.RunSpeed,
                null,
                moveSpeedAggro,
                this);
    }

    public override void PostInit()
    {
        base.PostInit();

        // EntityBandit.PostInit sets inventory slot 0 to the bare-hand item.
        // So, make sure we call SetupStartingItems after calling the base class.
        SetupStartingItems();


        // Does a quick local scan to see what pathing blocks, if any, are nearby.
        // If one is found nearby, then it'll use that code for pathing.
        SetupAutoPathingBlocks();
    }

    /// <summary>
    /// Sets up the starting items. Adopted from <see cref="EntityTrader.SetupStartingItems"/>
    /// and from <see cref="EntityPlayerLocal.SetupStartingItems"/>.
    /// </summary>
    protected virtual void SetupStartingItems()
    {
        // Safety check for inventory
        if (inventory == null || itemsOnEnterGame == null)
        {
            return;
        }

        for (var i = 0; i < itemsOnEnterGame.Count; i++)
        {
            var itemStack = itemsOnEnterGame[i];

            // Safety check for null item stack
            if (itemStack == null)
            {
                continue;
            }

            var itemClass = ItemClass.GetForId(itemStack.itemValue.type);
            if (itemClass == null)
            {
                continue;
            }

            if (itemClass.HasQuality)
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6);
            else
                itemStack.count = itemClass.Stacknumber.Value;

            inventory.SetItem(i, itemStack);
            if (i == 0)
                inventory.SetHoldingItemIdx(i);
        }
    }

    public override void OnUpdateLive()
    {
        // Comprehensive initialization checks to prevent collision system errors
        // Check basic null references first
        if (inventory == null || world == null)
        {
            return;
        }

        // Check player stats initialization
        if (!bPlayerStatsChanged)
        {
            return;
        }

        // Check if entity is properly spawned
        if (!IsSpawned())
        {
            return;
        }

        // Check physics components
        if (PhysicsTransform == null || emodel == null)
        {
            return;
        }

        // Check if the entity's root transform is valid
        if (RootTransform == null || !RootTransform.gameObject.activeInHierarchy)
        {
            return;
        }

        // Additional safety: wait a few frames after spawn to ensure full initialization
        if (!_isFullyInitialized)
        {
            _initializationFrameCount++;
            // Wait at least 3 frames for all systems to initialize
            if (_initializationFrameCount < 3)
            {
                return;
            }
            _isFullyInitialized = true;
        }

        // Now safe to call base update which includes collision checks
        try
        {
            base.OnUpdateLive();
        }
        catch (NullReferenceException ex)
        {
            // Log the error but don't crash - entity might still be initializing
            Log.Warning($"EntityNPCBandit.OnUpdateLive NullRef for {entityName} ({entityId}): {ex.Message}");
            // Reset initialization to try again next frame
            _isFullyInitialized = false;
            _initializationFrameCount = 0;
            return;
        }

        // Wake them up if they are sleeping, since the trigger sleeper makes them go idle again.
        if (!sleepingOrWakingUp && IsAlwaysAwake)
        {
            IsSleeping = true;
            ConditionalTriggerSleeperWakeUp();
        }

        // Potential work around for NPC stuck for 3 seconds in crouch after being stunned
        // bodyDamage is a struct so no null check needed
        if (bodyDamage.CurrentStun is EnumEntityStunType.Getup or EnumEntityStunType.Prone)
        {
            SetHeight(this.physicsBaseHeight);
        }
    }

    public override void OnAddedToWorld()
    {
        base.OnAddedToWorld();

        // Reset initialization tracking when added to world
        _isFullyInitialized = false;
        _initializationFrameCount = 0;

        if (IsAlwaysAwake)
        {
            // Set in EntityAlive.TriggerSleeperPose() - resetting here
            IsSleeping = false;
        }

        // Safety check before showing holding item
        if (inventory != null)
        {
            this.ShowHoldingItem(true);
        }
    }

    public override bool IsSavedToFile()
    {
        // Safety check for Buffs
        if (Buffs == null)
        {
            return base.IsSavedToFile();
        }

        // Has a leader cvar set, good enough, as the leader may already be disconnected,
        // so we'll fail a GetLeaderOrOwner()
        if (Buffs.HasCustomVar("Leader"))
            return true;

        // If they have a cvar persist, keep them around.
        if (Buffs.HasCustomVar("Persist"))
            return true;

        return base.IsSavedToFile();
    }

    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();

        var entityClass = EntityClass.list[base.entityClass];
        if (entityClass == null)
        {
            return;
        }

        IsAlwaysAwake = false;
        if (entityClass.Properties?.Values != null)
        {
            if (entityClass.Properties.Values.ContainsKey("SleeperInstantAwake"))
                IsAlwaysAwake = StringParsers.ParseBool(entityClass.Properties.Values["SleeperInstantAwake"]);

            if (entityClass.Properties.Values.ContainsKey("IsAlwaysAwake"))
                IsAlwaysAwake = StringParsers.ParseBool(entityClass.Properties.Values["IsAlwaysAwake"]);
        }
    }

    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        try
        {
            var strPatrol = _br.ReadString();
            _patrolCoordinates.Clear();
            _tempList.Clear();

            if (!string.IsNullOrEmpty(strPatrol))
            {
                foreach (var strPatrolPoint in strPatrol.Split(';'))
                {
                    if (string.IsNullOrEmpty(strPatrolPoint))
                        continue;

                    var position = ModGeneralUtilities.StringToVector3(strPatrolPoint);
                    if (position != Vector3.zero)
                        UpdatePatrolPoints(position); // this method also updates _tempList
                }
            }

            _guardPosition = ModGeneralUtilities.StringToVector3(_br.ReadString());
            _guardLookPosition = ModGeneralUtilities.StringToVector3(_br.ReadString());

            if (Buffs != null)
            {
                Buffs.Read(_br);
            }
        }
        catch (Exception ex)
        {
            Log.Out($"Read exception for: {entityName} ( {entityId} ) : {ex}");
        }
    }

    public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
    {
        base.Write(_bw, _bNetworkWrite);
        try
        {
            var strPatrolCoordinates = "";
            foreach (var coordinate in _patrolCoordinates)
                strPatrolCoordinates += ";" + coordinate;
            _bw.Write(strPatrolCoordinates);

            _bw.Write(_guardPosition.ToString());
            _bw.Write(_guardLookPosition.ToString());

            if (Buffs != null)
            {
                Buffs.Write(_bw);
            }
        }
        catch (Exception ex)
        {
            Log.Out($"Write exception for: {entityName} ( {entityId} ) : {ex}");
        }
    }

    /// <inheritdoc/>
    public void SetupAutoPathingBlocks()
    {
        // Safety check for Buffs
        if (Buffs == null)
        {
            return;
        }

        // If we already have a pathing code, don't re-scan.
        if (Buffs.HasCustomVar("PathingCode") &&
            (Buffs.GetCustomVar("PathingCode") < 0 || Buffs.GetCustomVar("PathingCode") > 0))
            return;

        // Check if pathing blocks are defined.
        var blocks = EntityUtilities.ConfigureEntityClass(entityId, "PathingBlocks");
        if (blocks == null || blocks.Count == 0)
            blocks = new List<string> { "PathingCube", "PathingCube2" };

        // Scan for the blocks in the area
        var pathingVectors = ModGeneralUtilities.ScanAutoConfigurationBlocks(position, blocks, 2);
        if (pathingVectors == null || pathingVectors.Count == 0)
            return;

        // Find the nearest block, and if its a sign, read its code.
        var target = ModGeneralUtilities.FindNearestBlock(position, pathingVectors);

        // Safety check for GameManager and World
        if (GameManager.Instance?.World == null)
        {
            return;
        }

        if (GameManager.Instance.World.GetTileEntity(0, new Vector3i(target))
            is not TileEntitySign tileEntitySign)
            return;

        // Safety check for sign text
        if (tileEntitySign.signText == null)
        {
            return;
        }

        var text = tileEntitySign.signText.Text;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // We need to apply the buffs during this scan,
        // as the creation of the entity + adding buffs is not really MP safe.
        var task = PathingCubeParser.GetValue(text, "task");
        if (!string.IsNullOrEmpty(task))
        {
            Log.Out($"SetupAutoPathingBlocks for: {entityName} ( {entityId} ) : Task: {task}");
            switch (task.ToLower())
            {
                case "stay":
                    Buffs.AddBuff("buffOrderStay", -1, false);
                    break;
                case "wander":
                    Buffs.AddBuff("buffOrderWander", -1, false);
                    break;
                case "guard":
                    // Use the buff that issues the "guard" order, not the one that issues the "stay" order
                    Buffs.AddBuff("buffOrderGuard", -1, false);
                    break;
                // This entity can't accept the "follow" task.
                default:
                    Log.Out($"    Entity: {entityName} ( {entityId} ) : Cannot perform task: {task}");
                    break;
            }
        }

        var buffs = PathingCubeParser.GetValue(text, "buff");
        if (!string.IsNullOrEmpty(buffs))
        {
            foreach (var b in buffs.Split(','))
            {
                var buff = b.Trim();
                if (!string.IsNullOrEmpty(buff))
                    Buffs.AddBuff(buff, -1, false);
            }
        }

        // Set up the pathing code.
        Buffs.SetCustomVar("PathingCode", -1f);

        var pc = PathingCubeParser.GetValue(text, "pc");
        if (!string.IsNullOrEmpty(pc) && StringParsers.TryParseFloat(pc, out var pathingCode))
        {
            Buffs.SetCustomVar("PathingCode", pathingCode);
        }
    }

    /// <inheritdoc/>
    public void UpdatePatrolPoints(Vector3 position)
    {
        // Center the x and z values of the passed in blocks for a unique check.
        var temp = EntityUtilities.CenterPosition(position);

        if (!_tempList.Contains(temp))
        {
            _tempList.Add(temp);
            if (!_patrolCoordinates.Contains(position))
                _patrolCoordinates.Add(position);
        }
    }
}
