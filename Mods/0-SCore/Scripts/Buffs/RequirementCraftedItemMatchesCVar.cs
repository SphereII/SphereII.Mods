using System.Collections.Generic;
using System.Xml.Linq;

// True when the current ItemValue's type id equals the value held in a cvar -
// i.e. "this is the same item as the one recorded there".
//
// Written for repeat-craft detection. MinEventActionSetCVarFromItemType stores
// the type of each craft; this reads it back on the next one, so an award can
// pay full XP for a different item and little or nothing for the same one
// queued over and over. A cooldown cannot do this job: when an item takes
// longer to craft than the cooldown lasts, every craft in a queue of twenty
// scores exactly as if it had been twenty different items.
//
// <requirement name="!CraftedItemMatchesCVar, SCore" cvar="$craftingclubs_lbd_lastitem"/>
//
// Supports the ! prefix for negation like any requirement. Reads the cvar from
// the target entity, defaulting to self.
public class RequirementCraftedItemMatchesCVar : TargetedCompareRequirementBase
{
    private string _cvar;

    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params)) return false;
        var matches = Matches(_params);
        return invert ? !matches : matches;
    }

    private bool Matches(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(_cvar)) return false;

        var itemValue = _params.ItemValue;
        if (itemValue == null || itemValue.IsEmpty()) return false;

        var entity = _params.Self;
        if (entity?.Buffs == null) return false;

        // An unrecorded cvar reads as 0, which no item type uses, so the first
        // craft after a reset always counts as different.
        return entity.Buffs.GetCustomVar(_cvar) == itemValue.type;
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        if (base.ParseXAttribute(_attribute)) return true;

        if (_attribute.Name.LocalName == "cvar")
        {
            _cvar = _attribute.Value;
            return true;
        }

        return false;
    }

    public override void GetInfoStrings(ref List<string> list)
    {
        list.Add(string.Format("Crafted Item Is {0}The Same As Last", invert ? "NOT " : ""));
    }
}
