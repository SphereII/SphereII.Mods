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
    private GameRandom random;

    private string fireParticle = Configuration.GetPropertyValue(AdvFeatureClass, "FireParticle");
    private string smokeParticle = Configuration.GetPropertyValue(AdvFeatureClass, "SmokeParticle");
    private const string saveFile = "FireManager.dat";
    private ThreadManager.ThreadInfo dataSaveThreadInfo;

    public bool Enabled { private set; get; }
    public static FireManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FireManager();
                instance.Init();
            }
            return instance;
        }
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

        Log.Out("Starting Fire Manager");
        Log.Out($" :: Fire Interval Check time: {checkTime}");
        Load();

        ModEvents.GameUpdate.RegisterHandler(new Action(this.FireUpdate));
    }


    // General call to remove the fire from a block, and add an extinguished counter, so blocks can be temporarily immune to restarting.
    public void Extinguish(Vector3i _blockPos)
    {
        var worldTime = GameManager.Instance.World.GetWorldTime();
        var expiry = worldTime + 600;
        if (!ExtinguishPositions.ContainsKey(_blockPos))
            ExtinguishPositions[_blockPos] = expiry;

        var wasBurning = isBurning(_blockPos);
        Remove(_blockPos);
        if ( wasBurning)
            BlockUtilitiesSDX.addParticlesCentered(smokeParticle, _blockPos);
    }

    // Poor man's timed cache
    public void CheckExtinguishedPosition()
    {
        var worldTime = GameManager.Instance.World.GetWorldTime();
        foreach (var position in ExtinguishPositions)
        {
            Remove(position.Key);
            if (position.Value < worldTime)
            {
                ExtinguishPositions.TryRemove(position.Key, out var _);
                BlockUtilitiesSDX.removeParticles(position.Key);
            }
        }
    }
    public void FireUpdate()
    {
        // No fires, no updates.
        if (FireMap.Count == 0) return;

        // Make sure to only run it once
        lock (locker)
        {
            currentTime -= Time.deltaTime;
            if (currentTime > 0f) return;

            CheckBlocks();
        }
    }

    public void CheckBlocks()
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Checking Blocks for Fire: {FireMap.Count} Blocks registered. Extinguished Blocks: {ExtinguishPositions.Count}");
        currentTime = checkTime;

        var Changes = new List<BlockChangeInfo>();
        var neighbors = new List<Vector3i>();

        var alternate = false;
        // Check if any of the extinguished positions can be re-ignited, and clears them.
        CheckExtinguishedPosition();

        foreach (var posDict in FireMap)
        {
            var pos = posDict.Key;
            if (!isFlammable(pos))
            {
                Remove(pos);
                continue;
            }

            var block = GameManager.Instance.World.GetBlock(pos);
            if (alternate)
            {
                block.Block.DamageBlock(GameManager.Instance.World, 0, pos, block, (int)fireDamage, -1);
            }
            else
            {
                block.damage += (int)fireDamage;
                if (block.damage >= block.Block.MaxDamage)
                {
                    BlockValue blockValue2 = block.Block.DowngradeBlock;
                    blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, GameManager.Instance.World.GetGameRandom(), pos.x, pos.z, false, QuestTags.none);
                    blockValue2.rotation = block.rotation;
                    blockValue2.meta = block.meta;
                    block = blockValue2;
                }
                Changes.Add(new BlockChangeInfo(0, pos, block));
            }
            // Since the block could be damaged or replaced by the above, we want to check to see if we need to remove it or not.
            if (!isFlammable(pos) || block.isair)
            {
                Remove(pos);
                continue;
            }
            //    

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
    public bool isFlammable(Vector3i BlockPos)
    {
        if (ExtinguishPositions.ContainsKey(BlockPos)) return false;
        
        // If its already burning, then don't do any other check
        if (isBurning(BlockPos)) return true;

        var blockValue = GameManager.Instance.World.GetBlock(BlockPos);
        if (blockValue.Block.HasAnyFastTags(FastTags.Parse("inflammable"))) return false;
        if (blockValue.ischild) return false;
        if (blockValue.isair) return false;
        if (blockValue.isWater) return false;
        if (IsNearWater(BlockPos)) return false;


        if (blockValue.Block.HasAnyFastTags(FastTags.Parse("flammable"))) return true;
        var blockMaterial = blockValue.Block.blockMaterial;
        if (blockMaterial.DamageCategory == "wood") return true;
        if (blockMaterial.SurfaceCategory == "plant") return true;

        return false;
    }

    public void Write(BinaryWriter _bw)
    {
        // Save the burning blocks.
        var writeOut = "";
        foreach (var temp in FireMap)
            writeOut += $"{temp};";
        writeOut = writeOut.TrimEnd(';');
        _bw.Write(writeOut);

        // Save the blocks we've put out and put in a dampner
        writeOut = "";
        foreach (var temp in ExtinguishPositions.Keys)
            writeOut += $"{temp};";
        writeOut = writeOut.TrimEnd(';');
        _bw.Write(writeOut);
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
        BlockUtilitiesSDX.removeParticles(_blockPos);
        FireMap.TryRemove(_blockPos, out var block);
    }

    // Add flammable blocks to the Fire Map
    public void Add(Vector3i _blockPos)
    {
        if (!isFlammable(_blockPos)) return;

        if (!GameManager.Instance.HasBlockParticleEffect(_blockPos))
            BlockUtilitiesSDX.addParticlesCentered(fireParticle, _blockPos);

        var block = GameManager.Instance.World.GetBlock(_blockPos);
        FireMap.TryAdd(_blockPos, block);
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
                        Log.Out("Reading From file");
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
}

