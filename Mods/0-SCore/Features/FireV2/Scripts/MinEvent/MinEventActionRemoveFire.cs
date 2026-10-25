using JetBrains.Annotations;

/// <summary>
/// Remove Fire within the specified range, using the specified target.
/// </summary>
/// <remarks>
/// Example:
///     This will remove fire from all blocks with a range of 5 from the position of the target.
///     <triggered_effect trigger="onSelfDamagedBlock" action="RemoveFire, SCore" target="positionAOE" range="5"/> 
/// </remarks>
[UsedImplicitly]
public class MinEventActionRemoveFire : MinEventActionRemoveBuff
{
    private static readonly string AdvFeatureClass = "FireManagement";

    public override void Execute(MinEventParams @params)
    {
        if (FireManager.Instance == null) return;
        if (FireManager.Instance.Enabled == false) return;

        var position = @params.Position;
        var entityId = @params.Self.entityId;
        if (targetType != TargetTypes.positionAOE)
        {
            if (Voxel.voxelRayHitInfo.bHitValid)
            {
                var hitInfo = Voxel.voxelRayHitInfo;
                if (hitInfo == null) return;
                position = hitInfo.hit.blockPos;
            }
        }
        AdvLogging.DisplayLog(AdvFeatureClass, $"Executing RemoveFire() at {position}  Self: {@params.Self.position} Range: {maxRange}");

        // The server decides which fires are in range: a client's copy of the fire map can be missing
        // fires, which left them burning on dedicated servers.
        FireManager.Instance.ExtinguishArea(World.worldToBlockPos(position), (int)maxRange, entityId);


    }

}