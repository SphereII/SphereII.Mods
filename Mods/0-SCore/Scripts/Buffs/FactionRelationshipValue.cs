using System;
using System.Xml.Linq;

/// <summary>
/// <para>
/// Compares the faction relationship between the entity holding the buff ("self") and the target.
/// The <c>value</c> attribute can be the name of a faction relationship, or a standard numeric
/// value (a float or a cvar value).
/// </para>
/// 
/// <para>
/// Here are the valid faction relationship names, and their numeric relationship values.
/// <list type="bullet">
/// <item>"hate": 0</item>
/// <item>"dislike": 200</item>
/// <item>"neutral": 400</item>
/// <item>"like": 600</item>
/// <item>"love": 800</item>
/// </list>
/// The names are not case-sensitive.
/// </para>
/// 
/// <para>
/// The names' values are the <em>lower limit</em> in the range of values for that relationship.
/// For example, anything below 200 is considered "hate," but if you specify the relationship
/// value <em>using the name "hate",</em> then the relationship value will be zero.
/// </para>
/// 
/// <example>
/// Example: The faction relationship must be "like" or higher.
/// <code>
/// &lt;requirement name="FactionRelationshipValue, SCore" operation="GTE" value="like" />
/// &lt;!-- You can also use the numeric value for "like": -->
/// &lt;requirement name="FactionRelationshipValue, SCore" operation="GTE" value="600" />
/// </code>
/// </example>
/// 
/// <example>
/// Example: You have set a custom cvar named "charisma", with a value of 0 - 1000, and the faction
/// relationship must be lower than that value.
/// <code>
/// &lt;requirement name="FactionRelationshipValue, SCore" operation="LT" value="@charisma" />
/// </code>
/// </example>
/// </summary>
public class FactionRelationshipValue : RequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
        {
            return false;
        }

        var isValid = compareValues(GetFactionRelationship(_params), operation, value);

        return invert ? !isValid : isValid;
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        if (string.Equals(_attribute.Name.LocalName, "value", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(_attribute.Value)
            && Enum.TryParse<FactionManager.Relationship>(
                _attribute.Value.ToLower(),
                true,
                out var relationship))
        {
            value = (int)relationship;
            return true;
        }

        return base.ParseXAttribute(_attribute);
    }

    private static float GetFactionRelationship(MinEventParams _params)
    {
        return EntityUtilities.GetFactionRelationship(
            _params.Self,
            _params.Other);
    }
}