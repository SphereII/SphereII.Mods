using UnityEngine;

public class DialogActionPickUpNPC : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = playerUI.xui.Dialog.Respondent as EntityAliveSDX;
        if (myEntity == null) return;

        if (!string.IsNullOrEmpty(ID))
        {
            if (myEntity.lootContainer.items.Length > 0)
            {
                GameManager.ShowTooltip(player as EntityPlayerLocal, "npcHasItems", string.Empty, "ui_denied", null);
                return;
            }
        }
        var itemValue = myEntity.GetItemValue();
        var itemStack = new ItemStack(itemValue, 1);
        if (player.inventory.CanTakeItem(itemStack) || player.bag.CanTakeItem(itemStack))
        {
           CollectEntityServer(myEntity.entityId, player.entityId);
        }
        else
        {
            GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied", null);
        }
    }
    
    private static void CollectEntityServer(int entityId, int playerId)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityId, playerId), false);
            return;
        }
        var entity = GameManager.Instance.World.GetEntity(entityId);
        if (GameManager.Instance.World.IsLocalPlayer(playerId))
        {
            var myEntity = GameManager.Instance.World.GetEntity(entityId) as EntityAliveSDX;
            if (myEntity == null) return;
            myEntity.Collect(playerId);
        }
        else
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityId, playerId), false, playerId, -1, -1, -1);
        }
        GameManager.Instance.World.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Killed);
    }
}