using System;
using System.Collections.Generic;
using System.Globalization;
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
        else if (npc is EntityAliveSDX npcSDXSerial && npcSDXSerial.lootContainer != null)
        {
            string bagStr = SerializeItemStackArray(npcSDXSerial.lootContainer.items);
            itemValue.SetMetadata("Bag", bagStr, TypedMetadataValue.TypeTag.String);
            if (!string.IsNullOrEmpty(npcSDXSerial.lootContainer.lootListName))
                itemValue.SetMetadata("LootListName", npcSDXSerial.lootContainer.lootListName, TypedMetadataValue.TypeTag.String);
        }

        itemValue.SetMetadata("CurrentWeapon", npc.inventory?.holdingItem.GetItemName(), TypedMetadataValue.TypeTag.String);

        // Prevent this item from being dragged into any container (chests, other NPC bags, etc.).
        // The XUiC_ItemStack_SlotTags NoStorage check reads this and blocks placement.
        itemValue.SetMetadata("NoStorage", 1, TypedMetadataValue.TypeTag.Integer);

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
            else if (npc is EntityAliveSDX npcSDX3)
            {
                if (npcSDX3.lootContainer == null)
                {
                    // TileEntity.get_blockValue() dereferences this.chunk; a null chunk throws a
                    // NullReferenceException when the looting window opens. Resolve the entity's
                    // current chunk so blockValue returns a valid (if irrelevant) block.
                    Chunk chunk = npc.world?.GetChunkSync(
                        World.toChunkXZ((int)npc.position.x),
                        World.toChunkXZ((int)npc.position.z)) as Chunk;
                    npcSDX3.lootContainer = new SCoreLootContainer(chunk) { EntityId = npc.entityId };
                    npcSDX3.lootContainer.SetContainerSize(new Vector2i(8, 6));
                }

                if (npcSDX3.lootContainer.items.Length < slots.Length)
                    npcSDX3.lootContainer.items = slots;
                else
                    for (int i = 0; i < slots.Length && i < npcSDX3.lootContainer.items.Length; i++)
                        npcSDX3.lootContainer.items[i] = slots[i];
                npcSDX3.lootContainer.SetModified();
            }
        }

        string lootList = itemValue.GetMetadata("LootListName") as string;
        if (!string.IsNullOrEmpty(lootList) && npc is EntityAliveSDX sdxForLoot && sdxForLoot.lootContainer != null)
            sdxForLoot.lootContainer.lootListName = lootList;

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

    // ItemValue.Stat is a struct of (PassiveEffects type, bool isBoosted, short value). Its
    // constructor takes (_type, _base, _added) but only stores the sum and "was anything added",
    // so the base/added split cannot be recovered. The fields are therefore round-tripped
    // directly instead of through the constructor, which would force a guess at the split.
    //
    // Separator hierarchy for a slot string, outermost first. Every level needs its own
    // character because mods now nest their own stats:
    //
    //     ';'  slots
    //     ','  fields within a slot: name,count,quality,useTimes,mods,stats
    //     '|'  mods within the mods field
    //     '@'  fields within one mod: name@quality@useTimes@stats
    //     '~'  stats within a stats field (the slot's own, or a mod's)
    //     ':'  fields within one stat: type:isBoosted:value
    //
    // None of these can appear in an item name, an enum name or an invariant number, so no
    // escaping is needed.
    private const char ModSeparator = '|';
    private const char ModFieldSeparator = '@';
    private const char StatSeparator = '~';
    private const char StatFieldSeparator = ':';

    public static string SerializeStats(ItemValue.Stat[] stats)
    {
        if (stats == null || stats.Length == 0) return "";

        List<string> serialized = new List<string>(stats.Length);
        foreach (var stat in stats)
            serialized.Add($"{stat.type}{StatFieldSeparator}{(stat.isBoosted ? 1 : 0)}{StatFieldSeparator}{stat.value}");

        return string.Join(StatSeparator.ToString(), serialized);
    }

    public static ItemValue.Stat[] DeserializeStats(string data)
    {
        // Null rather than an empty array: that is what an ItemValue carries when it has no
        // stats, and what the game's own reader leaves in place.
        if (string.IsNullOrEmpty(data)) return null;

        List<ItemValue.Stat> result = new List<ItemValue.Stat>();
        foreach (var entry in data.Split(StatSeparator))
        {
            if (string.IsNullOrEmpty(entry)) continue;

            string[] fields = entry.Split(StatFieldSeparator);
            if (fields.Length != 3) continue;

            // An effect name this build no longer knows is dropped rather than allowed to fall
            // back to PassiveEffects 0, which is a real effect and would apply a wrong modifier.
            if (!Enum.TryParse(fields[0], out PassiveEffects type)) continue;
            if (!short.TryParse(fields[2], out short value)) continue;

            result.Add(new ItemValue.Stat
            {
                type = type,
                isBoosted = fields[1] == "1",
                value = value
            });
        }

        return result.Count > 0 ? result.ToArray() : null;
    }

    // A mod carries its own quality, durability and stats, so a name alone loses everything a
    // player cares about on a modded weapon. An empty string stands for an empty mod slot, which
    // keeps the remaining mods on their original indexes.
    private static string SerializeMod(ItemValue mod)
    {
        if (mod == null || mod.IsEmpty() || mod.ItemClass == null) return "";

        return string.Join(ModFieldSeparator.ToString(),
            mod.ItemClass.GetItemName(),
            mod.Quality.ToString(CultureInfo.InvariantCulture),
            mod.UseTimes.ToString(CultureInfo.InvariantCulture),
            SerializeStats(mod.Stats));
    }

    private static ItemValue DeserializeMod(string data)
    {
        if (string.IsNullOrEmpty(data)) return ItemValue.None.Clone();

        string[] fields = data.Split(ModFieldSeparator);

        ItemClass modClass = ItemClass.GetItemClass(fields[0]);
        if (modClass == null) return ItemValue.None.Clone();

        // false: restore the saved state exactly rather than letting the constructor install
        // default parts over it.
        ItemValue mod = new ItemValue(modClass.Id, false);

        // Strings written before mods carried their own data hold just the name, so every field
        // past the first is optional.
        if (fields.Length > 1 && ushort.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort quality))
            mod.Quality = quality;

        if (fields.Length > 2 && float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float useTimes))
            mod.UseTimes = useTimes;

        if (fields.Length > 3)
            mod.Stats = DeserializeStats(fields[3]);

        return mod;
    }

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

            // Base Item Data. UseTimes is written invariant: it lands in a comma-delimited
            // field, so a locale that formats decimals with a comma would split one value
            // across two columns and corrupt every field after it.
            string itemStr = string.Join(",",
                stack.itemValue.ItemClass.GetItemName(),
                stack.count.ToString(CultureInfo.InvariantCulture),
                stack.itemValue.Quality.ToString(CultureInfo.InvariantCulture),
                stack.itemValue.UseTimes.ToString(CultureInfo.InvariantCulture));

            // Mods (Attachments). Written positionally - an empty slot becomes an empty entry -
            // so mods keep the indexes the item's mod slots gave them.
            string modStr = "";
            var modifications = stack.itemValue.Modifications;
            if (modifications != null && modifications.Length > 0)
            {
                bool anyMod = false;
                foreach (var mod in modifications)
                {
                    if (mod == null || mod.IsEmpty()) continue;
                    anyMod = true;
                    break;
                }

                // An item with only empty slots still serializes as an empty field, the same as
                // an item with no mod array at all.
                if (anyMod)
                {
                    List<string> serializedMods = new List<string>(modifications.Length);
                    foreach (var mod in modifications)
                        serializedMods.Add(SerializeMod(mod));

                    modStr = string.Join(ModSeparator.ToString(), serializedMods);
                }
            }

            itemStr += "," + modStr;

            // Stats (special modifiers). Always written, even when empty, so the field count
            // per slot stays fixed and the column positions above never shift.
            itemStr += "," + SerializeStats(stack.itemValue.Stats);

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
            
            if (parts.Length > 2 && ushort.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort quality))
                itemValue.Quality = quality;

            if (parts.Length > 3 && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float useTimes))
                itemValue.UseTimes = useTimes;

            // Mods
            if (parts.Length > 4 && !string.IsNullOrEmpty(parts[4]))
            {
                string[] modEntries = parts[4].Split(ModSeparator);
                itemValue.Modifications = new ItemValue[modEntries.Length];
                for (int m = 0; m < modEntries.Length; m++)
                    itemValue.Modifications[m] = DeserializeMod(modEntries[m]);
            }

            // Stats. Absent on strings written before stats were serialized, which leaves
            // Stats null - the same state a freshly constructed ItemValue has.
            if (parts.Length > 5)
                itemValue.Stats = DeserializeStats(parts[5]);

            result[i] = new ItemStack(itemValue, count);
        }

        return result;
    }
}