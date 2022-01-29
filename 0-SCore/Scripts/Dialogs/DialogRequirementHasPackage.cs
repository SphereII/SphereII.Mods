using System;

public class DialogRequirementHasPackage : BaseDialogRequirement
{

    //  <requirement type="HasPackage, SCore" requirementtype="Hide" value="NPCAnimalBasic" /> <!-- true if this package -->
    //  <requirement type="HasPackage, SCore" requirementtype="Hide" value="!NPCAnimalBasic" /> <!-- not have this package -->
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
        foreach (var availablePackage in myEntity.AIPackages)
        {
            if (availablePackage.ToLower() == package.ToLower())
            {
                result = true;
                break;
            }
        }

        // Flip the resultant 
        if (Value.Contains("!"))
            return !result;
        return result;
    }
}


