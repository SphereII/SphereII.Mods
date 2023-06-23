using Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Block that allows you to control the water pipes and systems in the enhanced farming features.
/// </summary>
public class BlockCropControlPanel : Block
{
    private string controlPanelName = "CropControlPanel";
    private string activateSound;

    /// <summary>
    /// Default control options for the Control panel:
    /// Enable Debugging
    /// Disable Debugging
    /// turnonWater: Allows water to flow past
    /// turnoffWater: Blocks water from flowing past
    /// </summary>
    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("debugcontrol_enable", "electric_switch", true, false),
        new BlockActivationCommand("debugcontrol_disable", "electric_switch", true, false),
        new BlockActivationCommand("debugcontrol_turnonWater", "electric_switch", true, false),
        new BlockActivationCommand("debugcontrol_turnoffWater", "electric_switch", true, false),
        new BlockActivationCommand("debugcontrol_startbot", "electric_switch", true, false),
    };

    public static List<BlockValue> GetNeighborByName(Vector3i _blockPos, string blockName)
    {
        List<BlockValue> list = new List<BlockValue>();
        var neighbors = BlockCropControlPanel.GetNeighbors(_blockPos);
        foreach( var neighbor in neighbors)
        {
            if ( neighbor.Block.GetBlockName().ToLower()== blockName.ToLower())
                list.Add(neighbor);
        }
        return list;
    }
    public static List<BlockValue> GetNeighbors(Vector3i _blockPos)
    {
        var neighbors = new List<BlockValue>();
        foreach (var direction in Vector3i.AllDirections)
        {
            var position = _blockPos + direction;
            var blockValue = GameManager.Instance.World.GetBlock(position);
            neighbors.Add(blockValue);
        }
        return neighbors;
    }
    
    /// <summary>
    /// Reads in the following Properties from the blocks.xml entry:
    /// 
    /// ControlPanelName: Used for display purposes
    /// ActivateSound: The sound used to activate the block
    /// </summary>
    public override void Init()
    {
        base.Init();
        this.Properties.ParseString("ControlPanelName", ref controlPanelName);
        this.Properties.ParseString("ActivateSound", ref this.activateSound);
    }

    public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
    {
        return base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck);
    }
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();

        var neighbor = BlockCropControlPanel.GetNeighborByName(_blockPos, "controlPanelTop01");
        if (neighbor.Count > 0)
            localizedBlockName = $"Enhanced {localizedBlockName}";
        return localizedBlockName;
    }
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        ((Chunk)_world.ChunkClusters[_clrIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));
        this.cmds[0].enabled = !CropManager.Instance.DebugMode;
        this.cmds[1].enabled = CropManager.Instance.DebugMode;

        var neighbor = BlockCropControlPanel.GetNeighborByName(_blockPos, "controlPanelTop01");
        this.cmds[2].enabled = neighbor.Count > 0;
        this.cmds[3].enabled = neighbor.Count > 0;

        neighbor = BlockCropControlPanel.GetNeighborByName(_blockPos, "decoClassicDogHouse");
        this.cmds[4].enabled = neighbor.Count > 0;
        return this.cmds;
    }

    public void UpdateValves(Vector3i _blockPos, bool turnOn)
    {
        foreach (var direction in Vector3i.AllDirections)
        {
            var position = _blockPos + direction;
            var blockValue = GameManager.Instance.World.GetBlock(position);
            if (blockValue.Block is BlockWaterPipeSDX)
                WaterPipeManager.Instance.ToggleWaterValve(_blockPos, position, turnOn);
        }

        WaterPipeManager.Instance.ClearPipes();

        WaterPipeManager.Instance.GetWaterForPosition(_blockPos);
    }
    public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {

        switch (_commandName)
        {
            case "debugcontrol_enable":
                CropManager.Instance.DebugMode = true;
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound, 0f);
                GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("debugcontrol_turnon"));
                break;
            case "debugcontrol_enable_disable":
                CropManager.Instance.DebugMode = false;
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound, 0f);
                GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("debugcontrol_turnoff"));
                break;
            case "debugcontrol_turnonWater":
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound, 0f);
                UpdateValves(_blockPos, true);
                GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("debugcontrol_turnonWater"));
                break;
            case "debugcontrol_turnoffWater":
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound, 0f);
                UpdateValves(_blockPos, false);
                GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("debugcontrol_turnoffWater"));
                break;

            case "debugcontrol_startbot":
                Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound, 0f);
                GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("debugcontrol_startingbot"));
                break;

        }
        return false;
    }


}

