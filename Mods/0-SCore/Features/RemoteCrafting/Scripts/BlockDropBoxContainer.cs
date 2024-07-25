using SCore.Features.RemoteCrafting.Scripts;
using UnityEngine;

public class BlockDropBoxContainer : BlockSecureLootSigned
{
    private float _distance;
    private float _updateTime;
    public override void Init()
    {
        base.Init();
        var strDistance = Configuration.GetPropertyValue("AdvancedRecipes", "Distance");
        _distance = 30f;
        if (!string.IsNullOrEmpty(strDistance))
            _distance = StringParsers.ParseFloat(strDistance);
        Properties.ParseFloat("Distance", ref _distance);

        _updateTime = 100UL;
        Properties.ParseFloat("UpdateTick", ref _updateTime);

    }

    public override ulong GetTickRate()
    {
        return (ulong)_updateTime;
    }
    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
        if (!world.IsRemote())
        {
            world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, GetTickRate());
        }
    }

    public override bool UpdateTick(WorldBase world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue,
        bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        var tileLootContainer = (TileEntityLootContainer) world.GetTileEntity(_clrIdx, _blockPos);
        if (tileLootContainer == null) return false;

        if (!tileLootContainer.IsUserAccessing())
        {
            var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            foreach (var itemStack in tileLootContainer.GetItems())
            {
                if (itemStack.IsEmpty()) continue;
                // If we successfully added, clear the stack.
                if (RemoteCraftingUtils.AddToNearbyContainer(primaryPlayer, itemStack, _distance))
                    itemStack.Clear();
            }
            tileLootContainer.bTouched = true;
            tileLootContainer.SetModified();
        }

        world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, GetTickRate());
        return true;
    }
}