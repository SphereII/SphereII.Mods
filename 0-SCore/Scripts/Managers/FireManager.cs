using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections.Concurrent;
public class FireManager
{
    private static readonly string AdvFeatureClass = "FireManagement";
    static object locker = new object();

    private static FireManager instance = null;
    private static ConcurrentDictionary<Vector3i, BlockValue> FireMap = new ConcurrentDictionary<Vector3i, BlockValue>();
    public static ConcurrentDictionary<Vector3i, float> ExtinguishPositions = new ConcurrentDictionary<Vector3i, float>();
    private float checkTime = 120f;
    private float currentTime = 0f;
    private float fireDamage = 1f;
    private float smokeTime = 60f;
    private GameRandom random = new GameRandom();
    private float heatMapStrength = 0f;

    public string fireParticle = Configuration.GetPropertyValue(AdvFeatureClass, "FireParticle");
    public string smokeParticle = Configuration.GetPropertyValue(AdvFeatureClass, "SmokeParticle");
    private const string saveFile = "FireManager.dat";
    private ThreadManager.ThreadInfo dataSaveThreadInfo;

    private static BlockValue burntGround = new BlockValue((uint)Block.GetBlockByName("terrBurntForestGround").blockID);

    public bool Enabled { private set; get; }
    public static FireManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FireManager();
            }
            return instance;
        }
    }

    public void Init(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
    {
        Init();
    }
    public void Init()
    {
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "FireEnable");
        if (!StringParsers.ParseBool(option))
        {
            Log.Out("Fire Manager is disabled.");
            Enabled = false;
            return;
        }
        random = GameManager.Instance.World.GetGameRandom();

        Enabled = true;
        option = Configuration.GetPropertyValue(AdvFeatureClass, "CheckInterval");
        if (!string.IsNullOrEmpty(option))
            checkTime = StringParsers.ParseFloat(option);

        var strDamage = Configuration.GetPropertyValue(AdvFeatureClass, "FireDamage");
        if (!string.IsNullOrWhiteSpace(strDamage))
            fireDamage = StringParsers.ParseFloat(strDamage);
        currentTime = -1;

        var heatMap = Configuration.GetPropertyValue(AdvFeatureClass, "HeatMapStrength");
        if (!string.IsNullOrWhiteSpace(heatMap))
            heatMapStrength = StringParsers.ParseFloat(heatMap);

        var smoke = Configuration.GetPropertyValue(AdvFeatureClass, "SmokeTime");
        if (!string.IsNullOrWhiteSpace(smoke))
            smokeTime = StringParsers.ParseFloat(smoke);

        Log.Out("Starting Fire Manager");
        Log.Out($" :: Fire Interval Check time: {checkTime}");


        fireParticle = Configuration.GetPropertyValue(AdvFeatureClass, "FireParticle");
        smokeParticle = Configuration.GetPropertyValue(AdvFeatureClass, "SmokeParticle");

        // Register the particle effects before anything. This is causing the Unknown Particle warnings. 
        ParticleEffect.RegisterBundleParticleEffect(fireParticle);
        ParticleEffect.RegisterBundleParticleEffect(smokeParticle);

        Load();

        ModEvents.GameUpdate.RegisterHandler(new Action(this.FireUpdate));
    }


    // General call to remove the fire from a block, and add an extinguished counter, so blocks can be temporarily immune to restarting.
    public void Extinguish(Vector3i _blockPos)
    {
        var worldTime = GameManager.Instance.World.GetWorldTime();
        var expiry = worldTime + smokeTime;
        if (!ExtinguishPositions.ContainsKey(_blockPos))
            ExtinguishPositions[_blockPos] = expiry;

        var wasBurning = isBurning(_blockPos);
        if (wasBurning)
        {
            Remove(_blockPos);
            var block = GameManager.Instance.World.GetBlock(_blockPos);
            var _smokeParticle = smokeParticle;
            if (block.Block.Properties.Contains("SmokeParticle"))
                _smokeParticle = block.Block.Properties.GetString("SmokeParticle");

            if (!block.isair)
                BlockUtilitiesSDX.addParticlesCenteredNetwork(_smokeParticle, _blockPos);
        }
    }

    // Poor man's timed cache
    public void CheckExtinguishedPosition()
    {
        var worldTime = GameManager.Instance.World.GetWorldTime();
        foreach (var position in ExtinguishPositions)
        {
            Remove(position.Key);
            if (position.Value < worldTime || GameManager.Instance.World.GetBlock(position.Key + Vector3i.down).isair)
            {
                ExtinguishPositions.TryRemove(position.Key, out var _);
                BlockUtilitiesSDX.removeParticlesNetPackage(position.Key);
            }
        }
    }
    public void FireUpdate()
    {
        // Make sure to only run it once
        lock (locker)
        {
            currentTime -= Time.deltaTime;
            if (currentTime > 0f) return;



            // No fires, no updates.
            if (FireMap.Count == 0) return;

            CheckBlocks();
        }
    }

    public void CheckBlocks()
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Checking Blocks for Fire: {FireMap.Count} Blocks registered. Extinguished Blocks: {ExtinguishPositions.Count}");
        currentTime = checkTime;

        CheckExtinguishedPosition();

        var Changes = new List<BlockChangeInfo>();
        var neighbors = new List<Vector3i>();


        var alternate = false;

        ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[0];
        if (chunkCluster == null) return;

        foreach (var posDict in FireMap)
        {
            var pos = posDict.Key;
            if (!isFlammable(pos))
            {
                Remove(pos);
                continue;
            }

            // Get block specific damages
            var block = GameManager.Instance.World.GetBlock(pos);
            if (block.Block.Properties.Contains("FireDamage"))
                block.damage += block.Block.Properties.GetInt("FireDamage");
            else
                block.damage += (int)fireDamage;

            if (alternate) // This follows the game rules more but is a heavier FPS hitter.
            {
                block.Block.DamageBlock(GameManager.Instance.World, 0, pos, block, (int)fireDamage, -1);
            }
            else
            {
                if (block.damage >= block.Block.MaxDamage)
                {
                    block.Block.SpawnDestroyParticleEffect(GameManager.Instance.World, block, pos, 1f, block.Block.tintColor, -1);
                    BlockValue blockValue2 = block.Block.DowngradeBlock;

                    // Check if there's another placeholder for this block.
                    if (!blockValue2.isair)
                        blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, GameManager.Instance.World.GetGameRandom(), pos.x, pos.z, false, QuestTags.none);
                    blockValue2.rotation = block.rotation;
                    blockValue2.meta = block.meta;
                    block = blockValue2;

                    // If there is terrain under it, convert it to burnt ground.
                    var blockBelow = GameManager.Instance.World.GetBlock(pos + Vector3i.down);
                    if (blockBelow.Block.shape.IsTerrain())
                        Changes.Add(new BlockChangeInfo(0, pos, burntGround));

                }

                Changes.Add(new BlockChangeInfo(0, pos, block));
            }

            // If the new block has changed, check to make sure the new block is flammable. Note: it checks the blockValue, not blockPos, since the change hasn't been commited yet.
            if (!isFlammable(block) || block.isair)
            {
                BlockUtilitiesSDX.removeParticlesNetPackage(pos);
                //Extinguish(pos);
                continue;
            }

            if (!GameManager.Instance.HasBlockParticleEffect(pos))
            {
                var _fireParticle = fireParticle;
                if (block.Block.Properties.Contains("FireParticle"))
                    _fireParticle = block.Block.Properties.GetString("FireParticle");

                BlockUtilitiesSDX.addParticlesCenteredNetwork(_fireParticle, pos);
            }
            // If we are damaging a block, allow the fire to spread.
            neighbors.AddRange(CheckNeighbors(pos));

            FireMap[pos] = block;

        }

        // Send all the changes in one shot
        GameManager.Instance.SetBlocksRPC(Changes);

        // Spread the fire to the neighbors. We delay this here so the fire does not spread too quickly or immediately, getting stuck in the above loop.
        foreach (var pos in neighbors)
            Add(pos);

        Save();
    }

    // Check to see if the nearby blocks can catch fire.
    public List<Vector3i> CheckNeighbors(Vector3i BlockPos)
    {
        var neighbors = new List<Vector3i>();
        foreach (var direction in Vector3i.AllDirections)
        {
            var position = BlockPos + direction;
            if (FireMap.ContainsKey(position))
                continue;
            if (isFlammable(position))
                neighbors.Add(position);
        }
        return neighbors;
    }

    public bool IsNearWater(Vector3i _blockPos)
    {
        foreach (var direction in Vector3i.AllDirections)
        {
            var position = _blockPos + direction;
            var blockValue = GameManager.Instance.World.GetBlock(position);
            if (blockValue.isWater) return true;
            if (blockValue.Block is BlockLiquidv2) return true;
        }
        return false;
    }

    public bool isFlammable(BlockValue blockValue)
    {
        if (blockValue.Block.HasAnyFastTags(FastTags.Parse("inflammable"))) return false;
        if (blockValue.ischild) return false;
        if (blockValue.isair) return false;
        if (blockValue.isWater) return false;

        if (blockValue.Block.HasAnyFastTags(FastTags.Parse("flammable"))) return true;
        var blockMaterial = blockValue.Block.blockMaterial;

        var matID = Configuration.GetPropertyValue(AdvFeatureClass, "MaterialID");
        if (matID.Contains(blockMaterial.id)) return true;
        var matDamage = Configuration.GetPropertyValue(AdvFeatureClass, "MaterialDamage");
        if (matDamage.Contains(blockMaterial.DamageCategory)) return true;

        var matSurface = Configuration.GetPropertyValue(AdvFeatureClass, "MaterialSurface");
        if (matSurface.Contains(blockMaterial.SurfaceCategory)) return true;

        return false;

    }
    public bool isFlammable(Vector3i BlockPos)
    {
        if (ExtinguishPositions.ContainsKey(BlockPos)) return false;
        // If its already burning, then don't do any other check
        if (isBurning(BlockPos)) return true;

        if (IsNearWater(BlockPos)) return false;

        // Check the block value.
        var blockValue = GameManager.Instance.World.GetBlock(BlockPos);
        if (isFlammable(blockValue))
            return true;

        return false;
    }

    public void Write(BinaryWriter _bw)
    {
        // Save the burning blocks.
        var writeOut = "";
        foreach (var temp in FireMap)
            writeOut += $"{temp.Key};";
        writeOut = writeOut.TrimEnd(';');
        _bw.Write(writeOut);

        // Save the blocks we've put out and put in a dampner
        var writeOut2 = "";
        foreach (var temp in ExtinguishPositions.Keys)
            writeOut2 += $"{temp};";
        writeOut2 = writeOut2.TrimEnd(';');
        _bw.Write(writeOut2);
    }

    public void Read(BinaryReader _br)
    {
        // Read burning blocks
        var positions = _br.ReadString();
        foreach (var position in positions.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
            Add(vector);
        }

        // Read extinguished blocks.
        var extingished = _br.ReadString();
        foreach (var position in extingished.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
            Extinguish(vector);
        }
    }
    public void Remove(Vector3i _blockPos)
    {
        if (!FireMap.ContainsKey(_blockPos)) return;
        BlockUtilitiesSDX.removeParticlesNetPackage(_blockPos);
        FireMap.TryRemove(_blockPos, out var block);
    }

    // Add flammable blocks to the Fire Map
    public void Add(Vector3i _blockPos)
    {
        if (GameManager.Instance.World.IsWithinTraderArea(_blockPos))
            return;

        if (!isFlammable(_blockPos)) return;

        var block = GameManager.Instance.World.GetBlock(_blockPos);

        var _fireParticle = fireParticle;
        if (block.Block.Properties.Contains("FireParticle"))
            _fireParticle = block.Block.Properties.GetString("FireParticle");

        BlockUtilitiesSDX.addParticlesCenteredNetwork(_fireParticle, _blockPos);

        if (FireMap.TryAdd(_blockPos, block))
        {
            if (heatMapStrength != 0)
                GameManager.Instance.World?.aiDirector?.NotifyActivity(EnumAIDirectorChunkEvent.Campfire, _blockPos, block.Block.HeatMapStrength, (ulong)block.Block.HeatMapTime);
        }
    }

    public bool isBurning(Vector3i _blockPos)
    {
        return FireMap.ContainsKey(_blockPos);
    }
    private int saveDataThreaded(ThreadManager.ThreadInfo _threadInfo)
    {
        PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
        string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
        if (!Directory.Exists(GameIO.GetSaveGameDir()))
        {
            return -1;
        }
        if (File.Exists(text))
        {
            File.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak"), true);
        }
        pooledExpandableMemoryStream.Position = 0L;
        StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
        MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
        return -1;
    }

    public void Save()
    {
        if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_FireDataSave"))
        {
            PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
            using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
            {
                pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
                this.Write(pooledBinaryWriter);
            }
            this.dataSaveThreadInfo = ThreadManager.StartThread("silent_FireDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.saveDataThreaded), null, System.Threading.ThreadPriority.Normal, pooledExpandableMemoryStream, null, false);
        }
    }

    public void Load()
    {
        string path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
        if (Directory.Exists(GameIO.GetSaveGameDir()) && File.Exists(path))
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                    {
                        pooledBinaryReader.SetBaseStream(fileStream);
                        this.Read(pooledBinaryReader);
                    }
                }
            }
            catch (Exception)
            {
                path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak");
                if (File.Exists(path))
                {
                    using (FileStream fileStream2 = File.OpenRead(path))
                    {
                        using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
                        {
                            pooledBinaryReader2.SetBaseStream(fileStream2);
                            this.Read(pooledBinaryReader2);
                        }
                    }
                }
            }
        }
    }

    public void CleanUp()
    {
        Log.Out("Fire Manager Clean up");
        this.WaitOnSave();
        this.Save();
        this.WaitOnSave();
        FireMap.Clear();
        ExtinguishPositions.Clear();

    }

    public void Reset()
    {
        Log.Out("Removing all blocks that are on fire and smoke.");
        lock (locker)
        {
            foreach (var position in FireMap.Keys)
                Remove(position);

            foreach (var position in ExtinguishPositions.Keys)
                BlockUtilitiesSDX.removeParticlesNetPackage(position);

            FireMap.Clear();
            ExtinguishPositions.Clear();
            Save();
        }
    }
    private void WaitOnSave()
    {
        if (this.dataSaveThreadInfo != null)
        {
            this.dataSaveThreadInfo.WaitForEnd();
            this.dataSaveThreadInfo = null;
        }
    }
}

