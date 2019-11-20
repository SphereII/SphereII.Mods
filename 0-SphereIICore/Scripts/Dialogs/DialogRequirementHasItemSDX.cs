using UnityEngine;
public class DialogRequirementHasItemSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player)
    {
        int count = 0;

        if (string.IsNullOrEmpty(Value))
            Value = "1";

        float flValue = 1f;
        float.TryParse(Value, out flValue);

        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        XUiM_PlayerInventory playerInventory = uiforPlayer.xui.PlayerInventory;
        ItemValue item = ItemClass.GetItem(ID);
        if (item != null)
        {
            count = playerInventory.Backpack.GetItemCount(item, -1, -1, false);
            count += playerInventory.Toolbelt.GetItemCount(item, false, -1, -1);
            if (flValue <= count)
                return true;
        }

        return false;
    }

}


