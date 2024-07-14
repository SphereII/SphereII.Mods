using System.Xml.Linq;
using JetBrains.Annotations;

/// <summary>
/// Checks to see if there's any fire blocks within the specified range, using the player's position as center.
/// </summary>
/// <remarks>
/// Example:
///
///     The cvar specified, by default _closeFires, will contain the number of burning blocks in the range.
///    	<triggered_effect trigger="onSelfBuffUpdate" action="CheckFireProximity, SCore" range="5" cvar="_closeFires" />
/// </remarks>
[UsedImplicitly]
public class MinEventActionCheckFireProximity : MinEventActionRemoveBuff
{
    string cvar = "_closeFires";


    public override void Execute(MinEventParams @params)
    {
        if (FireManager.Instance.Enabled == false) return;

        var position = new Vector3i(@params.Self.position);
        var count = FireManager.Instance.CloseFires(position, (int)maxRange);
        @params.Self.Buffs.SetCustomVar(cvar, count);
    }

    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag) return true;
        var name = attribute.Name.LocalName;
        if (name != "cvar") return false;
        cvar = attribute.Value;
        return true;

    }
}