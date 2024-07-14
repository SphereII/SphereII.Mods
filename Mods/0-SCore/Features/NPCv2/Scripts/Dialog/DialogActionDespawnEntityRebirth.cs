public class DialogActionDespawnEntityRebirth : DialogActionAddBuff
{
    public override BaseDialogAction.ActionTypes ActionType => BaseDialogAction.ActionTypes.AddBuff;

    public override void PerformAction(EntityPlayer player)
    {
        var entityId = -1;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
        {
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");
        }

        if (entityId == -1)
        {
            return;
        }

        Entity myEntity = GameManager.Instance.World.GetEntity(entityId);

        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageDespawnEntityRebirth>().Setup(myEntity.entityId), false);
        }
        else
        {
            player.Companions.Remove((EntityAlive)myEntity);
            myEntity.bWillRespawn = false;
            GameManager.Instance.World.RemoveEntity(myEntity.entityId, EnumRemoveEntityReason.Unloaded);
        }
    }
}
