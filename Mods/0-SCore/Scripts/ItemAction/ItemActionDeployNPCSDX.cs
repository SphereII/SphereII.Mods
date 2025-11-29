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

        var itemActionDataSpawnVehicle = (ItemActionSpawnVehicle.ItemActionDataSpawnVehicle)_actionData;
        if (!itemActionDataSpawnVehicle.ValidPosition) return;

        var holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;

        // ------------------------------------------------------------------
        // 1. Determine Entity Class ID
        // ------------------------------------------------------------------
        int entityClassID = -1;
        bool isFreshSpawn = false;

        // Option A: Restored NPC (Metadata has the Class ID)
        if (holdingItemItemValue.HasMetadata("EntityClassId"))
        {
            // Handle int/long variants just in case
            object classIdObj = holdingItemItemValue.GetMetadata("EntityClassId");
            if (classIdObj is int id) entityClassID = id;
            else if (classIdObj is long lId) entityClassID = (int)lId;
        }

        // Option B: Fresh Spawn from XML (e.g. <property name="EntityClass" value="zombieBoe" />)
        if (entityClassID == -1 && holdingItemItemValue.ItemClass.Properties.Values.ContainsKey("EntityClass"))
        {
            var entityClassName = holdingItemItemValue.ItemClass.Properties.Values["EntityClass"];
            entityClassID = EntityClass.FromString(entityClassName);
            isFreshSpawn = true;
        }

        if (entityClassID == -1)
        {
            Log.Warning("ItemActionDeployNPCSDX: No EntityClass defined for this item.");
            return;
        }

        // ------------------------------------------------------------------
        // 2. NETWORK LOGIC
        // ------------------------------------------------------------------
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            // CLIENT: Send request to Server
            // We MUST send the ItemValue because it contains all the data (Inventory, Stats) 
            // in its Metadata dictionary.
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageDeployNPCSDX>().Setup(
                    entityClassID,
                    itemActionDataSpawnVehicle.Position,
                    new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f),
                    holdingItemItemValue.Clone(), 
                    entityPlayerLocal.entityId
                ),
                true
            );
        }
        else
        {
            // SERVER: Execute Spawn
            Vector3 rotation = new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f);
            Entity entity = EntityFactory.CreateEntity(entityClassID,
                itemActionDataSpawnVehicle.Position + Vector3.up * 0.25f, rotation);
            EntityAliveSDX entityAlive = entity as EntityAliveSDX;

            if (entityAlive != null)
            {
                // Prevent fresh inventory generation if we are about to restore one
                if (!isFreshSpawn)
                {
                    entityAlive.Buffs.SetCustomVar("InitialInventory", 1);
                }

                // Hydrate Data (Pre-Spawn)
                // We use the Utility to unpack the Metadata Strings into the Entity
                EntitySyncUtils.SetNPCItemValue(entityAlive, holdingItemItemValue);

                // Fix Position
                // Hydration might overwrite position if we saved it in metadata (optional), 
                // so we force the deploy position here.
                entityAlive.SetPosition(itemActionDataSpawnVehicle.Position + Vector3.up * 0.25f);

                // Handle properties for fresh spawns (not restored ones)
                if (isFreshSpawn)
                {
                    string entityName = holdingItemItemValue.ItemClass.Properties.GetStringValue("EntityName");
                    if (!string.IsNullOrEmpty(entityName))
                        entityAlive.SetEntityName(entityName);

                    if (holdingItemItemValue.ItemClass.Properties.GetBool("AutoHire"))
                        EntityUtilities.Hire(entityAlive.entityId, entityPlayerLocal);
                }

                // Spawn into World
                GameManager.Instance.World.SpawnEntityInWorld(entityAlive);
                entityAlive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
                          entityAlive.SendSyncData();
                // Hydrate Data (Pre-Spawn)
                // We use the Utility to unpack the Metadata Strings into the Entity
               // EntitySyncUtils.SetNPCItemValue(entityAlive, holdingItemItemValue);
            }


            // ------------------------------------------------------------------
            // 3. UI & ANIMATION
            // ------------------------------------------------------------------
            if (itemActionDataSpawnVehicle.VehiclePreviewT)
            {
                Object.Destroy(itemActionDataSpawnVehicle.VehiclePreviewT.gameObject);
            }

            entityPlayerLocal.RightArmAnimationUse = true;
            entityPlayerLocal.DropTimeDelay = 0.5f;
            entityPlayerLocal.inventory.DecHoldingItem(1);
            entityPlayerLocal.PlayOneShot((this.soundStart != null) ? this.soundStart : "placeblock", false);
        }
    }
}