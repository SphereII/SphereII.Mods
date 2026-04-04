using Audio;
using Platform;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class BlockPoweredPortal : BlockPowered
{
    private string buffCooldown = "buffTeleportCooldown";
    private int delay = 1000;
    private string location;
    private bool display = false;
    private string buffActivate = "";
    private string displayBuff = "";

    // Block instances are singletons shared across all placements of the same block type.
    // Per-portal data (chunk observers) must be keyed by world position, not stored as instance fields.
    private static readonly Dictionary<Vector3i, ChunkManager.ChunkObserver> _chunkObservers
        = new Dictionary<Vector3i, ChunkManager.ChunkObserver>();

    public BlockPoweredPortal()
    {
        this.HasTileEntity = true;
    }

    public override void Init()
    {
        // Touch the singleton early so its GameStartDone handler is registered before
        // the event fires. Block.Init() runs during block registration, well before
        // GameStartDone, guaranteeing the save file will be loaded on startup.
        _ = PortalManager.Instance;

        if (Properties.Values.ContainsKey("CooldownBuff"))
            buffCooldown = Properties.Values["CooldownBuff"];
        if (Properties.Values.ContainsKey("ActivateBuff"))
            buffActivate = Properties.Values["ActivateBuff"];
        if (Properties.Values.ContainsKey("Delay"))
            delay = StringParsers.ParseSInt32(Properties.Values["Delay"]);
        if (Properties.Values.ContainsKey("Location"))
            location = Properties.Values["Location"];
        if (Properties.Values.ContainsKey("Display"))
            display = StringParsers.ParseBool(Properties.Values["Display"]);
        if (Properties.Values.ContainsKey("DisplayBuff"))
            displayBuff = Properties.Values["DisplayBuff"];

        base.Init();
    }

    // --- Chunk observer helpers ---

    private static void RegisterChunkObserver(Vector3i blockPos)
    {
        if (!_chunkObservers.ContainsKey(blockPos))
            _chunkObservers[blockPos] = GameManager.Instance.AddChunkObserver(blockPos, true, 1, -1);
    }

    private static void UnregisterChunkObserver(Vector3i blockPos)
    {
        if (_chunkObservers.TryGetValue(blockPos, out var observer))
        {
            GameManager.Instance.RemoveChunkObserver(observer);
            _chunkObservers.Remove(blockPos);
        }
    }

    // --- Block lifecycle ---

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        base.PlaceBlock(_world, _result, _ea);
        var tileEntity = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityPoweredPortal;
        tileEntity?.SetOwner(PlatformManager.InternalLocalUserIdentifier);
        RegisterChunkObserver(_result.blockPos);
    }

    public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
    {
        if (_blockPos.y > 253) return false;
        if (!GameManager.Instance.IsEditMode() && ((World)_world).IsWithinTraderArea(_blockPos)) return false;
        var block = _blockValue.Block;
        return (!block.isMultiBlock || _blockPos.y + block.multiBlockPos.dim.y < 254)
            && (GameManager.Instance.IsEditMode() || _bOmitCollideCheck || !overlapsWithOtherBlock(_world, _clrIdx, _blockPos, _blockValue));
    }

    private bool overlapsWithOtherBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        if (!isMultiBlock)
        {
            int type = _world.GetBlock(_clrIdx, _blockPos).type;
            return type != 0 && !Block.list[type].blockMaterial.IsGroundCover && !_world.IsWater(_blockPos);
        }
        byte rotation = _blockValue.rotation;
        for (int i = multiBlockPos.Length - 1; i >= 0; i--)
        {
            Vector3i pos = _blockPos + multiBlockPos.Get(i, _blockValue.type, (int)rotation);
            int type2 = _world.GetBlock(_clrIdx, pos).type;
            if (type2 != 0 && !Block.list[type2].blockMaterial.IsGroundCover && !_world.IsWater(pos))
                return true;
        }
        return false;
    }

    public override TileEntityPowered CreateTileEntity(Chunk chunk)
    {
        return new TileEntityPoweredPortal(chunk);
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
    {
        var tileEntity = new TileEntityPoweredPortal(_chunk);
        tileEntity.localChunkPos = World.toBlock(_blockPos);
        if (!string.IsNullOrEmpty(location))
        {
            PortalManager.Instance.AddPosition(_blockPos, location);
            tileEntity.SetText(location, true);
        }
        tileEntity.InitializePowerData();
        _chunk.AddTileEntity(tileEntity);
        _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos) { bNeedsTemperature = true });
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
        RegisterChunkObserver(_blockPos);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        if (_blockValue.ischild) return;
        PortalManager.Instance.AddPosition(_blockPos);
        RegisterChunkObserver(_blockPos);
    }

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
        if (_blockValue.ischild) return;
        PortalManager.Instance.RemovePosition(_blockPos);
        UnregisterChunkObserver(_blockPos);
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        if (_blockValue.ischild) return;
        UnregisterChunkObserver(_blockPos);
    }

    // --- Teleportation ---

    public bool CanUseTeleport(EntityAlive player, Vector3i blockPos)
    {
        if (string.IsNullOrEmpty(buffActivate)) return true;
        if (player.Buffs.HasBuff(buffActivate)) return true;

        Manager.BroadcastPlayByLocalPlayer(blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
        GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("xuiPortalDenied"), string.Empty, "ui_denied", null);
        return false;
    }

    public void TeleportPlayer(EntityAlive _player, Vector3i _blockPos)
    {
        var tileEntity = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null) return;
        if (requiredPower > 0 && !tileEntity.IsPowered) return;
        if (!CanUseTeleport(_player, _blockPos)) return;
        if (_player.Buffs.HasBuff(buffCooldown)) return;
        _player.Buffs.AddBuff(buffCooldown);

        // Resolve the destination parent position on the main thread before the Task delay.
        var destBase = PortalManager.Instance.GetDestination(_blockPos);
        if (destBase == Vector3i.zero) return;

        // Verify the destination portal is powered (if required)
        var destTile = GameManager.Instance.World.GetTileEntity(0, destBase) as TileEntityPoweredPortal;
        if (destTile != null && requiredPower > 0 && !destTile.IsPowered) return;

        // Force-load the destination chunk (true = require loaded) so the player
        // doesn't fall through unloaded terrain on arrival.
        var destObserver = GameManager.Instance.AddChunkObserver(destBase, true, 2, -1);

        // Capture the main thread SynchronizationContext so SetPosition is called safely.
        var ctx = SynchronizationContext.Current;
        Task.Delay(delay).ContinueWith(t =>
            ctx.Post(_ =>
            {
                GameManager.Instance.RemoveChunkObserver(destObserver);

                // Center the player inside the portal's horizontal footprint.
                // destBase is the parent (corner) position; for a 3x3x3 portal dim.x/2 = 1, dim.z/2 = 1.
                var spawnPos = destBase;
                var destBlock = GameManager.Instance.World.GetBlock(destBase);
                if (destBlock.Block.isMultiBlock)
                {
                    var dim = destBlock.Block.multiBlockPos.dim;
                    spawnPos = new Vector3i(destBase.x + dim.x / 2, destBase.y + 1, destBase.z + dim.z / 2);
                }
                else
                {
                    spawnPos = new Vector3i(destBase.x, destBase.y + 1, destBase.z);
                }

                _player.SetPosition(spawnPos);
            }, null));
    }

    // --- Activation ---

    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("portalActivate", "pen", true, true),
        new BlockActivationCommand("edit", "pen", false, false),
        new BlockActivationCommand("lock", "lock", false, false),
        new BlockActivationCommand("unlock", "unlock", false, false),
        new BlockActivationCommand("keypad", "keypad", false, false)
    };

    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            return OnBlockActivated(_world, _cIdx, parentPos, _world.GetBlock(parentPos), _player);
        }
        var tileEntity = (TileEntityPoweredPortal)_world.GetTileEntity(_cIdx, _blockPos);
        if (tileEntity == null) return false;

        _player.AimingGun = false;
        _world.GetGameManager().TELockServer(_cIdx, tileEntity.ToWorldPos(), tileEntity.entityId, _player.entityId, null);
        return true;
    }

    public override bool OnBlockActivated(string commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            return OnBlockActivated(commandName, _world, _cIdx, parentPos, _world.GetBlock(parentPos), _player);
        }
        var tileEntity = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null) return false;

        switch (commandName)
        {
            case "portalActivate":
                if (GameManager.Instance.IsEditMode() || !tileEntity.IsLocked() || tileEntity.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if (requiredPower <= 0 || tileEntity.IsPowered)
                        TeleportPlayer(_player, _blockPos);
                }
                return false;
            case "edit":
                if (GameManager.Instance.IsEditMode() || !tileEntity.IsLocked() || tileEntity.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if (string.IsNullOrEmpty(location))
                        return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                }
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case "lock":
                tileEntity.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(_player, "containerLocked");
                return true;
            case "unlock":
                tileEntity.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(_player, "containerUnlocked");
                return true;
            case "keypad":
                if (string.IsNullOrEmpty(location))
                    XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player), tileEntity);
                return true;
            default:
                return false;
        }
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var tileEntity = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null) return new BlockActivationCommand[0];

        PlatformUserIdentifierAbs localUser = PlatformManager.InternalLocalUserIdentifier;
        PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntity.GetOwner());
        bool isOwner = tileEntity.LocalPlayerIsOwner();
        bool isACL = !isOwner && playerData?.ACL != null && playerData.ACL.Contains(localUser);

        cmds[0].enabled = true;
        cmds[1].enabled = string.IsNullOrEmpty(location);
        cmds[2].enabled = !tileEntity.IsLocked() && (isOwner || isACL);
        cmds[3].enabled = tileEntity.IsLocked() && isOwner;
        cmds[4].enabled = (!tileEntity.IsUserAllowed(localUser) && tileEntity.HasPassword() && tileEntity.IsLocked()) || isOwner;
        return cmds;
    }

    // --- Visuals ---

    public void ToggleAnimator(Vector3i blockPos, bool force = false)
    {
        if (GameManager.IsDedicatedServer) return;

        var ebcd = GameManager.Instance.World.GetChunkFromWorldPos(blockPos)?.GetBlockEntity(blockPos);
        if (ebcd == null || ebcd.transform == null) return;

        var tileEntity = GameManager.Instance.World.GetTileEntity(0, blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null) return;

        var animator = ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return;

        bool isOn;
        if (force || requiredPower <= 0)
            isOn = force;
        else
            isOn = tileEntity.IsPowered;

        if (animator.GetBool("portalOn") == isOn) return;
        animator.SetBool("portalOn", isOn);
        animator.SetBool("portalOff", !isOn);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (GameManager.IsDedicatedServer) return;
        if (_ebcd == null || _blockValue.ischild) return;

        _ebcd.bHasTransform = false;
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);

        var tileEntity = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity != null)
        {
            // _blockPos is already the parent (ischild was checked above); use it directly.
            // Computing GetParentPos on a parent block applies a rotation offset that can
            // return a different position, creating a duplicate PortalMap entry.
            PortalManager.Instance.AddPosition(_blockPos, tileEntity.signText.Text);
        }

        _ebcd.bHasTransform = true;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (!display) return "";

        if (_blockValue.ischild)
        {
            var parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            return GetActivationText(_world, _world.GetBlock(parentPos), _clrIdx, parentPos, _entityFocusing);
        }

        if (requiredPower > 0)
        {
            var tileEntity = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityPoweredPortal;
            if (tileEntity == null) return "";
            if (!tileEntity.IsPowered)
            {
                ToggleAnimator(_blockPos, false);
                return $"{Localization.Get("teleporttoNeedPower")}...";
            }
        }

        if (!string.IsNullOrEmpty(displayBuff) && !_entityFocusing.Buffs.HasBuff(displayBuff))
            return $"{Localization.Get("teleportto")}...";

        if (PortalManager.Instance.IsLinked(_blockPos))
            return $"{Localization.Get("teleportto")} {PortalManager.Instance.GetDestinationName(_blockPos)}";

        return Localization.Get("portal_configure");
    }
}
