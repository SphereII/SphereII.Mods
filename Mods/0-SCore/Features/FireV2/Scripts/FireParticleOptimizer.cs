using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For .ToList() if you iterate and modify

public class FireParticleOptimizer
{
    // Changed from List to HashSet for O(1) lookups/removals
    public HashSet<Vector3i> activeFireParticleObjects = new HashSet<Vector3i>();


    // A small margin to prevent floating point issues if using floats instead of Vector3i
    private const float CULL_EPSILON = 0.1f;

    // A value of 1 means direct neighbors (face-adjacent)
    // A value of 2 means even diagonal neighbors across a corner, etc.
    private int _cullDistance;

    public FireParticleOptimizer(int cullDistance = 1) // Provide a default or require it
    {
        _cullDistance = cullDistance;
    }

    public void UpdateCullDistance(int cullDistance)
    {
        _cullDistance = cullDistance;
    }

    public void UpdateAndOptimizeFireParticlesRoutine(Dictionary<Vector3i, FireBlockData> fireMap, FireConfig fireConfig)
    {
        // *** Step 1: DETERMINE THE NEW OPTIMAL PARTICLE POSITIONS (Deterministic Culling) ***
        var newOptimalParticlePositions = new HashSet<Vector3i>();

        // This loop ensures we check every current fire block position.
        foreach (var currentPos in fireMap.Keys)
        {
            bool shouldPlaceParticle = true;

            // Check if a neighbor *of this fire block* is a "better" candidate for the particle.
            // A "better" candidate must be an *active fire block* and satisfy the tie-breaker rule.
            foreach (var neighborOffset in GetNeighborOffsets(_cullDistance))
            {
                Vector3i neighborPos = currentPos + neighborOffset;

                // Skip self and non-fire positions
                if (neighborOffset == Vector3i.zero || !fireMap.ContainsKey(neighborPos)) continue;

                // If a neighbor is lexicographically smaller (the tie-breaker you defined), 
                // the neighbor is the 'chosen' one for the particle, and we should NOT place a particle here.
                if (IsPositionLexicographicallySmaller(neighborPos, currentPos))
                {
                 //   shouldPlaceParticle = false;
                    break; // Neighbor is 'better', so currentPos is redundant.
                }
            }

            if (shouldPlaceParticle)
            {
                newOptimalParticlePositions.Add(currentPos);
            }
        }


        // *** Step 2: APPLY CHANGES (Add/Remove particles based on optimal set) ***

        // A. Identify and Remove particles that are no longer optimal (culling or fire burned out)
        // Compare old active set with new optimal set.
        var particlesToDeactivate = activeFireParticleObjects
            .Except(newOptimalParticlePositions)
            .ToList();

        foreach (var pos in particlesToDeactivate)
        {
            BlockUtilitiesSDX.removeParticles(pos);
            activeFireParticleObjects.Remove(pos);
        }

        // B. Identify and Add particles for new optimal positions
        // Compare new optimal set with old active set.
        var particlesToActivate = newOptimalParticlePositions
            .Except(activeFireParticleObjects)
            .ToList();

        foreach (var currentPos in particlesToActivate)
        {
            // Add Particle (including configuration logic)
            var particle = fireConfig.FireParticle;
            if (fireMap.TryGetValue(currentPos, out var data))
            {
                particle = data.FireParticle;
            }

            BlockUtilitiesSDX.addParticles(particle, currentPos);
            activeFireParticleObjects.Add(currentPos);
        }

        Log.Out($"Optimized Particles: Fires: {fireMap.Count} : Particles: {activeFireParticleObjects.Count}");
    }

    /// <summary>
    /// Helper to generate offsets for neighbors within a given Manhattan distance.
    /// </summary>
    private IEnumerable<Vector3i> GetNeighborOffsets(int distance)
    {
        for (int x = -distance; x <= distance; x++)
        {
            for (int y = -distance; y <= distance; y++)
            {
                for (int z = -distance; z <= distance; z++)
                {
                    // Only consider positions within the specified Manhattan distance
                    if (Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z) <= distance)
                    {
                        yield return new Vector3i(x, y, z);
                    }
                }
            }
        }
    }

    // Helper to establish a consistent ordering for culling
    private bool IsPositionLexicographicallySmaller(Vector3i a, Vector3i b)
    {
        if (a.x < b.x) return true;
        if (a.x > b.x) return false;
        if (a.y < b.y) return true;
        if (a.y > b.y) return false;
        return a.z < b.z;
    }
}