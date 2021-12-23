using UnityEngine;

public class DialogRequirementHasQuestSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int) player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == 0)
            return false;

        var myEntity = player.world.GetEntity(entityId) as EntityAliveSDX;
        if (myEntity == null) return false;
        if (myEntity.NPCInfo.Quests.Count > 0)
            return true;

        return false;
    }
}