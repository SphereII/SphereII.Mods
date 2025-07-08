
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Audio;
using Harmony.SoundFeatures;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireHandler : IFireHandler
{
    private readonly Dictionary<Vector3i, BlockValue> _fireMap = new Dictionary<Vector3i, BlockValue>();
    private readonly Dictionary<Vector3i, float> _extinguishedPositions = new Dictionary<Vector3i, float>();
    private readonly FireEvents _events;
    private readonly FireConfig _config;
    private readonly GameRandom _random = GameManager.Instance.World.GetGameRandom();
    private ThreadManager.ThreadInfo _dataSaveThreadInfo;
    private const string SaveFile = "FireManager.dat";
    private const int FIRES_PER_FRAME = 10; // Adjustable batch size
    private Queue<Vector3i> _processingQueue = new Queue<Vector3i>();
    private List<BlockChangeInfo> _pendingChanges = new List<BlockChangeInfo>();
    private bool _isProcessing = false;
    private float _lastProcessTime;
    private const float PROCESS_INTERVAL = 0.1f; // 100ms between processing batches

    public FireHandler(FireEvents events, FireConfig config)
    {
        _events = events;
        _config = config;
    }

    public void AddFire(Vector3i position, int entityId = -1)
    {
        if (!_config.Enabled || _fireMap.ContainsKey(position) || !IsFlammable(position))
            return;

        var block = GameManager.Instance.World.GetBlock(position);
        _fireMap[position] = block;

        // Start visual and audio effects
        StartFireEffects(position);

        // Raise event
        _events.RaiseFireStarted(position, entityId);
    }

    public void RemoveFire(Vector3i position, int entityId = -1, bool showSmoke = true)
    {
        if (!_fireMap.Remove(position))
            return;

        StopFireEffects(position);

        if (!showSmoke) return;
        // Add to extinguished positions with expiry time
        var worldTime = GameManager.Instance.World.GetWorldTime();
        _extinguishedPositions[position] = worldTime + _config.SmokeTime;

        // Start smoke effect if configured
        if (_config.SmokeTime > 0)
        {
            _events.RaiseSmokeStarted(position);
        }

        _events.RaiseFireExtinguished(position, entityId);
    }

    public void SpreadFire(Vector3i sourcePosition)
    {
        if (!_config.FireSpread)
            return;

        foreach (var direction in Vector3i.AllDirections)
        {
            var neighborPos = sourcePosition + direction;

            if (_fireMap.ContainsKey(neighborPos) ||
                _extinguishedPositions.ContainsKey(neighborPos))
                continue;

            if (IsFlammable(neighborPos) && _random.NextDouble() > _config.ChanceToExtinguish)
            {
                AddFire(neighborPos);
                _events.RaiseFireSpread(neighborPos, -1);
            }
        }
    }

    private bool IsFlammable(BlockValue blockValue)
    {
        if (blockValue.Block.HasAnyFastTags(FastTags<TagGroup.Global>.Parse("inflammable"))) return false;
        if (blockValue.ischild) return false;
        if (blockValue.isair) return false;
        if (blockValue.isWater) return false;

        // if (blockValue.Block.Properties.Values.ContainsKey("Explosion.ParticleIndex")) return true;

        if (blockValue.Block.HasAnyFastTags(FastTags<TagGroup.Global>.Parse("flammable"))) return true;
        var blockMaterial = blockValue.Block.blockMaterial;

        var matID = _config.MaterialID;
        if (matID.Contains(blockMaterial.id)) return true;

        var matDamage = _config.MaterialDamage;
        if (!string.IsNullOrEmpty(matDamage) && blockMaterial.DamageCategory != null)
            if (matDamage.Contains(blockMaterial.DamageCategory))
                return true;

        var matSurface = _config.MaterialSurface;
        if (string.IsNullOrEmpty(matSurface) || blockMaterial.SurfaceCategory == null) return false;
        return matSurface.Contains(blockMaterial.SurfaceCategory);
    }
    public bool IsFlammable(Vector3i position)
    {
        if (GameManager.Instance.World.IsWithinTraderArea(position)) return false;
        if (_extinguishedPositions.ContainsKey(position)) return false;

        // If its already burning, then don't do any other check
        if (IsBurning(position)) return true;
        if (IsNearWater(position)) return false;

        // Check the block value.
        var blockValue = GameManager.Instance.World.GetBlock(position);
        return IsFlammable(blockValue);
    }

    private static bool IsNearWater(Vector3i blockPos)
    {
        if (CropManager.Instance != null)
        {
            if (CropManager.Instance.IsNearWater(blockPos, 5)) return true;    
        }
        
        foreach (var direction in Vector3i.AllDirections)
        {
            var position = blockPos + direction;
            var blockValue = GameManager.Instance.World.GetBlock(position);
            if (blockValue.isWater) return true;
            if (blockValue.Block is BlockLiquidv2) return true;
            // A21 water check.
            if (GameManager.Instance.World.GetWaterPercent(position) > 0.25f) return true;
        }

        return false;
    }
    public bool IsBurning(Vector3i position)
    {
        return _fireMap.ContainsKey(position);
    }

    public void UpdateFires()
    {
        var currentTime = Time.realtimeSinceStartup;
        if (currentTime - _lastProcessTime < PROCESS_INTERVAL) return;
        _lastProcessTime = currentTime;

        // Initialize processing queue if needed
        if (!_isProcessing)
        {
            _processingQueue = new Queue<Vector3i>(_fireMap.Keys);
            _isProcessing = true;
            _pendingChanges.Clear();
        }

        // Process batch
        ProcessFireBatch();

        // Check if we're done processing all fires
        if (_processingQueue.Count == 0)
        {
            FinalizeBatchProcessing();
        }
    }

    private void ProcessFireBatch()
    {
        var processedCount = 0;
        var blocksToRemove = new List<Vector3i>();
        var rainfallValue = WeatherManager.Instance.GetCurrentRainfallValue();

        while (_processingQueue.Count > 0 && processedCount < FIRES_PER_FRAME)
        {
            var position = _processingQueue.Dequeue();
            processedCount++;

            if (!_fireMap.TryGetValue(position, out var block)) continue;

            var chanceToExtinguish = _config.ChanceToExtinguish;
            
            // Process single fire
            if (ProcessSingleFire(position, block, chanceToExtinguish, rainfallValue, blocksToRemove))
            {
                SpreadFire(position); // Only spread if fire continues
            }
        }

        // Remove destroyed blocks from this batch
        foreach (var position in blocksToRemove)
        {
            RemoveFire(position);
        }
    }

    private bool ProcessSingleFire(Vector3i position, BlockValue block, float chanceToExtinguish, float rainfallValue, List<Vector3i> blocksToRemove)
    {
        if (IsNearWater(position))
        {
            blocksToRemove.Add(position);
            return false;
        }

        // Get block specific damages
        var damage = GetBlockFireDamage(block);
        
        if (block.Block.Properties.Contains("ChanceToExtinguish"))
            block.Block.Properties.ParseFloat("ChanceToExtinguish", ref chanceToExtinguish);

        block.damage += damage;

        if (block.damage >= block.Block.MaxDamage)
        {
            HandleBlockDestruction(position, block);
            return false;
        }

        _pendingChanges.Add(new BlockChangeInfo(0, position, block));

        // Check for natural extinguishing
        var extinguishChance = chanceToExtinguish * (rainfallValue > 0.25f ? 2f : 1f);
        if (_random.RandomRange(0f, 1f) < extinguishChance)
        {
            blocksToRemove.Add(position);
            return false;
        }

        _fireMap[position] = block;
        return true;
    }

    private int GetBlockFireDamage(BlockValue block)
    {
        var damage = (int)_config.FireDamage;
        
        if (block.Block.Properties.Contains("FireDamage"))
            damage = block.Block.Properties.GetInt("FireDamage");

        if (block.Block.blockMaterial.Properties.Contains("FireDamage"))
            damage = block.Block.blockMaterial.Properties.GetInt("FireDamage");

        return damage;
    }

    private void HandleBlockDestruction(Vector3i position, BlockValue block)
    {
        block.Block.SpawnDestroyFX(GameManager.Instance.World, block, position, block.Block.tintColor, -1);
        _events.RaiseBlockDestroyed(position, block);

        var blockValue2 = GetDowngradedBlock(block);

        if (block.Block.Properties.Values.ContainsKey("Explosion.ParticleIndex") ||
            block.Block.Properties.Classes.ContainsKey("Explosion"))
        {
            block.Block.OnBlockDestroyedByExplosion(GameManager.Instance.World, 0, position, block, -1);
        }

        if (!blockValue2.isair)
        {
            blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2,
                GameManager.Instance.World.GetGameRandom(), position.x, position.z);
            blockValue2.rotation = block.rotation;
            blockValue2.meta = block.meta;
        }

        _pendingChanges.Add(new BlockChangeInfo(0, position, blockValue2));
    }

    private BlockValue GetDowngradedBlock(BlockValue block)
    {
        if (block.Block.Properties.Values.ContainsKey("FireDowngradeBlock"))
            return Block.GetBlockValue(block.Block.Properties.Values["FireDowngradeBlock"]);
            
        return block.Block.DowngradeBlock;
    }

    private void FinalizeBatchProcessing()
    {
        // Apply all accumulated changes
        if (_pendingChanges.Count > 0)
        {
            GameManager.Instance.SetBlocksRPC(_pendingChanges);
            _pendingChanges.Clear();
        }

        // Update extinguished positions
        UpdateExtinguishedPositions();

        // Raise update event
        _events.RaiseFireUpdate(_fireMap.Count);

        // Reset processing state
        _isProcessing = false;
    }

    // Add a method to check processing status
    public bool IsProcessingFires()
    {
        return _isProcessing;
    }

    // Add a method to get progress
    public float GetProcessingProgress()
    {
        if (!_isProcessing || _fireMap.Count == 0) return 1.0f;
        return 1.0f - (_processingQueue.Count / (float)_fireMap.Count);
    }

    private void Write(BinaryWriter bw)
    {
        // Save the burning blocks.
        var writeOut = "";
        foreach (var temp in _fireMap)
            writeOut += $"{temp.Key};";
        writeOut = writeOut.TrimEnd(';');
        bw.Write(writeOut);

        // Save the blocks we've put out
        var writeOut2 = "";
        foreach (var temp in _extinguishedPositions.Keys)
            writeOut2 += $"{temp};";
        writeOut2 = writeOut2.TrimEnd(';');
        bw.Write(writeOut2);
    }

    public void Read(BinaryReader br)
    {
        // Read burning blocks
        var positions = br.ReadString();
        foreach (var position in positions.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
            StartFireEffects(vector);
        }
        

        // Read extinguished blocks.
        var extingished = br.ReadString();
        foreach (var position in extingished.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
            RemoveFire(vector, -1);
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

    private void StartFireEffects(Vector3i position)
    {
        // Add particle effect
        var fireParticle = GetRandomFireParticle(position);
        if (!string.IsNullOrEmpty(fireParticle))
        {
            BlockUtilitiesSDX.addParticlesCentered(fireParticle, position);
        }

        // Add light
        if (_config.LightIntensity > 0)
        {
            _events.RaiseLightAdded(position);
        }

        // Add sound
        if (!string.IsNullOrEmpty(_config.FireSound))
        {
            Manager.PlayInsidePlayerHead(_config.FireSound, -1, 0, true);
        }
    }

    private string GetRandomFireParticle(Vector3i blockPos)
    {
        var block = GameManager.Instance.World.GetBlock(blockPos);

        if (block.Block.blockMaterial.Properties.Contains("FireParticle"))
            return block.Block.blockMaterial.Properties.GetString("FireParticle");

        if (block.Block.Properties.Contains("FireParticle"))
            return block.Block.Properties.GetString("FireParticle");

        var fireParticle = _config.FireParticle;
        var randomFire = Configuration.GetPropertyValue("FireManagement", "RandomFireParticle");
        if (string.IsNullOrEmpty(randomFire)) return fireParticle;
        var fireParticles = randomFire.Split(',');
        var randomIndex = _random.RandomRange(0, fireParticles.Length);
        fireParticle = fireParticles[randomIndex];

        return fireParticle;
    }

    private void StopFireEffects(Vector3i position)
    {
        // Remove particles
        BlockUtilitiesSDX.removeParticles(position);

        // Remove light
        _events.RaiseLightRemoved(position);

        // Stop sound
        if (!string.IsNullOrEmpty(_config.FireSound))
        {
            Manager.StopLoopInsidePlayerHead(_config.FireSound, -1);
        }
    }

    private void UpdateExtinguishedPositions()
    {
        var currentTime = GameManager.Instance.World.GetWorldTime();
        var expiredPositions = _extinguishedPositions
            .Where(kvp => kvp.Value <= currentTime)
            .Select(kvp => kvp.Key)
            .ToList();

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
        foreach (var position in _fireMap.Keys)
            RemoveFire(position);

        foreach (var position in _extinguishedPositions.Keys)
            BlockUtilitiesSDX.removeParticles(position);

        _fireMap.Clear();
        _extinguishedPositions.Clear();
        SaveState();
    }

    public void ForceStop()
    {
     

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
}