public class DialogRequirementNotHaveItemSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var count = 0;

        if (string.IsNullOrEmpty(Value))
            Value = "1";

        var flValue = 1f;
        float.TryParse(Value, out flValue);

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var playerInventory = uiforPlayer.xui.PlayerInventory;
        var item = ItemClass.GetItem(ID);
        if (item != null)
        {
            count = playerInventory.Backpack.GetItemCount(item);
            count += playerInventory.Toolbelt.GetItemCount(item);
            AdvLogging.DisplayLog(AdvFeatureClass, "NotHaveItemSDX: " + item.ItemClass.GetItemName() + " Player has Count: " + count + " Needs: " + flValue);
            if (flValue > count)
                return true;
        }

        AdvLogging.DisplayLog(AdvFeatureClass, "NotHaveItemSDX: Player has have enough " + item.ItemClass.GetItemName() + " Count: " + count + " Needs: " + flValue);

        return false;
    }
}