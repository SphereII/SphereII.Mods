using System;
using System.Xml.Linq;

/// <summary>
/// <para>
/// Compares the value of a cvar on the entity holding the buff ("self"), with the value of a cvar
/// on the target entity. The "self" value is always on the left-hand side of the operation.
/// </para>
/// 
/// <para>
/// If only the "cvar" attribute is specified, it is used as the name of the cvar on the "self"
/// entity <em>and</em> the target entity. So, if you want to compare the same cvar on both
/// entities, you do not need to specify the "targetcvar" attribute.
/// </para>
/// 
/// <example>
/// Example: Require that the "self" entity's vanilla "bleedCounter" cvar is greater than or equal
/// to a cvar named "bleedChance" on the target entity.
/// <code>
/// &lt;requirement name="TargetCVarCompare, SCore" cvar="bleedCounter" operation="GTE" targetcvar="bleedChance" />
/// </code>
/// </example>
/// 
/// <example>
/// Example: Require that the "self" entity's vanilla "bleedCounter" cvar is greater than or equal
/// to the vanilla "bleedCounter" cvar on the target entity. Since the cvars have the same name,
/// the "targetcvar" attribute is optional.
/// <code>
/// &lt;requirement name="TargetCVarCompare, SCore" cvar="bleedCounter" operation="GTE" />
/// </code>
/// </example>
/// </summary>
public class TargetCVarCompare : RequirementBase
{
    private string selfCVarName;
    private string targetCVarName;

    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
        {
            return false;
        }

        var isValid = compareValues(
            _params.Self.GetCVar(selfCVarName),
            operation,
            _params.Other.GetCVar(targetCVarName));

        return invert ? !isValid : isValid;
    }

   

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        var localName = _attribute.Name.LocalName;

        if (string.Equals(localName, "cvar", StringComparison.OrdinalIgnoreCase))
        {
            selfCVarName = _attribute.Value;
            targetCVarName ??= _attribute.Value;
            return true;
        }

        if (string.Equals(localName, "targetcvar", StringComparison.OrdinalIgnoreCase))
        {
            targetCVarName = _attribute.Value;
            return true;
        }

        return base.ParseXAttribute(_attribute);
    }
}
