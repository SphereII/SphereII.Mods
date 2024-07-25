/// <summary>
/// A Dialog Requirement that checks to see if the player has the specified item in their inventory / tool belt, and determines
/// if the Response is available to the player.
/// 
/// Syntax:
///
/// The player must have this item in their inventory / toolbelt 
/// <requirement type="HasItemSDX, SCore" id="meleeWpnClubT0WoodenClub" requirementtype="Hide" />
///
///  The player must have 10 of these items in their inventory / toolbelt.
/// <requirement type="HasItemSDX, SCore" id="meleeWpnClubT0WoodenClub" value="10" requirementtype="Hide" />
/// </summary>

public class DialogRequirementHasItemSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var count = 0;

        if (string.IsNullOrEmpty(Value))
            Value = "1";

        float.TryParse(Value, out var flValue);

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var playerInventory = uiforPlayer.xui.PlayerInventory;
        var item = ItemClass.GetItem(ID);
        if (item != null)
        {
            count = playerInventory.Backpack.GetItemCount(item, -1, -1, false);
            count += playerInventory.Toolbelt.GetItemCount(item, false, -1, -1);
            AdvLogging.DisplayLog(AdvFeatureClass, "HasItemSDX: " + item.ItemClass.GetItemName() + " Player has Count: " + count + " Needs: " + flValue);
            if (flValue <= count)
                return true;
        }

        AdvLogging.DisplayLog(AdvFeatureClass, "HasItemSDX: Player does not have enough " + item.ItemClass.GetItemName() + " Count: " + count + " Needs: " + flValue);

        return false;
    }

}


