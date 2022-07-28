//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using System.Collections.Concurrent;
//public class FireManager
//{
//    private static readonly string AdvFeatureClass = "FireManagement";
//    static object locker = new object();

//    private static FireManager instance = null;
//    private static List<Vector3i> FireMap = new List<Vector3i>();
//    public static List<Vector3i> ExtinguishPositions = new List<Vector3i>();

//    private float checkTime = 120f;
//    private float currentTime = 0f;
//    private float fireDamage = 1f;
//    private GameRandom random;


//    private const string saveFile = "FireManager.dat";
//    private ThreadManager.ThreadInfo dataSaveThreadInfo;

//    public bool Enabled { private set; get; }
//    public static FireManager Instance
//    {
//        get
//        {
//            if (instance == null)
//            {
//                instance = new FireManager();
//                instance.Init();
//            }
//            return instance;
//        }
//    }
//    public void Init()
//    {
//        var option = Configuration.GetPropertyValue(AdvFeatureClass, "FireEnable");
//        if (!StringParsers.ParseBool(option))
//        {
//            Log.Out("Fire Manager is disabled.");
//            Enabled = false;
//            return;
//        }
//        random = GameManager.Instance.World.GetGameRandom();

//        Enabled = true;
//        option = Configuration.GetPropertyValue(AdvFeatureClass, "CheckInterval");
//        if (!string.IsNullOrEmpty(option))
//            checkTime = StringParsers.ParseFloat(option);

//        var strDamage = Configuration.GetPropertyValue(AdvFeatureClass, "FireDamage");
//        if (!string.IsNullOrWhiteSpace(strDamage))
//            fireDamage = StringParsers.ParseFloat(strDamage);
//        currentTime = -1;

//        Log.Out("Starting Fire Manager");
//        Log.Out($" :: Fire Interval Check time: {checkTime}");
//        Load();

//        ModEvents.GameUpdate.RegisterHandler(new Action(this.FireUpdate));
//    }


//    public void FireUpdate()
//    {
//        if (FireMap.Count == 0) return;

//        lock (locker)
//        {
//            currentTime -= Time.deltaTime;
//            if (currentTime > 0f) return;

//            CheckBlocks();
//        }
//    }

//    public void CheckBlocks()
//    {
//        AdvLogging.DisplayLog(AdvFeatureClass, $"Checking Blocks for Fire: {FireMap.Count} Blocks registered.");
//        Log.Out($"Checking Blocks for Fire: {FireMap.Count} Blocks registered.");
//        currentTime = checkTime;
//        var particle = Configuration.GetPropertyValue(AdvFeatureClass, "FireParticle");
//        List<BlockChangeInfo> Changes = new List<BlockChangeInfo>();

//        // Pre-populate the cleanup.
//        var cleanUps = new List<Vector3i>();
//        var neighbors = new List<Vector3i>();

//        for( int x = FireMap.Count -1; x >= 0; x-- )
//        {
//            var pos = FireMap[x];
//            var removeItem = false;
//            var block = GameManager.Instance.World.GetBlock(pos);
//            if (block.ischild) removeItem = true;
//            if (block.isair) removeItem = true;
//            if (!isFlammable(pos)) removeItem = true;
//            if (!isBurning(pos)) removeItem = true;


//            if (removeItem)
//            {
//                FireMap
//                cleanUps.Add(pos);
//                continue;
//            }
//            block.damage += (int)fireDamage;
//            if (block.damage <= block.Block.MaxDamage)
//            {
//                Changes.Add(new BlockChangeInfo(0, pos, block));

//                // If we are damaging a block, allow the fire to spread.
//                neighbors.AddRange(CheckNeighbors(pos));
//            }
//            else
//            {
//                Changes.Add(new BlockChangeInfo(0, pos, BlockValue.Air));
//            }
//            if (!GameManager.Instance.HasBlockParticleEffect(pos))
//                BlockUtilitiesSDX.addParticlesCentered(particle, pos);
//        }
//        foreach (var pos in FireMap)
//        {
//            var removeItem = false;
//            var block = GameManager.Instance.World.GetBlock(pos);
//            if (block.ischild) removeItem = true;
//            if (block.isair) removeItem = true;
//            if (!isFlammable(pos)) removeItem = true;
//            if (!isBurning(pos)) removeItem = true;


//            if (removeItem)
//            {
//                cleanUps.Add(pos);
//                continue;
//            }
//            block.damage += (int)fireDamage;
//            if (block.damage <= block.Block.MaxDamage)
//            {
//                Changes.Add(new BlockChangeInfo(0, pos, block));

//                // If we are damaging a block, allow the fire to spread.
//                neighbors.AddRange(CheckNeighbors(pos));
//            }
//            else
//            {
//                Changes.Add(new BlockChangeInfo(0, pos, BlockValue.Air));
//            }
//            if ( !GameManager.Instance.HasBlockParticleEffect(pos))
//                BlockUtilitiesSDX.addParticlesCentered(particle, pos);
            

//        }

//        GameManager.Instance.SetBlocksRPC(Changes);

//        // Spread the fire to the neighbors
//        foreach (var pos in neighbors)
//            FireManager.Instance.Add(pos);

//        // Remove any invalid blocks from tracking
//        foreach (var pos in cleanUps)
//            FireManager.Instance.Remove(pos);

//        Save();
//    }

//    public List<Vector3i> CheckNeighbors(Vector3i BlockPos)
//    {
//        var neighbors = new List<Vector3i>();
//        foreach (var direction in Vector3i.AllDirections)
//        {
//            var position = BlockPos + direction;
//            if (FireMap.Contains(position))
//                continue;
//            if (isFlammable(position))
//                neighbors.Add(position);
//        }
//        return neighbors;
//    }

//    public bool IsNearWater(Vector3i _blockPos)
//    {
//        foreach (var direction in Vector3i.AllDirections)
//        {
//            var position = _blockPos + direction;
//            var blockValue = GameManager.Instance.World.GetBlock(position);
//            if (blockValue.isWater) return true;
//            if (blockValue.Block is BlockLiquidv2) return true;
//        }
//        return false;
//    }
//    public bool isFlammable(Vector3i BlockPos)
//    {
//        var blockValue = GameManager.Instance.World.GetBlock(BlockPos);
//        if (blockValue.isair) return false;
//        if (blockValue.isWater) return false;
//        if (IsNearWater(BlockPos)) return false;
//        if (blockValue.Block.HasAnyFastTags(FastTags.Parse("flammable"))) return true;
//        var blockMaterial = blockValue.Block.blockMaterial;
//        if (blockMaterial.DamageCategory == "wood") return true;
//        if (blockMaterial.SurfaceCategory == "plant") return true;

//        return false;
//    }

//    public void Write(BinaryWriter _bw)
//    {
//        var writeOut = "";
//        foreach (var temp in FireMap)
//            writeOut += $"{temp};";

//        writeOut = writeOut.TrimEnd(';');
//        _bw.Write(writeOut);
//    }

//    public void Read(BinaryReader _br)
//    {
//        var positions = _br.ReadString();
//        foreach (var position in positions.Split(';'))
//        {
//            if (string.IsNullOrEmpty(position)) continue;
//            var vector = StringParsers.ParseVector3i(position);
//            Add(vector);
//        }
//    }
//    public void Remove(Vector3i _blockPos)
//    {
//        BlockUtilitiesSDX.removeParticles(_blockPos);
//        FireManager.FireMap.Remove(_blockPos);
//    }

  
//    public void Add(Vector3i _blockPos)
//    {
//        if (FireManager.FireMap.Contains(_blockPos))
//            return;

//        if (isFlammable(_blockPos))
//            FireManager.FireMap.Add(_blockPos);
//    }

//    public bool isBurning(Vector3i _blockPos)
//    {
//        return FireManager.FireMap.Contains(_blockPos);
//    }
//    private int saveDataThreaded(ThreadManager.ThreadInfo _threadInfo)
//    {
//        PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
//        string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
//        if (!Directory.Exists(GameIO.GetSaveGameDir()))
//        {
//            return -1;
//        }
//        if (File.Exists(text))
//        {
//            File.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak"), true);
//        }
//        pooledExpandableMemoryStream.Position = 0L;
//        StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
//        MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
//        return -1;
//    }

//    public void Save()
//    {
//        if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_FireDataSave"))
//        {
//            PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
//            using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
//            {
//                pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
//                this.Write(pooledBinaryWriter);
//            }
//            this.dataSaveThreadInfo = ThreadManager.StartThread("silent_FireDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.saveDataThreaded), null, System.Threading.ThreadPriority.Normal, pooledExpandableMemoryStream, null, false);
//        }
//    }

//    public void Load()
//    {
//        string path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
//        if (Directory.Exists(GameIO.GetSaveGameDir()) && File.Exists(path))
//        {
//            try
//            {
//                using (FileStream fileStream = File.OpenRead(path))
//                {
//                    using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
//                    {
//                        Log.Out("Reading From file");
//                        pooledBinaryReader.SetBaseStream(fileStream);
//                        this.Read(pooledBinaryReader);
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak");
//                if (File.Exists(path))
//                {
//                    using (FileStream fileStream2 = File.OpenRead(path))
//                    {
//                        using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
//                        {
//                            pooledBinaryReader2.SetBaseStream(fileStream2);
//                            this.Read(pooledBinaryReader2);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}

