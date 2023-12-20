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

        var autoHire = false;
        var updateMetaData = true;
        var entityName = string.Empty;
        var holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;
        var entityClassID = -1;
        
        if (holdingItemItemValue.HasMetadata("EntityClassId"))
        {
            entityClassID = (int)holdingItemItemValue.GetMetadata("EntityClassId");
        }
        else
        {
            if (holdingItemItemValue.ItemClass.Properties.Values.ContainsKey("EntityClass"))
            {
                var entityClass = holdingItemItemValue.ItemClass.Properties.Values["EntityClass"];
                if (string.IsNullOrEmpty(entityClass)) return;
                entityClassID = EntityClass.FromString(entityClass);
                autoHire = holdingItemItemValue.ItemClass.Properties.GetBool("AutoHire");
                updateMetaData = false;
                entityName = holdingItemItemValue.ItemClass.Properties.GetStringValue("EntityName");
            }
        }

        if (entityClassID == -1)
        {
            Log.Out("No Such Entity");
            return;
        }
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
            entityAlive.Buffs.SetCustomVar("InitialInventory", 1);
            GameManager.Instance.World.SpawnEntityInWorld(entityAlive);
            entityAlive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

            if (!string.IsNullOrEmpty(entityName))
            {
                entityAlive.SetEntityName(entityName);
            }
            // We don't want to update the meta data if it doesn't have any.
            if (updateMetaData)
            {
                entityAlive.SetItemValue(holdingItemItemValue);
            }

            if (autoHire)
            {
                EntityUtilities.Hire(entityAlive.entityId, entityPlayerLocal);
            }
            

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