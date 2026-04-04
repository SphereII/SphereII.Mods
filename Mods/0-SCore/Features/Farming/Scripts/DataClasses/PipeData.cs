using System.Collections.Generic;
using UnityEngine;

// Refactored PipeData using an iterative BFS search
public class PipeData
{
    private readonly int _maxDistance;
    private readonly Vector3i _startPos;

    public PipeData(Vector3i startPos, int maxDistance)
    {
        _startPos = startPos;
        _maxDistance = maxDistance;
    }

    /// <summary>
    /// Searches the pipe network for the nearest water source using BFS.
    /// </summary>
    /// <param name="position">The starting position (usually a sprinkler or pipe).</param>
    /// <returns>The Vector3i of the water source, or Vector3i.zero if none found.</returns>
    public Vector3i DiscoverWaterFromPipes(Vector3i position)
    {
        // 1. Setup BFS structures
        Queue<Vector3i> queue = new Queue<Vector3i>();
        HashSet<Vector3i> visited = new HashSet<Vector3i>();
        
        // Track distance from start to respect maxPipes limit
        Dictionary<Vector3i, int> distances = new Dictionary<Vector3i, int>();

        queue.Enqueue(position);
        visited.Add(position);
        distances[position] = 0;

        while (queue.Count > 0)
        {
            Vector3i current = queue.Dequeue();
            int currentDist = distances[current];

            // 2. Stop if we've reached the search limit
            if (currentDist >= _maxDistance) continue;

            // 3. Check all 6 directions (Up, Down, Left, Right, Forward, Back)
            foreach (Vector3i neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;
                visited.Add(neighbor);

                // 4. Check if this neighbor is a water source
                if (WaterPipeManager.Instance.IsDirectWaterSource(neighbor))
                {
                    return neighbor;
                }

                // 5. If it's a pipe, continue the search through it
                BlockValue bv = GameManager.Instance.World.GetBlock(neighbor);
                if (bv.Block is BlockWaterPipeSDX) //
                {
                    distances[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return Vector3i.zero;
    }

    private IEnumerable<Vector3i> GetNeighbors(Vector3i pos)
    {
        yield return pos + Vector3i.up;
        yield return pos + Vector3i.down;
        yield return pos + Vector3i.left;
        yield return pos + Vector3i.right;
        yield return pos + Vector3i.forward;
        yield return pos + Vector3i.back;
    }
}