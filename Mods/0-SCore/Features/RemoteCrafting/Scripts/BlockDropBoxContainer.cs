using SCore.Features.RemoteCrafting.Scripts;
using UnityEngine;

public class BlockDropBoxContainer : BlockCompositeTileEntity
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
    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue,  PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
        if (!world.IsRemote())
        {
            world.GetWBT().AddScheduledBlockUpdate(_blockPos, this.blockID, GetTickRate());
        }
    }

    public override bool UpdateTick(WorldBase world, Vector3i _blockPos, BlockValue _blockValue,
        bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        var tileEntity = world.GetTileEntity(_blockPos) as TileEntityComposite;
        if (tileEntity == null) return false;

        if (!tileEntity.IsUserAccessing())
        {
            var lootable = tileEntity.GetFeature<TEFeatureStorage>();
            if (lootable != null)
            {
                // Sort around the box itself, for the box's owner. This used to search around
                // GetPrimaryPlayer(): on a P2P host that is the host, so every player's box emptied
                // into the host's containers; on a dedicated server it is null and nothing sorted.
                var items = lootable.ItemGrid.items;
                if (items != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i].IsEmpty()) continue;
                        if (RemoteCraftingUtils.AddToContainersNearBox(tileEntity, items[i], _distance))
                            items[i] = ItemStack.Empty.Clone();
                    }
                }
                lootable.ItemGrid.Touch();
                lootable.SetModified();
            }
        }

        world.GetWBT().AddScheduledBlockUpdate(_blockPos, this.blockID, GetTickRate());
        return true;
    }
}