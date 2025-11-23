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

        var autoHire = false;
        var updateMetaData = true;
        var entityName = string.Empty;
        var holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;
        var entityClassID = -1;

        // 1. Determine the Entity Class
        // NOTE: Even with the new system, 'EntityClassId' MUST be present in the Item Metadata 
        // so we know what prefab to spawn before we load the binary data.
        if (holdingItemItemValue.HasMetadata("EntityClassId"))
        {
            // This handles both the "Old Way" and the "New SCoreNPCManager Way"
            entityClassID = (int)holdingItemItemValue.GetMetadata("EntityClassId");
        }
        else
        {
            // Fresh spawn from an XML defined Item (e.g. <property name="EntityClass" value="zombieBoe" />)
            if (holdingItemItemValue.ItemClass.Properties.Values.ContainsKey("EntityClass"))
            {
                var entityClass = holdingItemItemValue.ItemClass.Properties.Values["EntityClass"];
                if (string.IsNullOrEmpty(entityClass)) return;
                
                entityClassID = EntityClass.FromString(entityClass);
                
                // Fresh XML spawns need AutoHire and don't have metadata to restore
                autoHire = holdingItemItemValue.ItemClass.Properties.GetBool("AutoHire");
                updateMetaData = false; 
                entityName = holdingItemItemValue.ItemClass.Properties.GetStringValue("EntityName");
            }
        }

        if (entityClassID == -1)
        {
            Log.Out("[0-SCore] ItemActionDeployNPCSDX: No EntityClassId found on item or metadata.");
            return;
        }

        // 2. Network Handling
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageDeployNPCSDX>().Setup(entityClassID,
                    itemActionDataSpawnVehicle.Position, new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f),
                    holdingItemItemValue.Clone(), entityPlayerLocal.entityId), true);
        }
        else
        {
            // 3. Instantiate the Entity
            var entity = EntityFactory.CreateEntity(entityClassID,
                itemActionDataSpawnVehicle.Position + Vector3.up * 0.25f,
                new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f));

            var entityAlive = entity as EntityAliveSDX;
            if (entityAlive == null) return;

            // Prevent the entity from generating a fresh inventory from loot.xml/archetypes 
            // just in case we are about to overwrite it with saved data.
            entityAlive.Buffs.SetCustomVar("InitialInventory", 1);

            // 4. Spawn into World
            GameManager.Instance.World.SpawnEntityInWorld(entityAlive);

            // 5. Hydrate Data (Restore State)
            // We call this BEFORE setting specific overrides (like SpawnerSource) 
            // because loading binary data might overwrite these values with old saved states.
            if (updateMetaData)
            {
                // This will trigger the SCoreNPCManager load if the "NPCID" metadata exists.
                entityAlive.SetItemValue(holdingItemItemValue);
            }

            // 6. Apply Overrides (Post-Hydration)
            // Force StaticSpawner so the game saves this entity and doesn't despawn it like a biome zombie.
            entityAlive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

            // Apply Name override from XML if it was a fresh spawn
            if (!string.IsNullOrEmpty(entityName))
            {
                entityAlive.SetEntityName(entityName);
            }

            // Apply Hire Logic for fresh spawns
            if (autoHire)
            {
                EntityUtilities.Hire(entityAlive.entityId, entityPlayerLocal);
            }
        }

        // 7. Cleanup Interaction
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