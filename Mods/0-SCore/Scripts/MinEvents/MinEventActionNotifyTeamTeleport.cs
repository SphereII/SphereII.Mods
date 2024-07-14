//        <triggered_effect trigger = "onSelfBuffUpdate" action="NotifyTeamTeleport, SCore" />

using System.Collections.Generic;

public class MinEventActionNotifyTeamTeleport : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var leader = _params.Self as EntityPlayer;
        if (leader == null) return;

        foreach (var cvar in leader.Buffs.CVars)
        {
            if (cvar.Key.StartsWith("hired_"))
            {
                var entity = GameManager.Instance.World.GetEntity((int)cvar.Value) as EntityAliveSDX;
                if (entity)
                {
                    if (entity.IsDead()) continue;

                    var distance = entity.GetDistance(leader);
                    if (distance > 60)
                    {
                        switch (EntityUtilities.GetCurrentOrder(entity.entityId))
                        {
                            case EntityUtilities.Orders.Loot:
                            case EntityUtilities.Orders.Follow:
                                entity.TeleportToPlayer(leader, true);
                                break;
                            case EntityUtilities.Orders.Stay:
                            case EntityUtilities.Orders.Wander:
                            default:
                                break;
                        }
                    }

                }

            }
        }
    }
}