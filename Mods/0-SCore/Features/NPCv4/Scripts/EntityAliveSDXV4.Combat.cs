using UnityEngine;

public partial class EntityAliveSDXV4
{
    // =========================================================================
    // Combat overrides
    // =========================================================================

    public new void SetRevengeTarget(EntityAlive _other)
    {
        if (IsOnMission()) return;
        if (!_combatComp.ShouldAllowRevengeTarget(_other)) return;
        base.SetRevengeTarget(_other);
        Buffs.AddBuff("buffNotifyTeamAttack");
    }

    public new void SetAttackTarget(EntityAlive _attackTarget, int _attackTargetTime)
    {
        if (IsOnMission()) return;
        if (!_combatComp.ShouldAllowAttackTarget(_attackTarget)) return;
        base.SetAttackTarget(_attackTarget, _attackTargetTime);
        Buffs.AddBuff("buffNotifyTeamAttack");
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
    {
        if (IsOnMission()) return 0;
        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable")) return 0;
        if (Buffs.HasBuff("buffInvulnerable")) return 0;
        if (!EntityTargetingUtilities.CanTakeDamage(this, world.GetEntity(_damageSource.getEntityId()))) return 0;

        ToggleTraderID(false);
        var damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        ToggleTraderID(true);
        return damage;
    }

    public override bool CanDamageEntity(int _sourceEntityId)
        => EntityTargetingUtilities.CanTakeDamage(this, world.GetEntity(_sourceEntityId));

    public override bool IsAttackValid()
    {
        if (IsOnMission()) return false;
        return base.IsAttackValid();
    }

    public override void ProcessDamageResponse(DamageResponse _dmResponse)
    {
        if (IsOnMission()) return;
        base.ProcessDamageResponse(_dmResponse);
    }

    public override bool IsImmuneToLegDamage => IsOnMission() || base.IsImmuneToLegDamage;

    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
    {
        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable")) return;
        if (Buffs.HasBuff("buffInvulnerable")) return;

        if (!isEntityRemote) emodel.avatarController.UpdateBool("IsBusy", false);
        ToggleTraderID(false);
        base.ProcessDamageResponseLocal(_dmResponse);
        ToggleTraderID(true);
    }

    // =========================================================================
    // ExecuteAction
    // =========================================================================

    public bool ExecuteAction(bool _bAttackReleased, int actionIndex)
    {
        if (!_bAttackReleased)
        {
            if (emodel && emodel.avatarController && emodel.avatarController.IsAnimationAttackPlaying()) return false;
            if (!IsAttackValid()) return false;
        }
        if (_bLastAttackReleased && GetSoundAttack() != null)
            PlayOneShot(GetSoundAttack(), false, true);
        _bLastAttackReleased = _bAttackReleased;
        attackingTime = 60;
        ItemAction action = inventory.holdingItem.Actions[actionIndex];
        action?.ExecuteAction(inventory.holdingItemData.actionData[actionIndex], _bAttackReleased);
        return true;
    }
}
