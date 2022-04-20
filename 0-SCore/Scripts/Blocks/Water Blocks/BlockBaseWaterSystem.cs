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
        if (CropManager.Instance.DebugMode)
        {
            var result = true;
            var waterSource = WaterPipeManager.Instance.GetWaterForPosition(_blockPos);
            if (waterSource == Vector3i.zero)
                result = false;
            return $"{Localization.Get("has_water")}: {result}";
        }
        return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
    }

}

