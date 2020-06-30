using UnityEngine;
public class DialogRequirementPatrolSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {

        int entityID = 0;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if(entityID == 0)
            return false;

        EntityAliveSDX myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if(myEntity != null)
        {
            bool isTame = false;
            if(base.Value.EqualsCaseInsensitive("not"))
                isTame = !EntityUtilities.isTame(entityID, player);
            else
                isTame = EntityUtilities.isTame(entityID, player);

            if(isTame)
            {
                if(myEntity.PatrolCoordinates.Count > 0)
                    return true;
                else
                    return false;
            }

            return isTame;
        }
        return false;

    }

}


