using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationFailOnDistanceToLeader : UAIConsiderationBase
    {
        private float _maxDistance = 20f;
        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("max_distance"))
                _maxDistance = StringParsers.ParseFloat(parameters["max_distance"], 0, -1, NumberStyles.Any);
        }

        public override float GetScore(Context _context, object target)
        {
            // If we don't have a leader, pass the consideration
            var leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId);
            if (leader == null)
                return 1f;

            // The cvars override the XML value for this NPC only. This consideration is shared by
            // every NPC using the package, so the override must not be written back to the field.
            var maxDistance = _maxDistance;

            // If the player has a fail on distance, use that.
            var distance = EntityUtilities.GetCVarValue(leader.entityId, "FailOnDistance");
            if (distance > 0)
                maxDistance = distance;

            // If the entity has a fail on distance, use that.
            distance = EntityUtilities.GetCVarValue(_context.Self.entityId, "FailOnDistance");
            if (distance > 0)
                maxDistance = distance;

            // if the leader is too far away, then fail this consideration, aborting the task.
            var distanceToLeader = Vector3.Distance(_context.Self.position, leader.position);
            if (distanceToLeader > maxDistance)
                return 0f;

            // If the target is a position, then just pass it. Otherwise, we want to see if it'll take it out of the range.
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 1f;

            // If its dead, don't bother.
            if (targetEntity.IsDead()) return 0f;
            
            // if our target will take us out of range of the player, don't bother going for it.
            var entityPosition = Vector3.Distance(_context.Self.position, targetEntity.position);
            if (entityPosition > maxDistance)
                return 0f;
            
            return 1f;
        }
    }
}