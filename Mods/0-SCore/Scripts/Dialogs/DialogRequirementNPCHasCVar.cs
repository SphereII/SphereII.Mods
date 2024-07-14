/// <summary>
/// <para>
/// Requires that the dialog's NPC must consider its own cvar specified by <see cref="BaseDialogRequirement.ID"/>,
/// whose existence and value matches an expression specified by <see cref="IDialogOperator.Operator"/>
/// and <see cref="BaseDialogRequirement.Value"/>.
/// </para>
/// 
/// <example>
/// Example: Hide the response unless the NPC has a cvar named "myCVar" with a value less than 5.
/// <code>
/// &lt;requirement type="NPCHasCVarSDX, SCore" requirementtype="Hide" id="myCVar" operator="lte" value="5" />
/// </code>
/// </example>
/// 
/// <example>
/// Example: Hide the response unless the NPC has a cvar named "myCVar" with any value (including zero).
/// <code>
/// &lt;requirement type="NPCHasCVarSDX, SCore" requirementtype="Hide" id="myCVar" />
/// </code>
/// </example>
/// 
/// <example>
/// Example: Hide the response unless the NPC does <b>not</b> have a cvar named "myCVar".
/// <code>
/// &lt;requirement type="NPCHasCVarSDX, SCore" requirementtype="Hide" id="myCVar" operator="not" />
/// </code>
/// </example>
/// </summary>
public class DialogRequirementNPCHasCVarSDX : BaseDialogRequirement, IDialogOperator
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    /// <summary>
    /// <para>
    /// Operator to use. This is set in
    /// <see cref="SphereII_DialogFromXML_Extensions.SphereII__DialogFromXML_ParseRequirement"/>.
    /// </para>
    /// 
    /// <para>
    /// Valid values:
    /// <list>
    ///    <item>
    ///        <term>"eq"</term>
    ///        <description>
    ///        The cvar value must exist, and its value must be equal to <see cref="BaseDialogAction.Value"/>.
    ///        This is the default.
    ///        </description>
    ///    </item>
    ///    <item>
    ///        <term>"neq"</term>
    ///        <description>The cvar value must not be equal to <see cref="BaseDialogAction.Value"/>.</description>
    ///    </item>
    ///    <item>
    ///        <term>"lt"</term>
    ///        <description>The cvar value must be strictly less than <see cref="BaseDialogAction.Value"/>.</description>
    ///    </item>
    ///    <item>
    ///        <term>"lte"</term>
    ///        <description>The cvar value must be less than or equal to <see cref="BaseDialogAction.Value"/>.</description>
    ///    </item>
    ///    <item>
    ///        <term>"gt"</term>
    ///        <description>The cvar value must be strictly greater than <see cref="BaseDialogAction.Value"/>.</description>
    ///    </item>
    ///    <item>
    ///        <term>"gte"</term>
    ///        <description>The cvar value must be greater than or equal to <see cref="BaseDialogAction.Value"/>.</description>
    ///    </item>
    ///    <item>
    ///        <term>"not"</term>
    ///        <description>The cvar must not exist, or its value must be equal to zero. <see cref="BaseDialogAction.Value"/> is ignored.</description>
    ///    </item>
    /// </list>
    /// </para>
    /// </summary>
    public string Operator { get; set; } = "eq";

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var invert = Operator.ToLower() == "not";

        if (talkingTo.Buffs.HasCustomVar(ID))
        {
            var npcValue = talkingTo.Buffs.GetCustomVar(ID);

            // If value is not specified, accepted it, unless the cvar isn't supposed to exist.
            if (string.IsNullOrEmpty(Value))
            {
                var passes = invert ? npcValue == 0 : true;
                AdvLogging.DisplayLog(AdvFeatureClass, $"{GetType()} HasCvar: {ID} Operator: {Operator} Value is empty, returning {passes}");
                return passes;
            }

            float.TryParse(Value, out var flValue);

            AdvLogging.DisplayLog(AdvFeatureClass, $"{GetType()} HasCvar: {ID} Value: {flValue} NPC Value: {npcValue} Operator: {Operator}");

            return Operator.ToLower() switch
            {
                "lt" => npcValue < flValue,
                "lte" => npcValue <= flValue,
                "gt" => npcValue > flValue,
                "gte" => npcValue >= flValue,
                "neq" => flValue != npcValue,
                "not" => npcValue == 0,
                _ => flValue == npcValue,
            }; ;
        }

        return invert;
    }

}
