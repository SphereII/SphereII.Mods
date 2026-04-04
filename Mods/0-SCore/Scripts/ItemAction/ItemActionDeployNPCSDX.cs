using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionDeployNPCSDX : ItemActionSpawnVehicle
{
    // Override ExecuteAction to mimic ItemActionSpawnVehicle structure
    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        // 1. INPUT & VALIDATION (Matches ItemActionSpawnVehicle lines 125-144)
        if (!_bReleased) return;

        EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
        if (!entityPlayerLocal) return;

        if (Time.time - _actionData.lastUseTime < this.Delay) return;
        if (Time.time - _actionData.lastUseTime < Constants.cBuildIntervall) return;

        ItemActionSpawnVehicle.ItemActionDataSpawnVehicle spawnVehicleData = (ItemActionSpawnVehicle.ItemActionDataSpawnVehicle)_actionData;
        
        // Ensure the preview indicates a valid position
        if (!spawnVehicleData.ValidPosition) return;

        _actionData.lastUseTime = Time.time;
        ItemValue holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;

        // 2. DETERMINE ENTITY CLASS (Specific to NPC logic)
        // We cannot rely on 'this.entityId' like SpawnVehicle does because NPCs change per ItemValue (Metadata)
        int entityClassID = -1;
        bool isFreshSpawn = false;

        // Option A: Restored NPC (Metadata)
        if (holdingItemItemValue.HasMetadata("EntityClassId"))
        {
            object classIdObj = holdingItemItemValue.GetMetadata("EntityClassId");
            if (classIdObj is int id) entityClassID = id;
            else if (classIdObj is long lId) entityClassID = (int)lId;
        }

        // Option B: Fresh Spawn (XML Property)
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

        // 3. NETWORK / SPAWN LOGIC (Matches ItemActionSpawnVehicle lines 146-173)
        // Note: In Single Player, IsServer is TRUE, so it runs the "else" block immediately.
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            // CLIENT: Request spawn from server
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageDeployNPCSDX>().Setup(
                    entityClassID,
                    spawnVehicleData.Position,
                    new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f),
                    holdingItemItemValue.Clone(),
                    entityPlayerLocal.entityId
                ),
                true
            );
        }
        else
        {
            // SERVER (or Single Player): Perform the spawn
            Vector3 rotation = new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f);
            Entity entity = EntityFactory.CreateEntity(entityClassID, spawnVehicleData.Position + Vector3.up * 0.25f, rotation);

            var entityAlive = entity as EntityAlive;
            var iEntity     = entityAlive as IEntityAliveSDX;

            if (entityAlive != null && iEntity != null)
            {
                // Set the guard BEFORE spawn so PostInit's AddToInventory/SetupStartingItems
                // do not overwrite this entity with fresh XML items.
                if (!isFreshSpawn)
                    entityAlive.Buffs.SetCustomVar("InitialInventory", 1);

                entityAlive.SetPosition(spawnVehicleData.Position + Vector3.up * 0.25f);
                entityAlive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

                // Spawn first — EntityTrader.PostInit() (called inside SpawnEntityInWorld) creates
                // a fresh TileEntityTrader and assigns it to lootContainer.  Any loot data we set
                // before spawn would be wiped.  We hydrate AFTER spawn so we overwrite the
                // now-initialized containers instead.
                GameManager.Instance.World.SpawnEntityInWorld(entityAlive);

                EntitySyncUtils.SetNPCItemValue(entityAlive, holdingItemItemValue);

                // Re-pin position in case hydration moved the entity.
                entityAlive.SetPosition(spawnVehicleData.Position + Vector3.up * 0.25f);

                // Handle properties for fresh spawns
                if (isFreshSpawn)
                {
                    string entityName = holdingItemItemValue.ItemClass.Properties.GetStringValue("EntityName");
                    if (!string.IsNullOrEmpty(entityName))
                        entityAlive.SetEntityName(entityName);

                    if (holdingItemItemValue.ItemClass.Properties.GetBool("AutoHire"))
                        EntityUtilities.Hire(entityAlive.entityId, entityPlayerLocal);
                }

                EntityUtilities.SetLeaderAndOwner(entityAlive.entityId, entityPlayerLocal.entityId);

                iEntity.SendSyncData();
            }
        }

        // 4. CLEANUP & ANIMATION (Matches ItemActionSpawnVehicle lines 174-184)
        entityPlayerLocal.RightArmAnimationUse = true;
        entityPlayerLocal.DropTimeDelay = 0.5f;
        entityPlayerLocal.inventory.DecHoldingItem(1);
        entityPlayerLocal.PlayOneShot((this.soundStart != null) ? this.soundStart : "placeblock", false);
        
        // IMPORTANT: Clear the preview mesh (The red/green ghost wireframe)
        // This was missing in your original snippet but is present in SpawnVehicle line 183
        this.ClearPreview(_actionData); 
        
    }
}