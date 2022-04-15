using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class Pipe
{
    public Vector3i BlockPos { get; set; } = Vector3i.zero;
    public List<Vector3i> pipes = new List<Vector3i>();

    private GameRandom rand;
    private List<Vector3i> waterSource = new List<Vector3i>();

    public Pipe( Vector3i blockPos)
    {
        BlockPos = blockPos;
        rand = GameManager.Instance.World.GetGameRandom();
    }

    public void Add(Vector3i pos)
    {
        if (pipes.Contains(pos)) return;
        pipes.Add(pos);
    }

    public void AddWater(Vector3i pos)
    {
        if ( waterSource.Contains(pos)) return;
        waterSource.Add(pos);
    }
    public bool IsWaterSource()
    {
        return waterSource.Count > 0;
    }
    public Vector3i GetWaterSource()
    {
        if( IsWaterSource() )
            return waterSource[rand.RandomRange(0, waterSource.Count)];
        return Vector3i.zero;
    }
}
public class PipeData
{
    public Vector3i BlockPos { get; set; } = Vector3i.zero;
    public List<Pipe> pipes = new List<Pipe>();
    public PipeData(Vector3i blockPos)
    {
        BlockPos = blockPos;
    }

    public List<Vector3i> GetPipes()
    {
        var pipe = new Pipe(BlockPos);
        foreach( var direction in Vector3i.AllDirections)
        {
            var position = BlockPos + direction;

            // If it's a pipe, add it to the list, and keep going.
            var blockValue = GameManager.Instance.World.GetBlock(position);
            if (blockValue.Block is BlockWaterPipeSDX)
                pipe.Add(position);

            if (CropManager.Instance.IsWaterSource(position))
                pipe.AddWater(position);
        }

        return pipe.pipes;
    }
}

