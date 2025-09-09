using Audio;
using System.Collections.Generic; // Needed for Dictionary
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
    // Static dictionaries to store state per block position
    // NOTE: Assumes block updates are single-threaded or access is appropriately managed.
    private static Dictionary<Vector3i, bool?> _connectionStatusCache = new Dictionary<Vector3i, bool?>();
    private static Dictionary<Vector3i, bool?> _manualOverrideStates = new Dictionary<Vector3i, bool?>();

    // Helper methods to access state dictionaries safely
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

    public bool IsWaterSourceUnlimited() {
        return waterType.ToLower() == "unlimited";
    }
    public float GetWaterRange()
    {
        return _waterRange;
    }

    // --- Block Lifecycle & State Cleanup ---
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        WaterPipeManager.Instance.RemoveValve(_blockPos);
        StopSprinklerSound(_blockPos);
        // Clean up state for this position
        ClearConnectionStatus(_blockPos);
        ClearManualOverride(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue) {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        StopSprinklerSound(_blockPos);
        // Clean up state for this position
        ClearConnectionStatus(_blockPos);
        ClearManualOverride(_blockPos);
    }

     public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        if (_blockValue.ischild) return;

        // Explicitly clear state for loaded blocks to ensure fresh checks? Or rely on GetValueOrDefault?
        // Let's clear to be safe and force initial check.
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

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue,  PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);

        // Ensure state is clear when block is first added
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

    public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue,
                                           Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
    {
        base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
        // Invalidate the connection status cache for this specific block position
         SetConnectionStatus(_myBlockPos, null); // Set to null to force re-check in UpdateTick
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

    public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        return true;
    }
    // --- Activation Command Logic ---
     public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
         // Determine the current effective state (ON or OFF) for THIS block position
         bool currentStateIsOn = false;
         bool? currentManualOverride = GetManualOverride(_blockPos);
         bool? currentConnectionStatus = GetConnectionStatus(_blockPos);


         if (currentManualOverride.HasValue)
         {
             // Manual override is active
             currentStateIsOn = currentManualOverride.Value;
         }
         else
         {
             // Automatic mode - check cached/actual connection
             if (currentConnectionStatus == null)
             {
                 // Status not determined yet for this block, check it now for UI
                 currentConnectionStatus = CheckWaterConnection(_blockPos);
                 // Cache the status temporarily for this check
                 SetConnectionStatus(_blockPos, currentConnectionStatus);
             }
             currentStateIsOn = currentConnectionStatus ?? false; // Default to off if still somehow null
         }

         // Enable/disable commands based on state
         this.cmds[0].enabled = !currentStateIsOn; // Enable "Manual ON" only if currently OFF
         this.cmds[1].enabled = currentStateIsOn;  // Enable "Manual OFF" only if currently ON
         this.cmds[2].enabled = currentManualOverride.HasValue; // Enable "Automatic Mode" only if override is active

        return this.cmds;
    }

      public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
     {
         bool stateChanged = false;
         bool finalState = false;
         bool? newManualState = null; // Use null to signify clearing override

         switch (_commandName)
         {
             case "sprinkler_manual_on":
                 newManualState = true;
                 finalState = true;
                 stateChanged = true;
                 GameManager.ShowTooltip(_player, Localization.Get("sprinkler_tooltip_manual_on"));
                 break;

             case "sprinkler_manual_off":
                 newManualState = false;
                 finalState = false;
                 stateChanged = true;
                 GameManager.ShowTooltip(_player, Localization.Get("sprinkler_tooltip_manual_off"));
                 break;

            case "sprinkler_auto_mode":
                newManualState = null; // Signal to clear override
                // Don't set finalState here, let UpdateTick determine it
                SetManualOverride(_blockPos, null); // Update state immediately
                SetConnectionStatus(_blockPos, null); // Force re-check in next tick
                GameManager.ShowTooltip(_player, Localization.Get("sprinkler_tooltip_auto_mode"));
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound);
                return true; // Activation processed

         }

         if (stateChanged)
         {
             SetManualOverride(_blockPos, newManualState); // Update the state for this block pos
             ToggleSprinkler(_blockPos, finalState); // Update visual state immediately
             WaterPipeManager.Sync(_blockPos, finalState); // Sync the new manual state
             Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound);
             return true; // Activation processed
         }

         return false; // Command not handled here
     }
    // --- End Activation Command Logic ---


    public BlockEntityData GetBlockEntity(Vector3i position) {
        return GameManager.Instance.World.GetChunkFromWorldPos(position)?.GetBlockEntity(position);
    }

    public void ToggleSprinkler(Vector3i _blockPos, bool enabled = true) {
        var ebcd = GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null) return ;

        var animator = ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return ;
        if (enabled)
        {
            animator.SetBool(IsSprinklerOn, true);
        }
        else
        {
            animator.SetBool(IsSprinklerOn, false);
            StopSprinklerSound(_blockPos);
        }
        if ( _muteSprinklerSound)
            StopSprinklerSound(_blockPos);
    }

    // Check connection specific to the block position
    private bool CheckWaterConnection(Vector3i _blockPos)
    {
        if (!CropManager.Instance.RequirePipesForSprinklers)
        {
            return true;
        }

        Vector3i[] adjacentPositions = new Vector3i[]
        {
            _blockPos + Vector3i.forward, _blockPos + Vector3i.back,
            _blockPos + Vector3i.left, _blockPos + Vector3i.right,
            _blockPos + Vector3i.down
        };

        foreach (var pos in adjacentPositions)
        {
            BlockValue adjacentBlock = GameManager.Instance.World.GetBlock(pos);
             if (adjacentBlock.Block is BlockWaterPipeSDX || adjacentBlock.Block is BlockWaterSourceSDX || WaterPipeManager.Instance.IsDirectWaterSource(pos))
             {
                var waterSource = WaterPipeManager.Instance.GetWaterForPosition(pos);
                if (waterSource != Vector3i.zero) return true;
             }
        }
        return false;
    }

    public override ulong GetTickRate()
    {
        return (ulong)10f;
    }

    // UpdateTick now uses the block position to manage state
    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd) {
        _world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, this.blockID, this.GetTickRate());

        bool shouldBeOn = false;
        bool stateDetermined = false;
        bool? currentManualOverride = GetManualOverride(_blockPos);
        bool? currentConnectionStatus = GetConnectionStatus(_blockPos);

        if (currentManualOverride.HasValue)
        {
            shouldBeOn = currentManualOverride.Value;
            stateDetermined = true;
        }
        else
        {
            if (currentConnectionStatus == null)
            {
                currentConnectionStatus = CheckWaterConnection(_blockPos);
                SetConnectionStatus(_blockPos, currentConnectionStatus); // Cache the result
                 // Sync state immediately after re-evaluation ONLY if not manually overridden
                 WaterPipeManager.Sync(_blockPos, currentConnectionStatus.Value);
            }
            shouldBeOn = currentConnectionStatus ?? false;
            stateDetermined = true;
        }

        if (stateDetermined)
        {
            ToggleSprinkler(_blockPos, shouldBeOn);
        }

        return true;
    }
    
    /// <summary>
    /// Public static method to invalidate the connection status cache for a specific block position.
    /// This forces a recheck on the next UpdateTick for that sprinkler.
    /// </summary>
    /// <param name="pos">The position of the sprinkler block.</param>
    public static void InvalidateConnectionCache(Vector3i pos)
    {
        // Setting the status to null forces a re-check in UpdateTick
        SetConnectionStatus(pos, null);

        // Optional: Could also trigger an immediate block update if the game engine supports it,
        //           otherwise it relies on the next scheduled UpdateTick.
        // Example: GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(....);
    }

 
}