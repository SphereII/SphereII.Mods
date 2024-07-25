using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockPlantGrowingSDX : BlockPlantGrowing
{
    public static Dictionary<Vector3i, Vector3i> WaterSources = new Dictionary<Vector3i, Vector3i>();
    private bool requireWater = false;
    private int waterRange = 5;
    private bool willWilt = false;

    protected BlockValue wiltedPlant = BlockValue.Air;
    public override void LateInit()
    {
        base.LateInit();
        if (this.Properties.Values.ContainsKey("RequireWater"))
            this.requireWater = StringParsers.ParseBool(this.Properties.Values["RequireWater"]);

        if (this.Properties.Values.ContainsKey("Wilt"))
            this.willWilt = StringParsers.ParseBool(this.Properties.Values["Wilt"]);

        if (this.Properties.Values.ContainsKey("WaterRange"))
            this.waterRange = int.Parse(this.Properties.Values["WaterRange"]);

        if (this.Properties.Values.ContainsKey("PlantGrowing.Wilt"))
            this.wiltedPlant = ItemClass.GetItem(this.Properties.Values["PlantGrowing.Wilt"], false).ToBlockValue();
    }


    // Records the plant when placed as a seed
    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        base.PlaceBlock(_world, _result, _ea);
        CropManager.Instance.Add(_result.blockPos, waterRange);
    }

    // Checks the preview if the plant can actually go there.
    public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
    {
        if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
            return false;

        if (requireWater == false) return true;
        return CropManager.Instance.IsNearWater(_blockPos, waterRange);
    }

    // When chunk is loaded, force add the block. This will be valiated on the update check in the crop manager, but
    // the assumption here is if the block was there to begin with, it's allowed.
    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        CropManager.Instance.ForceAdd(_blockPos);
    }

    // Remove from the map when unloading
    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        CropManager.Instance.Remove(_blockPos);
    }

    // When the block is added
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        CropManager.Instance.Add(_blockPos, waterRange);
    }

    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        if (requireWater)
        {
            var plantData = CropManager.Instance.GetPlant(_blockPos);
            if (plantData != null && plantData.CanStay() == false)
            {
                // This Removes unregisters the block if it cannot stay, such as if it can't find water, etc.
                // It'll call CheckPlantAlive() and re-scan for water before it dies.
                plantData.Remove();
                return false;
            }
        }

        return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
    }
    public override bool CheckPlantAlive(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        // Allow the plant to follow the basic rules.
        var result = base.CheckPlantAlive(_world, _clrIdx, _blockPos, _blockValue);
        if (result == false) return false;

        // check if we are near a water source.
        if (CropManager.Instance.IsNearWater(_blockPos, waterRange)) return true;

        // Only wilt if the property is set.
        if (willWilt)
        {
            Wilt(_world, _clrIdx, _blockPos, _blockValue);
            return false;
        }
        return true;

    }

    public void Wilt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        // Handle wilting.
        _blockValue.type = this.wiltedPlant.type;
        BiomeDefinition biome = ((World)_world).GetBiome(_blockPos.x, _blockPos.z);
        if (biome != null && biome.Replacements.ContainsKey(_blockValue.type))
            _blockValue.type = biome.Replacements[_blockValue.type];
        BlockValue blockValue = BlockPlaceholderMap.Instance.Replace(_blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z, false);
        blockValue.rotation = _blockValue.rotation;
        blockValue.meta = _blockValue.meta;
        blockValue.meta2 = 0;
        _blockValue = blockValue;
        _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
    }
}

