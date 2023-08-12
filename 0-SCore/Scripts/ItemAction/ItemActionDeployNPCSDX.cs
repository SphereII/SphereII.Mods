using System.Collections.Generic;
using Platform;
using SCore.Scripts.NetPackage;
using UnityEngine;

public class ItemActionDeployNPCSDX : ItemActionSpawnVehicle
{
    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        var entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
        if (!entityPlayerLocal) return;
        if (!_bReleased) return;
        if (Time.time - _actionData.lastUseTime < this.Delay) return;
        if (Time.time - _actionData.lastUseTime < Constants.cBuildIntervall) return;

        var itemActionDataSpawnVehicle = (ItemActionSpawnVehicle.ItemActionDataSpawnVehicle) _actionData;
        if (!itemActionDataSpawnVehicle.ValidPosition) return;

        var holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;
        var entityClassID = (int)holdingItemItemValue.GetMetadata("EntityClassId");
    
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageDeployNPCSDX>().Setup(entityClassID,
                    itemActionDataSpawnVehicle.Position, new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f),
                    holdingItemItemValue.Clone(), entityPlayerLocal.entityId), true);
        }
        else
        {
            var entity = EntityFactory.CreateEntity(entityClassID,
                itemActionDataSpawnVehicle.Position + Vector3.up * 0.25f,
                new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f));

            var entityAlive = entity as EntityAliveSDX;
            if (entityAlive == null)
            {
                return;
            }
            
            // Setting the spawner source and item value needs to happen after the SpawnEntityInWorld, otherwise it won't "take"
            GameManager.Instance.World.SpawnEntityInWorld(entityAlive);
            entityAlive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            entityAlive.SetItemValue(holdingItemItemValue);
            

        }

        if (itemActionDataSpawnVehicle.VehiclePreviewT)
        {
            UnityEngine.Object.Destroy(itemActionDataSpawnVehicle.VehiclePreviewT.gameObject);
        }

        entityPlayerLocal.RightArmAnimationUse = true;
        entityPlayerLocal.DropTimeDelay = 0.5f;
        entityPlayerLocal.inventory.DecHoldingItem(1);
        entityPlayerLocal.PlayOneShot((this.soundStart != null) ? this.soundStart : "placeblock", false);
    }
}