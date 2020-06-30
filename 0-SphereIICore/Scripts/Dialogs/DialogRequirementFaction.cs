using UnityEngine;
public class DialogRequirementFaction : BaseDialogRequirement
{

    //  <requirement type="Faction, Mods" requirementtype="Hide" value="neutral" /> 

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        int entityID = 0;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        EntityAlive myEntity = player.world.GetEntity(entityID) as EntityAlive;
        if(myEntity != null)
        {
            FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(myEntity, player);
            if (myRelationship.ToString().ToLower() == ID.ToLower())
                return true;
        }
        return false;
    }
}


