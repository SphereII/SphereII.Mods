using UnityEngine;

public class DialogActionAddItemSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";

        int.TryParse(Value, out var flValue);

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var playerInventory = uiforPlayer.xui.PlayerInventory;
        if (playerInventory == null) return;

        var item = ItemClass.GetItem(ID);
        if (item == null)
        {
            Log.Out("AddItemSDX: Item Not Found: " + ID);
            return;
        }
        var itemStack = new ItemStack(item, flValue);
        if (!playerInventory.AddItem(itemStack, true))
            player.world.gameManager.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero);
    }
}
