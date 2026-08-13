using System.Text;
using System.Xml.Linq;

// Logs the block the event is about: its name, and the tags it actually resolves
// to at runtime after Extends inheritance.
//
// Written because "which block did you just hit, and what tags does it carry"
// turned out to be the question behind several rounds of guesswork. Block tags
// are not visible from the XML alone - a block with no Tags property of its own
// inherits its parent's - and the crafted or targeted block is not something a
// log message can otherwise name.
//
// <triggered_effect trigger="onSelfPrimaryActionRayHit" action="LogTargetBlock, SCore"
//                   prefix="LBD BLOCK"/>
//
// prefix   optional label for the line. Defaults to "SCore BLOCK".
//
// Prints "<prefix>: <blockName> tags=[a,b,c]" for the targeted block, and falls
// back to the ItemValue's block when there is no target - which is what a craft
// event carries.
public class MinEventActionLogTargetBlock : MinEventActionBase
{
    private string _prefix = "SCore BLOCK";

    public override void Execute(MinEventParams _params)
    {
        Block block = null;
        var source = "target";

        if (!_params.BlockValue.isair && _params.BlockValue.Block != null)
        {
            block = _params.BlockValue.Block;
        }
        else if (_params.ItemValue != null && _params.ItemValue.ItemClass != null
                 && _params.ItemValue.ItemClass.IsBlock())
        {
            block = _params.ItemValue.ToBlockValue().Block;
            source = "item";
        }

        if (block == null)
        {
            Log.Out($"{_prefix}: no block on this event");
            return;
        }

        var sb = new StringBuilder();
        sb.Append(_prefix).Append(": ").Append(block.GetBlockName())
          .Append(" (").Append(source).Append(") tags=[");
        var first = true;
        foreach (var tag in block.Tags.GetTagNames())
        {
            if (!first) sb.Append(',');
            sb.Append(tag);
            first = false;
        }
        sb.Append(']');
        Log.Out(sb.ToString());
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        if (base.ParseXmlAttribute(_attribute)) return true;

        if (_attribute.Name.LocalName == "prefix")
        {
            _prefix = _attribute.Value;
            return true;
        }

        return false;
    }
}
