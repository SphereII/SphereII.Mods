
using UnityEngine;

namespace UAI
{

    // This consideration is unique for a reason. If the NPC is allowed to scan for 10 targets to base all its considerations on, the player, depending on how many other entities are around, may be 11th.
    // As such, it would never evaluate if the npc has a leader, which needs to take priority over all other targets.
    // Thus, this consideration will be true if the NPC has a leader, regardless of if its close by.
    public class UAIConsiderationTargetLeader : UAIConsiderationBase
    {
     //  private readonly string value = "Leader";

        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            var _leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId) as EntityAlive;
            if (_leader == null)
                return 0f;

            if (_leader.entityId == targetEntity.entityId)
                return 1f;

            return 0f;
        }
    }
}