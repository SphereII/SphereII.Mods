using System.Xml;

public class MinEventActionAddFireDamage : MinEventActionRemoveBuff
{

//  		<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore"  />

    public override void Execute(MinEventParams _params)
    {

        if (Voxel.voxelRayHitInfo.bHitValid)
        {
            var hitInfo = Voxel.voxelRayHitInfo;
            if (hitInfo == null) return;

            FireManager.Instance.Add(hitInfo.hit.blockPos);
        }
    }

}