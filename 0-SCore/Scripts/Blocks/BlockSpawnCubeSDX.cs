using System;
using System.Collections.Generic;
using UnityEngine;

internal class BlockSpawnCubeSDX : BlockPlayerSign
{
    // public string SpawnGroup;
    // public string task = "Wander";

    private readonly BlockActivationCommand[] cmds =
    {
        new BlockActivationCommand("edit", "pen", true),
        new BlockActivationCommand("Trigger", "trigger", true)
    };

    public override bool OnBlockActivated(string commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            var parentPos = list[_blockValue.type].multiBlockPos.GetParentPos(_blockPos, _blockValue);
            var block = _world.GetBlock(parentPos);
            return base.OnBlockActivated(commandName, _world, _cIdx, parentPos, block, _player);
        }

        var tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null) return false;
        switch (commandName)
        {
            case "edit":
                return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
            case "trigger":
                CheckForSpawn(_world, _cIdx, _blockPos, _blockValue, true);
                break;
            default:
                return false;
        }

        return true;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (_world.IsEditor() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
            return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
        return "";
    }

    //private BlockActivationCommand[] cmds = new BlockActivationCommand[0];
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var tileEntitySign = (TileEntitySign)_world.GetTileEntity(_clrIdx, _blockPos);
        if (tileEntitySign == null) return new BlockActivationCommand[0];
        if (_world.IsEditor() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)) return cmds;

        //        Debug.Log("No commands.");
        return new BlockActivationCommand[0];
    }

    public string SetValue(string signText, string key, string value)
    {
        var newSign = "";
        // If the sign doesn't have the key, then just add it, and return it.
        if (!signText.Contains(key + "="))
        {
            signText += ";" + key + "=" + value;
            return signText;
        }

        // Loop through the text
        foreach (var text in signText.Split(';'))
        {
            var parse = text.Split('=');
            if (parse.Length == 2)
            {
                if (parse[0].ToLower() == key.ToLower())
                    parse[1] = value;

                newSign += parse[0] + "=" + parse[1] + ";";
            }
        }

        // Remove the trail semo-colon
        newSign.TrimEnd(';');
        return newSign;
    }

    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            var chunkCluster = _world.ChunkClusters[_clrIdx];
            if (chunkCluster == null) return false;

            if ((Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos) == null) return false;

            if (!Properties.Values.ContainsKey("Config")) return false;
        }
        return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);

    }
    public void CheckForSpawn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool force = false)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        var tileEntitySign = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null)
            return;

        var signText = tileEntitySign.GetText();
        var entityClassID = PathingCubeParser.GetValue(signText, "entityid");

        // If there's already an entityID, check 
        if (!string.IsNullOrEmpty(entityClassID))
            // make sure its an int.
            if (StringParsers.TryParseSInt32(entityClassID, out var entityid))
            {
                // Check if the entity is still spawned, and if so, don't respawn.
                var spawnedEntity = GameManager.Instance.World.GetEntity(entityid);
                if (spawnedEntity != null)
                    return;
            }

        EntityAlive myEntity = null;
        // entityclass:zombieWightFeral;task:wander
        if (string.IsNullOrEmpty(signText))
            return;

        try
        {
            // Read the entity class
            // ec = entityclass:   ec=zombieBoe
            // eg = entitygroup:   eg=ZombiesAll
            // task = Tasks:        Wander, Stay
            // pc : Pathing Code:  pc=3
            // Sign String:     ec=zombieBoe;task=Stay;pc=4
            var entityClass = PathingCubeParser.GetValue(signText, "ec");
            var entityGroup = PathingCubeParser.GetValue(signText, "eg");
            var Task = PathingCubeParser.GetValue(signText, "task");
            var Buff = PathingCubeParser.GetValue(signText, "buff");
            var PathingCode = PathingCubeParser.GetValue(signText, "pc");

            // Set up a throttle time
            var ThrottleTime = PathingCubeParser.GetValue(signText, "time");

            // Default the float throttle time to be in the past; this will get updated if its parsed correctly.
            float throttleTime = GameManager.Instance.World.GetWorldTime() + 100;
            if (!string.IsNullOrEmpty(ThrottleTime))
                throttleTime = StringParsers.ParseFloat(ThrottleTime);

           // Debug.Log("Throttle Time: " + ThrottleTime + " " + throttleTime + " World Time: " + GameManager.Instance.World.GetWorldTime());
            if (!force && throttleTime > GameManager.Instance.World.GetWorldTime())
            {
                Debug.Log("World time not expired.");
                return;
            }

            // If the class is empty, check to see if we have a group to spawn from.
            if (string.IsNullOrEmpty(entityClass))
            {
                // No entity class or group? Do nothing.
                if (string.IsNullOrEmpty(entityGroup))
                    return;

                var ClassID = 0;
                var EntityID = EntityGroups.GetRandomFromGroup(entityGroup, ref ClassID);
                if (EntityID == 0) // Invalid group.
                    return;

                myEntity = EntityFactory.CreateEntity(EntityID, _blockPos.ToVector3()) as EntityAlive;
            }
            else
            {
                myEntity = EntityFactory.CreateEntity(EntityClass.FromString(entityClass), _blockPos.ToVector3()) as EntityAlive;
            }

            // Not a valid entity.
            if (myEntity == null)
                return;

            // Set a Wander task is not defined.
            if (string.IsNullOrEmpty(Task))
                Task = "Wander";

            if (myEntity is IEntityOrderReceiverSDX entityOrderReceiver)
            {
                // If there's a pathing code, set, otherwise, do a scan.
                if (string.IsNullOrEmpty(PathingCode))
                {
                    entityOrderReceiver.SetupAutoPathingBlocks();
                }
                else
                {
                    if (StringParsers.TryParseFloat(PathingCode, out var pathingCode))
                        myEntity.Buffs.SetCustomVar("PathingCode", pathingCode);
                }
            }

            // Update the sign with the new entity ID.
            var newSign = SetValue(signText, "entityid", myEntity.entityId.ToString());
            newSign = SetValue(signText, "time", (GameManager.Instance.World.GetWorldTime() + 5000).ToString());
            tileEntitySign.SetText(newSign);

            var entityCreationData = new EntityCreationData(myEntity);
            entityCreationData.id = -1;
            GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
            myEntity.OnEntityUnload();

            var nearbyEntities = new List<Entity>();

            // Search in the bounds are to try to find the most appealing entity to follow.
            var bb = new Bounds(_blockPos, new Vector3(2, 2,2));

            GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), bb, nearbyEntities);
            for (var i = nearbyEntities.Count - 1; i >= 0; i--)
            {
                var x = nearbyEntities[i] as EntityAlive;

                if (x == null) continue;
                if (x.entityClass == myEntity.entityId) continue;

                // We need to apply the buffs during this scan, as the creation of the entity + adding buffs is not really MP safe.
                if (Task.ToLower() == "stay")
                    x.Buffs.AddBuff("buffOrderStay");
                if (Task.ToLower() == "wander")
                    x.Buffs.AddBuff("buffOrderWander");
                if (Task.ToLower() == "guard")
                    // Use the buff that issues the "guard" order, not the one that issues the "stay" order
                    x.Buffs.AddBuff("buffOrderGuard");

                if (Task.ToLower() == "follow")
                    x.Buffs.AddBuff("buffOrderFollow");

                if (!string.IsNullOrEmpty(Buff))
                    x.Buffs.AddBuff(Buff);

                // Center the entity to its block position.
                x.SetPosition(EntityUtilities.CenterPosition(_blockPos));
            }

            // Destroy the block after spawn.
            DamageBlock(GameManager.Instance.World, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage, -1, null, false);

        }
        catch (Exception ex)
        {
            Debug.Log("Invalid String on Sign: " + signText + " Example:  ec=zombieBoe;buff=buffOrderStay;pc=0 or  eg=zombiesAll: " + ex);
        }
    }

    public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
    {
        var blockValue= base.OnBlockPlaced(_world, _clrIdx, _blockPos, _blockValue, _rnd);
        var tileEntitySign = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign != null)
        {
            if (Properties.Values.ContainsKey("Config"))
            {
                tileEntitySign.SetText(Properties.Values["Config"]);
                CheckForSpawn(_world, _clrIdx, _blockPos, _blockValue, true);
            }
        }
        return blockValue;
    }
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        var tileEntitySign = _world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign != null)
            if (Properties.Values.ContainsKey("Config"))
            { 
                tileEntitySign.SetText(Properties.Values["Config"]);
                CheckForSpawn(_world, _chunk.ClrIdx, _blockPos, _blockValue, true);
            }
        
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        CheckForSpawn(_world, _clrIdx, _blockPos, _blockValue);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (_ebcd == null)
            return;

        var chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
        if (chunk == null)
            return;

        var tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null)
        {
            tileEntitySign = new TileEntitySign(chunk);
            if (tileEntitySign != null)
            {
                if (Properties.Values.ContainsKey("Config"))
                    tileEntitySign.SetText(Properties.Values["Config"]);

                tileEntitySign.localChunkPos = World.toBlock(_blockPos);
                chunk.AddTileEntity(tileEntitySign);
            }
        }

        // Hide the sign, so its not visible. Without this, it errors out.
        _ebcd.bHasTransform = false;
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);

        // Re-show the transform. This won't have a visual effect, but fixes when you pick up the block, the outline of the block persists.
        _ebcd.bHasTransform = true;
    }
}