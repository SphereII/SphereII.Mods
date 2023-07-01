using System.Collections.Generic;
using UnityEngine;

public class PipeData
{
    private Vector3i BlockPos = Vector3i.zero;
    private Dictionary<Vector3i, Pipe> pipes = new Dictionary<Vector3i, Pipe>();
    private int maxPipes = 50;
    private int currentPipe = 0;
    public PipeData(Vector3i blockPos, int maxpipeCount = -1)
    {
        BlockPos = blockPos;
        maxPipes = maxpipeCount;
        if (maxpipeCount == -1)
            maxPipes = WaterPipeManager.Instance.GetMaxPipeCount();
    }
    public Pipe GetNeighbors()
    {
        var pipe = new Pipe(BlockPos);
        foreach( var direction in Vector3i.AllDirections)
        {
            var position = BlockPos + direction;

            if (WaterPipeManager.Instance.IsValveOff(position)) continue;

            // If it's a pipe, add it to the list, and keep going.
            var blockValue = GameManager.Instance.World.GetBlock(position);
            if (blockValue.Block is BlockWaterPipeSDX)
                pipe.Add(position);

            if (WaterPipeManager.Instance.IsDirectWaterSource(position))
                pipe.AddWater(position);

        }

        return pipe;
    }

    public void ClearPipes()
    {
        pipes.Clear();
    }
    public Vector3i GetWaterSource()
    {
        foreach ( var pipe in pipes)
        {
            if (pipe.Value.IsWaterSource())
            {
                return pipe.Value.GetWaterSource();
            }
        }
        return Vector3i.zero;
    }

  

    public bool FindAllPipes(Pipe _pipe, bool findFirst = true)
    {
        // If we exceeded our max, stop searching. This is the limit of the pipe system.
        if (currentPipe >= maxPipes) return true;
        currentPipe++;

        // Generate a new set of PipeData. This will hold data on connecting pipes and water sources,
        // and collect data from all the neighbors.
        var pipeData = new PipeData(_pipe.GetBlockPos());
        var neighbors = pipeData.GetNeighbors();

        //// If the neighbors have a water source, then end the loop. 
        if (neighbors.IsWaterSource() && findFirst)
        {
            // Add the watert source as part of the piping system
            var waterSource = neighbors.GetWaterSource();
            var pipe = new Pipe(waterSource);
            pipe.AddWater(waterSource);

            if (!pipes.ContainsKey(waterSource))
                pipes.Add(waterSource, pipe);
            return true;
        }

        // Loop around the pipes and check if they have already been indexed. If not, keep 
        // searching.
        foreach (var position in neighbors.GetPipes())
        {
            if (pipes.ContainsKey(position)) continue;

            // Register the pipe to the list so we can keep track of it.
            var pipe = new Pipe(position);
            pipes.Add(position, pipe);

            // Keep searching if we haven't discovered this pipe yet.
            var result = FindAllPipes(pipe, findFirst);
            if (result)
                return true;

        }
        return false;
    }

    public Vector3i DiscoverWaterFromPipes(Vector3i position)
    {
        currentPipe = 0;
        var startPipe = new Pipe(position);

        // Clear the pipes so we don't detect any dead branches in the pipes.
        ClearPipes();

        FindAllPipes(startPipe);
        foreach (var pipe in pipes)
        {
            if (pipe.Value.IsWaterSource())
                return pipe.Value.GetWaterSource();
        }
        return Vector3i.zero;
    }
  
}

