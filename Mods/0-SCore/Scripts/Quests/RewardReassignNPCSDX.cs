using System.Collections.Generic;
using UnityEngine;

internal class RewardReassignNPCSDX : RewardExp
{
    // If the QuestNPC has other NPCs that have assigned it as their leader, this class will transfer the leadership flag to the player.
    //		<reward type="ReassignNPCSDX, SCore"  /> 
    public override void GiveReward(EntityPlayer player)
    {
        var questNPC = GameManager.Instance.World.Entities.dict[OwnerQuest.QuestGiverID] as EntityAliveSDX;
        if (questNPC)
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

    public void CheckSurroundingEntities(EntityAliveSDX questNPC, EntityPlayer player)
    {
        var NearbyEntities = new List<Entity>();
        var bb = new Bounds(questNPC.position, new Vector3(questNPC.GetSeeDistance(), 20f, questNPC.GetSeeDistance()));
        questNPC.world.GetEntitiesInBounds(typeof(EntityAliveSDX), bb, NearbyEntities);
        for (var i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            var x = (EntityAliveSDX)NearbyEntities[i];
            if (x != questNPC && x.IsAlive())
                if (x.Buffs.HasCustomVar("Leader") && x.Buffs.GetCustomVar("Leader") == questNPC.entityId)
                    EntityUtilities.SetOwner(x.entityId, player.entityId);
        }
    }
}