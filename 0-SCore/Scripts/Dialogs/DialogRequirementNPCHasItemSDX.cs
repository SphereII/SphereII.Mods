/// <summary>
/// A Dialog Requirement that checks to see if the NPC has the specified item in their inventory / tool belt, and determines
/// if the Response is available to the player.
/// 
/// Syntax:
///
/// The NPC must have this item in their inventory / toolbelt 
/// <requirement type="NPCHasItemSDX, SCore" id="meleeWpnClubT0WoodenClub" requirementtype="Hide" />
///

/// </summary>

public class DialogRequirementNPCHasItemSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var myEntity = talkingTo as EntityAliveSDX;
        if (myEntity == null) return false;

        var item = ItemClass.GetItem(ID);
        if (item == null) return false;
        if (myEntity.lootContainer.HasItem(item)) return true;
        if (myEntity.inventory.GetItemCount(item) > 0 ) return true;
        if (myEntity.bag.GetItemCount(item) > 0) return true;
        return false;
    }

}


