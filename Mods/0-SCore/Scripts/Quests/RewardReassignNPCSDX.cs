using System.Collections.Generic;
using UnityEngine;

internal class RewardReassignNPCSDX : RewardExp
{
    // If the QuestNPC has other NPCs that have assigned it as their leader, this class will transfer the leadership flag to the player.
    //		<reward type="ReassignNPCSDX, SCore"  /> 
    public override void GiveReward(EntityPlayer player)
    {
        var questNPC = GameManager.Instance.World.Entities.dict[OwnerQuest.QuestGiverID] as EntityAlive;
        if (questNPC is IEntityAliveSDX)
            CheckSurroundingEntities(questNPC, player);
    }

    public override BaseReward Clone()
    {
        var rewardNPC = new RewardReassignNPCSDX();
        CopyValues(rewardNPC);
        return rewardNPC;
    }

    public override void SetupReward()
    {
        Description = "Reassign NPC Quest";
        SetupValueText();
        Icon = "ui_game_symbol_trophy";
    }

    // Token: 0x060047B6 RID: 18358 RVA: 0x001FC954 File Offset: 0x001FAB54
    private void SetupValueText()
    {
        ValueText = "Value Test";
    }

    public void CheckSurroundingEntities(EntityAlive questNPC, EntityPlayer player)
    {
        var NearbyEntities = new List<Entity>();
        var bb = new Bounds(questNPC.position, new Vector3(questNPC.GetSeeDistance(), 20f, questNPC.GetSeeDistance()));
        // EntityTrader, not EntityAliveSDX: it is the nearest common base of the legacy and V4
        // NPC classes. Vanilla traders are filtered out by the IEntityAliveSDX check below.
        questNPC.world.GetEntitiesInBounds(typeof(EntityTrader), bb, NearbyEntities);
        for (var i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            if (NearbyEntities[i] is not EntityAlive x || x is not IEntityAliveSDX) continue;
            if (x != questNPC && x.IsAlive())
                if (x.Buffs.HasCustomVar("Leader") && x.Buffs.GetCustomVar("Leader") == questNPC.entityId)
                    EntityUtilities.SetOwner(x.entityId, player.entityId);
        }
    }
}