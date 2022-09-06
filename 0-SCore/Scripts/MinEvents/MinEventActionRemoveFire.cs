using System.Xml;

public class MinEventActionRemoveFire : MinEventActionRemoveBuff
{
    private static readonly string AdvFeatureClass = "FireManagement";

    // <triggered_effect trigger="onSelfDamagedBlock" action="RemoveFire, SCore" target="positionAOE" range="5"/> <!-- range is int -->

    public override void Execute(MinEventParams _params)
    {
        if (FireManager.Instance == null) return;
        if (FireManager.Instance.Enabled == false) return;

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
        AdvLogging.DisplayLog(AdvFeatureClass, $"Executing RemoveFire() at {position}  Self: {_params.Self.position} Range: {maxRange}");

        int range = (int)maxRange;
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                for (int y = -range; y <= range; y++)
                {
                    var vector = new Vector3i(position.x + x, position.y + y, position.z + z);
                    if (FireManager.Instance.isBurning(vector))
                    {
                        FireManager.Instance.Remove(vector);
                        FireManager.Instance.Extinguish(vector);
                    }

                }
            }
        }


    }

}