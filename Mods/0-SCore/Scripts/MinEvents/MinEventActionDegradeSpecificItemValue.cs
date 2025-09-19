using System;
using System.Collections.Generic;
using System.Xml.Linq; // For XAttribute
using SCore.Features.ItemDegradation.Utils;
using UnityEngine; // For ItemDegradationHelpers


// <triggered_effect trigger="onSelfItemDegrade" action="DegradeSpecificItemValueMod, SCore" item_name="myItem" tags="anytags" slots="equipment" />
// <triggered_effect trigger="onSelfItemDegrade" action="DegradeSpecificItemValueMod, SCore" item_name="myItem" tags="anytags" slots="bags,inventory,equipment" />

public class MinEventActionDegradeSpecificItemValueMod : MinEventActionTargetedBase
{
    private FastTags<TagGroup.Global> _parsedTags = FastTags<TagGroup.Global>.none;
    private bool _hasTagsToSearch;
    private string ItemName { get; set; } = string.Empty;
    private string TagsString { get; set; } = string.Empty;

    private bool _bEquipment;
    private bool _bToolbelt;
    private bool _bBag;

    private int _degradeOverride = 0;

    public override void Execute(MinEventParams @params)
    {
        if (@params.Self is not EntityPlayerLocal player) return;

        if (string.IsNullOrEmpty(ItemName) && !_hasTagsToSearch) return;

        FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.none;
        if (!string.IsNullOrEmpty(TagsString))
            tags = FastTags<TagGroup.Global>.Parse(TagsString);

        List<ItemValue> itemValues = new();
        if (_bBag)
            ItemDegradationHelpers.FindItemValues(player.bag.GetSlots(), ItemName, tags, itemValues);
        if (_bEquipment)
            ItemDegradationHelpers.FindItemValues(player.equipment.m_slots, ItemName, tags, itemValues);
        if (_bToolbelt)
            ItemDegradationHelpers.FindItemValues(player.inventory.GetSlots(), ItemName, tags, itemValues);

        foreach (var itemValue in itemValues)
        {
            ItemDegradationHelpers.CheckModification(itemValue, player, _degradeOverride);
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var handledByBase = base.ParseXmlAttribute(_attribute);
        if (handledByBase) return true;

        var localName = _attribute.Name.LocalName;
        if (localName.Equals("item_name", StringComparison.OrdinalIgnoreCase))
        {
            ItemName = _attribute.Value;
            return true;
        }

        if (localName.Equals("tags", StringComparison.OrdinalIgnoreCase))
        {
            TagsString = _attribute.Value;
            _parsedTags = FastTags<TagGroup.Global>.Parse(TagsString);
            _hasTagsToSearch = !_parsedTags.IsEmpty; 
            return true;
        }

        if (localName == "slots")
        {
            var slots = _attribute.Value.ToLower();
            _bBag = slots.Contains("bag");
            _bEquipment = slots.Contains("equipment");
            _bToolbelt = slots.Contains("inventory");
            return true;
        }

        if (localName == "DegradeOverride")
        {
            _degradeOverride = StringParsers.ParseSInt32(_attribute.Value);
        }

        return false;
    }
}