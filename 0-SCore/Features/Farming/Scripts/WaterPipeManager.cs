using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


// Water Pipe Manager to handle all piping data.
public class WaterPipeManager
{
    private static readonly string AdvFeatureClass = "CropManagement";
    private static WaterPipeManager instance = null;
    private Dictionary<Vector3i, PipeData> Pipes = new Dictionary<Vector3i, PipeData>();
    private int maxPipes = 50;
    private static Dictionary<Vector3i, Vector3i> WaterValve = new Dictionary<Vector3i, Vector3i>();

   
    public static WaterPipeManager Instance
    {
        get
        {
            if (instance == null)
            {
                Log.Out("Initializing Water Pipe Manager");
                instance = new WaterPipeManager();
                instance.Init();
            }
            return instance;
        }
    }

    public Dictionary<Vector3i, Vector3i> GetWaterValves()
    {
        return WaterValve;
    }
    public int GetWaterDamage(Vector3i WaterPos)
    {

        var waterBlock = GameManager.Instance.World.GetBlock(WaterPos);
        var value = -1;
        if (waterBlock.Block.Properties.Contains("WaterDamage"))
            value = waterBlock.Block.Properties.GetInt("WaterDamage");
        else
            value = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "WaterDamage"));

        if (value < 0)
            value = 1;
        return value; 

    }

    public string GetWaterSummary( Vector3i plantPos)
    {
        var result = true;
        Vector3i WaterPos;

        var sourceBlock = GameManager.Instance.World.GetBlock(plantPos);

        var plantData = CropManager.Instance.Get(plantPos);
        if (plantData != null)
        {
            WaterPos = plantData.WaterPos;
        }
        else
        {
            WaterPos = WaterPipeManager.Instance.GetWaterForPosition(plantPos);
        }

        // If there's no set water position, create a temporary plant, to allow it to scan around.
        if (WaterPos == Vector3i.zero)
        {
            var tempPlant = new PlantData(plantPos);
            result = tempPlant.IsNearWater();
        }

        if ( !result )
            return $"Block: {sourceBlock.Block.GetLocalizedBlockName()} - {Localization.Get("has_water")}: {result}";

        var waterBlock = GameManager.Instance.World.GetBlock(WaterPos);

        return $"Block: {sourceBlock.Block.GetLocalizedBlockName()} - {Localization.Get("has_water")}: {result}: Water Source {WaterPos}: Water Block: {waterBlock.Block.GetBlockName()} Water Damage Property: {GetWaterDamage(WaterPos)} Durability: {waterBlock.Block.MaxDamage - waterBlock.damage} / {waterBlock.Block.MaxDamage}";


    }
    public Vector3i GetWaterForPosition(Vector3i position)
    {
        if (Pipes.ContainsKey(position))
            return Pipes[position].GetWaterSource();

        var pipeData = new PipeData(position);
        Pipes.Add(position, pipeData);
        return  pipeData.DiscoverWaterFromPipes(position);
    }

    public void ToggleWaterValve(Vector3i valve, Vector3i pipePosition, bool turnOn = false)
    {
        if (turnOn)
        {
            WaterValve.Add(valve, pipePosition);
        }
        else
        {
            WaterValve.Remove(valve);
        }
    }

    public void RemoveValve( Vector3i valve)
    {
        if (WaterValve.ContainsKey(valve))
            WaterValve.Remove(valve);
    }
    public void AddValve(Vector3i valve)
    {
        if (WaterValve.ContainsKey(valve))
            return;

        WaterValve.Add(valve, valve);
    }

  
     public bool IsValveOff(Vector3i position)
    {
        return WaterValve.ContainsValue(position);
    }
    public int GetMaxPipeCount()
    {
        return maxPipes;
    }
    public PipeData GetPipeData(Vector3i position)
    {
        if (Pipes.ContainsKey(position))
            return Pipes[position];
        return new PipeData(position);
    }
    public void Init()
    {
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "MaxPipeLength");
        if (!string.IsNullOrEmpty(option))
        {
            if (StringParsers.TryParseSInt32(option, out int maxpipes, 0, -1, NumberStyles.Integer))
                maxPipes = maxpipes;
        }

    }

    public void ClearPipes()
    {
        Pipes.Clear();
    }

    public void ToggleWaterParticles(bool turnOn = true)
    {
        // Clear the particles.
        foreach (var pipe in Pipes)
            BlockUtilitiesSDX.removeParticles(pipe.Key);

        if (!turnOn) return;

        foreach (var pipe in Pipes)
        {
            BlockUtilitiesSDX.addParticlesCentered("", pipe.Key);
        }
    }

    public bool IsWaterSource(Vector3i position)
    {
        if (IsDirectWaterSource(position))
            return true;
        var blockValue = GameManager.Instance.World.GetBlock(position);
        if (blockValue.Block is BlockWaterSourceSDX) return true;
        return false;
    }
    public bool IsDirectWaterSource(Vector3i position)
    {
        var blockValue = GameManager.Instance.World.GetBlock(position);
        if (blockValue.Block is BlockLiquidv2) return true;

    
        if (blockValue.Block.Properties.Contains("WaterSource"))
        {
            if (blockValue.Block.Properties.GetBool("WaterSource"))
                return true;
            return false;
        }

        if (blockValue.Block.Properties.Values.ContainsKey("WaterType"))
        {
            if (blockValue.Block.Properties.Values["WaterType"].ToLower() == "unlimited")
            {
                return true;
            }
        }

        // A21 water support
        return GameManager.Instance.World.GetWaterPercent(position) > 0.25;
    }

    // Counts how many water blocks are in the surrounding area.
    public int CountWaterBlocks(Vector3i position, int range = 4)
    {
        int count = 0;
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                for (int y = position.y - range; y <= position.y + range; y++)
                {
                    var waterCheck = new Vector3i(position.x + x, y, position.z + z);
                    if (GameManager.Instance.World.IsWater(waterCheck))
                        count++;
                }
            }
        }
        return count;
    }

}


