using System.Collections.Generic;
using System.Xml.Linq;

// Tests the tags of the BLOCK an item places, for the item currently in play -
// typically the thing just crafted on onSelfItemCrafted.
//
// RequirementIsTargetBlock reads _params.BlockValue, i.e. the block you are
// looking at. There was no way to ask "is the item I just crafted a block, and
// does that block carry tag X". Craftable blocks have no entry in items.xml and
// no UnlockedBy property, so ItemHasProperty and ItemHasTags both miss them -
// which left craftingTraps and craftingElectrician unable to score at all, since
// every one of their recipes produces a block.
//
// <requirement name="RequirementCraftedBlockHasTags, SCore" tags="trapsSkill"/>
//
// tags          comma separated; any match passes unless has_all_tags is true.
// has_all_tags  require every listed tag rather than any.
// Supports the ! prefix for negation, as all requirements do.
public class RequirementCraftedBlockHasTags : TargetedCompareRequirementBase
{
    private FastTags<TagGroup.Global> _blockTags;
    private bool _hasAllTags;

    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params)) return false;
        var valid = IsValidBlock(_params);
        return invert ? !valid : valid;
    }

    private bool IsValidBlock(MinEventParams _params)
    {
        var itemValue = _params.ItemValue;
        if (itemValue == null || itemValue.ItemClass == null) return false;

        // Only block-placing items resolve to a block; everything else is not ours.
        if (!itemValue.ItemClass.IsBlock()) return false;

        var block = itemValue.ToBlockValue().Block;
        if (block == null) return false;

        return _hasAllTags
            ? block.HasAllFastTags(_blockTags)
            : block.HasAnyFastTags(_blockTags);
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXAttribute(_attribute);
        if (flag) return true;

        switch (_attribute.Name.LocalName)
        {
            case "tags":
                _blockTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
                return true;
            case "has_all_tags":
                _hasAllTags = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
                return true;
        }

        return false;
    }

    public override void GetInfoStrings(ref List<string> list)
    {
        list.Add(string.Format("Crafted Block Has {0}Tags", invert ? "NOT " : ""));
    }
}
