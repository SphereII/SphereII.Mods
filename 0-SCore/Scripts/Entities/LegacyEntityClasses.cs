using UnityEngine;

// Legacy classes for backwards compatibility. These won't be expanded
public class EntityBanditSDX : EntityBandit
{
    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (isEntityRemote && _partialTicks > 1f) _dist /= _partialTicks;
        speedForward *= 0.5f;
        speedStrafe *= 0.5f;
        speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            var num = Mathf.Sin(-rotation.y * 3.14159274f / 180f);
            var num2 = Mathf.Cos(-rotation.y * 3.14159274f / 180f);
            speedForward += num2 * _dist.z - num * _dist.x;
            speedStrafe += num2 * _dist.x + num * _dist.z;
        }

        if (Mathf.Abs(_dist.y) > 0.001f) speedVertical += _dist.y;
        SetMovementState();
    }
}

public class EntityGenericSDX : EntityAlive
{
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        inventory.SetSlots(new[]
        {
            new ItemStack(inventory.GetBareHandItemValue(), 1)
        });
    }

    public override void InitFromPrefab(int _entityClass)
    {
        base.InitFromPrefab(_entityClass);
        inventory.SetSlots(new[]
        {
            new ItemStack(inventory.GetBareHandItemValue(), 1)
        });
    }
}


public class EntitySurvivorSDX : EntitySurvivor
{
    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (isEntityRemote && _partialTicks > 1f) _dist /= _partialTicks;
        speedForward *= 0.5f;
        speedStrafe *= 0.5f;
        speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            var num = Mathf.Sin(-rotation.y * 3.14159274f / 180f);
            var num2 = Mathf.Cos(-rotation.y * 3.14159274f / 180f);
            speedForward += num2 * _dist.z - num * _dist.x;
            speedStrafe += num2 * _dist.x + num * _dist.z;
        }

        if (Mathf.Abs(_dist.y) > 0.001f) speedVertical += _dist.y;
        SetMovementState();
    }
}


public class EntityZombieCopSDX : EntityZombie
{
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        inventory.SetSlots(new[]
        {
            new ItemStack(inventory.GetBareHandItemValue(), 1)
        });
    }

    public override void InitFromPrefab(int _entityClass)
    {
        base.InitFromPrefab(_entityClass);
        inventory.SetSlots(new[]
        {
            new ItemStack(inventory.GetBareHandItemValue(), 1)
        });
    }
}


public class EntityZombieCrawlSDX : EntityZombie
{
    public new int GetWalkType()
    {
        return 4;
    }
}


internal class EntityWanderingTrader : EntityNPC
{
    private int DefaultTraderID;

    // Post Init turns on God mode, so we'll turn it off.
    public override void PostInit()
    {
        base.PostInit();
        IsGodMode.Value = false;

        if (NPCInfo != null)
            DefaultTraderID = NPCInfo.TraderID;
    }

    public void ToggleTraderID(bool Restore)
    {
        if (NPCInfo == null)
            return;

        // Check if we are restoring the default trader ID.
        if (Restore)
            NPCInfo.TraderID = DefaultTraderID;
        else
            NPCInfo.TraderID = 0;
    }

    public override void OnUpdateLive()
    {
        // Check if there's a player within 10 meters of us. If not, resume wandering.
        emodel.avatarController.UpdateBool("IsBusy", false);

        var entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(position, Vector3.one * 10f));
        if (entitiesInBounds.Count > 0)
            for (var i = 0; i < entitiesInBounds.Count; i++)
                if (entitiesInBounds[i] is EntityPlayer)
                    emodel.avatarController.UpdateBool("IsBusy", true);


        // Check the state to see if the controller IsBusy or not. If it's not, then let it walk.
        var isBusy = false;
        emodel.avatarController.TryGetBool("IsBusy", out isBusy);

        if (IsAlert)
            isBusy = false;

        if (isBusy == false)
            base.OnUpdateLive();
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        emodel.avatarController.UpdateBool("IsBusy", true);
        return base.OnEntityActivated(_indexInBlockActivationCommands, _tePos, _entityFocusing);
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
    {
        // If we are being attacked, let the state machine know it can fight back
        emodel.avatarController.UpdateBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        var Damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        ToggleTraderID(true);
        return Damage;
    }

    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
    {
        // If we are being attacked, let the state machine know it can fight back
        emodel.avatarController.UpdateBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        base.ProcessDamageResponseLocal(_dmResponse);
        ToggleTraderID(true);
    }

    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (isEntityRemote && _partialTicks > 1f) _dist /= _partialTicks;
        speedForward *= 0.5f;
        speedStrafe *= 0.5f;
        speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            var num = Mathf.Sin(-rotation.y * 3.14159274f / 180f);
            var num2 = Mathf.Cos(-rotation.y * 3.14159274f / 180f);
            speedForward += num2 * _dist.z - num * _dist.x;
            speedStrafe += num2 * _dist.x + num * _dist.z;
        }

        if (Mathf.Abs(_dist.y) > 0.001f) speedVertical += _dist.y;
        SetMovementState();
    }
}