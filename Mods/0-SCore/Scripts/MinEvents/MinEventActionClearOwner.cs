using Platform;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
public class MinEventActionClearOwner : MinEventActionRemoveBuff
{
    //  		<triggered_effect trigger="onSelfDamagedBlock" action="ClearOwner, SCore" />
    public override void Execute(MinEventParams _params)
    {
        if (Voxel.voxelRayHitInfo.bHitValid)
        {
            var hitInfo = Voxel.voxelRayHitInfo;
            if (hitInfo == null) return;
            var position = hitInfo.hit.blockPos;
            var tileEntity = GameManager.Instance.World.GetTileEntity(0, position) as TileEntityPoweredTrigger;
            if (tileEntity == null) return;

            tileEntity.SetOwner(null);
        }
    }
}