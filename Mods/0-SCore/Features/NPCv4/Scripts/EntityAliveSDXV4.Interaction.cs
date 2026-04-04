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

    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
    {
        if (IsDead() || NPCInfo == null)
            return new[] { new EntityActivationCommand("Search", "search", true) };

        if (EntityTargetingUtilities.IsEnemy(this, _entityFocusing))
            return new EntityActivationCommand[0];

        if (!EntityUtilities.IsHuman(entityId))
            return new EntityActivationCommand[0];

        var target = EntityUtilities.GetAttackOrRevengeTarget(entityId);
        if (target != null && EntityTargetingUtilities.CanDamage(this, target))
            return new EntityActivationCommand[0];

        return new[] { new EntityActivationCommand("Greet " + EntityName, "talk", true) };
    }

    public override bool OnEntityActivated(int indexInBlockActivationCommands, Vector3i tePos,
        EntityAlive entityFocusing)
    {
        var localPlayer = entityFocusing as EntityPlayerLocal;

        if (IsDead())
        {
            GameManager.Instance.TELockServer(0, tePos, entityId, entityFocusing.entityId, null);
            return true;
        }

        if (EntityTargetingUtilities.IsEnemy(this, entityFocusing))
        {
            if (localPlayer != null)
                GameManager.ShowTooltip(localPlayer, Localization.Get("entityaliveSDXEnemy"));
            return false;
        }

        if (_enemyDistanceToTalk > 0 && SCoreUtils.IsEnemyNearby(this, _enemyDistanceToTalk))
        {
            if (localPlayer != null)
                GameManager.ShowTooltip(localPlayer, Localization.Get("entityaliveSDXEnemyNearby"));
            return false;
        }

        Buffs.SetCustomVar("Persist", 1);
        SetLookPosition(entityFocusing.getHeadPosition());

        entityFocusing.Buffs.SetCustomVar("CurrentNPC", entityId);
        Buffs.SetCustomVar("CurrentPlayer", entityFocusing.entityId);

        var ui = LocalPlayerUI.GetUIForPlayer(entityFocusing as EntityPlayerLocal);
        ui.xui.Dialog.Respondent = this;

        return base.OnEntityActivated(indexInBlockActivationCommands, tePos, entityFocusing);
    }
}
