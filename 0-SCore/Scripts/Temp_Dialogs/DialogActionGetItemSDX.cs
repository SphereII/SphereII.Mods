public class DialogActionGetItemSDX : DialogActionAddBuff
{
    private readonly string name = string.Empty;

    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";

        var flValue = 1;
        int.TryParse(Value, out flValue);

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var playerInventory = uiforPlayer.xui.PlayerInventory;

        var itemStack = new ItemStack(ItemClass.GetItem(ID), flValue);
        playerInventory.AddItem(itemStack, true);
    }
}