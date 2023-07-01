using System;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

// <triggered_effect trigger="onSelfBuffRemove" action="RandomLootSDX, SCore" lootgroup="brassResource" count="1" />

public class MinEventActionRandomLootSDX : MinEventActionBase
{
    private int CreateItemCount = 1;

    private string lootgroup = "";

    public override void Execute(MinEventParams _params)
    {
        Debug.Log("Executing Random Loot");
        var entity = _params.Self as EntityAliveSDX;

        // Only EntityAliveSDX 
        if (entity == null)
            return;

        var _random = GameManager.Instance.World.GetGameRandom();
        var Count = 1f;

        if (entity.Buffs.HasCustomVar("spLootExperience"))
            Count = entity.Buffs.GetCustomVar("spLootExperience");

        Count = MathUtils.Clamp(Count, 1, 5);
        var MaxCount = (int)Math.Round(Count);

        // Loot group
        if (lootgroup.Length > 0)
        {
            Debug.Log("Generating Items : " + MaxCount);
            for (var x = 0; x < MaxCount; x++)
            {
                var item = LootContainer.GetRewardItem(lootgroup, Count);
                Debug.Log("Adding Item: " + item);
                entity.lootContainer.AddItem(item);
            }
        }
    }

    public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
    {
        return base.CanExecute(_eventType, _params) && _params.Self as EntityAliveSDX != null;
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
            {
                if (name == "lootgroup")
                {
                    lootgroup = _attribute.Value;
                    return true;
                }

                if (name == "count")
                {
                    CreateItemCount = (int)StringParsers.ParseFloat(_attribute.Value);
                    return true;
                }
            }
        }

        return flag;
    }
}