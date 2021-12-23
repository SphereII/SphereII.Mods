public class DialogActionGiveToNPC : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";

        int.TryParse(Value, out var flValue);

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var playerInventory = uiforPlayer.xui.PlayerInventory;

        var itemStack = new ItemStack(ItemClass.GetItem(ID, false), flValue);
        playerInventory.RemoveItem(itemStack);
    }
}
