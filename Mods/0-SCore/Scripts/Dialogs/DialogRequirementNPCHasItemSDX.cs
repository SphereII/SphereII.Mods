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
        var myEntity = talkingTo as IEntityAliveSDX;
        if (myEntity == null) return false;

        var entityAlive = myEntity as EntityAlive;
        var item = ItemClass.GetItem(ID);
        if (item == null) return false;

        // EntityTrader-based NPCs (EntityAliveSDX, EntityAliveSDXV4) store player-accessible
        // inventory in HarvestManager rather than lootContainer.
        if (entityAlive is EntityTrader && HarvestManager.Has(entityAlive.entityId))
        {
            if (HarvestManager.GetOrCreate(entityAlive.entityId).HasItem(item)) return true;
        }
        else if (entityAlive.lootContainer?.HasItem(item) == true) return true;

        if (entityAlive.inventory.GetItemCount(item) > 0) return true;
        if (entityAlive.bag.GetItemCount(item) > 0) return true;
        return false;
    }

}


