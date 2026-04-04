using UnityEngine;
public class DialogRequirementHasQuestSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return false;

        var myEntity = player.world.GetEntity(entityID) as IEntityAliveSDX;
        if (myEntity == null) return false;
        var entityAlive = myEntity as EntityTrader;
        if (entityAlive?.NPCInfo?.Quests?.Count <= 0) return false;
        return true;
    }
}


