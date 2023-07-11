using System;

public class DialogRequirementHasTag : BaseDialogRequirement
{
    //  <requirement type="HasTag, SCore" requirementtype="Hide" value="zombie" /> 
    //  <requirement type="HasTag, SCore" requirementtype="Hide" value="zombie,human" /> 

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (talkingTo != null) return talkingTo.HasAnyTags(FastTags.Parse(Value));
        
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int) player.Buffs.GetCustomVar("CurrentNPC");

        var myEntity = player.world.GetEntity(entityId) as EntityAlive;
        return myEntity != null && myEntity.HasAnyTags(FastTags.Parse(Value));

    }
}