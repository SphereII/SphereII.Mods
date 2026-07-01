using UAI;
using UnityEngine;

public partial class EntityAliveSDXV4
{
    // =========================================================================
    // Loot / activation / interaction
    // =========================================================================

    /// <summary>
    /// Suppresses the loot prompt while the NPC is on a mission.
    /// </summary>
    public override string GetLootList()
        => IsOnMission() ? "" : base.GetLootList();

    public new EntityActivationCommand[] GetActivationCommands()
    {
        if (IsDead() || NPCInfo == null)
            return new[] { new EntityActivationCommand("Search", "search") { enabled = true } };

        var _entityFocusing = GameManager.Instance.World.GetPrimaryPlayer();
        if (_entityFocusing != null && EntityTargetingUtilities.IsEnemy(this, _entityFocusing))
            return new EntityActivationCommand[0];

        if (!EntityUtilities.IsHuman(entityId))
            return new EntityActivationCommand[0];

        var target = EntityUtilities.GetAttackOrRevengeTarget(entityId);
        if (target != null && EntityTargetingUtilities.CanDamage(this, target))
            return new EntityActivationCommand[0];

        return new[] { new EntityActivationCommand("Greet " + EntityName, "talk") { enabled = true } };
    }

    public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
    {
        if (IsDead())
        {
            if (_playerFocusing != null)
                if (lootContainer != null) EntityUtilities.OpenContainer(_playerFocusing, lootContainer);
            return;
        }

        if (_playerFocusing != null && EntityTargetingUtilities.IsEnemy(this, _playerFocusing))
        {
            GameManager.ShowTooltip(_playerFocusing, Localization.Get("entityaliveSDXEnemy"));
            return;
        }

        if (_enemyDistanceToTalk > 0 && SCoreUtils.IsEnemyNearby(this, _enemyDistanceToTalk))
        {
            if (_playerFocusing != null)
                GameManager.ShowTooltip(_playerFocusing, Localization.Get("entityaliveSDXEnemyNearby"));
            return;
        }

        Buffs.SetCustomVar("Persist", 1);
        if (_playerFocusing != null)
            SetLookPosition(_playerFocusing.getHeadPosition());

        if (_playerFocusing != null)
        {
            _playerFocusing.Buffs.SetCustomVar("CurrentNPC", entityId);
            Buffs.SetCustomVar("CurrentPlayer", _playerFocusing.entityId);
            var ui = LocalPlayerUI.GetUIForPlayer(_playerFocusing);
            ui.xui.Dialog.Respondent = this;
        }

        base.OnEntityActivated(_command, _playerFocusing);
    }
}
