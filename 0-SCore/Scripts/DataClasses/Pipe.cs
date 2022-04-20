using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Pipe
{
    private Vector3i blockPos = Vector3i.zero;
    private List<Vector3i> pipes = new List<Vector3i>();
    private List<Vector3i> waterSource = new List<Vector3i>();

    public Pipe(Vector3i blockPos)
    {
        this.blockPos = blockPos;
    }
    public Vector3i GetBlockPos()
    {
        return blockPos;
    }
    //public void GetNeighbors()
    //{
    //    pipes.Clear();
    //    waterSource.Clear();

    //    foreach (var direction in Vector3i.AllDirections)
    //    {
    //        var position = blockPos + direction;

    //        if (WaterPipeManager.Instance.IsValveOff(position)) continue;

    //        // If it's a pipe, add it to the list, and keep going.
    //        var blockValue = GameManager.Instance.World.GetBlock(position);
    //        if (blockValue.Block is BlockWaterPipeSDX)
    //            Add(position);
    //        if (WaterPipeManager.Instance.IsDirectWaterSource(position))
    //            AddWater(position);
    //    }
    //}
    public List<Vector3i> GetPipes()
    {
        return pipes;
    }
    public void Add(Vector3i pos)
    {
        if (pipes.Contains(pos)) return;
        pipes.Add(pos);
    }

    public void AddWater(Vector3i pos)
    {
        if (waterSource.Contains(pos)) return;
        waterSource.Add(pos);
    }
    public bool IsWaterSource()
    {
        return waterSource.Count > 0;
    }

    // If there's more than one, distribute them a bit so not all water blocks get hit for the same amount.
    public Vector3i GetWaterSource()
    {
        if (IsWaterSource())
            return waterSource[GameManager.Instance.World.GetGameRandom().RandomRange(0, waterSource.Count)];
        return Vector3i.zero;
    }
}