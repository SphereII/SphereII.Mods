using System;
using System.Collections.Generic;
using UnityEngine;

// A spawn cube is a POI programming marker: an invisible sign block whose text describes what to
// spawn ("ec=zombieBoe;task=Stay;pc=4"), consumed by CheckForSpawn below.
//
// This used to extend BlockSign, which the game abandoned in 3.0 - it creates no tile entity, so
// every GetFeature<TEFeatureSignable>() here returned null and CheckForSpawn bailed at its second
// line. Spawn cubes could not spawn anything. The sign text now lives in the composite tile entity
// declared in SpawnCubeBlocks.xml.
//
// Activation stays gated to editor/debug: in normal play this block offers no commands at all, so
// players can't retarget a POI's spawners by editing the sign.
internal class BlockSpawnCubeSDX : BlockCompositeTileEntity
{
    private const string TriggerCommand = "Trigger";

    private readonly BlockActivationCommand triggerCmd = new BlockActivationCommand(TriggerCommand, "trigger", true);
    private BlockActivationCommand[] designerCmds;

    // Spawn cubes are POI wiring, not player-facing blocks - only expose them to prefab designers.
    private static bool IsDesignerMode(WorldBase _world)
    {
        return _world.IsEditor() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
    }

    private static TEFeatureSignable GetSignable(WorldBase _world, Vector3i _blockPos)
    {
        return (_world.GetTileEntity(_blockPos) as TileEntityComposite)?.GetFeature<TEFeatureSignable>();
    }

    // Seed the sign from the block's Config property, but never overwrite text that is already
    // there: CheckForSpawn writes the spawned entity id and the respawn throttle back into it, and
    // this runs again every time the block's transform is rebuilt (i.e. on every chunk load).
    private void SeedConfigText(WorldBase _world, Vector3i _blockPos)
    {
        if (!Properties.Values.ContainsKey("Config")) return;

        var signable = GetSignable(_world, _blockPos);
        if (signable == null) return;
        if (!string.IsNullOrEmpty(signable.signText?.Text)) return;

        signable.SetText(Properties.Values["Config"]);
    }

    public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        // Trigger is always offered to designers, so don't defer to the feature commands' enabled state.
        return IsDesignerMode(_world);
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (!IsDesignerMode(_world)) return BlockActivationCommand.Empty;

        // "edit" comes from TEFeatureSignable; append the SCore-only force-spawn command.
        var featureCmds = base.GetBlockActivationCommands(_world, _blockValue, _blockPos, _entityFocusing);
        if (designerCmds == null || designerCmds.Length != featureCmds.Length + 1)
            designerCmds = new BlockActivationCommand[featureCmds.Length + 1];

        Array.Copy(featureCmds, designerCmds, featureCmds.Length);
        designerCmds[featureCmds.Length] = triggerCmd;
        return designerCmds;
    }

    public override bool OnBlockActivated(string commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
    {
        if (!IsDesignerMode(_world)) return false;

        // Compared case-insensitively: the command is registered as "Trigger" but the old switch
        // tested for "trigger", so force-spawn never actually ran even when it was reachable.
        if (string.Equals(commandName, TriggerCommand, StringComparison.OrdinalIgnoreCase))
        {
            CheckForSpawn(_world, _blockPos, _blockValue, true);
            return true;
        }

        return base.OnBlockActivated(commandName, _world, _blockPos, _blockValue, _player);
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (!IsDesignerMode(_world)) return "";
        return base.GetActivationText(_world, _blockValue, _blockPos, _entityFocusing);
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

    public override bool UpdateTick(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            var chunkCluster = _world.ChunkCache;
            if (chunkCluster == null) return false;

            if ((Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos) == null) return false;

            if (!Properties.Values.ContainsKey("Config")) return false;
        }
        return base.UpdateTick(_world, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);

    }
    public void CheckForSpawn(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool force = false)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        var TEFeatureSignable = GetSignable(_world, _blockPos);
        if (TEFeatureSignable == null)
            return;

        var signText = TEFeatureSignable.signText.Text;
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

             var rotation = new Vector3(0f, (float)(90 * (_blockValue.rotation & 3)), 0f);

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

                  myEntity = EntityFactory.CreateEntity(EntityID, _blockPos.ToVector3(), rotation) as EntityAlive;
              }
              else
              {
                  myEntity = EntityFactory.CreateEntity(EntityClass.FromString(entityClass), _blockPos.ToVector3(),rotation) as EntityAlive;
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

            // Update the sign with the new entity ID. The throttle has to be layered onto the
            // result of the first call - writing both from signText dropped the entity id, which
            // is what stops the cube respawning while its entity is still alive.
            var newSign = SetValue(signText, "entityid", myEntity.entityId.ToString());
            newSign = SetValue(newSign, "time", (GameManager.Instance.World.GetWorldTime() + 5000).ToString());
            TEFeatureSignable.SetText(newSign);

            var entityCreationData = new EntityCreationData(myEntity);
            entityCreationData.id = -1;
            entityCreationData.rot = rotation;
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

                x.SetRotation(rotation);
                // Center the entity to its block position.
                x.SetPosition(EntityUtilities.CenterPosition(_blockPos));
            }

            // Destroy the block after spawn.
            DamageBlock(GameManager.Instance.World, new BlockValueRef(_blockPos), _blockValue, Block.list[_blockValue.type].MaxDamage, -1, null, false);

        }
        catch (Exception ex)
        {
            Debug.Log("Invalid String on Sign: " + signText + " Example:  ec=zombieBoe;buff=buffOrderStay;pc=0 or  eg=zombiesAll: " + ex);
        }
    }

    public override BlockValue OnBlockPlaced(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
    {
        var blockValue = base.OnBlockPlaced(_world, _blockPos, _blockValue, _rnd);

        // The composite tile entity is created in OnBlockAdded, so it usually does not exist yet
        // here; SeedConfigText no-ops in that case and OnBlockAdded does the work.
        SeedConfigText(_world, _blockPos);
        CheckForSpawn(_world, _blockPos, _blockValue, true);
        return blockValue;
    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue,  PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue,_addedByPlayer);
        SeedConfigText(_world, _blockPos);
        CheckForSpawn(_world, _blockPos, _blockValue, true);
    }

    public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _blockPos, _blockValue);
        CheckForSpawn(_world, _blockPos, _blockValue);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (_ebcd == null)
            return;

        // Hide the sign, so its not visible. Without this, it errors out.
        // This also keeps TEFeatureSignable from wiring a text mesh onto a model that has none -
        // it skips the whole setup when bHasTransform is false. The base call still builds and
        // registers the composite tile entity.
        _ebcd.bHasTransform = false;
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);

        // Re-show the transform. This won't have a visual effect, but fixes when you pick up the block, the outline of the block persists.
        _ebcd.bHasTransform = true;

        SeedConfigText(_world, _blockPos);
    }
}
