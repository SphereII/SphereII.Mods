using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Shared helper for land-claim checks used by both the farming task and the consideration.
///
/// Uses PersistentPlayerList.m_lpBlockMap — the game's own authoritative dictionary that maps
/// every active keystone position to its owner.  No chunk walking required; the map is kept
/// current by PlaceLandProtectionBlock / RemoveLandProtectionBlock.
///
/// Pass a non-null <paramref name="ownerFilter"/> to restrict to a specific player's claims.
/// </summary>
public static class FarmLandClaimUtils
{
    public static bool IsPlotInLandClaim(
        Vector3i plotPos,
        PersistentPlayerData ownerFilter = null)
    {
        int claimSize = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize);
        if (claimSize <= 0) claimSize = 41;

        var persistentList = GameManager.Instance?.GetPersistentPlayerList();
        if (persistentList?.m_lpBlockMap == null) return false;

        foreach (var entry in persistentList.m_lpBlockMap)
        {
            // Axis-aligned distance check — same logic as World.IsLandProtectedBlock.
            if (Math.Abs(entry.Key.x - plotPos.x) > claimSize) continue;
            if (Math.Abs(entry.Key.z - plotPos.z) > claimSize) continue;

            if (ownerFilter != null && entry.Value != ownerFilter) continue;

            return true;
        }

        return false;
    }
}

namespace UAI
{
    /// <summary>
    /// Returns 0f when <see cref="UAIConsiderationFarmPlotInLandClaimV4"/> would return 1f.
    /// XML: &lt;consideration class="NotFarmPlotInLandClaim, SCore" distance="50" /&gt;
    /// </summary>
    public class UAIConsiderationNotFarmPlotInLandClaimV4 : UAIConsiderationFarmPlotInLandClaimV4
    {
        public override float GetScore(Context _context, object target)
            => base.GetScore(_context, target) == 1f ? 0f : 1f;
    }

    /// <summary>
    /// Returns 1f if at least one unvisited farm plot within <c>distance</c> blocks is inside
    /// any active land claim.
    ///
    /// XML:
    ///   &lt;consideration class="FarmPlotInLandClaim, SCore" distance="50" /&gt;
    ///
    /// General-purpose gate for behaviors that should only fire near settled land.
    /// The per-entity 2-second cache keeps evaluation cost negligible.
    /// </summary>
    public class UAIConsiderationFarmPlotInLandClaimV4 : UAIConsiderationBase
    {
        private int _distance = 50;

        private static readonly Dictionary<int, (float score, float time)> _cache
            = new Dictionary<int, (float, float)>();
        private const float CacheTtl = 2f;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("distance"))
                StringParsers.TryParseSInt32(parameters["distance"], out _distance, 0, -1, NumberStyles.Integer);
        }

        public override float GetScore(Context _context, object target)
        {
            int id = _context.Self.entityId;
            float now = Time.time;

            if (_cache.TryGetValue(id, out var cached) && now - cached.time < CacheTtl)
                return cached.score;

            float score = EvaluateScore(_context);
            _cache[id] = (score, now);
            return score;
        }

        private float EvaluateScore(Context _context)
        {
            var npcPos = new Vector3i(_context.Self.position);
            if (FarmPlotManager.Instance == null) return 0f;

            foreach (var plot in FarmPlotManager.Instance.GetCloseEntry(npcPos, _distance))
            {
                if (plot.Visited) continue;
                if (FarmLandClaimUtils.IsPlotInLandClaim(plot.GetBlockPos()))
                    return 1f;
            }

            return 0f;
        }
    }
}
