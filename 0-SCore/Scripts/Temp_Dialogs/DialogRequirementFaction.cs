public class DialogRequirementFactionSDX : BaseDialogRequirement
{
    //  <requirement type="Faction, SCore" requirementtype="Hide" id="neutral" /> 

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int) player.Buffs.GetCustomVar("CurrentNPC");

        var myEntity = player.world.GetEntity(entityID) as EntityAlive;
        if (myEntity != null)
        {
            var myRelationship = FactionManager.Instance.GetRelationshipTier(myEntity, player);
            if (myRelationship.ToString().ToLower() == ID.ToLower())
                return true;
        }

        return false;
    }
}