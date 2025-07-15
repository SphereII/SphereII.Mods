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
    /// <summary>
    /// Updates particle systems based on new fire positions,
    /// optimizing by removing particles next to others.
    /// </summary>
    /// <param name="currentFireBlockPositions">The set of all blocks currently on fire.</param>
public void UpdateAndOptimizeFireParticles(Dictionary<Vector3i, BlockValue> fireMap, string fireParticle) 
{
    // --- Step 1: Clean up particles that are no longer associated with a fire block ---
    // Iterate over a copy of currently active particle positions to avoid modifying collection during iteration
    List<Vector3i> particlesToDeactivate = new List<Vector3i>();
    foreach (var activeParticlePos in activeFireParticleObjects) // activeFireParticleObjects is HashSet now
    {
        if (!fireMap.ContainsKey(activeParticlePos))
        {
            // This particle is at a position no longer reported as on fire.
            particlesToDeactivate.Add(activeParticlePos);
        }
    }

    foreach (var pos in particlesToDeactivate)
    {
        BlockUtilitiesSDX.removeParticles(pos);
        activeFireParticleObjects.Remove(pos); // Remove from our tracking
    }

    // --- Step 2: Add new particles and remove redundant ones within current fire regions ---
    // Create a temporary set to hold the positions where particles *should* exist after optimization.
    // This allows us to determine what to add and what to remove more cleanly.
    HashSet<Vector3i> optimalParticlePositions = new HashSet<Vector3i>();

    // Sort the fire positions to ensure deterministic culling (important for 'lexicographically smaller' rule)
    // Or just iterate them as they come if the culling rule handles arbitrary order.
    // Iterating the dictionary keys directly is fine for this if GetNeighborOffsets is consistent.
    foreach (var firePos in fireMap.Keys) 
    {
        // If we already decided to place a particle here, skip.
        if (optimalParticlePositions.Contains(firePos)) continue;

        // Check if this fire position is redundant to an already selected optimal particle position.
        bool isRedundantToOptimal = false;
        foreach (var neighborOffset in GetNeighborOffsets(_cullDistance))
        {
            Vector3i neighborPos = firePos + neighborOffset;
            if (neighborOffset == Vector3i.zero) continue; 
            
            // Check against positions already *selected as optimal* for this frame.
            if (optimalParticlePositions.Contains(neighborPos))
            {
                isRedundantToOptimal = true;
                break;
            }
        }

        if (!isRedundantToOptimal)
        {
            // This position is a good candidate for a particle. Add it to our optimal set.
            optimalParticlePositions.Add(firePos);
        }
    }

    // --- Step 3: Synchronize active particles with the optimal set ---

    // Find particles to remove (those in activeFireParticleObjects but not in optimalParticlePositions)
    var toRemove = activeFireParticleObjects.Except(optimalParticlePositions).ToList();
    foreach (var pos in toRemove)
    {
        BlockUtilitiesSDX.removeParticles(pos);
        activeFireParticleObjects.Remove(pos);
    }

    // Find particles to add (those in optimalParticlePositions but not in activeFireParticleObjects)
    var toAdd = optimalParticlePositions.Except(activeFireParticleObjects).ToList();
    foreach (var pos in toAdd)
    {
        BlockUtilitiesSDX.addParticles(fireParticle, pos);
        activeFireParticleObjects.Add(pos);
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
