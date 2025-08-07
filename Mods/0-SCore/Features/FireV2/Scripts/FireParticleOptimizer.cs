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
    /// <summary>
    /// Updates particle systems based on new fire positions,
    /// optimizing by removing particles next to others.
    /// </summary>
    /// <param name="currentFireBlockPositions">The set of all blocks currently on fire.</param>
 public void UpdateAndOptimizeFireParticlesRoutine(Dictionary<Vector3i, FireBlockData> fireMap, FireConfig fireConfig)
    {
        // Step 1: Clean up particles that are no longer associated with a fire block
        // This is still done in one go as it's typically less resource-intensive.
        var particlesToDeactivate = activeFireParticleObjects.Where(pos => !fireMap.ContainsKey(pos)).ToList();
        foreach (var pos in particlesToDeactivate)
        {
            BlockUtilitiesSDX.removeParticles(pos);
            activeFireParticleObjects.Remove(pos);
        }

        // Step 2: Add new particles and remove redundant ones within current fire regions
        var firePositionsToProcess = new Queue<Vector3i>(fireMap.Keys.Except(activeFireParticleObjects));
       
        while (firePositionsToProcess.Count > 0)
        {
            var currentPos = firePositionsToProcess.Dequeue();
            
            // Check if this fire position is redundant to an already selected optimal particle position.
            bool isRedundantToOptimal = false;
            foreach (var neighborOffset in GetNeighborOffsets(_cullDistance))
            {
                Vector3i neighborPos = currentPos + neighborOffset;
                if (neighborOffset == Vector3i.zero) continue;

                if (activeFireParticleObjects.Contains(neighborPos))
                {
                    isRedundantToOptimal = true;
                    break;
                }
            }

            if (!isRedundantToOptimal)
            {
                
                var particle = fireConfig.FireParticle;
                if (fireMap.TryGetValue(currentPos, out var data))
                {
                //    Debug.Log($"Default Particle: {particle}  Position Particle: {data.FireParticle}");
                    particle = data.FireParticle;
                    
                }

                // This position is a good candidate for a particle. Add it.
                BlockUtilitiesSDX.addParticles(particle, currentPos);
                activeFireParticleObjects.Add(currentPos);
            }
            
       
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
