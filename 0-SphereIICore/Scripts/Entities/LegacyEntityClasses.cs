using System.Collections.Generic;
using UnityEngine;

// Legacy classes for backwards compatibility. These won't be expanded
public class EntityBanditSDX : EntityBandit
{
    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (this.isEntityRemote && _partialTicks > 1f)
        {
            _dist /= _partialTicks;
        }
        this.speedForward *= 0.5f;
        this.speedStrafe *= 0.5f;
        this.speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            float num = Mathf.Sin(-this.rotation.y * 3.14159274f / 180f);
            float num2 = Mathf.Cos(-this.rotation.y * 3.14159274f / 180f);
            this.speedForward += num2 * _dist.z - num * _dist.x;
            this.speedStrafe += num2 * _dist.x + num * _dist.z;
        }
        if (Mathf.Abs(_dist.y) > 0.001f)
        {
            this.speedVertical += _dist.y;
        }
        this.SetMovementState();
    }
}

public class EntityGenericSDX : EntityAlive
{
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        this.inventory.SetSlots(new ItemStack[]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });



    }

    public override void InitFromPrefab(int _entityClass)
    {
        base.InitFromPrefab(_entityClass);
        this.inventory.SetSlots(new ItemStack[]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });
    }




}


public class EntitySurvivorSDX : EntitySurvivor
{
    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (this.isEntityRemote && _partialTicks > 1f)
        {
            _dist /= _partialTicks;
        }
        this.speedForward *= 0.5f;
        this.speedStrafe *= 0.5f;
        this.speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            float num = Mathf.Sin(-this.rotation.y * 3.14159274f / 180f);
            float num2 = Mathf.Cos(-this.rotation.y * 3.14159274f / 180f);
            this.speedForward += num2 * _dist.z - num * _dist.x;
            this.speedStrafe += num2 * _dist.x + num * _dist.z;
        }
        if (Mathf.Abs(_dist.y) > 0.001f)
        {
            this.speedVertical += _dist.y;
        }
        this.SetMovementState();
    }
}


public class EntityZombieCopSDX : EntityZombie
{
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        this.inventory.SetSlots(new ItemStack[]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });
    }

    public override void InitFromPrefab(int _entityClass)
    {
        base.InitFromPrefab(_entityClass);
        this.inventory.SetSlots(new ItemStack[]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });
    }


}


public class EntityZombieCrawlSDX : EntityZombie
{
    public int GetWalkType()
    {
        return 4;
    }

}


class EntityWanderingTrader : EntityNPC
{

    int DefaultTraderID = 0;

    // Post Init turns on God mode, so we'll turn it off.
    public override void PostInit()
    {
        base.PostInit();
        this.IsGodMode.Value = false;

        if (this.NPCInfo != null)
            DefaultTraderID = this.NPCInfo.TraderID;
    }

    public void ToggleTraderID(bool Restore)
    {
        if (this.NPCInfo == null)
            return;

        // Check if we are restoring the default trader ID.
        if (Restore)
            this.NPCInfo.TraderID = DefaultTraderID;
        else
            this.NPCInfo.TraderID = 0;
    }

    public override void OnUpdateLive()
    {
        // Check if there's a player within 10 meters of us. If not, resume wandering.
        this.emodel.avatarController.SetBool("IsBusy", false);

        List<global::Entity> entitiesInBounds = global::GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * 10f));
        if (entitiesInBounds.Count > 0)
        {
            for (int i = 0; i < entitiesInBounds.Count; i++)
            {
                if (entitiesInBounds[i] is EntityPlayer)
                    this.emodel.avatarController.SetBool("IsBusy", true);
            }

        }


        // Check the state to see if the controller IsBusy or not. If it's not, then let it walk.
        bool isBusy = false;
        this.emodel.avatarController.TryGetBool("IsBusy", out isBusy);

        if (IsAlert)
            isBusy = false;

        if (isBusy == false)
            base.OnUpdateLive();

    }
    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        this.emodel.avatarController.SetBool("IsBusy", true);
        return base.OnEntityActivated(_indexInBlockActivationCommands, _tePos, _entityFocusing);
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
    {
        // If we are being attacked, let the state machine know it can fight back
        this.emodel.avatarController.SetBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        int Damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        ToggleTraderID(true);
        return Damage;
    }

    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
    {
        // If we are being attacked, let the state machine know it can fight back
        this.emodel.avatarController.SetBool("IsBusy", false);

        // Turn off the trader ID while it deals damage to the entity
        ToggleTraderID(false);
        base.ProcessDamageResponseLocal(_dmResponse);
        ToggleTraderID(true);
    }

    protected override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
    {
        if (this.isEntityRemote && _partialTicks > 1f)
        {
            _dist /= _partialTicks;
        }
        this.speedForward *= 0.5f;
        this.speedStrafe *= 0.5f;
        this.speedVertical *= 0.5f;
        if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
        {
            float num = Mathf.Sin(-this.rotation.y * 3.14159274f / 180f);
            float num2 = Mathf.Cos(-this.rotation.y * 3.14159274f / 180f);
            this.speedForward += num2 * _dist.z - num * _dist.x;
            this.speedStrafe += num2 * _dist.x + num * _dist.z;
        }
        if (Mathf.Abs(_dist.y) > 0.001f)
        {
            this.speedVertical += _dist.y;
        }
        this.SetMovementState();
    }
}
