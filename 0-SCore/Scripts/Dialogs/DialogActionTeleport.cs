
/*                
 *                
 *  <!-- Portal01 -->              
 * <action type="Teleport, SCore" id="Portal01" />
 * 
 * <!-- Bedroll -->
 * <action type="Teleport, SCore" id="Bedroll" />
 * 
 * <!-- Land claim -->
 * <action type="Teleport, SCore" id="Landclaim" />
 * 
 *  * <!-- Backpack -->
 * <action type="Teleport, SCore" id="Backpack" />

*/
using Platform;

public class DialogActionTeleport : DialogActionAddBuff
{
    public override BaseDialogAction.ActionTypes ActionType => BaseDialogAction.ActionTypes.AddBuff;

    public override void PerformAction(EntityPlayer player)
    {
        var location = base.ID;
        if (string.IsNullOrEmpty(location)) return;

        var destination = PortalManager.Instance.GetDestination(location);
        if (destination != Vector3i.zero)
        {
            player.SetPosition(destination);
            return;
        }

        var entityplayerLocal = player as EntityPlayerLocal;
        if (entityplayerLocal == null) return;
        switch( location)
        {
            case "Bedroll":
                if ( entityplayerLocal.CheckSpawnPointStillThere())
                {
                    var spawnPoint = entityplayerLocal.GetSpawnPoint();
                    destination = new Vector3i( spawnPoint.position);
                }
                break;
            case "Backpack":
                
                destination = entityplayerLocal.GetLastDroppedBackpackPosition();
                break;
            case "Landclaim":
                PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
                PersistentPlayerData playerData = GameManager.Instance.World.GetGameManager().GetPersistentPlayerList().GetPlayerData(internalLocalUserIdentifier);
                if (playerData == null) return;
                playerData.GetLandProtectionBlock(out destination);
                break;
            default:
                return;
        }

        if (destination == Vector3i.zero) return;
        player.SetPosition(destination);
    }

}
