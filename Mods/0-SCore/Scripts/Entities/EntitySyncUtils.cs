using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public static class EntitySyncUtils
{
    public static void Collect(int _entityId, int _playerId)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAliveSDXCollect>().Setup(_entityId, _playerId));
            return;
        }
        var entity = GameManager.Instance.World.GetEntity(_entityId) as EntityAlive;
        if (entity == null || entity is not IEntityAliveSDX) return;

        if (GameManager.Instance.World.IsLocalPlayer(_playerId))
        {
            CollectClient(entity, _playerId);
        }
        else
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAliveSDXCollect>().Setup(_entityId, _playerId), false, _playerId);
        }
        GameManager.Instance.World.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Killed);
    }

    public static void CollectClient(int _entityId, int _playerId)
    {
        var entity = GameManager.Instance.World.GetEntity(_entityId) as EntityAlive;
        if (entity == null || entity is not IEntityAliveSDX) return;
        CollectClient(entity, _playerId);
    }

    public static void CollectClient(EntityAlive entity, int _playerId)
    {

        // 2. SERVER LOGIC: Execute the pickup.
        EntityPlayerLocal player = GameManager.Instance.World.GetEntity(_playerId) as EntityPlayerLocal;
        if (player == null) return;

        // A. Generate Item with Metadata
        // GetNPCItemValue handles serializing inventory, stats, buffs, and cvars
        // into the ItemValue metadata strings.
        ItemValue itemValue = GetNPCItemValue(entity);

        if (itemValue?.type == 0)
        {
            Log.Error($"[0-SCore] EntitySyncUtils.Collect: Failed to generate ItemValue for {entity.EntityName}. Aborting pickup.");
            return;
        }

        ItemStack itemStack = new ItemStack(itemValue, 1);

        // B. Add to Inventory
        // Modifying the server-side player object triggers vanilla networking to sync
        // the new item (and its metadata) to the client automatically.
        if (!player.inventory.AddItem(itemStack))
        {
            if (!player.bag.AddItem(itemStack))
            {
                // If both Inventory and Bag are full, drop it on the ground.
                GameManager.Instance.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero, _playerId);
            }
        }
        
        // clears the cvars
        EntityUtilities.ExecuteCMD(entity.entityId, "Dismiss", player);

        // Cleaning up bad cvar format.
        player.Buffs.SetCustomVar($"hired_${entity.entityId}", 0f);

        // Release the HarvestManager container for trader-type NPCs.
        HarvestManager.Remove(entity.entityId);
     
        
    }


    public static ItemValue GetNPCItemValue(EntityAlive npc)
    {
        var iNpc = npc as IEntityAliveSDX;
        if (iNpc == null) return ItemValue.None;

        // 1. Identify Target Item
        string targetItemClass = "spherePickUpNPC";
        EntityClass currentEntityClass = EntityClass.list[npc.entityClass];
        if (currentEntityClass.Properties.Values.ContainsKey("PickUpItem"))
            targetItemClass = currentEntityClass.Properties.Values["PickUpItem"];

        ItemClass itemClass = ItemClass.GetItemClass(targetItemClass, true);
        if (itemClass == null) return ItemValue.None;

        ItemValue itemValue = new ItemValue(itemClass.Id, false);
        itemValue.Metadata = new Dictionary<string, TypedMetadataValue>();

        // 2. Core Stats
        itemValue.SetMetadata("NPCName", iNpc.FirstName, TypedMetadataValue.TypeTag.String);
        itemValue.SetMetadata("EntityClassId", npc.entityClass, TypedMetadataValue.TypeTag.Integer);
        itemValue.SetMetadata("Health", (int)npc.Health, TypedMetadataValue.TypeTag.Integer);
        itemValue.SetMetadata("MaxHealth", (int)npc.Stats.Health.Max, TypedMetadataValue.TypeTag.Integer);

        if (!string.IsNullOrEmpty(iNpc.Title))
            itemValue.SetMetadata("MyTitle", iNpc.Title, TypedMetadataValue.TypeTag.String);

        // 3. Ownership — V3 has belongsPlayerId; V4 tracks ownership via leader cvars only.
        if (npc is EntityAliveSDX v3get)
            itemValue.SetMetadata("BelongsToPlayer", v3get.belongsPlayerId, TypedMetadataValue.TypeTag.Integer);

        var leader = EntityUtilities.GetLeaderOrOwner(npc.entityId);
        if (leader)
            itemValue.SetMetadata("Leader", leader.entityId, TypedMetadataValue.TypeTag.Integer);

        // 4. CVars
        int cvarCount = 0;
        foreach (var cvar in npc.Buffs.CVars)
        {
            itemValue.SetMetadata($"CVar_{cvarCount}", $"{cvar.Key}:{cvar.Value}", TypedMetadataValue.TypeTag.String);
            cvarCount++;
        }
        itemValue.SetMetadata("CVarCount", cvarCount, TypedMetadataValue.TypeTag.Integer);

        // 5. Buffs
        int buffCount = 0;
        foreach (var buff in npc.Buffs.ActiveBuffs)
        {
            itemValue.SetMetadata($"Buff_{buffCount}", buff.BuffName, TypedMetadataValue.TypeTag.String);
            buffCount++;
        }
        itemValue.SetMetadata("BuffCount", buffCount, TypedMetadataValue.TypeTag.Integer);

        // 6. Inventory & Equipment
        string inventoryStr = SerializeItemStackArray(npc.inventory.GetSlots());
        itemValue.SetMetadata("Inventory", inventoryStr, TypedMetadataValue.TypeTag.String);

        // 7. Bag / Loot Container
        // Both EntityAliveSDX and EntityAliveSDXV4 extend EntityTrader.  OpenInventory routes
        // their player-accessible bag through HarvestManager, not npc.lootContainer.
        // Serialize from HarvestManager when present; fall back to lootContainer for any
        // non-trader entity that reaches this path.
        if (npc is EntityTrader && HarvestManager.Has(npc.entityId))
        {
            var hc = HarvestManager.GetOrCreate(npc.entityId);
            string bagStr = SerializeItemStackArray(hc.items);
            itemValue.SetMetadata("Bag", bagStr, TypedMetadataValue.TypeTag.String);
            if (!string.IsNullOrEmpty(hc.lootListName))
                itemValue.SetMetadata("LootListName", hc.lootListName, TypedMetadataValue.TypeTag.String);
        }
        else if (npc.lootContainer != null)
        {
            string bagStr = SerializeItemStackArray(npc.lootContainer.items);
            itemValue.SetMetadata("Bag", bagStr, TypedMetadataValue.TypeTag.String);
            if (!string.IsNullOrEmpty(npc.lootContainer.lootListName))
                itemValue.SetMetadata("LootListName", npc.lootContainer.lootListName, TypedMetadataValue.TypeTag.String);
        }

        itemValue.SetMetadata("CurrentWeapon", npc.inventory?.holdingItem.GetItemName(), TypedMetadataValue.TypeTag.String);

        return itemValue;
    }

    public static void SetNPCItemValue(EntityAlive npc, ItemValue itemValue)
    {
        if (itemValue == null) return;
        var iNpc = npc as IEntityAliveSDX;
        if (iNpc == null) return;

        // 1. Core Stats
        var entityName = itemValue.GetMetadata("NPCName") as string;
        if (!string.IsNullOrEmpty(entityName))
        {
            iNpc.FirstName = entityName;
            npc.entityName = entityName;
        }

        var myTitle = itemValue.GetMetadata("MyTitle") as string;
        if (!string.IsNullOrEmpty(myTitle)) iNpc.Title = myTitle;

        if (itemValue.GetMetadata("Health") is int hp) npc.Health = hp;

        // V3-specific ownership field; V4 ownership is handled via leader cvars.
        if (itemValue.GetMetadata("BelongsToPlayer") is int pId && npc is EntityAliveSDX v3set)
            v3set.belongsPlayerId = pId;

        if (itemValue.GetMetadata("Leader") is int lId)
            EntityUtilities.SetLeaderAndOwner(npc.entityId, lId);

        // 2. CVars
        if (itemValue.GetMetadata("CVarCount") is int cvarCount)
        {
            for (int i = 0; i < cvarCount; i++)
            {
                string cvarStr = itemValue.GetMetadata($"CVar_{i}") as string;
                if (string.IsNullOrEmpty(cvarStr)) continue;
                string[] split = cvarStr.Split(':');
                if (split.Length == 2 && StringParsers.TryParseFloat(split[1], out float value))
                    npc.Buffs.AddCustomVar(split[0], value);
            }
        }

        // 3. Buffs
        if (itemValue.GetMetadata("BuffCount") is int buffCount)
        {
            for (int i = 0; i < buffCount; i++)
            {
                string buffName = itemValue.GetMetadata($"Buff_{i}") as string;
                if (!string.IsNullOrEmpty(buffName))
                    npc.Buffs.AddBuff(buffName);
            }
        }

        // 4. Inventory (Hand)
        string invStr = itemValue.GetMetadata("Inventory") as string;
        if (!string.IsNullOrEmpty(invStr))
        {
            ItemStack[] slots = DeserializeItemStackArray(invStr);
            npc.inventory.SetSlots(slots);
            if (npc.inventory.holdingItem != null)
                iNpc.UpdateWeapon(npc.inventory.holdingItemItemValue.ItemClass?.GetItemName() ?? "");
        }

        // 5. Bag (Loot Container)
        string bagStr = itemValue.GetMetadata("Bag") as string;
        if (!string.IsNullOrEmpty(bagStr))
        {
            ItemStack[] slots = DeserializeItemStackArray(bagStr);

            // For EntityTrader-based entities (both EntityAliveSDX and EntityAliveSDXV4),
            // the player-accessible inventory is served by HarvestManager — restore there so
            // the OpenInventory dialog finds it under the new entity ID.
            if (npc is EntityTrader)
            {
                var hc = HarvestManager.GetOrCreate(npc.entityId);
                for (int i = 0; i < slots.Length && i < hc.items.Length; i++)
                    hc.items[i] = slots[i];
            }
            else
            {
                if (npc.lootContainer == null)
                {
                    Chunk chunk = null;
                    npc.lootContainer = new TileEntityLootContainer(chunk);
                    npc.lootContainer.entityId = npc.entityId;
                    npc.lootContainer.SetContainerSize(new Vector2i(8, 6));
                }

                if (npc.lootContainer.items.Length < slots.Length)
                    npc.lootContainer.items = slots;
                else
                    for (int i = 0; i < slots.Length && i < npc.lootContainer.items.Length; i++)
                        npc.lootContainer.items[i] = slots[i];
                npc.lootContainer.SetModified();
            }
        }

        string lootList = itemValue.GetMetadata("LootListName") as string;
        if (!string.IsNullOrEmpty(lootList) && npc.lootContainer != null)
            npc.lootContainer.lootListName = lootList;

        npc.Buffs.SetCustomVar("WeaponTypeNeedsUpdate", 1);

        var currentWeapon = itemValue.GetMetadata("CurrentWeapon") as string;
        // Store weapon name in the concrete type's _currentWeapon field.
        if (npc is EntityAliveSDX v3w) v3w._currentWeapon = currentWeapon;
        else if (npc is EntityAliveSDXV4 v4w) v4w._currentWeapon = currentWeapon;

        if (!string.IsNullOrEmpty(currentWeapon))
            iNpc.UpdateWeapon(currentWeapon);
    }

    // -------------------------------------------------------------------------
    // HELPERS: String Serialization for ItemStacks
    // -------------------------------------------------------------------------

    public static string SerializeItemStackArray(ItemStack[] stacks)
    {
        if (stacks == null || stacks.Length == 0) return "";

        List<string> serializedSlots = new List<string>();

        foreach (var stack in stacks)
        {
            if (stack.IsEmpty())
            {
                serializedSlots.Add("AIR"); 
                continue;
            }

            // Base Item Data
            string itemStr = $"{stack.itemValue.ItemClass.GetItemName()},{stack.count},{stack.itemValue.Quality},{stack.itemValue.UseTimes}";

            // Mods (Attachments)
            if (stack.itemValue.Modifications != null && stack.itemValue.Modifications.Length > 0)
            {
                List<string> mods = new List<string>();
                foreach (var mod in stack.itemValue.Modifications)
                {
                    if (mod != null && !mod.IsEmpty())
                        mods.Add(mod.ItemClass.GetItemName());
                }
                if (mods.Count > 0)
                {
                    itemStr += "," + string.Join("|", mods);
                }
                else
                {
                    itemStr += ","; 
                }
            }
            else
            {
                itemStr += ",";
            }
            
            serializedSlots.Add(itemStr);
        }

        return string.Join(";", serializedSlots);
    }

    public static ItemStack[] DeserializeItemStackArray(string data)
    {
        if (string.IsNullOrEmpty(data)) return new ItemStack[0];

        string[] slots = data.Split(';');
        ItemStack[] result = new ItemStack[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            string slotData = slots[i];
            if (slotData == "AIR" || string.IsNullOrEmpty(slotData))
            {
                result[i] = ItemStack.Empty.Clone();
                continue;
            }

            string[] parts = slotData.Split(',');
            if (parts.Length < 2) 
            {
                result[i] = ItemStack.Empty.Clone();
                continue;
            }

            string itemName = parts[0];
            if (int.TryParse(parts[1], out int count) == false) count = 1;
            
            ItemClass itemClass = ItemClass.GetItemClass(itemName);
            if (itemClass == null)
            {
                result[i] = ItemStack.Empty.Clone();
                continue;
            }

            ItemValue itemValue = new ItemValue(itemClass.Id, false);
            
            if (parts.Length > 2 && ushort.TryParse(parts[2], out ushort quality))
                itemValue.Quality = quality;
                
            if (parts.Length > 3 && float.TryParse(parts[3], out float useTimes))
                itemValue.UseTimes = useTimes;

            // Mods
            if (parts.Length > 4 && !string.IsNullOrEmpty(parts[4]))
            {
                string[] modNames = parts[4].Split('|');
                itemValue.Modifications = new ItemValue[modNames.Length];
                for(int m=0; m < modNames.Length; m++)
                {
                    ItemClass modClass = ItemClass.GetItemClass(modNames[m]);
                    if (modClass != null)
                        itemValue.Modifications[m] = new ItemValue(modClass.Id);
                    else
                        itemValue.Modifications[m] = ItemValue.None.Clone();
                }
            }

            result[i] = new ItemStack(itemValue, count);
        }

        return result;
    }
}