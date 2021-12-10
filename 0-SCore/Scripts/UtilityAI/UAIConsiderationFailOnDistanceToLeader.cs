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

            // if the leader is too far away, then fail this consideration, aborting the task.
            var distanceToLeader = Vector3.Distance(_context.Self.position, leader.position);
            if (distanceToLeader > _maxDistance)
                return 0f;

            return 1f;
        }
    }
}