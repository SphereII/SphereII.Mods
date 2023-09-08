public class BlockBaseWaterSystem : Block
{
    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("debugcontrol_enable", "electric_switch", false, false),
    };

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        this.cmds[0].enabled = false;
        return this.cmds;
    }

 
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var localPlayer = _entityFocusing as EntityPlayerLocal;
           
        if (localPlayer == null) return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);

        if (localPlayer.playerInput.PermanentActions.Activate.IsPressed || localPlayer.playerInput.Activate.IsPressed)
        {
            return WaterPipeManager.Instance.GetWaterSummary(_blockPos);
        }
        return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
    }

    public override string GetCustomDescription(Vector3i _blockPos, BlockValue _bv)
    {
        var localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
        if (localPlayer == null) return base.GetCustomDescription(_blockPos, _bv);
        
        if (localPlayer.playerInput.PermanentActions.Activate.IsPressed || localPlayer.playerInput.Activate.IsPressed)
        {
            return WaterPipeManager.Instance.GetWaterSummary(_blockPos);
        }
        return base.GetCustomDescription(_blockPos, _bv);
    }
}

