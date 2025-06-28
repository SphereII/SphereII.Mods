using System; // Required for Tuple
using System.Collections.Generic;
using UnityEngine;

// Refactored PipeData using Iterative BFS.
// Incorporates neighbor finding logic previously in the separate Pipe class.
public class PipeData
{
    private readonly Vector3i _startBlockPos; // Store starting pos for context if needed
    private readonly int _maxPipes = 50;    // Max search depth/count

    /// <summary>
    /// Constructor, mainly sets the search limit.
    /// </summary>
    /// <param name="startBlockPos">The initial position where the search originates.</param>
    /// <param name="maxPipeCount">Max number of pipe segments to check.</param>
    public PipeData(Vector3i startBlockPos, int maxPipeCount = -1)
    {
        this._startBlockPos = startBlockPos;
        this._maxPipes = (maxPipeCount == -1)
            ? WaterPipeManager.Instance.GetMaxPipeCount() // Get from manager
            : maxPipeCount;                               // Use provided value
    }

    /// <summary>
    /// Iteratively searches for the nearest connected water source using Breadth-First Search (BFS).
    /// </summary>
    /// <param name="startPosition">The position to start searching from (pipe or block needing water).</param>
    /// <returns>The position of the found water source, or Vector3i.zero if none found within limits.</returns>
    public Vector3i DiscoverWaterFromPipes(Vector3i startPosition)
    {
        Queue<Vector3i> queue = new Queue<Vector3i>();
        HashSet<Vector3i> visited = new HashSet<Vector3i>();
        int pipeCount = 0;

        // Start BFS from the initial position
        queue.Enqueue(startPosition);
        visited.Add(startPosition);

        while (queue.Count > 0 && pipeCount < _maxPipes)
        {
            Vector3i currentPos = queue.Dequeue();
            pipeCount++; // Count nodes processed

            // Get neighbors (water sources and pipes) for the current position
            var (pipeNeighbors, waterSourceNeighbors) = GetNeighbors(currentPos);

            // Check if any neighbor is a direct water source
            if (waterSourceNeighbors.Count > 0)
            {
                // Found the closest direct water source(s) via this path
                return waterSourceNeighbors[0]; // Return the first one found
            }

            // Add unvisited pipe neighbors to the queue
            foreach (Vector3i pipeNeighborPos in pipeNeighbors)
            {
                if (!visited.Contains(pipeNeighborPos))
                {
                    visited.Add(pipeNeighborPos);
                    queue.Enqueue(pipeNeighborPos);
                }
            }
        }

        // No water source found within the maximum pipe count limit
        return Vector3i.zero;
    }


    /// <summary>
    /// Static helper to get lists of adjacent pipe and direct water source neighbors.
    /// </summary>
    /// <param name="position">The position to check neighbors for.</param>
    /// <returns>A Tuple containing two lists: (List of pipe neighbors, List of direct water source neighbors).</returns>
    private static Tuple<List<Vector3i>, List<Vector3i>> GetNeighbors(Vector3i position)
    {
        List<Vector3i> pipeNeighbors = new List<Vector3i>();
        List<Vector3i> waterSourceNeighbors = new List<Vector3i>();
        var world = GameManager.Instance.World; // Cache world instance

        foreach (var direction in Vector3i.AllDirections)
        {
            var neighborPos = position + direction;

            // TODO: Refine valve checking logic if needed based on implementation details.

            BlockValue blockValue = world.GetBlock(neighborPos);

            // Check for direct water source first
            if (WaterPipeManager.Instance.IsDirectWaterSource(neighborPos))
            {
                waterSourceNeighbors.Add(neighborPos);
                // Continue checking other neighbors
            }
            // Check if it's a water pipe block (and not also direct water)
            else if (blockValue.Block is BlockWaterPipeSDX)
            {
                pipeNeighbors.Add(neighborPos); // Add as a pipe to explore
            }
            // Potential: Check if neighbor is a Sprinkler (BlockWaterSourceSDX)
            // else if (blockValue.Block is BlockWaterSourceSDX) { /* Decide if sprinklers act as pipes */ }
        }
        // Return tuple containing the lists
        return new Tuple<List<Vector3i>, List<Vector3i>>(pipeNeighbors, waterSourceNeighbors);
    }

} // End of PipeData class