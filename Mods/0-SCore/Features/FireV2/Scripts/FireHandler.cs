using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Audio;
using Harmony.SoundFeatures;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireHandler : IFireHandler
{
  

    private Dictionary<Vector3i, FireBlockData> _fireMap = new Dictionary<Vector3i, FireBlockData>();
    private Dictionary<Vector3i, float> _extinguishedPositions = new Dictionary<Vector3i, float>();
    private readonly FireEvents _events;
    private readonly FireConfig _config;
    private readonly FireParticleOptimizer _fireParticleOptimizer;
    private readonly GameRandom _random = GameManager.Instance.World.GetGameRandom();
    private ThreadManager.ThreadInfo _dataSaveThreadInfo;
    private const string SaveFile = "FireManager.dat";
    private const int FIRES_PER_FRAME = 10; // Adjustable batch size
    private Queue<Vector3i> _processingQueue = new Queue<Vector3i>();
    private List<BlockChangeInfo> _pendingChanges = new List<BlockChangeInfo>();
    private List<Vector3i> _removeFires = new List<Vector3i>();
    private bool _isProcessing = false;
    private float _lastProcessTime;
    private const float PROCESS_INTERVAL = 0.1f; // 100ms between processing batches
    private FireNetworkManager _fireNetworkManager = new FireNetworkManager();
    public FireHandler(FireEvents events, FireConfig config)
    {
        _events = events;
        _config = config;
        _fireParticleOptimizer = new FireParticleOptimizer();
    }

    public int GetFiresPerFrame()
    {
        return FIRES_PER_FRAME;
    }

    public bool IsProcessing()
    {
        return _isProcessing;
    }

    public Dictionary<Vector3i, FireBlockData> GetFireMap()
    {
        return _fireMap;
    }

    public void AddFire(Vector3i position, int entityId = -1)
    {
        if (!_config.Enabled || _fireMap.ContainsKey(position) || !IsFlammable(position))
            return;

        var block = GameManager.Instance.World.GetBlock(position);

        var fireBlockData = new FireBlockData
        {
            BlockValue = block,
            FireDamage = GetBlockFireDamage(block),
            ChanceToExtinguish = GetBlockChanceToExtinguish(block),
            DowngradeBlock = GetDowngradedBlock(block),
            FireParticle = _config.GetRandomFireParticle(position),
            SmokeParticle = _config.GetRandomSmokeParticle(position)
        };
        _fireMap[position] = fireBlockData;
        
        StartFireEffects(position, fireBlockData.FireParticle);
        _events.RaiseFireStarted(position, entityId);
    }

    public void RemoveFire(Vector3i position, int entityId = -1, bool showSmoke = true)
    {
        StopFireEffects(position);
        if (!_fireMap.Remove(position)) return;

        if (showSmoke)
        {
            var worldTime = GameManager.Instance.World.GetWorldTime();
            _extinguishedPositions[position] = worldTime + _config.SmokeTime;

            if (_config.SmokeTime > 0)
            {
                _events.RaiseSmokeStarted(position);
            }
        }

        _events.RaiseFireExtinguished(position, entityId);
    }

    public void SpreadFire(Vector3i sourcePosition)
    {
        if (!_config.FireSpread) return;
        if (_extinguishedPositions.ContainsKey(sourcePosition)) return;

        foreach (var direction in Vector3i.AllDirections)
        {
            var neighborPos = sourcePosition + direction;

            if (_fireMap.ContainsKey(neighborPos) || _extinguishedPositions.ContainsKey(neighborPos))
                continue;

            if (IsFlammable(neighborPos) && _random.NextDouble() > _config.ChanceToExtinguish)
            {
                FireManager.Instance.AddFire(neighborPos);
                //AddFire(neighborPos);
                _events.RaiseFireSpread(neighborPos, -1);
            }
        }
    }

    private bool IsFlammable(BlockValue blockValue)
    {
        if (blockValue.Block.HasAnyFastTags(FastTags<TagGroup.Global>.Parse("inflammable"))) return false;
        if (blockValue.ischild || blockValue.isair || blockValue.isWater) return false;

        if (blockValue.Block.HasAnyFastTags(FastTags<TagGroup.Global>.Parse("flammable"))) return true;
        var blockMaterial = blockValue.Block.blockMaterial;

        var matID = _config.MaterialID;
        if (matID.Contains(blockMaterial.id)) return true;

        var matDamage = _config.MaterialDamage;
        if (!string.IsNullOrEmpty(matDamage) && blockMaterial.DamageCategory != null && matDamage.Contains(blockMaterial.DamageCategory))
            return true;

        var matSurface = _config.MaterialSurface;
        if (!string.IsNullOrEmpty(matSurface) && blockMaterial.SurfaceCategory != null)
            return matSurface.Contains(blockMaterial.SurfaceCategory);
            
        return false;
    }

    public bool IsFlammable(Vector3i position)
    {
        if (GameManager.Instance.World.IsWithinTraderArea(position)) return false;
        if (_extinguishedPositions.ContainsKey(position)) return false;
        if (IsBurning(position)) return true;
        if (IsNearWater(position)) return false;

        var blockValue = GameManager.Instance.World.GetBlock(position);
        return IsFlammable(blockValue);
    }

    private static bool IsNearWater(Vector3i blockPos)
    {
        if (CropManager.Instance != null && CropManager.Instance.IsNearWater(blockPos, 5)) return true;

        foreach (var direction in Vector3i.AllDirections)
        {
            var position = blockPos + direction;
            var blockValue = GameManager.Instance.World.GetBlock(position);
            if (blockValue.isWater || blockValue.Block is BlockLiquidv2) return true;
            if (GameManager.Instance.World.GetWaterPercent(position) > 0.25f) return true;
        }

        return false;
    }

    public bool IsBurning(Vector3i position)
    {
        return _fireMap.ContainsKey(position);
    }

    public void DisplayStatus(double seconds = -1f)
    {
        if (!_config.Logging) return;
        if (_fireMap.Count == 0) return;
        var message = $"Processing Current Fires: {_fireMap.Count} : Max Per Frame: {FIRES_PER_FRAME} : Processing Queue: {_processingQueue.Count}";
        if (seconds >= 0)
            message = $"Processed: Current Fires: {_fireMap.Count} : Max Per Frame: {FIRES_PER_FRAME} : Processing Queue: {_processingQueue.Count} Time: {seconds}";
        Log.Out(message);
    }

    public void UpdateFires()
    {
        var currentTime = Time.realtimeSinceStartup;
        if (currentTime - _lastProcessTime < PROCESS_INTERVAL) return;
        _lastProcessTime = currentTime;

        if (!_isProcessing)
        {
            _processingQueue = new Queue<Vector3i>(_fireMap.Keys);
            _isProcessing = true;
            _pendingChanges.Clear();
            _removeFires.Clear();
        }

        ProcessFireBatch();

        if (_processingQueue.Count == 0)
        {
            _isProcessing = false;
            FinalizeBatchProcessing();
        }
    }

    private void ProcessFireBatch()
    {
        var processedCount = 0;
        var rainfallValue = WeatherManager.Instance.GetCurrentRainfallPercent();

        while (_processingQueue.Count > 0 && processedCount < FIRES_PER_FRAME)
        {
            var position = _processingQueue.Dequeue();
            processedCount++;

            if (!_fireMap.TryGetValue(position, out var fireBlockData)) continue;

            ProcessSingleFire(position, fireBlockData, rainfallValue);
        }
    }

    private void ProcessSingleFire(Vector3i position, FireBlockData fireBlockData, float rainfallValue)
    {
        var block = GameManager.Instance.World.GetBlock(position);

        if (IsNearWater(position) || _extinguishedPositions.ContainsKey(position) || block.isair)
        {
            _removeFires.Add(position);
            return;
        }

        fireBlockData.BlockValue.damage += fireBlockData.FireDamage;

        if (fireBlockData.BlockValue.damage >= fireBlockData.BlockValue.Block.MaxDamage)
        {
            fireBlockData.BlockValue.Block.SpawnDestroyFX(GameManager.Instance.World, fireBlockData.BlockValue, position,
                fireBlockData.BlockValue.Block.tintColor, -1);
            _events.RaiseBlockDestroyed(position, fireBlockData.BlockValue);
            _events.RaiseOnBlockDestroyedCount(1);
            _fireNetworkManager.SyncBlockDestroyedByFire(1);
            
            var blockValue2 = fireBlockData.DowngradeBlock;

            if (fireBlockData.BlockValue.Block.Properties.Values.ContainsKey("Explosion.ParticleIndex") ||
                fireBlockData.BlockValue.Block.Properties.Classes.ContainsKey("Explosion"))
            {
                fireBlockData.BlockValue.Block.OnBlockDestroyedByExplosion(GameManager.Instance.World, 0, position,
                    fireBlockData.BlockValue, -1);
            }

            if (!blockValue2.isair)
            {
                blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2,
                    GameManager.Instance.World.GetGameRandom(), position.x, position.z);
                blockValue2.rotation = fireBlockData.BlockValue.rotation;
            }
            _removeFires.Add(position);
           // GameManager.Instance.World.SetBlockRPC(0, position, blockValue2);
            _pendingChanges.Add(new BlockChangeInfo(0, position, blockValue2));   
            
             return;
        }

        _fireMap[position] = fireBlockData;
      //  GameManager.Instance.World.SetBlockRPC(0, position, fireBlockData.BlockValue);
        _pendingChanges.Add(new BlockChangeInfo(0, position, fireBlockData.BlockValue));

        var extinguishChance = fireBlockData.ChanceToExtinguish * (rainfallValue > 0.25f ? 2f : 1f);
        if (_random.RandomRange(0f, 1f) < extinguishChance)
        {
            _removeFires.Add(position);
            return;
        }

        SpreadFire(position);
    }

    public void FinalizeBatchProcessing()
    {
        if (_pendingChanges.Count > 0)
        {
            GameManager.Instance.SetBlocksRPC(_pendingChanges);
        }

        foreach (var position in _removeFires)
        {
            RemoveFire(position);
        }
        _removeFires.Clear();

        UpdateExtinguishedPositions();
        // if ( _fireMap.Count > 300)
        //     _fireParticleOptimizer.UpdateCullDistance(2);
        // else
        //     _fireParticleOptimizer.UpdateCullDistance(1);
        // _fireParticleOptimizer.UpdateAndOptimizeFireParticlesRoutine(_fireMap, _config);

        _events.RaiseFireUpdate(_fireMap.Count);
    }

    private float GetBlockChanceToExtinguish(BlockValue block)
    {
        var chance = _config.ChanceToExtinguish;
        if (block.Block.Properties.Contains("ChanceToExtinguish"))
            block.Block.Properties.ParseFloat("ChanceToExtinguish", ref chance);
        return chance;
    }

    private int GetBlockFireDamage(BlockValue block)
    {
        var damage = (int)_config.FireDamage;

        if (block.Block.Properties.Contains(""))
            damage = block.Block.Properties.GetInt("FireDamage");

        if (block.Block.blockMaterial.Properties.Contains("FireDamage"))
            damage = block.Block.blockMaterial.Properties.GetInt("FireDamage");

        return damage;
    }
    
    
    private BlockValue GetDowngradedBlock(BlockValue block)
    {
        if (block.Block.Properties.Values.ContainsKey("FireDowngradeBlock"))
            return Block.GetBlockValue(block.Block.Properties.Values["FireDowngradeBlock"]);

        return block.Block.DowngradeBlock;
    }

    
    public List<Vector3i> GetRemovalBlocks()
    {
        return _removeFires;
    }

    public List<BlockChangeInfo> GetPendingList()
    {
        return _pendingChanges;
    }

    private void StartFireEffects(Vector3i position, string fireParticle)
    {
        if (!string.IsNullOrEmpty(fireParticle) && ThreadManager.IsMainThread())
        {
            BlockUtilitiesSDX.addParticlesCentered(fireParticle, position);
        }

        if (_config.LightIntensity > 0)
        {
            _events.RaiseLightAdded(position);
        }
    }
    
    private void StopFireEffects(Vector3i position)
    {
        BlockUtilitiesSDX.removeParticles(position);
        _events.RaiseLightRemoved(position);
    }

    private void UpdateExtinguishedPositions()
    {
        var currentTime = GameManager.Instance.World.GetWorldTime();
        var expiredPositions = new List<Vector3i>();

        foreach (var kvp in _extinguishedPositions)
        {
            if (kvp.Value <= currentTime)
            {
                expiredPositions.Add(kvp.Key);
            }
        }

        foreach (var position in expiredPositions)
        {
            _extinguishedPositions.Remove(position);
            _events.RaiseSmokeEnded(position);
        }
    }

    public int CloseFires(Vector3i position, int range = 5)
    {
        var count = 0;
        for (var x = position.x - range; x <= position.x + range; x++)
        {
            for (var z = position.z - range; z <= position.z + range; z++)
            {
                for (int y = position.y - 2; y <= position.y + 2; y++)
                {
                    var localPosition = new Vector3i(x, y, z);
                    if (IsBurning(localPosition)) count++;
                }
            }
        }
        return count;
    }

    public void Reset()
    {
        Log.Out("Removing all blocks that are on fire and smoke.");
        List<Vector3i> positionsToRemove = new List<Vector3i>(_fireMap.Keys);
        foreach (var position in positionsToRemove)
        {
            RemoveFire(position);
        }
        
        List<Vector3i> extinguishedToRemove = new List<Vector3i>(_extinguishedPositions.Keys);
        foreach (var position in extinguishedToRemove)
        {
            BlockUtilitiesSDX.removeParticles(position);
            _extinguishedPositions.Remove(position);
        }
        
        _fireMap.Clear();
        SaveState();
    }

    public bool IsAnyFireBurning()
    {
        return _fireMap.Count > 0;
    }

    public void ForceStop()
    {
        // Implementation
    }

    public bool IsPositionCloseToFire(Vector3i position, int range = 5)
    {
        for (var x = position.x - range; x <= position.x + range; x++)
        {
            for (var z = position.z - range; z <= position.z + range; z++)
            {
                for (var y = position.y - 2; y <= position.y + 2; y++)
                {
                    var localPosition = new Vector3i(x, y, z);
                    if (IsBurning(localPosition)) return true;
                }
            }
        }
        return false;
    }
    
    private void Write(BinaryWriter bw)
    {
        var writeOut = "";
        foreach (var temp in _fireMap)
            writeOut += $"{temp.Key};";
        writeOut = writeOut.TrimEnd(';');
        bw.Write(writeOut);

        var writeOut2 = "";
        foreach (var temp in _extinguishedPositions.Keys)
            writeOut2 += $"{temp};";
        writeOut2 = writeOut2.TrimEnd(';');
        bw.Write(writeOut2);
    }

    public void Read(BinaryReader br)
    {
        var positions = br.ReadString();
        foreach (var position in positions.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
        }

        var extingished = br.ReadString();
        foreach (var position in extingished.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
        }
    }

    public void SaveState()
    {
        if (_dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_FireDataSave"))
        {
            Log.Out($"FireManager saving {_fireMap.Count} Fires...");
            var pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
            using (var pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
            {
                pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
                Write(pooledBinaryWriter);
            }

            _dataSaveThreadInfo = ThreadManager.StartThread("silent_FireDataSave", null,
                SaveDataThreaded, null, pooledExpandableMemoryStream);
        }
        else
        {
            Log.Out("Not Saving. Thread still running?");
        }
    }

    public int SaveDataThreaded(ThreadManager.ThreadInfo threadInfo)
    {
        var pooledExpandableMemoryStream =
            (PooledExpandableMemoryStream)threadInfo.parameter;
        if (!Directory.Exists(GameIO.GetSaveGameDir())) return -1;

        var text = $"{GameIO.GetSaveGameDir()}/{SaveFile}";
        if (File.Exists(text))
            File.Copy(text, $"{GameIO.GetSaveGameDir()}/{SaveFile}.bak", true);

        pooledExpandableMemoryStream.Position = 0L;
        StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
        Log.Out("FireManager saved {0} bytes", new object[] {
            pooledExpandableMemoryStream.Length
        });
        MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);

        return -1;
    }

    public void LoadState()
    {
        var path = $"{GameIO.GetSaveGameDir()}/{SaveFile}";
        if (!Directory.Exists(GameIO.GetSaveGameDir()) || !File.Exists(path)) return;

        try
        {
            using var fileStream = File.OpenRead(path);
            using var pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false);
            pooledBinaryReader.SetBaseStream(fileStream);
            Read(pooledBinaryReader);
        }
        catch (Exception)
        {
            path = $"{GameIO.GetSaveGameDir()}/{SaveFile}.bak";
            if (File.Exists(path))
            {
                using var fileStream2 = File.OpenRead(path);
                using var pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false);
                pooledBinaryReader2.SetBaseStream(fileStream2);
                Read(pooledBinaryReader2);
            }
        }

        Log.Out($"Fire Manager {path} Loaded: {_fireMap.Count}");
    }
}