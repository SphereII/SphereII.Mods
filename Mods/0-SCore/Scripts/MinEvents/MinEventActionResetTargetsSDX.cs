using UnityEngine;

public class MinEventActionResetTargetsSDX : MinEventActionRemoveBuff
{

    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i];
            if (entity != null)
            {
                entity.SetAttackTarget(null, 0);
                entity.SetRevengeTarget(null);
            }
        }
    }
}