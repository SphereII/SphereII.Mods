using Audio;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class BlockPoweredPortal : BlockPowered
{ 

    private string buffCooldown = "buffTeleportCooldown";
    private int delay = 1000;
    private string location;
    private bool display = false;
    private string displayBuff = "";
    public ChunkManager.ChunkObserver ChunkObserver;


    public BlockPoweredPortal()
    {
        this.HasTileEntity = true;
    }
    public override void Init()
    {
        if (Properties.Values.ContainsKey("CooldownBuff"))
            buffCooldown = Properties.Values["CooldownBuff"];

        if (Properties.Values.ContainsKey("Delay"))
        {
            var delayString = Properties.Values["Delay"];
            delay = StringParsers.ParseSInt32(delayString);
        }

        if (Properties.Values.ContainsKey("Location"))
            location = Properties.Values["Location"];

        if (Properties.Values.ContainsKey("Display"))
             display = StringParsers.ParseBool(Properties.Values["Display"]);

        if (Properties.Values.ContainsKey("DisplayBuff"))
            displayBuff = Properties.Values["DisplayBuff"];

        
        base.Init();
    }

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        base.PlaceBlock(_world, _result, _ea);
        var tileEntity = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityPoweredPortal;
        tileEntity?.SetOwner(PlatformManager.InternalLocalUserIdentifier);
        ChunkObserver = GameManager.Instance.AddChunkObserver(_result.blockPos, true, 1, -1);
    }

    public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
    {
        if (_blockPos.y > 253)
        {
            return false;
        }
        if (!GameManager.Instance.IsEditMode() && ((World)_world).IsWithinTraderArea(_blockPos))
        {
            return false;
        }
        var block = _blockValue.Block;
        return (!block.isMultiBlock || _blockPos.y + block.multiBlockPos.dim.y < 254) && (GameManager.Instance.IsEditMode() || _bOmitCollideCheck || !this.overlapsWithOtherBlock(_world, _clrIdx, _blockPos, _blockValue));
    }
    private bool overlapsWithOtherBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        if (!this.isMultiBlock)
        {
            int type = _world.GetBlock(_clrIdx, _blockPos).type;
            return type != 0 && !Block.list[type].blockMaterial.IsGroundCover && !_world.IsWater(_blockPos);
        }
        byte rotation = _blockValue.rotation;
        for (int i = this.multiBlockPos.Length - 1; i >= 0; i--)
        {
            Vector3i pos = _blockPos + this.multiBlockPos.Get(i, _blockValue.type, (int)rotation);
            int type2 = _world.GetBlock(_clrIdx, pos).type;
            if (type2 != 0 && !Block.list[type2].blockMaterial.IsGroundCover && !_world.IsWater(pos))
            {
                return true;
            }
        }
        return false;
    }
    public override TileEntityPowered CreateTileEntity(Chunk chunk)
    {
        return new TileEntityPoweredPortal(chunk);
    }

    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
{
       new BlockActivationCommand("portalActivate", "pen", true, true),

        new BlockActivationCommand("edit", "pen", false, false),
        new BlockActivationCommand("lock", "lock", false, false),
        new BlockActivationCommand("unlock", "unlock", false, false),
        new BlockActivationCommand("keypad", "keypad", false, false)

};

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
        PortalManager.Instance.RemovePosition(_blockPos);
    }
    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        PortalManager.Instance.AddPosition(_blockPos);
        ChunkObserver ??= GameManager.Instance.AddChunkObserver(_blockPos, true, 1, -1);
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        var tileEntity = new TileEntityPoweredPortal(_chunk);
        tileEntity.localChunkPos = World.toBlock(_blockPos);
        if ( !string.IsNullOrEmpty(location))
            tileEntity.SetText(location);

        tileEntity.InitializePowerData();

        _chunk.AddTileEntity(tileEntity);
        PortalManager.Instance.AddPosition(_blockPos);

        _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true
        });
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
        ChunkObserver ??= GameManager.Instance.AddChunkObserver(_blockPos, true, 1, -1);

    }


    //public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity)
    //{
    //    TeleportPlayer(_entity as EntityAlive, _blockPos);
    //    return base.OnEntityCollidedWithBlock(_world, _clrIdx, _blockPos, _blockValue, _entity);
    //}
    //public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
    //{
    //    TeleportPlayer(entity as EntityAlive, new Vector3i(_x, _y, _z));
    //    base.OnEntityWalking(_world, _x, _y, _z, _blockValue, entity);
    //}
    public void TeleportPlayer(EntityAlive _player, Vector3i _blockPos)
    {
        var tileEntity = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null) return;
        if (requiredPower > 0 && !tileEntity.IsPowered) return;

        if (_player.Buffs.HasBuff(buffCooldown)) return;
        _player.Buffs.AddBuff(buffCooldown);

        Task task = Task.Delay(delay)
             .ContinueWith(t => Teleport(_player, _blockPos));

    }
    
    private void Teleport(EntityAlive _player, Vector3i _blockPos)
    {
        var destination = PortalManager.Instance.GetDestination(_blockPos);
        if (destination != Vector3i.zero)
        {
            // Check if the destination is powered.
            var tileEntity = GameManager.Instance.World.GetTileEntity(0, destination) as TileEntityPoweredPortal;
            if (tileEntity != null)
            {
                if (requiredPower > 0 && !tileEntity.IsPowered) return;
            }
            _player.SetPosition(destination);
        }
    }

    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return this.OnBlockActivated(_world, _cIdx, parentPos, block, _player);
        }
        var tileEntitySign = (TileEntityPoweredPortal)_world.GetTileEntity(_cIdx, _blockPos);
        if (tileEntitySign == null)
        {
            return false;
        }
        _player.AimingGun = false;
        Vector3i blockPos = tileEntitySign.ToWorldPos();
        _world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySign.entityId, _player.entityId, null);
        return true;
    }

    public override bool OnBlockActivated(string commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return this.OnBlockActivated(commandName, _world, _cIdx, parentPos, block, _player);
        }
        var tileEntity = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null)
        {
            return false;
        }
        switch (commandName)
        {
            case "portalActivate":
                if (GameManager.Instance.IsEditMode() || !tileEntity.IsLocked() || tileEntity.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if ( requiredPower <= 0 )
                        TeleportPlayer(_player, _blockPos);

                    if ( requiredPower > 0 && tileEntity.IsPowered)
                        TeleportPlayer(_player, _blockPos);


                }
                return false;
            case "edit":
                if (GameManager.Instance.IsEditMode() || !tileEntity.IsLocked() || tileEntity.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if (string.IsNullOrEmpty(location))
                        return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                }
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case "lock":
                tileEntity.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerLocked");
                return true;
            case "unlock":
                tileEntity.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerUnlocked");
                return true;
            case "keypad":
                if ( string.IsNullOrEmpty(location))
                    XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal), tileEntity);
                return true;
            default:
                return false;
        }
    }
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var tileEntity = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPoweredPortal; 
        if (tileEntity == null)
        {
            return new BlockActivationCommand[0];
        }
        PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntity.GetOwner());
        bool flag = tileEntity.LocalPlayerIsOwner();
        bool flag2 = !tileEntity.LocalPlayerIsOwner() && (playerData != null && playerData.ACL != null) && playerData.ACL.Contains(internalLocalUserIdentifier);
        this.cmds[0].enabled = true;
        this.cmds[1].enabled = string.IsNullOrEmpty(location);
        this.cmds[2].enabled = (!tileEntity.IsLocked() && (flag || flag2));
        this.cmds[3].enabled = (tileEntity.IsLocked() && flag);
        this.cmds[4].enabled = ((!tileEntity.IsUserAllowed(internalLocalUserIdentifier) && tileEntity.HasPassword() && tileEntity.IsLocked()) || flag);
        return this.cmds;
    }

    public void ToggleAnimator(Vector3i blockPos, bool force = false)
    {
        if (GameManager.IsDedicatedServer) return;

        var _ebcd = GameManager.Instance.World.GetChunkFromWorldPos(blockPos)?.GetBlockEntity(blockPos);
        if (_ebcd == null || _ebcd.transform == null)
            return;

        var tileEntity = GameManager.Instance.World.GetTileEntity(0, blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null) return;

        var isOn = force;
        var animator = _ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return;

        var currentState = animator.GetBool("portalOn");
        //if (PortalManager.Instance.IsLinked(blockPos))
        //    isOn = true;
        //else
        //    isOn = false;

        if ( force || requiredPower <= 0 )
        {
            if (currentState == isOn) return;

            animator.SetBool("portalOn", isOn);
            animator.SetBool("portalOff", !isOn);
        }
        else
        {
            if (currentState == tileEntity.IsPowered) return;
            animator.SetBool("portalOn", tileEntity.IsPowered);
            animator.SetBool("portalOff", !tileEntity.IsPowered);
//            return;

            //// If not powered, don't animate.
            //if (tileEntity.IsPowered)
            //{
            //}
        }
    }
    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (GameManager.IsDedicatedServer) return;

        if (_ebcd == null)
            return;

        if (_blockValue.ischild) return;

        // Hide the sign, so its not visible. Without this, it errors out.
        _ebcd.bHasTransform = false;

        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);

        var tileEntity = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity != null)
        {
            var text = tileEntity.GetText();
            //if (PortalManager.Instance.CountLocations(text) < 2)
            PortalManager.Instance.AddPosition(_blockPos, text);
            //else
            //    tileEntity.SetText("Invalid Location");
        }

        // Re-show the transform. This won't have a visual effect, but fixes when you pick up the block, the outline of the block persists.
        _ebcd.bHasTransform = true;
    }
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (display == false) return "";

        if (_blockValue.ischild)
        {
            var parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            return GetActivationText(_world, _world.GetBlock(parentPos), _clrIdx, parentPos, _entityFocusing);
        }
        if ( requiredPower > 0 )
        {
            var tileEntity = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityPoweredPortal;
            if (tileEntity == null) return "";
            if (tileEntity.IsPowered == false)
            {
                ToggleAnimator(_blockPos, false);
                return $"{Localization.Get("teleporttoNeedPower")}...";
            }
        }

        if ( !string.IsNullOrEmpty(displayBuff))
        {
            if (_entityFocusing.Buffs.HasBuff(displayBuff) == false) return $"{Localization.Get("teleportto")}...";
        }
        var text = "";

        PortalManager.Instance.AddPosition(_blockPos);
        if ( PortalManager.Instance.IsLinked(_blockPos))
        {
            var destination = PortalManager.Instance.GetDestinationName(_blockPos);
            return $"{Localization.Get("teleportto")} {destination}";
        }
        return $"{Localization.Get("portal_configure")} {text}";
    }
}