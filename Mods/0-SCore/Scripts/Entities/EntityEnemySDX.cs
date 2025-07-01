/*
 * Class: EntityAliveEnemySDX
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base entityAlive. This is meant to be a base class, which other classes can extend
 *      from, giving them the ability to spawn into horde spawners and accept orders.
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features. 
 *
 *      <property name="Class" value="EntityEnemySDX, SCore" />
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// <para>
/// This mod is an extension of the base EntityEnemy. This is meant to be a base class, which other
/// classes can extend from, giving them the ability to spawn into horde spawners and accept orders.
/// </para>
/// <example>
/// Add the following class to entities that are meant to use these features. 
/// <code>
/// &lt;property name="Class" value="EntityEnemySDX, SCore" /&gt;
/// </code>
/// </example>
/// </summary>
public class EntityEnemySDX : EntityHuman, IEntityOrderReceiverSDX
{
    public float flEyeHeight = -1f;
    public bool isAlwaysAwake;
    public Random random = new Random();


    #region Interface Backing Fields
    // NOTE: These backing fields exist to implement the interface. They are not currently being
    // used, and are not being written or read when this entity is serialized. This may change in
    // the future, but only in an update that is understood to break game saves.
    private readonly List<Vector3> _patrolCoordinates = new List<Vector3>();
    private Vector3 _guardPosition = Vector3.zero;
    private Vector3 _guardLookPosition = Vector3.zero;
    // We use a tempList to store the patrol coordinates of each vector, but centered over the block. This allows us to check to make sure each
    // vector we are storing is on a new block, and not just  10.2 and 10.4. This helps smooth out the entity's walk. However, we do want accurate patrol points,
    // so we store the accurate patrol positions for the entity.
    private readonly List<Vector3> _tempList = new List<Vector3>();
    #endregion

    /// <inheritdoc/>
    public List<Vector3> PatrolCoordinates => _patrolCoordinates;

    /// <inheritdoc/>
    public Vector3 GuardPosition { get => _guardPosition; set => _guardPosition = value; }

    /// <inheritdoc/>
    public Vector3 GuardLookPosition { get => _guardLookPosition; set => _guardLookPosition = value; }

    /// <inheritdoc/>
    public Vector3 Position => position;
    public override float GetEyeHeight() {
        if (walkType == 21)
        {
            return 0.15f;
        }
        if (walkType == 22)
        {
            return 0.6f;
        }
        if (!IsCrouching)
        {
            return height * 0.8f;
        }

        return height * 0.5f;
        // return flEyeHeight == -1f ? base.GetEyeHeight() : flEyeHeight;
    }

    // public override float GetEyeHeight()
    // {
    //     
    //     if (flEyeHeight == -1f)
    //         return base.GetEyeHeight();
    //
    //     return flEyeHeight;
    // }

    public override float GetMoveSpeed()
    {
        return IsCrouching
            ? EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, moveSpeed, this)
            : EffectManager.GetValue(PassiveEffects.WalkSpeed, null, moveSpeed, this);
    }

    public override float GetMoveSpeedAggro()
    {
        return IsBloodMoon
            ? EffectManager.GetValue(PassiveEffects.RunSpeed, null, moveSpeedAggroMax, this)
            : EffectManager.GetValue(
                // Horde entities use WalkSpeed, but that messes up melee attacking. Luckily when
                // an entity finds an attack target they're no longer considered part of a horde.
                IsHordeZombie ? PassiveEffects.WalkSpeed : PassiveEffects.RunSpeed,
                null,
                moveSpeedAggro,
                this);
    }

    public override void PostInit()
    {
        base.PostInit();
        SetupStartingItems();
        inventory.SetHoldingItemIdx(0);
        // Does a quick local scan to see what pathing blocks, if any, are nearby. If one is found nearby, then it'll use that code for pathing.
        SetupAutoPathingBlocks();
    }

    /// <summary>
    /// Sets up the starting items. Copied from <see cref="EntityTrader.SetupStartingItems"/>.
    /// </summary>
    protected virtual void SetupStartingItems()
    {
        for (var i = 0; i < itemsOnEnterGame.Count; i++)
        {
            var itemStack = itemsOnEnterGame[i];
            var forId = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6);
            else
                itemStack.count = forId.Stacknumber.Value;
            inventory.SetItem(i, itemStack);
        }
    }

    public override void OnUpdateLive()
    {
        base.OnUpdateLive();

        // Wake them up if they are sleeping, since the trigger sleeper makes them go idle again.
        if (!sleepingOrWakingUp && isAlwaysAwake)
        {
            IsSleeping = true;
            ConditionalTriggerSleeperWakeUp();
        }

        if (!this.isEntityRemote)
        {
            if (!this.IsDead() && this.world.worldTime >= this.timeToDie && !this.attackTarget)
            {
                this.Kill(DamageResponse.New(true));
            }
        }
    }

    public override void OnAddedToWorld()
    {
        base.OnAddedToWorld();

        if (isAlwaysAwake)
        {
            // Set in EntityAlive.TriggerSleeperPose() - resetting here
            IsSleeping = false;
        }
    }

    public override bool IsSavedToFile()
    {
        // Has a leader cvar set, good enough, as the leader may already be disconnected, so we'll fail a GetLeaderOrOwner()
        if (Buffs.HasCustomVar("Leader")) return true;

        // If they have a cvar persist, keep them around.
        if (Buffs.HasCustomVar("Persist")) return true;

        // If its dynamic spawn, don't let them stay.
        if (GetSpawnerSource() == EnumSpawnerSource.Dynamic) return false;

        // If its biome spawn, don't let them stay.
        if (GetSpawnerSource() == EnumSpawnerSource.Biome) return false;
        return true;
    }

    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        var _entityClass = EntityClass.list[entityClass];

        flEyeHeight = EntityUtilities.GetFloatValue(entityId, "EyeHeight");

        isAlwaysAwake = false;
        if (_entityClass.Properties.Values.ContainsKey("SleeperInstantAwake"))
            isAlwaysAwake = StringParsers.ParseBool(_entityClass.Properties.Values["SleeperInstantAwake"], 0, -1, true);

        if (_entityClass.Properties.Values.ContainsKey("IsAlwaysAwake"))
            isAlwaysAwake = StringParsers.ParseBool(_entityClass.Properties.Values["IsAlwaysAwake"], 0, -1, true);
    }

    // Un-comment ONLY when we release a version that can break game saves
    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        try
        {
            var strPatrol = _br.ReadString();
            _patrolCoordinates.Clear();
            _tempList.Clear();
            foreach (var strPatrolPoint in strPatrol.Split(';'))
            {
                var temp = ModGeneralUtilities.StringToVector3(strPatrolPoint);
                if (temp != Vector3.zero)
                    UpdatePatrolPoints(temp); // call this method to also update _tempList
            }

            var strGuardPosition = _br.ReadString();
            _guardPosition = ModGeneralUtilities.StringToVector3(_br.ReadString());
            _guardLookPosition = ModGeneralUtilities.StringToVector3(_br.ReadString());

            Buffs.Read(_br);
        }
        catch (Exception ex)
        {
            Log.Out($"Read exception for: {entityName} ( {entityId} ) : {ex}");
        }
    }

    public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
    {
        base.Write(_bw, _bNetworkWrite);
        try
        {
            var strPatrolCoordinates = "";
            foreach (var temp in _patrolCoordinates) strPatrolCoordinates += ";" + temp;

            _bw.Write(strPatrolCoordinates);
            _bw.Write(_guardPosition.ToString());
            _bw.Write(_guardLookPosition.ToString());
            Buffs.Write(_bw);
        }
        catch (Exception ex)
        {
            Log.Out($"Write exception for: {entityName} ( {entityId} ) : {ex}");
        }
    }

    /// <inheritdoc/>
    public void SetupAutoPathingBlocks()
    {
        // If we already have a pathing code, don't re-scan.
        if (Buffs.HasCustomVar("PathingCode") && (Buffs.GetCustomVar("PathingCode") < 0 || Buffs.GetCustomVar("PathingCode") > 0))
            return;

        // Check if pathing blocks are defined.
        var blocks = EntityUtilities.ConfigureEntityClass(entityId, "PathingBlocks");
        if (blocks.Count == 0)
            blocks = new List<string> { "PathingCube", "PathingCube2" };

        //Scan for the blocks in the area
        var pathingVectors = ModGeneralUtilities.ScanAutoConfigurationBlocks(position, blocks, 2);
        if (pathingVectors == null || pathingVectors.Count == 0) return;

        // Find the nearest block, and if its a sign, read its code.
        var target = ModGeneralUtilities.FindNearestBlock(position, pathingVectors);
        var tileEntitySign = GameManager.Instance.World.GetTileEntity(0, new Vector3i(target)) as TileEntitySign;
        if (tileEntitySign == null) return;

        var text = tileEntitySign.signText.Text;

        // We need to apply the buffs during this scan, as the creation of the entity + adding buffs is not really MP safe.
        var task = PathingCubeParser.GetValue(text, "task");
        if (!string.IsNullOrEmpty(task))
        {
            Log.Out($"SetupAutoPathingBlocks for: {entityName} ( {entityId} ) : Task: {task}");
            // We need to apply the buffs during this scan, as the creation of the entity + adding buffs is not really MP safe.
            if (task.ToLower() == "stay")
                Buffs.AddBuff("buffOrderStay", -1, false);
            else if (task.ToLower() == "wander")
                Buffs.AddBuff("buffOrderWander", -1, false);
            else if (task.ToLower() == "guard")
                // Use the buff that issues the "guard" order, not the one that issues the "stay" order
                Buffs.AddBuff("buffOrderGuard", -1, false);
            // This entity can't accept the "follow" task.
            else
                Log.Out($"    Entity: {entityName} ( {entityId} ) : Cannot perform task: {task}");
        }

        var Buff = PathingCubeParser.GetValue(text, "buff");
        foreach (var buff in Buff.Split(','))
            Buffs.AddBuff(buff, -1, false);


        // Set up the pathing code.
        Buffs.SetCustomVar("PathingCode", -1f);

        var PathingCode = PathingCubeParser.GetValue(text, "pc");
        if (StringParsers.TryParseFloat(PathingCode, out var pathingCode))
            Buffs.SetCustomVar("PathingCode", pathingCode);
    }

    /// <inheritdoc/>
    public void UpdatePatrolPoints(Vector3 position)
    {
        // Center the x and z values of the passed in blocks for a unique check.
        var temp = EntityUtilities.CenterPosition(position);

        if (!_tempList.Contains(temp))
        {
            _tempList.Add(temp);
            if (!_patrolCoordinates.Contains(position))
                _patrolCoordinates.Add(position);
        }
    }

    //public override void AwardKill(EntityAlive killer)
    //{
    //    if (killer != null && killer != this)
    //    {
    //        EntityPlayer entityPlayer = killer as EntityPlayer;
    //        if (entityPlayer)
    //        {
    //            if (!entityPlayer.isEntityRemote)
    //                SCoreQuestEventManager.Instance.EntityEnemyKilled(EntityClass.list[entityClass].entityClassName);
    //        }
    //    }
    //    base.AwardKill(killer);
    //}
}