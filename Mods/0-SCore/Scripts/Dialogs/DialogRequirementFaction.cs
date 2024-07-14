using System;

public class DialogRequirementFaction : BaseDialogRequirement
{

    //  <requirement type="Faction, SCore" requirementtype="Hide" value="neutral" /> 

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        var myEntity = player.world.GetEntity(entityId) as EntityAlive;
        if (myEntity == null) return false;

        var myRelationship = FactionManager.Instance.GetRelationshipTier(myEntity, player);
        return string.Equals(myRelationship.ToString(), ID, StringComparison.CurrentCultureIgnoreCase);
    }
}


