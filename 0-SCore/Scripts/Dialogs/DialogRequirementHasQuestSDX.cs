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

        var myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (myEntity == null) return false;
        if (myEntity.NPCInfo.Quests.Count <= 0) return false;
        Debug.Log("NPC has Quests: " + myEntity.NPCInfo.Quests.Count);
        return true;
    }
}


