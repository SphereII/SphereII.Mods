using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WaterData
{
    private static readonly string AdvFeatureClass = "CropManagement";
    public Vector3i WaterPos { get; set; } = Vector3i.zero;
    private int maxPipes = 50;
    private int currentPipe = 0;
    public Dictionary<Vector3i, Pipe> Pipes = new Dictionary<Vector3i, Pipe>();

    public WaterData()
    {
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "MaxPipeLength");
        if (!string.IsNullOrEmpty(option))
        {
            if (StringParsers.TryParseSInt32(option, out int maxpipes, 0, -1, NumberStyles.Integer))
                maxPipes = maxpipes;

        }

    }

    // Searches and finds all the inter-connected Pipes.
    public void FindAllPipes(Pipe _pipe)
    {
        // If we exceeded our max, stop searching. This is the limit of the pipe system.
        if (currentPipe > maxPipes) return;
        currentPipe++;

        var pipeData = new PipeData(_pipe.BlockPos);
        foreach (var position in pipeData.GetPipes())
        {
            if (!Pipes.ContainsKey(position))
            {
                var pipe = new Pipe(position);
                Pipes.Add(position, pipe);

                // Keep searching if we haven't discovered this pipe yet.
                FindAllPipes(pipe);
            }
        }
    }


    // Search for a water block, following all the water pipes.
    public Vector3i GetWaterSource(Vector3i position)
    {
        currentPipe = 0;
        var startPipe = new Pipe(position);
        FindAllPipes(startPipe);
        foreach (var pipe in Pipes)
        {
            if (pipe.Value.IsWaterSource())
                return pipe.Value.GetWaterSource();

            foreach (var direction in Vector3i.AllDirections)
            {
                var blockPosition = pipe.Key + direction;
                if (CropManager.Instance.IsWaterSource(blockPosition))
                    return blockPosition;
            }
        }
        return Vector3i.zero;
    }

}

