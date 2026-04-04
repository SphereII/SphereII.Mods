using Audio;
using System.Collections.Generic;
using UnityEngine;

// Refactored version of Farming/Scripts/Blocks/Water Blocks/BlockWaterSourceSDX.cs
// State stored in static dictionaries keyed by block position.
public class BlockWaterSourceSDX : BlockBaseWaterSystem
{
    private float _waterRange = 5f;
    private string waterType = "Limited";
    private static readonly int IsSprinklerOn = Animator.StringToHash("isSprinklerOn");
    private bool _muteSprinklerSound = false;
    private string activateSound = "electric_switch"; // Default activation sound

    // --- State Storage ---
    private static Dictionary<Vector3i, bool?> _connectionStatusCache = new Dictionary<Vector3i, bool?>();
    private static Dictionary<Vector3i, bool?> _manualOverrideStates = new Dictionary<Vector3i, bool?>();

    private static bool? GetConnectionStatus(Vector3i pos) => _connectionStatusCache.GetValueOrDefault(pos, null);
    private static void SetConnectionStatus(Vector3i pos, bool? status) => _connectionStatusCache[pos] = status;
    private static void ClearConnectionStatus(Vector3i pos) => _connectionStatusCache.Remove(pos);

    private static bool? GetManualOverride(Vector3i pos) => _manualOverrideStates.GetValueOrDefault(pos, null);
    private static void SetManualOverride(Vector3i pos, bool? status) => _manualOverrideStates[pos] = status;
    private static void ClearManualOverride(Vector3i pos) => _manualOverrideStates.Remove(pos);
    // --- End State Storage ---


    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("sprinkler_manual_on", "electric_switch", true, false),
        new BlockActivationCommand("sprinkler_manual_off", "electric_switch", true, false),
        new BlockActivationCommand("sprinkler_auto_mode", "electric_switch", true, false)
    };


    public override void LateInit()
    {
        base.LateInit();
        if (this.Properties.Values.ContainsKey("WaterRange"))
            _waterRange = StringParsers.ParseFloat(this.Properties.Values["WaterRange"]);

        if (Properties.Values.ContainsKey("WaterType"))
            waterType = Properties.Values["WaterType"];

        if (this.Properties.Values.ContainsKey("MuteSound"))
            _muteSprinklerSound = StringParsers.ParseBool(this.Properties.Values["MuteSound"]);

        this.Properties.ParseString("ActivateSound", ref this.activateSound);
    }

    public bool IsWaterSourceUnlimited()
    {
        return waterType.ToLower() == "unlimited";
    }

    public float GetWaterRange()
    {
        return _waterRange;
    }

    private static bool HasAdjacentPipe(Vector3i blockPos)
    {
        var world = GameManager.Instance.World;
        Vector3i[] neighbors = {
            blockPos + Vector3i.up, blockPos + Vector3i.down,
            blockPos + Vector3i.left, blockPos + Vector3i.right,
            blockPos + Vector3i.forward, blockPos + Vector3i.back
        };
        foreach (var neighbor in neighbors)
        {
            if (world.GetBlock(neighbor).Block is BlockWaterPipeSDX)
                return true;
        }
        return false;
    }

    public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
    {
        if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
            return false;

        // Unlimited source sprinklers are self-contained — no pipe connection required
        if (IsWaterSourceUnlimited())
            return true;

        if (!CropManager.Instance.RequirePipesForSprinklers)
            return true;

        // Require at least one adjacent pipe block — water doesn't need to be
        // connected yet, allowing pipes to be laid before the water source is placed.
        return HasAdjacentPipe(_blockPos);
    }

    // --- Block Lifecycle & State Cleanup ---
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        WaterPipeManager.Instance.RemoveValve(_blockPos);
        StopSprinklerSound(_blockPos);
        ClearConnectionStatus(_blockPos);
        ClearManualOverride(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        StopSprinklerSound(_blockPos);
        ClearConnectionStatus(_blockPos);
        ClearManualOverride(_blockPos);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        if (_blockValue.ischild) return;

        ClearConnectionStatus(_blockPos);
        ClearManualOverride(_blockPos);

        var chunkCluster = _world.ChunkClusters[_clrIdx];
        if (chunkCluster == null) return;

        var chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
        if (chunk == null) return;
        chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true
        });
        if (!_world.IsRemote())
        {
            _world.GetWBT().AddScheduledBlockUpdate(chunk.ClrIdx, _blockPos, this.blockID, this.GetTickRate());
        }

        WaterPipeManager.Instance.AddValve(_blockPos);
    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue,
        PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);

        ClearConnectionStatus(_blockPos);
        ClearManualOverride(_blockPos);

        _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true
        });
        if (!_world.IsRemote())
        {
            _world.GetWBT().AddScheduledBlockUpdate(_chunk.ClrIdx, _blockPos, this.blockID, this.GetTickRate());
        }

        WaterPipeManager.Instance.AddValve(_blockPos);
    }

    public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos,
        BlockValue _myBlockValue,
        Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
    {
        base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged,
            _newNeighborBlockValue, _oldNeighborBlockValue);
        RefreshAllSprinklers(_myBlockPos);
    }
    // --- End Block Lifecycle & State Cleanup ---


    private void StopSprinklerSound(Vector3i _blockPos)
    {
        var ebcd = GameManager.Instance.World.GetChunkFromWorldPos(_blockPos)?.GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null) return;
        var audioSource = ebcd.transform.GetComponentInChildren<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx,
        Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        return true;
    }

    // --- Activation Command Logic ---
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue,
        int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        bool currentStateIsOn = false;
        bool? currentManualOverride = GetManualOverride(_blockPos);
        bool? currentConnectionStatus = GetConnectionStatus(_blockPos);

        if (currentManualOverride.HasValue)
        {
            currentStateIsOn = currentManualOverride.Value;
        }
        else
        {
            bool actualConnection = CheckWaterConnection(_blockPos);
            if (currentConnectionStatus == null || currentConnectionStatus.Value != actualConnection)
            {
                currentConnectionStatus = actualConnection;
                SetConnectionStatus(_blockPos, currentConnectionStatus);
            }
            currentStateIsOn = currentConnectionStatus.Value;
        }

        this.cmds[0].enabled = !currentStateIsOn; // Manual ON only if currently OFF
        this.cmds[1].enabled = currentStateIsOn;  // Manual OFF only if currently ON
        this.cmds[2].enabled = currentManualOverride.HasValue; // Auto only if override active

        return this.cmds;
    }

    public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos,
        BlockValue _blockValue, EntityPlayerLocal _player)
    {
        bool stateChanged = false;
        bool finalState = false;
        bool? newManualState = null;

        switch (_commandName)
        {
            case "sprinkler_manual_on":
                var currentConnectionStatus = CheckWaterConnection(_blockPos);
                SetConnectionStatus(_blockPos, currentConnectionStatus);
                if (currentConnectionStatus)
                {
                    newManualState = true;
                    finalState = true;
                    stateChanged = true;
                    GameManager.ShowTooltip(_player, Localization.Get("sprinkler_tooltip_manual_on"));
                }
                break;

            case "sprinkler_manual_off":
                newManualState = false;
                finalState = false;
                stateChanged = true;
                GameManager.ShowTooltip(_player, Localization.Get("sprinkler_tooltip_manual_off"));
                break;

            case "sprinkler_auto_mode":
                SetManualOverride(_blockPos, null);
                SetConnectionStatus(_blockPos, null);
                GameManager.ShowTooltip(_player, Localization.Get("sprinkler_tooltip_auto_mode"));
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound);
                return true;
        }

        if (stateChanged)
        {
            SetManualOverride(_blockPos, newManualState);
            ToggleSprinkler(_blockPos, finalState);
            WaterPipeManager.Sync(_blockPos, finalState);
            Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound);
            return true;
        }

        return false;
    }
    // --- End Activation Command Logic ---


    public BlockEntityData GetBlockEntity(Vector3i position, WorldBase world = null)
    {
        var w = world ?? GameManager.Instance.World;
        return w.GetChunkFromWorldPos(position)?.GetBlockEntity(position);
    }

    public void ToggleSprinkler(Vector3i _blockPos, bool enabled = true, WorldBase world = null)
    {
        var ebcd = GetBlockEntity(_blockPos, world);
        if (ebcd == null || ebcd.transform == null) return;

        var animator = ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return;

        animator.SetBool(IsSprinklerOn, enabled);
        if (!enabled || _muteSprinklerSound)
            StopSprinklerSound(_blockPos);
    }

    private bool CheckWaterConnection(Vector3i _blockPos)
    {
        var waterSource = WaterPipeManager.Instance.GetWaterForPosition(_blockPos);
        return waterSource != Vector3i.zero && waterSource != _blockPos;
    }

    public override ulong GetTickRate()
    {
        return (ulong)10f;
    }

    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue,
        bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        _world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, this.blockID, this.GetTickRate());
        RefreshSprinkler(_blockPos);
        return true;
    }

    /// <summary>
    /// Computes the current on/off state for the sprinkler and syncs it if changed.
    /// </summary>
    public static void RefreshSprinkler(Vector3i pos)
    {
        var world = GameManager.Instance.World;
        if (world == null) return;

        var blockValue = world.GetBlock(pos);
        if (!(blockValue.Block is BlockWaterSourceSDX sprinkler)) return;

        bool? manualOverride = GetManualOverride(pos);
        bool shouldBeOn = manualOverride.HasValue ? manualOverride.Value : sprinkler.CheckWaterConnection(pos);

        bool? cached = GetConnectionStatus(pos);
        if (cached.HasValue && cached.Value == shouldBeOn) return;

        SetConnectionStatus(pos, shouldBeOn);

        if (world.IsRemote())
            sprinkler.ToggleSprinkler(pos, shouldBeOn);
        else
            WaterPipeManager.Sync(pos, shouldBeOn);
    }

    /// <summary>
    /// Invalidates the water cache near a changed position and schedules an immediate tick
    /// for every registered sprinkler so they re-evaluate their connection state.
    /// </summary>
    public static void RefreshAllSprinklers(Vector3i changedPos)
    {
        WaterPipeManager.Instance.InvalidateWaterCacheNear(changedPos);
        var world = GameManager.Instance.World;
        var valves = WaterPipeManager.Instance.GetWaterValves();

        if (valves == null || world == null || world.IsRemote()) return;

        foreach (var sprinklerPos in valves)
        {
            SetConnectionStatus(sprinklerPos, null);
            var chunk = (Chunk)world.GetChunkFromWorldPos(sprinklerPos);
            if (chunk == null) continue;
            world.GetWBT().AddScheduledBlockUpdate(chunk.ClrIdx, sprinklerPos,
                world.GetBlock(sprinklerPos).type, 1);
        }
    }

    /// <summary>
    /// Forces a re-check on the next UpdateTick for the given sprinkler position.
    /// </summary>
    public static void InvalidateConnectionCache(Vector3i pos)
    {
        SetConnectionStatus(pos, null);
    }
}
