using System;

public class DialogRequirementHasTask : BaseDialogRequirement
{

    //  <requirement type="HasTask, SCore" requirementtype="Hide" value="LootBasic" /> <!-- true if this action name -->
    //  <requirement type="HasTask, SCore" requirementtype="Hide" value="!LootBasic" /> <!-- not have this action name -->
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        var myEntity = player.world.GetEntity(entityId) as EntityAlive;
        if (myEntity == null) return false;

        bool useAIPackages = EntityClass.list[myEntity.entityClass].UseAIPackages;
        if (!useAIPackages)
            return false;

        var result = false;


        // Check if we want a reverse condition
        var package = Value;
        if (package.StartsWith("!"))
            package = package.Replace("!", "");

        // Go through all the 
        foreach (var availablePackage in myEntity.AIPackages)
        {
            if (UAI.UAIBase.AIPackages.ContainsKey(availablePackage))
            {
                foreach (var item in UAI.UAIBase.AIPackages[availablePackage].GetActions())
                {
                    if (item.Name.ToLower() == Value.ToLower())
                    {
                        result = true;
                        break;
                    }
                }
            }
            if (result)
                break;
        }

        // Flip the results 
        if (Value.Contains("!"))
            return !result;
        return result;
    }
}


