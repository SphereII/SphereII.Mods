//        <triggered_effect trigger = "onSelfBuffUpdate" action="NotifyTeamAttack, SCore" target="selfAOE" range="4" />

public class MinEventActionNotifyTeamAttack : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        // Debug.Log("NotifyTeam Of Attack");
        // Grab the leader ID of the entity.
        var Leader = EntityUtilities.GetCVarValue(_params.Self.entityId, "Leader");
        if (Leader == 0f)
            return;

        for (var j = 0; j < targets.Count; j++)
        {
            // Check the entity to see if its leader matches the current one.
            var myLeader = EntityUtilities.GetCVarValue(targets[j].entityId, "Leader");
            if (myLeader == Leader)
                // Debug.Log(" Entity has the same Leader as me: " + targets[j].EntityName);
                // the current entity doesn't have an attack target, give them one.
                if (EntityUtilities.GetAttackOrRevengeTarget(targets[j].entityId) == null)
                    //  Debug.Log(" Sharing my Revenge Target with the team.");
                    targets[j].SetAttackTarget(EntityUtilities.GetAttackOrRevengeTarget(_params.Self.entityId) as EntityAlive, 500);
        }
    }
}