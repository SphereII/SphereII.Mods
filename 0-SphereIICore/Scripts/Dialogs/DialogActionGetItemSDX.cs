using UnityEngine;
public class DialogActionGetItemSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";

        int flValue = 1;
        int.TryParse(Value, out flValue);

        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        XUiM_PlayerInventory playerInventory = uiforPlayer.xui.PlayerInventory;

        ItemStack itemStack = new ItemStack(ItemClass.GetItem(ID, false), flValue);
        playerInventory.AddItem(itemStack, true);
    }

    private string name = string.Empty;
}
