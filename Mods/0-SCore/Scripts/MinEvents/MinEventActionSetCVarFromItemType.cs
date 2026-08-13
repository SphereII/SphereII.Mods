using System.Xml.Linq;

// Stores the current ItemValue's type id in a cvar, so a later event can tell
// whether it is looking at the same item again.
//
// Paired with RequirementCraftedItemMatchesCVar to give crafting skills a memory
// of the previous craft. A cooldown cannot throttle crafting - if an item takes
// longer to make than the cooldown lasts, every single craft scores - so
// repetition has to be detected by identity rather than by timing.
//
// <triggered_effect trigger="onSelfItemCrafted" action="SetCVarFromItemType, SCore"
//                   cvar="$craftingclubs_lbd_lastitem"/>
//
// Must be ordered AFTER the awards that read it, so the awards compare against
// the previous craft rather than the current one. An empty or missing ItemValue
// writes 0, which no real item type uses.
public class MinEventActionSetCVarFromItemType : MinEventActionTargetedBase
{
    private string _cvar;

    public override void Execute(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(_cvar)) return;

        var itemValue = _params.ItemValue;
        var value = (itemValue == null || itemValue.IsEmpty()) ? 0f : itemValue.type;

        if (targets == null || targets.Count == 0)
        {
            _params.Self?.Buffs?.SetCustomVar(_cvar, value);
            return;
        }

        for (var i = 0; i < targets.Count; i++)
        {
            targets[i]?.Buffs?.SetCustomVar(_cvar, value);
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        if (base.ParseXmlAttribute(_attribute)) return true;

        if (_attribute.Name.LocalName == "cvar")
        {
            _cvar = _attribute.Value;
            return true;
        }

        return false;
    }
}
