using Audio;
using System;
using System.Collections.Generic;
//using System.Linq; // Not currently used
//using System.Text; // Not currently used
//using System.Threading.Tasks; // Not currently used
using UnityEngine;

// Updated BlockCropControlPanel
public class BlockCropControlPanel : Block
{
    private string controlPanelName = "CropControlPanel";
    private string activateSound = "electric_switch"; // Default sound

    // Updated: Removed water valve commands
    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("debugcontrol_enable", "electric_switch", true, false),  // Toggle CropManager Debug ON
        new BlockActivationCommand("debugcontrol_disable", "electric_switch", true, false), // Toggle CropManager Debug OFF
        // Water commands removed as ToggleWaterValve no longer exists
        // new BlockActivationCommand("debugcontrol_turnonWater", "electric_switch", true, false),
        // new BlockActivationCommand("debugcontrol_turnoffWater", "electric_switch", true, false),
        new BlockActivationCommand("debugcontrol_startbot", "electric_switch", true, false), // Example bot command
    };

    // Helper to get neighbors by name (Unchanged)
    public static List<BlockValue> GetNeighborByName(Vector3i _blockPos, string blockName)
    {
        List<BlockValue> list = new List<BlockValue>();
        // Use World.GetBlock instead of static GetNeighbors if appropriate
        var world = GameManager.Instance.World;
        foreach (var direction in Vector3i.AllDirections)
        {
            var neighborPos = _blockPos + direction;
            var blockValue = world.GetBlock(neighborPos);
            // Check block name safely
            if (blockValue.Block != null && blockValue.Block.GetBlockName().Equals(blockName, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(blockValue);
            }
        }
        return list;
    }

    // GetNeighbors helper likely not needed if using GetNeighborByName
    /*
    public static List<BlockValue> GetNeighbors(Vector3i _blockPos)
    {
        var neighbors = new List<BlockValue>();
        var world = GameManager.Instance.World;
        foreach (var direction in Vector3i.AllDirections)
        {
            var position = _blockPos + direction;
            neighbors.Add(world.GetBlock(position));
        }
        return neighbors;
    }
    */

    public override void Init()
    {
        base.Init();
        this.Properties.ParseString("ControlPanelName", ref controlPanelName);
        this.Properties.ParseString("ActivateSound", ref this.activateSound);
    }

    // CanPlaceBlockAt remains unchanged unless specific placement rules needed
    // public override bool CanPlaceBlockAt(...) { ... }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();

        // Example enhancement based on neighbor (unchanged)
        var neighbor = GetNeighborByName(_blockPos, "controlPanelTop01");
        if (neighbor.Count > 0)
            localizedBlockName = $"Enhanced {localizedBlockName}";

        return localizedBlockName;
    }

    // Updated: Removed logic for water commands
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        // Chunk and trigger logic might be specific to game/modding framework (kept for context)
        // ((Chunk)_world.ChunkClusters[_clrIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));

        // Toggle Debug Mode commands based on CropManager state
        this.cmds[0].enabled = !CropManager.Instance.DebugMode; // Enable "ON" if currently OFF
        this.cmds[1].enabled = CropManager.Instance.DebugMode;  // Enable "OFF" if currently ON

        // Water commands removed (Indices 2, 3 are now the bot command)
        // var neighbor = GetNeighborByName(_blockPos, "controlPanelTop01"); // No longer needed for water cmds
        // this.cmds[2].enabled = neighbor.Count > 0; // Logic removed
        // this.cmds[3].enabled = neighbor.Count > 0; // Logic removed

        // Example: Enable bot command based on adjacent dog house (Index is now 2)
        var dogHouseNeighbor = GetNeighborByName(_blockPos, "decoClassicDogHouse");
        this.cmds[2].enabled = dogHouseNeighbor.Count > 0;

        return this.cmds;
    }

    // Removed: UpdateValves method is obsolete
    /*
    public void UpdateValves(Vector3i _blockPos, bool turnOn)
    {
        // This logic is no longer valid as ToggleWaterValve was removed.
        // foreach (var direction in Vector3i.AllDirections) { ... }
        // WaterPipeManager.Instance.InvalidateWaterCache(); // Renamed from ClearPipes
        // WaterPipeManager.Instance.GetWaterForPosition(_blockPos); // Recalculate (done lazily now)
    }
    */

    // Updated: Removed cases for water commands
    public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
    {
        switch (_commandName)
        {
            case "debugcontrol_enable":
                CropManager.Instance.DebugMode = true;
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound);
                GameManager.ShowTooltip(_player, Localization.Get("debugcontrol_turnon")); // Ensure localization exists
                return true; // Indicate command was handled

            case "debugcontrol_disable": // Corrected command name from original code example
                CropManager.Instance.DebugMode = false;
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound);
                GameManager.ShowTooltip(_player, Localization.Get("debugcontrol_turnoff")); // Ensure localization exists
                return true; // Indicate command was handled

            // Cases for debugcontrol_turnonWater / debugcontrol_turnoffWater removed

            case "debugcontrol_startbot":
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound);
                GameManager.ShowTooltip(_player, Localization.Get("debugcontrol_startingbot")); // Ensure localization exists
                // Add actual bot starting logic here
                return true; // Indicate command was handled
        }
        return false; // Command not handled
    }

} // End of BlockCropControlPanel class