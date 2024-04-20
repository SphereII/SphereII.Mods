using UnityEngine;

public class DialogActionAddItemRebirth : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";

        int.TryParse(Value, out var flValue);

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var playerInventory = uiforPlayer.xui.PlayerInventory;
        if (playerInventory == null) return;

        if (ID == "FuriousRamsaySpawnCube")
        {
            int entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

            if (entityId > 0)
            {
                var myEntity = GameManager.Instance.World.GetEntity(entityId) as EntityAliveV2;

                if (myEntity != null)
                {
                    if (myEntity.EntityClass.Properties.Values.ContainsKey("SpawnBlock"))
                    {
                        ID = "FuriousRamsaySpawnCube" + myEntity.EntityClass.Properties.Values["SpawnBlock"];
                    }
                }
            }
        }

        var item = ItemClass.GetItem(ID);
        if (item == null)
        {
            Log.Out("Item Not Found: " + ID);
            return;
        }
        var itemStack = new ItemStack(item, flValue);
        if (!playerInventory.AddItem(itemStack, true))
            player.world.gameManager.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero);
    }
}
