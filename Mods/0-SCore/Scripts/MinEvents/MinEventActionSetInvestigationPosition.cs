using System;
using System.Xml.Linq;


// <triggered_effect trigger = "onSelfBuffUpdate" action="SetInvestigationPosition, SCore" target="positionAOE" range="10" ticks="100" />
public class MinEventActionSetInvestigationPosition : MinEventActionTargetedBase
{
    private int _ticks = 1;

    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            targets[i]?.SetInvestigatePosition(_params.Position, _ticks);
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        if (string.Equals(_attribute.Name.LocalName, "ticks", StringComparison.OrdinalIgnoreCase))
        {
            _ticks = StringParsers.ParseSInt32(_attribute.Value);
            return true;
        }

        return base.ParseXmlAttribute(_attribute);
    }
}
