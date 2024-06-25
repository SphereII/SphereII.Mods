using System;
using System.Xml.Linq;

/// <summary>
/// <para>
/// This is a <see cref="MinEventActionTargetedBase"/> that will set the revenge target of all the
/// target entities.
/// </para>
/// <para>
/// This action takes a <c>value</c> attribute, which should be the entity ID of the revenge target.
/// If the value of the attribute is 0 or less, the revenge target will be set to null.
/// Omitting the "value" attribute will also set the revenge target to null.
/// </para>
/// 
/// <example>
/// Example: Set the revenge target of the entity you're attacking, to yourself.
/// This uses the read-only "_entityId" cvar that is introduced in this mod.
/// (This is only an example; the game does this automatically.)
/// <code>
/// &lt;triggered_effect trigger="onSelfAttackedOther" action="SetRevengeTarget, SCore" target="other" value="@_entityId" />
/// </code>
/// </example>
/// 
/// <example>
/// Example: When the buff updates, set the revenge targets of everything within 10 meters of you,
/// with the "friendly" tag, to your own revenge target, if you have one.
/// This uses the read-only "_revengeTargetId" cvar that is introduced in this mod.
/// <code>
/// &lt;triggered_effect trigger = "onSelfBuffUpdate" action="SetRevengeTarget, SCore" target="selfAOE" range="10" target_tags="friendly" value="@_revengeTargetId">
///     &lt;requirement name = "CVarCompare" target="self" cvar="_revengeTargetId" operation="GTE" value="0" />
/// &lt;/triggered_effect>
/// </code>
/// </example>
/// 
/// <example>
/// Example: Clear the revenge target of the entity that damaged you.
/// <code>
/// &lt;!-- To clear the revenge target, set the "value" attribute to zero: -->
/// &lt;triggered_effect trigger="onOtherDamagedSelf" action="SetRevengeTarget, SCore" target="other" value="0" />
/// &lt;!-- You can also use -1 or any other negative number: -->
/// &lt;triggered_effect trigger="onOtherDamagedSelf" action="SetRevengeTarget, SCore" target="other" value="-1" />
/// &lt;!-- Or, omit the "value" attribute altogether: -->
/// &lt;triggered_effect trigger="onOtherDamagedSelf" action="SetRevengeTarget, SCore" target="other" />
/// </code>
/// </example>
/// </summary>
public class MinEventActionSetRevengeTarget : MinEventActionTargetedBase
{
    private int _entityId = 0;

    public override void Execute(MinEventParams _params)
    {
        EntityAlive entity = null;

        if (_entityId > 0)
        {
            entity = GameManager.Instance.World.GetEntity(_entityId) as EntityAlive;
        }

        for (var i = 0; i < targets.Count; i++)
        {
            targets[i]?.SetRevengeTarget(entity);
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        if (string.Equals(_attribute.Name.LocalName, "value", StringComparison.OrdinalIgnoreCase))
        {
            _entityId = StringParsers.ParseSInt32(_attribute.Value);
            return true;
        }

        return base.ParseXmlAttribute(_attribute);
    }
}
