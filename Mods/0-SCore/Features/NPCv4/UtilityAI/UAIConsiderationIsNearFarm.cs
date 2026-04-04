using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationIsNearFarmV4 : UAIConsiderationTargetType
    {
        private int distance = 50;

        // Per-entity result cache: avoids running 6 FarmPlotManager queries every frame.
        // Keyed by entity ID; value is (score, Time.time when cached).
        // All AI ticks are main-thread so a static Dictionary is safe.
        private static readonly Dictionary<int, (float score, float time)> _cache
            = new Dictionary<int, (float, float)>();
        private const float CacheTtl = 1f; // seconds

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("distance"))
                StringParsers.TryParseSInt32(parameters["distance"], out distance, 0, -1, NumberStyles.Integer);
        }

        public override float GetScore(Context _context, object target)
        {
            int entityId = _context.Self.entityId;
            float now    = Time.time;

            if (_cache.TryGetValue(entityId, out var cached) && now - cached.time < CacheTtl)
                return cached.score;

            float score = EvaluateScore(_context);
            _cache[entityId] = (score, now);
            return score;
        }

        private float EvaluateScore(Context _context)
        {
            var position = new Vector3i(_context.Self.position);

            if (FarmPlotManager.Instance.GetFarmPlotsNearby(position) != null)                          return 1f;
            if (FarmPlotManager.Instance.GetClosesUnmaintained(position, distance) != null)              return 1f;
            if (FarmPlotManager.Instance.GetFarmPlotsNearbyWithPlants(position) != null)                 return 1f;
            if (FarmPlotManager.Instance.GetClosesUnmaintainedWithPlants(position, distance) != null)    return 1f;
            if (FarmPlotManager.Instance.GetClosePositions(position, distance).Count > 0)               return 1f;
            if (FarmPlotManager.Instance.GetCloseFarmPlotsWilted(position, distance).Count > 0)         return 1f;

            FarmPlotManager.Instance.ResetPlantsInRange(position, distance);
            return 0f;
        }
    }
}