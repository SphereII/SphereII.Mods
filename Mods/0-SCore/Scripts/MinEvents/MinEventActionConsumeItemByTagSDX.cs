using System.Xml.Linq;

// Removes one item carrying the tag from each target, taken from whichever store holds it
// (toolbelt, bag, loot container, then the harvest window - see EntityUtilities.GetItemStores).
//  <triggered_effect trigger="onSelfBuffStart" action="ConsumeItemByTagSDX, SCore" target="self" tag="medical" />
public class MinEventActionConsumeItemByTagSDX : MinEventActionTargetedBase
{
    private string _tag = string.Empty;

    public override void Execute(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(_tag)) return;

        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i];
            if (entity == null) continue;

            var stack = EntityUtilities.GetItemStackByTag(entity.entityId, _tag);
            if (stack.IsEmpty()) continue;

            EntityUtilities.DecItemFromAnyStore(entity, stack.itemValue, 1);
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;

        if (_attribute.Name.LocalName != "tag") return false;
        _tag = _attribute.Value;
        return true;
    }
}
