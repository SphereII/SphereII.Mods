using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections.Concurrent;
using Audio;
/// <summary>
/// SCore's FireManager allows flammable blocks to catch and spread fire.
/// </summary>
/// <remarks>
/// The Fire Manager allows blocks to catch on fire, and allows them to spread to other blocks around it at a timed interval.
/// The following parameters are configured through the blocks.xml
/// </remarks>

public class FireManager
{
    private const string AdvFeatureClass = "FireManagement";
    private static readonly object Locker = new object();

    private static readonly ConcurrentDictionary<Vector3i, BlockValue>
        FireMap = new ConcurrentDictionary<Vector3i, BlockValue>();

    private static readonly ConcurrentDictionary<Vector3i, float> ExtinguishPositions =
        new ConcurrentDictionary<Vector3i, float>();

    private float _checkTime = 120f;
    private float _currentTime;
    private const float CheckTimeLights = 0.8f;
    private float _currentTimeLights;
    private float _fireDamage = 1f;

    private float _smokeTime = 60f;
    private readonly GameRandom _random = GameManager.Instance.World.GetGameRandom();
    private bool _fireSpread = true;


    // Used to throttle sounds playing.
    private static readonly List<Vector3i> SoundPlaying = new List<Vector3i>();
    private static readonly List<Vector3i> ParticlePlaying = new List<Vector3i>();
    private const int MaxLights = 32;

    private static readonly HashSet<Light> Shutoff =
        new HashSet<Light>();

    private string _fireSound;
    private string _fireParticle;
    private string _smokeParticle;
    private const string SaveFile = "FireManager.dat";
    private ThreadManager.ThreadInfo _dataSaveThreadInfo;
    private float _chanceToExtinguish = 0.05f;
    public bool Enabled { private set; get; }

    public static FireManager Instance { get; private set; }

    public static void Init()
    {
        Instance = new FireManager();
        Instance.ReadConfig();
    }

    private void ReadConfig()
    {
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "FireEnable");
        if (!StringParsers.ParseBool(option))
        {
            Log.Out("Fire Manager is disabled.");
            Enabled = false;
            return;
        }

        if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"
            || GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting"
            || GamePrefs.GetString(EnumGamePrefs.GameMode) == "GameModeEditWorld")
        {
            Log.Out("Fire Manager is disabled in test worlds.");

            Enabled = false;
            return;
        }

        var fireSpreadString = Configuration.GetPropertyValue(AdvFeatureClass, "FireSpread");
        if (!StringParsers.ParseBool(fireSpreadString))
        {
            Debug.Log("Fire Spread Disabled.");
            _fireSpread = false;
        }


        Enabled = true;
        option = Configuration.GetPropertyValue(AdvFeatureClass, "CheckInterval");
        if (!string.IsNullOrEmpty(option))
            _checkTime = StringParsers.ParseFloat(option);

        var strDamage = Configuration.GetPropertyValue(AdvFeatureClass, "FireDamage");
        if (!string.IsNullOrWhiteSpace(strDamage))
            _fireDamage = StringParsers.ParseFloat(strDamage);
        _currentTime = -1;

        var smoke = Configuration.GetPropertyValue(AdvFeatureClass, "SmokeTime");
        if (!string.IsNullOrWhiteSpace(smoke))
            _smokeTime = StringParsers.ParseFloat(smoke);

        var strFireSound = Configuration.GetPropertyValue(AdvFeatureClass, "FireSound");
        if (!string.IsNullOrWhiteSpace(strFireSound))
            _fireSound = strFireSound;

        Log.Out("Starting Fire Manager");

        _fireParticle = Configuration.GetPropertyValue(AdvFeatureClass, "FireParticle");
        _smokeParticle = Configuration.GetPropertyValue(AdvFeatureClass, "SmokeParticle");

        var optionChanceToExtinguish = Configuration.GetPropertyValue(AdvFeatureClass, "ChanceToExtinguish");
        if (!string.IsNullOrEmpty(option))
            _chanceToExtinguish = StringParsers.ParseFloat(optionChanceToExtinguish);

        // Read the FireManager
        Load();

        // Only run the Update on the server, then just distribute the data to the clients using NetPackages.
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        Log.Out($" :: Fire Interval Check time: {_checkTime}");
        
        ModEvents.GameUpdate.RegisterHandler(FireUpdate);
        ModEvents.GameUpdate.RegisterHandler(LightsUpdate);
    }

    #region LightCode

    private void LightsUpdate()
    {
        // Make sure to only run it once
        lock (Locker)
        {
            _currentTimeLights -= Time.deltaTime;
            if (_currentTimeLights > 0f) return;
            _currentTimeLights = CheckTimeLights;
            ShutoffLights();
            CheckLights();
        }
    }


    private static void ShutoffLights()
    {
        if (Shutoff.Count == 0) return;
        if (GameManager.Instance.IsPaused()) return;
        Shutoff.RemoveWhere(light => light == null);
        foreach (var light in Shutoff)
        {
            light.intensity -= Time.deltaTime;
            // Log.Out("Toning down {0}", light.intensity);
            if (light.intensity > 0f) continue;
            // Log.Out("Culling light out of this world");
            var gameObject = light.gameObject;
            gameObject.transform.parent = null;
            UnityEngine.Object.Destroy(gameObject);
        }

        Shutoff.RemoveWhere(light => light.intensity <= 0f);
    }

    private static void CheckLights()
    {
        if (GameManager.Instance.IsPaused()) return;
        var allLights = UnityEngine.Object.FindObjectsOfType<Light>();
        if (allLights.Length <= MaxLights) return;
        var curLights = new List<Light>();
        for (var n = 0; n < allLights.Length; n += 1)
        {
            if (allLights[n].name != "FireLight") continue;
            if (Shutoff.Contains(allLights[n])) continue;
            curLights.Add(allLights[n]);
        }

        //   AdvLogging.DisplayLog(AdvFeatureClass,$"Detect currently {curLights.Count} lights, {Shutoff.Count} being culled");
        // Do nothing if we are (or will be) within limits
        if (curLights.Count <= MaxLights) return;
        // Otherwise choose more lights to shutoff
        while (curLights.Count >= MaxLights)
        {
            var idx = GameManager.Instance.World.GetGameRandom().RandomRange(curLights.Count);
            if (Shutoff.Contains(curLights[idx])) continue;
            // Log.Out("Selected {0} for culling", idx);
            Shutoff.Add(curLights[idx]);
            curLights.RemoveAt(idx);
        }
    }

    #endregion

    // Poor man's timed cache
    private void CheckExtinguishedPosition()
    {
        var worldTime = GameManager.Instance.World.GetWorldTime();
        foreach (var position in ExtinguishPositions)
        {
            Remove(position.Key);
            if (!(position.Value < worldTime) &&
                !GameManager.Instance.World.GetBlock(position.Key + Vector3i.down).isair) continue;
            ExtinguishPositions.TryRemove(position.Key, out var _);
            ClearPos(position.Key);
        }
    }

    public ConcurrentDictionary<Vector3i, BlockValue> GetFireMap()
    {
        return FireMap;
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
    private void FireUpdate()
    {
        // Make sure to only run it once
        lock (Locker)
        {
            _currentTime -= Time.deltaTime;
            if (_currentTime > 0f) return;

            // ThreadManager.StartCoroutine(ProcessBlocks());
            CheckBlocks();
        }
    }

    private void CheckBlocks()
    {
        if (GameManager.Instance.IsPaused()) return;
        if (!GameManager.Instance.gameStateManager.IsGameStarted()) return;

        AdvLogging.DisplayLog(AdvFeatureClass,
            $"Checking Blocks for Fire: {FireMap.Count} Blocks registered. Extinguished Blocks: {ExtinguishPositions.Count}");
        _currentTime = _checkTime;


        var changes = new List<BlockChangeInfo>();
        var neighbors = new List<Vector3i>();

        CheckExtinguishedPosition();

        var chunkCluster = GameManager.Instance.World.ChunkClusters[0];
        if (chunkCluster == null) return;

        foreach (var posDict in FireMap)
        {
            var blockPos = posDict.Key;
            if (!IsFlammable(blockPos))
            {
                Remove(blockPos);
                continue;
            }
            
            var block = GameManager.Instance.World.GetBlock(blockPos);
            // Get block specific damages
            var damage = (int) _fireDamage;
            if (block.Block.Properties.Contains("FireDamage"))
                damage = block.Block.Properties.GetInt("FireDamage");
            
            if (block.Block.blockMaterial.Properties.Contains("FireDamage"))
                damage = block.Block.blockMaterial.Properties.GetInt("FireDamage");
            
            if (block.Block.Properties.Contains("ChanceToExtinguish"))
                block.Block.Properties.ParseFloat("ChanceToExtinguish", ref _chanceToExtinguish);
            
            block.damage += damage;
            
            if (block.damage >= block.Block.MaxDamage)
            {
                block.Block.SpawnDestroyParticleEffect(GameManager.Instance.World, block, blockPos, 1f,
                    block.Block.tintColor, -1);
                var blockValue2 = block.Block.DowngradeBlock;
            
                if (block.Block.Properties.Values.ContainsKey("FireDowngradeBlock"))
                    blockValue2 = Block.GetBlockValue(block.Block.Properties.Values["FireDowngradeBlock"]);
            
                if (block.Block.Properties.Values.ContainsKey("Explosion.ParticleIndex") ||
                    block.Block.Properties.Classes.ContainsKey("Explosion"))
                    block.Block.OnBlockDestroyedByExplosion(GameManager.Instance.World, 0, blockPos, block, -1);
            
                // Check if there's another placeholder for this block.
                if (!blockValue2.isair)
                    blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2,
                        GameManager.Instance.World.GetGameRandom(), blockPos.x, blockPos.z);
                blockValue2.rotation = block.rotation;
                blockValue2.meta = block.meta;
                
                block = blockValue2;
            }
            
            if (!block.isair)
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageAddFirePosition>().Setup(blockPos, -1));
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageRemoveFirePosition>().Setup(blockPos, -1));
            }
            
            changes.Add(new BlockChangeInfo(0, blockPos, block));
            
            var rand = _random.RandomRange(0f, 1f);
           
            var blchanceToExtinguish = rand < _chanceToExtinguish;
            // If the new block has changed, check to make sure the new block is flammable. Note: it checks the blockValue, not blockPos, since the change hasn't been committed yet.
            if (!IsFlammable(block) || block.isair || blchanceToExtinguish )
            {
                // queue up the change
                if (DynamicMeshManager.Instance != null)
                    DynamicMeshManager.Instance.AddChunk(blockPos, true);
            
                Extinguish(blockPos);
                continue;
            }

            ToggleSound(blockPos, rand < 0.10);
            //ToggleParticle(blockPos, rand > 0.90);
            ToggleParticle(blockPos, true);
            
            // If we are damaging a block, allow the fire to spread.
            if (_fireSpread)
                neighbors.AddRange(CheckNeighbors(blockPos));
            
            FireMap[blockPos] = block;
        }

        // Send all the changes in one shot
        GameManager.Instance.SetBlocksRPC(changes);

        if (_fireSpread == false) return;

        // Spread the fire to the neighbors. We delay this here so the fire does not spread too quickly or immediately, getting stuck in the above loop.
        foreach (var pos in neighbors)
            Add(pos);

        //Debug.Log($"Sound Counter: {SoundPlaying.Count}, Fire Counter: {FireMap.Count} Particle Count: {ParticlePlaying.Count}: Heat: {heatMeter}");
    }

    //
    // private IEnumerator ProcessBlocks()
    // {
    //     if (GameManager.Instance.IsPaused()) yield break;
    //     if (!GameManager.Instance.gameStateManager.IsGameStarted()) yield break;
    //
    //     _running = true;
    //     AdvLogging.DisplayLog(AdvFeatureClass,
    //         $"Checking Blocks for Fire: {FireMap.Count} Blocks registered. Extinguished Blocks: {ExtinguishPositions.Count}");
    //     _currentTime = _checkTime;
    //
    //     CheckExtinguishedPosition();
    //
    //     var chunkCluster = GameManager.Instance.World.ChunkClusters[0];
    //     if (chunkCluster == null) yield break;
    //     foreach (var posDict in FireMap)
    //     {
    //         var blockPos = posDict.Key;
    //         if (!IsFlammable(blockPos))
    //         {
    //             Remove(blockPos);
    //             continue;
    //         }
    //
    //         var block = GameManager.Instance.World.GetBlock(blockPos);
    //         // Get block specific damages
    //         var damage = (int) _fireDamage;
    //         if (block.Block.Properties.Contains("FireDamage"))
    //             damage = block.Block.Properties.GetInt("FireDamage");
    //
    //         if (block.Block.blockMaterial.Properties.Contains("FireDamage"))
    //             damage = block.Block.blockMaterial.Properties.GetInt("FireDamage");
    //
    //         if (block.Block.Properties.Contains("ChanceToExtinguish"))
    //             block.Block.Properties.ParseFloat("ChanceToExtinguish", ref _chanceToExtinguish);
    //
    //         block.damage += damage;
    //
    //         if (block.damage >= block.Block.MaxDamage)
    //         {
    //             block.Block.SpawnDestroyParticleEffect(GameManager.Instance.World, block, blockPos, 1f,
    //                 block.Block.tintColor, -1);
    //             var blockValue2 = block.Block.DowngradeBlock;
    //
    //             if (block.Block.Properties.Values.ContainsKey("FireDowngradeBlock"))
    //                 blockValue2 = Block.GetBlockValue(block.Block.Properties.Values["FireDowngradeBlock"]);
    //
    //             if (block.Block.Properties.Values.ContainsKey("Explosion.ParticleIndex") ||
    //                 block.Block.Properties.Classes.ContainsKey("Explosion"))
    //                 block.Block.OnBlockDestroyedByExplosion(GameManager.Instance.World, 0, blockPos, block, -1);
    //
    //             // Check if there's another placeholder for this block.
    //             if (!blockValue2.isair)
    //                 blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2,
    //                     GameManager.Instance.World.GetGameRandom(), blockPos.x, blockPos.z);
    //             blockValue2.rotation = block.rotation;
    //             blockValue2.meta = block.meta;
    //             block = blockValue2;
    //         }
    //
    //         if (!block.isair)
    //         {
    //             SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
    //                 NetPackageManager.GetPackage<NetPackageAddFirePosition>().Setup(blockPos, -1));
    //         }
    //         else
    //         {
    //             SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
    //                 NetPackageManager.GetPackage<NetPackageRemoveFirePosition>().Setup(blockPos, -1));
    //         }
    //
    //
    //         var rand = _random.RandomRange(0f, 1f);
    //         var blchanceToExtinguish = rand < _chanceToExtinguish;
    //         // If the new block has changed, check to make sure the new block is flammable. Note: it checks the blockValue, not blockPos, since the change hasn't been committed yet.
    //         if (!IsFlammable(block) || block.isair || blchanceToExtinguish)
    //         {
    //             // queue up the change
    //             if (DynamicMeshManager.Instance != null)
    //                 DynamicMeshManager.Instance.AddChunk(blockPos, true);
    //
    //             Extinguish(blockPos);
    //             continue;
    //         }
    //         
    //         ToggleSound(blockPos, rand < 0.10);
    //         // //ToggleParticle(blockPos, rand > 0.90);
    //         ToggleParticle(blockPos, true);
    //
    //         // If we are damaging a block, allow the fire to spread.
    //         if (_fireSpread)
    //         {
    //             foreach (var pos in CheckNeighbors(blockPos))
    //                 Add(pos);
    //         }
    //
    //         FireMap[blockPos] = block;
    //         GameManager.Instance.World.SetBlock(0, blockPos, block, false, false);
    //         yield return new WaitForEndOfFrame();
    //     }
    //
    //     _running = false;
    // }

    private void ToggleSound(Vector3i blockPos, bool turnOn)
    {
        //if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (!ThreadManager.IsMainThread()) return; 
        // If there's a lot of fires going on, turn off the sounds.
        if (turnOn && FireMap.Count > 60)
        {
            turnOn = false;
        }

        var sound = GetFireSound(blockPos);
        if (turnOn)
        {
            if (SoundPlaying.Contains(blockPos))
                return;
            SoundPlaying.Add(blockPos);
            Manager.Play(blockPos, sound);
            return;
        }

        // No sound?
        if (!SoundPlaying.Contains(blockPos)) return;
        Manager.Stop(blockPos, sound);
        SoundPlaying.Remove(blockPos);
    }


    private void ToggleParticle(Vector3i blockPos, bool turnOn)
    {
       // if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
       if (!ThreadManager.IsMainThread()) return; 

        var randomFireParticle = GetRandomFireParticle(blockPos);
        if (turnOn)
        {
            if (ParticlePlaying.Contains(blockPos)) return;
            if (GameManager.Instance.HasBlockParticleEffect(blockPos)) return;
            ParticlePlaying.Add(blockPos);
            BlockUtilitiesSDX.addParticlesCentered(randomFireParticle, blockPos);
            return;
        }

        ParticlePlaying.Remove(blockPos);
        BlockUtilitiesSDX.removeParticles(blockPos);
    }

    private string GetFireSound(Vector3i blockPos)
    {
        var block = GameManager.Instance.World.GetBlock(blockPos);

        var fireSound = this._fireSound;
        if (block.Block.Properties.Contains("FireSound"))
            fireSound = block.Block.Properties.GetString("FireSound");
        return fireSound;
    }

    private string GetRandomFireParticle(Vector3i blockPos)
    {
        var block = GameManager.Instance.World.GetBlock(blockPos);

        var fireParticle = _fireParticle;
        if (block.Block.Properties.Contains("FireParticle"))
            fireParticle = block.Block.Properties.GetString("FireParticle");

        if (block.Block.blockMaterial.Properties.Contains("FireParticle"))
            fireParticle = block.Block.blockMaterial.Properties.GetString("FireParticle");

        var randomFire = Configuration.GetPropertyValue(AdvFeatureClass, "RandomFireParticle");
        if (string.IsNullOrEmpty(randomFire)) return fireParticle;
        var fireParticles = randomFire.Split(',');
        var randomIndex = _random.RandomRange(0, fireParticles.Length);
        fireParticle = fireParticles[randomIndex];

        return fireParticle;
    }

    private string GetRandomSmokeParticle(Vector3i blockPos)
    {
        var block = GameManager.Instance.World.GetBlock(blockPos);

        var smokeParticle = _smokeParticle;
        if (block.Block.Properties.Contains("SmokeParticle"))
            smokeParticle = block.Block.Properties.GetString("SmokeParticle");

        if (block.Block.blockMaterial.Properties.Contains("SmokeParticle"))
            smokeParticle = block.Block.blockMaterial.Properties.GetString("SmokeParticle");

        var randomSmoke = Configuration.GetPropertyValue(AdvFeatureClass, "RandomSmokeParticle");
        if (string.IsNullOrEmpty(randomSmoke)) return smokeParticle;
        var smokeParticles = randomSmoke.Split(',');
        var randomIndex = GameManager.Instance.World.GetGameRandom().RandomRange(0, smokeParticles.Length);
        smokeParticle = smokeParticles[randomIndex];

        return smokeParticle;
    }

    // Check to see if the nearby blocks can catch fire.
    private static List<Vector3i> CheckNeighbors(Vector3i blockPos)
    {
        var neighbors = new List<Vector3i>();
        foreach (var direction in Vector3i.AllDirections)
        {
            var position = blockPos + direction;
            if (FireMap.ContainsKey(position))
                continue;
            if (IsFlammable(position))
                neighbors.Add(position);
        }

        return neighbors;
    }

    private static bool IsNearWater(Vector3i blockPos)
    {
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

    private static bool IsFlammable(BlockValue blockValue)
    {
        if (blockValue.Block.HasAnyFastTags(FastTags.Parse("inflammable"))) return false;
        if (blockValue.ischild) return false;
        if (blockValue.isair) return false;
        if (blockValue.isWater) return false;

        // if (blockValue.Block.Properties.Values.ContainsKey("Explosion.ParticleIndex")) return true;

        if (blockValue.Block.HasAnyFastTags(FastTags.Parse("flammable"))) return true;
        var blockMaterial = blockValue.Block.blockMaterial;

        var matID = Configuration.GetPropertyValue(AdvFeatureClass, "MaterialID");
        if (matID.Contains(blockMaterial.id)) return true;

        var matDamage = Configuration.GetPropertyValue(AdvFeatureClass, "MaterialDamage");
        if (!string.IsNullOrEmpty(matDamage) && blockMaterial.DamageCategory != null)
            if (matDamage.Contains(blockMaterial.DamageCategory))
                return true;

        var matSurface = Configuration.GetPropertyValue(AdvFeatureClass, "MaterialSurface");
        if (string.IsNullOrEmpty(matSurface) || blockMaterial.SurfaceCategory == null) return false;
        return matSurface.Contains(blockMaterial.SurfaceCategory);
    }

    private static bool IsFlammable(Vector3i blockPos)
    {
        if (GameManager.Instance.World.IsWithinTraderArea(blockPos)) return false;
        if (ExtinguishPositions.ContainsKey(blockPos)) return false;

        // If its already burning, then don't do any other check
        if (IsBurning(blockPos)) return true;
        if (IsNearWater(blockPos)) return false;

        // Check the block value.
        var blockValue = GameManager.Instance.World.GetBlock(blockPos);
        return IsFlammable(blockValue);
    }

    private static void Write(BinaryWriter bw)
    {
        // Save the burning blocks.
        var writeOut = "";
        foreach (var temp in FireMap)
            writeOut += $"{temp.Key};";
        writeOut = writeOut.TrimEnd(';');
        bw.Write(writeOut);

        // Save the blocks we've put out
        var writeOut2 = "";
        foreach (var temp in ExtinguishPositions.Keys)
            writeOut2 += $"{temp};";
        writeOut2 = writeOut2.TrimEnd(';');
        bw.Write(writeOut2);
    }

    private void Read(BinaryReader br)
    {
        // Read burning blocks
        var positions = br.ReadString();
        foreach (var position in positions.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
            AddBlock(vector);
        }

        // Read extinguished blocks.
        var extingished = br.ReadString();
        foreach (var position in extingished.Split(';'))
        {
            if (string.IsNullOrEmpty(position)) continue;
            var vector = StringParsers.ParseVector3i(position);
            ExtinguishBlock(vector);
        }
    }

    public void ClearPos(Vector3i blockPos)
    {
        ToggleSound(blockPos, false);
        ToggleParticle(blockPos, false);
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(blockPos, -1));
            return;
        }

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
            NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(blockPos, -1));
    }

    public void Add(Vector3i blockPos, int entityID = -1)
    {
        if (!IsFlammable(blockPos))
            return;

        AddBlock(blockPos);

        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageAddFirePosition>().Setup(blockPos, entityID));
            return;
        }

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
            NetPackageManager.GetPackage<NetPackageAddFirePosition>().Setup(blockPos, entityID));
    }


    // General call to remove the fire from a block, and add an extinguished counter, so blocks can be temporarily immune to restarting.
    public void Extinguish(Vector3i blockPos, int entityID = -1)
    {
        ExtinguishBlock(blockPos);
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageAddExtinguishPosition>().Setup(blockPos, entityID));
            return;
        }

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
            NetPackageManager.GetPackage<NetPackageAddExtinguishPosition>().Setup(blockPos, entityID));
    }

    public void Remove(Vector3i blockPos, int entityID = -1)
    {
        RemoveFire(blockPos);

        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageRemoveFirePosition>().Setup(blockPos, entityID));
            return;
        }

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
            NetPackageManager.GetPackage<NetPackageRemoveFirePosition>().Setup(blockPos, entityID));
    }


    private void RemoveFire(Vector3i blockPos)
    {
        if (!FireMap.ContainsKey(blockPos)) return;
        ToggleSound(blockPos, false);
        ToggleParticle(blockPos, false);
        FireMap.TryRemove(blockPos, out _);
    }

    private void ExtinguishBlock(Vector3i blockPos)
    {
        var worldTime = GameManager.Instance.World.GetWorldTime();
        var expiry = worldTime + _smokeTime;

        // Seems like sometimes the dedicated and clients are out of sync, so this is a shot in the dark to see if we just skip the expired position check, and just
        // keep resetting the expired time.
        ExtinguishPositions[blockPos] = expiry;
        FireMap.TryRemove(blockPos, out _);

        var block = GameManager.Instance.World.GetBlock(blockPos);
        ToggleSound(blockPos, false);

        if (block.isair || !(_smokeTime > 0)) return;
        var randomSmokeParticle = GetRandomSmokeParticle(blockPos);
        BlockUtilitiesSDX.addParticlesCentered(randomSmokeParticle, blockPos);
    }


    // Add flammable blocks to the Fire Map
    public void AddBlock(Vector3i blockPos)
    {
        var block = GameManager.Instance.World.GetBlock(blockPos);
        if (!FireMap.TryAdd(blockPos, block)) return;

        ToggleSound(blockPos, true);
        ToggleParticle(blockPos, true);
    }

    public static bool IsBurning(Vector3i blockPos)
    {
        return Instance.Enabled && FireMap.ContainsKey(blockPos);
    }

    private int SaveDataThreaded(ThreadManager.ThreadInfo threadInfo)
    {
        var pooledExpandableMemoryStream =
            (PooledExpandableMemoryStream) threadInfo.parameter;
        var text = $"{GameIO.GetSaveGameDir()}/{SaveFile}";
        if (File.Exists(text))
            File.Copy(text, $"{GameIO.GetSaveGameDir()}/{SaveFile}.bak", true);

        pooledExpandableMemoryStream.Position = 0L;
        StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
        Log.Out("FireManager saved {0} bytes", new object[]
        {
            pooledExpandableMemoryStream.Length
        });
        MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);

        return -1;
    }

    private void Save()
    {
        if (_dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_FireDataSave"))
        {
            Log.Out($"FireManager saving {FireMap.Count} Fires...");
            var pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
            using (var pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
            {
                pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
                Write(pooledBinaryWriter);
            }

            _dataSaveThreadInfo = ThreadManager.StartThread("silent_FireDataSave", null,
                SaveDataThreaded, null,
                System.Threading.ThreadPriority.Normal, pooledExpandableMemoryStream);
        }
        else
        {
            Log.Out("Not Saving. Thread still running?");
        }
    }

    private void Load()
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

        Log.Out($"Fire Manager {path} Loaded: {FireMap.Count}");
    }

    public void Reset()
    {
        Log.Out("Removing all blocks that are on fire and smoke.");
        lock (Locker)
        {
            foreach (var position in FireMap.Keys)
                Remove(position);

            foreach (var position in ExtinguishPositions.Keys)
                BlockUtilitiesSDX.removeParticles(position);

            FireMap.Clear();
            ExtinguishPositions.Clear();
            Save();
        }
    }
}