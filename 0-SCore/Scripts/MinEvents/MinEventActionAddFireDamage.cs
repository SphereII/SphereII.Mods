using System.Xml;

public class MinEventActionAddFireDamage : MinEventActionRemoveBuff
{

    //  		<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" target="positionAOE" range="5" />

    public override void Execute(MinEventParams _params)
    {
        var position = _params.Position;
        if (targetType != TargetTypes.positionAOE)
        {
            if (Voxel.voxelRayHitInfo.bHitValid)
            {
                var hitInfo = Voxel.voxelRayHitInfo;
                if (hitInfo == null) return;
                position = hitInfo.hit.blockPos;
            }
        }
        int range = (int)maxRange;
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                for (int y = -range; y <= range; y++)
                {
                    var vector = new Vector3i(position.x + x, position.y + y, position.z + z);
                    FireManager.Instance.Add(vector);

                }
            }
        }
    }

}