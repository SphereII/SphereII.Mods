//        <triggered_effect trigger = "onSelfBuffUpdate" action="NotifyTeamTeleportNow, SCore" />

using System.Collections.Generic;

public class MinEventActionTeamTeleportNow : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var leader = _params.Self as EntityPlayer;
        if (leader == null) return;

        foreach (var cvar in leader.Buffs.CVars)
        {
            if (!cvar.Key.StartsWith("hired_")) continue;
            var entity = GameManager.Instance.World.GetEntity((int)cvar.Value) as EntityAliveSDX;
            if (!entity) continue;
            if (entity.IsDead()) continue;

            var distance = entity.GetDistance(leader);
            if (distance > 50) continue;

            // If the npc is guarding, don't follow this order.
            if (entity.Buffs.GetCustomVar("Guarding") > 0)
            {
                var display = $"{entity.EntityName} :: {Localization.Get("xuiSCoreGuarding")}";
                GameManager.ShowTooltip((EntityPlayerLocal)leader, display);
                continue;
            }
            
            switch (EntityUtilities.GetCurrentOrder(entity.entityId))
            {
                case EntityUtilities.Orders.Loot:
                case EntityUtilities.Orders.Follow:
                case EntityUtilities.Orders.Stay:
                    EntityUtilities.TeleportNow(entity.entityId, leader, 50);
                    break;
                case EntityUtilities.Orders.Guard:
                case EntityUtilities.Orders.Wander:
                default:
                    break;
            }
        }
    }
}