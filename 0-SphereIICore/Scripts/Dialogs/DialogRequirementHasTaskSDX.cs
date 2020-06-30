using UnityEngine;
public class DialogRequirementHasTaskSDX : BaseDialogRequirement
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
            string text2;

            EntityClass entityClass = EntityClass.list[ myEntity.entityClass];
            for( int x = 1; x < 20; x++ )
            {
                string text = EntityClass.PropAITask + x;

                if (entityClass.Properties.Values.ContainsKey(text))
                {
                    if (entityClass.Properties.Values.TryGetString(text, out text2) || text2.Length > 0)
                    {
                        if (text2.Contains(base.Value))
                            return true;

                        continue;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }
}


