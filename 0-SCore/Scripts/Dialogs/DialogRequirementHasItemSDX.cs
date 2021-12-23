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


