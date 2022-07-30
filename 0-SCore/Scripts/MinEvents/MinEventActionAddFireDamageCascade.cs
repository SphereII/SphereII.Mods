using System.Xml;

public class MinEventActionAddFireDamageCascade : MinEventActionRemoveBuff
{

    enum FilterTypeCascade
    {
        Type,
        Material,
        MaterialDamage,
        MaterialSurface
    }

    private FilterTypeCascade filterType = FilterTypeCascade.Type;
    /*
				<!-- The same type of block -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Type" />

				<!-- Shares the same material -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Material" />

				<!-- Shares the same material damage classification -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialDamage" />

				<!-- Shares the same material surface classification -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialSurface" />
    */
    public override void Execute(MinEventParams _params)
    {

        if (Voxel.voxelRayHitInfo.bHitValid)
        {
            var hitInfo = Voxel.voxelRayHitInfo;
            if (hitInfo == null) return;

            var position = hitInfo.hit.blockPos;
            var targetBlock = GameManager.Instance.World.GetBlock(position);

            int range = (int)maxRange;
            for (int x = -range; x <= range; x++)
            {
                for (int z = -range; z <= range; z++)
                {
                    for (int y = -range; y <= range; y++)
                    {
                        var vector = new Vector3i(position.x + x, position.y + y, position.z + z);
                        var neighborBlock = GameManager.Instance.World.GetBlock(vector);
                        switch( filterType)
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
                                if (neighborBlock.Block.blockMaterial.DamageCategory == targetBlock.Block.blockMaterial.DamageCategory)
                                    FireManager.Instance.Add(vector);
                                break;
                            case FilterTypeCascade.MaterialSurface:
                                if (neighborBlock.Block.blockMaterial.SurfaceCategory == targetBlock.Block.blockMaterial.SurfaceCategory)
                                    FireManager.Instance.Add(vector);
                                break;
                            default:
                                break;

                        }

                    }
                }
            }
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "filter" )
                {
                    filterType = EnumUtils.Parse<FilterTypeCascade>(_attribute.Value, true);
                    return true;
                }
            }
        }
        return flag;
    }
}