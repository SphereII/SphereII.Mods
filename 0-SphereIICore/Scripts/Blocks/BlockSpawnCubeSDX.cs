using Audio;
using System;
using UnityEngine;

class BlockSpawnCubeSDX : BlockPlayerSign
{
    public string SpawnGroup;
    public string Task = "Wander";

    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
{
    new BlockActivationCommand("edit", "pen", true),
    new BlockActivationCommand("Trigger", "trigger", true)

};

   
    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = Block.list[_blockValue.type].multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return this.OnBlockActivated(_indexInBlockActivationCommands, _world, _cIdx, parentPos, block, _player);
        }
        TileEntitySign tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null)
        {
            return false;
        }
        switch (_indexInBlockActivationCommands)
        {
            case 0:
                return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
            case 1:
                CheckForSpawn(_world, _cIdx, _blockPos, _blockValue);
                break;
            default:
                return false;
        }

        return true;
    }
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (_world.IsEditor() || _entityFocusing.IsGodMode.Value)
            return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
        else
            return "";
    }
    //private BlockActivationCommand[] cmds = new BlockActivationCommand[0];
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        TileEntitySign tileEntitySign = (TileEntitySign)_world.GetTileEntity(_clrIdx, _blockPos);
        if (tileEntitySign == null)
        {
            return new BlockActivationCommand[0];
        }
        if (_world.IsEditor() || _entityFocusing.IsGodMode.Value)
        {
            return cmds;
        }

        //        Debug.Log("No commands.");
        return new BlockActivationCommand[0];
    }

    public string GetValue(String signText, String key)
    {
        foreach (String text in signText.Split(';'))
        {
            string[] parse = text.Split('=');
            if (parse.Length == 2)
            {
                if (parse[0].ToLower() == key.ToLower())
                    return parse[1];
            }
        }
        return "";
    }

    public string SetValue(String signText, String key, String value)
    {
        String newSign = "";
        // If the sign doesn't have the key, then just add it, and return it.
        if (!signText.Contains(key + "="))
        {
            signText += ";" + key + "=" + value;
            return signText;
        }

        // Loop through the text
        foreach (String text in signText.Split(';'))
        {
            string[] parse = text.Split('=');
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
    public void CheckForSpawn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        TileEntitySign tileEntitySign = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null)
            return;

        string signText = tileEntitySign.GetText();
        String entityClassID = GetValue(signText, "entityid");

        // If there's already an entityID, check 
        if (!String.IsNullOrEmpty(entityClassID))
        {
            // make sure its an int.
            if (StringParsers.TryParseSInt32(entityClassID, out int entityid))
            {
                // Check if the entity is still spawned, and if so, don't respawn.
                Entity spawnedEntity = GameManager.Instance.World.GetEntity(entityid);
                if (spawnedEntity != null)
                    return;
            }

        }

        Entity myEntity = null;
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
            String entityClass = GetValue(signText, "ec");
            String entityGroup = GetValue(signText, "eg");
            String Task = GetValue(signText, "task");
            String PathingCode = GetValue(signText, "pc");

            // Set up a throttle time
            String ThrottleTime = GetValue(signText, "time");

            // Default the float throttle time to be in the past; this will get updated if its parsed correctly.
            float throttleTime = GameManager.Instance.World.GetWorldTime() + 100;
            if (!String.IsNullOrEmpty(ThrottleTime))
                throttleTime = StringParsers.ParseFloat(ThrottleTime);

            Debug.Log("Throttle Time: " + ThrottleTime + " " + throttleTime + " World Time: " + GameManager.Instance.World.GetWorldTime());
            if (throttleTime > GameManager.Instance.World.GetWorldTime())
            {
                Debug.Log("World time not expired.");
                return;
            }
            // If the class is empty, check to see if we have a group to spawn from.
            if (String.IsNullOrEmpty(entityClass))
            {
                // No entity class or group? Do nothing.
                if (String.IsNullOrEmpty(entityGroup))
                    return;

                int ClassID = 0;
                int EntityID = EntityGroups.GetRandomFromGroup(entityGroup, ref ClassID);
                if (EntityID == 0) // Invalid group.
                    return;
                myEntity = EntityFactory.CreateEntity(EntityID, _blockPos.ToVector3());
            }
            else
            {
                myEntity = EntityFactory.CreateEntity(EntityClass.FromString(entityClass), _blockPos.ToVector3());
            }

            // Not a valid entity.
            if (myEntity == null)
                return;

            // Set a Wander task is not defined.
            if (String.IsNullOrEmpty(Task))
                Task = "Wander";

            EntityAliveSDX entityAliveSDX = myEntity as EntityAliveSDX;
            if (entityAliveSDX != null)
            {
                // If there's a pathing code, set, otherwise, do a scan.
                if (String.IsNullOrEmpty(PathingCode))
                {
                    entityAliveSDX.SetupAutoPathingBlocks();
                }
                else
                {
                    if (StringParsers.TryParseFloat(PathingCode, out float pathingCode))
                        entityAliveSDX.Buffs.SetCustomVar("PathingCode", pathingCode);
                }
            }

            // Update the sign with the new entity ID.
            String newSign = SetValue(signText, "entityid", myEntity.entityId.ToString());
            newSign = SetValue(signText, "time", (GameManager.Instance.World.GetWorldTime() + 5000).ToString() );
            tileEntitySign.SetText(newSign);

            GameManager.Instance.World.SpawnEntityInWorld(myEntity);
            if (Task.ToLower() == "stay")
                EntityUtilities.SetCurrentOrder(myEntity.entityId, EntityUtilities.Orders.Stay);
            if (Task.ToLower() == "wander")
                EntityUtilities.SetCurrentOrder(myEntity.entityId, EntityUtilities.Orders.Wander);


        }
        catch (Exception ex)
        {
            Debug.Log("Invalid String on Sign: " + signText + " Example:  ec=zombieBoe;task=Wander;pc=0 or  eg=zombiesAll");
            return;
        }



    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        TileEntitySign tileEntitySign = _world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign != null)
        {
            if (this.Properties.Values.ContainsKey("Config"))
                tileEntitySign.SetText(this.Properties.Values["Config"]);
        }
        CheckForSpawn(_world, _chunk.ClrIdx, _blockPos, _blockValue);
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

        Chunk chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
        if (chunk == null)
            return;

        TileEntitySign tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null)
        {
            tileEntitySign = new TileEntitySign(chunk);
            if (tileEntitySign != null)
            {
                if (this.Properties.Values.ContainsKey("Config"))
                    tileEntitySign.SetText(this.Properties.Values["Config"]);

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

