using System.Xml.Linq;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Spreads fire over a wider area than a single block.
/// </summary>
/// <remarks>
/// This effect can allow fire to spread to to surrounding blocks. You can set up a filter based on the type of blocks you wanted affected.
/// Example:
///     <!-- The same type of block -->
///    <triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Type" />
///
///    <!-- Shares the same material -->
///    <triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Material" />
///
///    <!-- Shares the same material damage classification -->
///    <triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialDamage" />
///
///    <!-- Shares the same material surface classification -->
///    <triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialSurface" />
/// </remarks>
[UsedImplicitly]
public class MinEventActionAddFireDamageCascade : MinEventActionRemoveBuff
{
    private enum FilterTypeCascade
    {
        Type,
        Material,
        MaterialDamage,
        MaterialSurface
    }

    private FilterTypeCascade _filterType = FilterTypeCascade.Type;
 
    public override void Execute(MinEventParams @params)
    {
        var position = @params.Position;
        if (targetType != TargetTypes.positionAOE)
        {
            if (Voxel.voxelRayHitInfo.bHitValid)
            {
                var hitInfo = Voxel.voxelRayHitInfo;
                if (hitInfo == null) return;
                position = hitInfo.hit.blockPos;
            }
        }
        SpreadFire(position);
    }

    private void SpreadFire(Vector3 position)
    {
        var targetBlock = GameManager.Instance.World.GetBlock(new Vector3i(position));

        var range = (int) maxRange;
        for (var x = -range; x <= range; x++)
        {
            for (var z = -range; z <= range; z++)
            {
                for (var y = -range; y <= range; y++)
                {
                    var vector = new Vector3i(position.x + x, position.y + y, position.z + z);
                    var neighborBlock = GameManager.Instance.World.GetBlock(vector);
                    switch (_filterType)
                    {
                        case FilterTypeCascade.Type:
                            if (neighborBlock.type == targetBlock.type)
                                FireManager.Instance.Add(vector);
                            break;
                        case FilterTypeCascade.Material:
                            if (neighborBlock.Block.blockMaterial.id == targetBlock.Block.blockMaterial.id)
                                FireManager.Instance.Add(vector);
                            break;
                        case FilterTypeCascade.MaterialDamage:
                            if (neighborBlock.Block.blockMaterial.DamageCategory ==
                                targetBlock.Block.blockMaterial.DamageCategory)
                                FireManager.Instance.Add(vector);
                            break;
                        case FilterTypeCascade.MaterialSurface:
                            if (neighborBlock.Block.blockMaterial.SurfaceCategory ==
                                targetBlock.Block.blockMaterial.SurfaceCategory)
                                FireManager.Instance.Add(vector);
                            break;
                    }
                }
            }
        }
    }


    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag) return true;
        var name = attribute.Name.LocalName;
        if (name != "filter") return false;
        _filterType = EnumUtils.Parse<FilterTypeCascade>(attribute.Value, true);
        return true;
    }
}