using System;
using System.Globalization;
using System.Xml;
using UnityEngine;

// <triggered_effect trigger="onSelfBuffRemove" action="RandomLootSDX, Mods" lootgroup="brassResource" count="1" />

public class MinEventActionRandomLootSDX : MinEventActionBase
{
    public override void Execute(MinEventParams _params)
    {
        Debug.Log("Executing Random Loot");
        EntityAliveSDX entity = _params.Self as EntityAliveSDX;

        // Only EntityAliveSDX 
        if (entity == null)
            return;

        GameRandom _random = GameManager.Instance.World.GetGameRandom();
        float Count = 1f;

        if (entity.Buffs.HasCustomVar("spLootExperience"))
            Count = entity.Buffs.GetCustomVar("spLootExperience");

        Count = MathUtils.Clamp(Count, 1, 5);
        int MaxCount = (int)Math.Round(Count);

        // Loot group
        if (lootgroup.Length > 0)
        {
            Debug.Log("Generating Items : " + MaxCount);
            for (int x = 0; x < MaxCount; x++)
            {
                ItemStack item = LootContainer.GetRewardItem(lootgroup, Count);
                Debug.Log("Adding Item: " + item.ToString());
                entity.lootContainer.AddItem(item);
            }
        }
    }

    public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
    {
        return base.CanExecute(_eventType, _params) && _params.Self as EntityAliveSDX != null;
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "lootgroup")
                {
                    lootgroup = _attribute.Value;
                    return true;
                }
                if (name == "count")
                {
                    CreateItemCount = (int)StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
                    return true;
                }
            }
        }
        return flag;
    }

    private string lootgroup = "";
    private int CreateItemCount = 1;
}
