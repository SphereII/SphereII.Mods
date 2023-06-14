using JetBrains.Annotations;
using System;
using System.Linq;
using UnityEngine;

public class BlockMortSpawner : BlockLoot
{
    private GameObject GameObject;
    public string PropLootable = "Lootable";
    public string PropParticleAction = "ParticleAction";

    private MortSpawnerScript Script;

    private static bool Stopped(byte _metadata)
    {
        return (_metadata & (1 << 1)) != 0;
    }

    private static bool WasLooted(byte _metadata)
    {
        return (_metadata & (1 << 2)) != 0;
    }

    private static void SetTag(Transform root, Transform t, string tag)
    {
        t.tag = tag;
        foreach (Transform transform in t) SetTag(root, transform, tag);

        if (root == t) return;

        var go = t.gameObject.gameObject;
        var c = go.GetComponent<RootTransformRefParent>();
        if (c != null) return;

        c = go.AddComponent<RootTransformRefParent>();
        c.RootTransform = root;
    }

    private void AddScript(WorldBase _world, Vector3i _blockPos, BlockEntityData _ebcd)
    {
        if (_world.IsRemote()) return;

        if (_ebcd == null) _ebcd = _world.ChunkClusters[0].GetBlockEntity(_blockPos);

        if (_ebcd == null || _ebcd.transform == null) return;

        GameObject = _ebcd.transform.gameObject;
        if (GameObject == null) return;

        Script = GameObject.GetComponent<MortSpawnerScript>() ?? GameObject.AddComponent<MortSpawnerScript>();
        Script.Initialize(_world, _blockPos);
    }

    private void RemoveScript(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockEntityData _ebcd)
    {
        if (_world.IsRemote()) return;

        if (_ebcd == null) _ebcd = _world.ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);

        if (_ebcd == null || _ebcd.transform == null) return;

        GameObject = _ebcd.transform.gameObject;
        if (GameObject == null) return;

        Script = GameObject.GetComponent<MortSpawnerScript>();
        if (Script == null) return;

        Script.KillScript();
    }

    private void CheckParticles(BlockEntityData _ebcd)
    {
        var particleAction = "None";
        if (Properties.Values.ContainsKey(PropParticleAction)) particleAction = Properties.Values[PropParticleAction];
        if (particleAction == "None") return;

        var setState = particleAction == "Start";

        if (_ebcd == null || !_ebcd.bHasTransform) return;

        var componentsInChildren = _ebcd.transform.GetComponentsInChildren<Transform>(true);
        if (componentsInChildren == null) return;

        foreach (var transform in componentsInChildren)
        {
            if (transform.name != "particles") continue;

            transform.gameObject.SetActive(setState);
            break;
        }
    }


    #region overrides

    public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
    {
        base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
        if (!Stopped(_newBlockValue.meta2) || GameManager.IsDedicatedServer) return;

        CheckParticles(_world.ChunkClusters[_clrIdx].GetBlockEntity(_blockPos));
    }
    //public override void OnBlockValueChanged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
    //{
    //    base.OnBlockValueChanged(_world, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);

    //    if (!Stopped(_newBlockValue.meta2) || GameManager.IsDedicatedServer) return;

    //    CheckParticles(_world.ChunkClusters[_clrIdx].GetBlockEntity(_blockPos));
    //}
    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        RemoveScript(world, _chunk.ClrIdx, _blockPos, null);

        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        RemoveScript(_world, _clrIdx, _blockPos, null);

        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
    }

    public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
    {
        base.ForceAnimationState(_blockValue, _ebcd);

        if (Stopped(_blockValue.meta2) && !GameManager.IsDedicatedServer) CheckParticles(_ebcd);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);

        AddScript(_world, _blockPos, null);
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);

        AddScript(world, _blockPos, null);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);

        SetTag(_ebcd.transform, _ebcd.transform, "T_Block");
        AddScript(_world, _blockPos, _ebcd);
    }

    public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo attackHitInfo,
        bool _bByPassMaxDamage, bool bBypassMaxDamage, int recDepth)
    {
        var chunkCluster = _world.ChunkClusters[_clrIdx];
        if (chunkCluster == null) return 0;

        if (isMultiBlock && _blockValue.ischild)
        {
            var parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
            var block = chunkCluster.GetBlock(parentPos);

            if (block.ischild) Debug.Log("Block on position " + parentPos + " should be a parent but is not!");

            return block.ischild ? 0 : list[block.type].OnBlockDamaged(_world, _clrIdx, parentPos, block, _damagePoints, _entityIdThatDamaged, attackHitInfo, false, bBypassMaxDamage, recDepth);
        }

        var d = _blockValue.damage;
        var max = list[_blockValue.type].MaxDamage;

        if (d >= max || d + _damagePoints < max)
            return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, attackHitInfo, false, _bByPassMaxDamage, recDepth);

        chunkCluster.InvokeOnBlockDamagedDelegates(_blockPos, _blockValue, _damagePoints, _entityIdThatDamaged);

        if (!Stopped(_blockValue.meta2))
        {
            _blockValue.damage = 0;
            _blockValue.meta2 = (byte)(_blockValue.meta2 | (1 << 1));

            _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
            return 0;
        }

        if (OnBlockDestroyedBy(_world, _clrIdx, _blockPos, _blockValue, _entityIdThatDamaged, false) == DestroyedResult.Remove)
            return max;

        SpawnDestroyParticleEffect(_world, _blockValue, _blockPos, _world.GetLightBrightness(_blockPos + new Vector3i(0, 1, 0)), GetColorForSide(_blockValue, BlockFace.Top), _entityIdThatDamaged);

        if (DowngradeBlock.type == 0)
        {
            _world.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);

            //todo: should this be 0? seems a bug to return the old types max damage
            return list[_blockValue.type].MaxDamage;
        }

        //todo:update to include paint and density?
        var downgrade = DowngradeBlock;
        downgrade.rotation = _blockValue.rotation;
        downgrade.meta = _blockValue.meta;
        if (list[downgrade.type].shape.IsTerrain())
            _world.SetBlockRPC(_clrIdx, _blockPos, downgrade, list[downgrade.type].Density);
        else
            _world.SetBlockRPC(_clrIdx, _blockPos, downgrade);

        return list[_blockValue.type].MaxDamage;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var lootable = false;
        if (Properties.Values.ContainsKey(PropLootable) && !bool.TryParse(Properties.Values[PropLootable], out lootable))
            Debug.Log("Unable to parse " + PropLootable + " as a bool in " + _blockValue.Block.GetBlockName());

        return !lootable
            ? string.Empty
            : WasLooted(_blockValue.meta2)
                ? "Empty..."
                : !Stopped(_blockValue.meta2)
                    ? "Locked!"
                    : base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
    }

    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (!Stopped(_blockValue.meta2) || WasLooted(_blockValue.meta2)) return false;

        var lootable = false;
        if (Properties.Values.ContainsKey(PropLootable) && !bool.TryParse(Properties.Values[PropLootable], out lootable))
            Debug.Log("Unable to parse " + PropLootable + " as a bool in " + _blockValue.Block.GetBlockName());
        if (!lootable) return false;

        _blockValue.meta2 = (byte)(_blockValue.meta2 | (1 << 2));
        _world.SetBlockRPC(_cIdx, _blockPos, _blockValue);

        return base.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
    }

    #endregion
}

public class MortSpawnerScript : MonoBehaviour
{
    protected static string PropBuffs = "Buffs";
    protected static string PropRadiationDamage = "RadiationDamage";
    protected static string PropNumberToPause = "NumberToPause";
    protected static string PropPauseTime = "PauseTime";
    protected static string PropCheckArea = "CheckArea";
    protected static string PropRadiationArea = "RadiationArea";
    protected static string PropSpawnArea = "SpawnArea";
    protected static string PropEntityGroup = "EntityGroup";
    protected static string PropMaxSpawned = "MaxSpawned";
    protected static string PropNumberToSpawn = "NumberToSpawn";
    protected static string PropSpawnRadius = "SpawnRadius";
    protected static string PropTickRate = "TickRate";
    private BlockValue BlockValue;

    private string Buffs;
    private uint CheckArea;
    private uint CheckRadius;
    private string EntityGroup;
    private uint MaxSpawn;
    private ulong NextBuff;
    private ulong NextSpawn;
    private ulong NextTick;
    private uint NumberToPause;
    private uint NumberToPauseCounter;
    private uint NumberToSpawn;
    private uint PauseTime;

    private uint PauseTimeMinutes;

    // private List<MultiBuffClassAction> BuffActions;
    private Vector3i Position;
    private Vector3 Positionf;
    private uint RadiationArea;
    private uint RadiationDamage;
    private uint SpawnArea;
    private int SpawnerType;
    private ulong TickRate;
    private uint TickRateSeconds;

    [UsedImplicitly]
    private void Update()
    {
        if (GameTimer.Instance.ticks <= NextTick) return;

        NextTick = GameTimer.Instance.ticks + TickRate;

        if (GameManager.Instance.IsPaused()) return;

        var world = GameManager.Instance.World;
        if (world == null) return;

        BlockValue = world.GetBlock(0, Position);

        //block is no longer our spawner block, remove the script
        if (BlockValue.type != SpawnerType)
        {
            KillScript();

            return;
        }

        var spawnNeeded = false;
        uint numberSpawned = 0;

        if (CheckRadius > 0 && (NumberToSpawn > 0 || RadiationArea > 0) && MaxSpawn > 0 && CheckArea > 0)
        {
            foreach (var player in world.Players.list)
            {
                if (player == null || !player.IsAlive() || !player.IsSpawned()) continue;

                var distance = Vector3.Distance(player.position, Positionf);

                if (RadiationArea > 0 && RadiationArea >= distance) DoBuffsAndRadiation(player);

                if (NumberToSpawn > 0 && CheckRadius >= distance && !Stopped()) spawnNeeded = true;
            }

            if (!Stopped())
                foreach (var entity in world.Entities.list.OfType<EntityEnemy>())
                {
                    if (!entity.IsAlive()) continue;

                    if (Vector3.Distance(entity.position, Positionf) <= CheckArea) numberSpawned++;
                }
        }

        if (GameTimer.Instance.ticks <= NextSpawn) return;

        NextSpawn = NextTick;

        if (spawnNeeded && numberSpawned < MaxSpawn)
        {
            if (MaxSpawn - numberSpawned < NumberToSpawn) NumberToSpawn = MaxSpawn - numberSpawned;

            var areaSize = new Vector3(SpawnArea, SpawnArea, SpawnArea);

            for (var i = 0; i < NumberToSpawn; ++i)
            {
                int x;
                int y;
                int z;
                if (!world.FindRandomSpawnPointNearPosition(Position.ToVector3(), 15, out x, out y, out z, areaSize, true)) continue;

                var ClassID = 0;
                var entityId = EntityGroups.GetRandomFromGroup(EntityGroup, ref ClassID);
                var spawnEntity = EntityFactory.CreateEntity(entityId, new Vector3(x, y, z));
                spawnEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner, 0, EntityGroup);
                world.SpawnEntityInWorld(spawnEntity);

                if (NumberToPauseCounter > 0) NumberToPauseCounter--;
            }
        }

        if (NumberToPauseCounter > 0 || PauseTime == 0) return;

        NumberToPauseCounter = NumberToPause;

        NextSpawn = GameTimer.Instance.ticks + PauseTime;

        if (PauseTimeMinutes >= 999)
        {
            BlockValue.meta2 = (byte)(BlockValue.meta2 | (1 << 1));
            GameManager.Instance.World.SetBlockRPC(Position, BlockValue);
        }

        Alzheimer();
    }

    public void KillScript()
    {
        if (gameObject.GetComponent<MortSpawnerScript>() != null) Destroy(this);
    }

    private bool Stopped()
    {
        return (BlockValue.meta2 & (1 << 1)) != 0;
    }

    public void Initialize(WorldBase _world, Vector3i _blockPos)
    {
        Position = _blockPos;
        Positionf = Position.ToVector3();

        BlockValue = _world.GetBlock(0, Position);
        SpawnerType = BlockValue.type;

        var props = Block.list[BlockValue.type].Properties.Values;
        var blockName = Block.list[BlockValue.type].GetBlockName();

        TickRateSeconds = 10;
        if (props.ContainsKey(PropTickRate) && !uint.TryParse(props[PropTickRate], out TickRateSeconds)) Debug.Log("Unable to parse " + PropTickRate + " as a uint in " + blockName);
        TickRate = TickRateSeconds * 20uL;

        CheckRadius = 10;
        if (props.ContainsKey(PropSpawnRadius) && !uint.TryParse(props[PropSpawnRadius], out CheckRadius)) Debug.Log("Unable to parse " + PropSpawnRadius + " as an int in " + blockName);

        NumberToSpawn = 6;
        if (props.ContainsKey(PropNumberToSpawn) && !uint.TryParse(props[PropNumberToSpawn], out NumberToSpawn)) Debug.Log("Unable to parse " + PropNumberToSpawn + " as an int in " + blockName);

        MaxSpawn = 10;
        if (props.ContainsKey(PropMaxSpawned) && !uint.TryParse(props[PropMaxSpawned], out MaxSpawn)) Debug.Log("Unable to parse " + PropMaxSpawned + " as an int in " + blockName);

        EntityGroup = !props.ContainsKey(PropEntityGroup) ? string.Empty : props[PropEntityGroup];

        SpawnArea = 10;
        if (props.ContainsKey(PropSpawnArea) && !uint.TryParse(props[PropSpawnArea], out SpawnArea)) Debug.Log("Unable to parse " + PropSpawnArea + " as an int in " + blockName);

        RadiationArea = 0;
        if (props.ContainsKey(PropRadiationArea) && !uint.TryParse(props[PropRadiationArea], out RadiationArea)) Debug.Log("Unable to parse " + PropRadiationArea + " as an int in " + blockName);

        CheckArea = 10;
        if (props.ContainsKey(PropCheckArea) && !uint.TryParse(props[PropCheckArea], out CheckArea)) Debug.Log("Unable to parse " + PropCheckArea + " as an int in " + blockName);

        PauseTimeMinutes = 0;
        if (props.ContainsKey(PropPauseTime) && !uint.TryParse(props[PropPauseTime], out PauseTimeMinutes)) Debug.Log("Unable to parse " + PropPauseTime + " as an int in " + blockName);
        PauseTime = PauseTimeMinutes * 1200;

        NumberToPause = 0;
        if (props.ContainsKey(PropNumberToPause) && !uint.TryParse(props[PropNumberToPause], out NumberToPause)) Debug.Log("Unable to parse " + PropNumberToPause + " as an int in " + blockName);
        NumberToPauseCounter = NumberToPause;

        RadiationDamage = 0;
        if (props.ContainsKey(PropRadiationDamage) && !uint.TryParse(props[PropRadiationDamage], out RadiationDamage))
            Debug.Log("Unable to parse " + PropRadiationDamage + " as an int in " + blockName);

        //buffs
        Buffs = !props.ContainsKey(PropBuffs) ? string.Empty : props[PropBuffs];
        if (Buffs == string.Empty) return;

        var buffList = Buffs.Split(',');
        //if (BuffActions == null) BuffActions = new List<MultiBuffClassAction>();
        //foreach (var buffItem in buffList)
        //{
        //  if (!MultiBuffClass.s_classes.ContainsKey(buffItem.Trim())) continue;

        //  BuffActions.Add(MultiBuffClassAction.NewAction(buffItem.Trim()));
        //}
    }

    private void DoBuffsAndRadiation(EntityAlive player)
    {
        if (RadiationDamage > 0) player.DamageEntity(DamageSource.radiation, (int)RadiationDamage, false);

        //  if (BuffActions == null || BuffActions.Count == 0 || GameTimer.Instance.ticks <= NextBuff) return;

        NextBuff = GameTimer.Instance.ticks + 200;

        //using (var enumerator = BuffActions.GetEnumerator())
        //{
        //  while (enumerator.MoveNext())
        //  {
        //    if (enumerator.Current != null) enumerator.Current.Execute(BlockValue.type, player, false, EnumBodyPartHit.None, null);
        //  }
        //}
    }

    private static void Alzheimer()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}