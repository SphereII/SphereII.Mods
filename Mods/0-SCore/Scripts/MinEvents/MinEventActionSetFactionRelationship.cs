using System.Xml;
using System.Xml.Linq;
using UnityEngine;

/// <summary>
/// <para>
/// Sets the target's relationship with the specified faction.
/// </para>
/// <para>
/// Supported tag attributes:
/// <list type="bullet">
///     <item>
///         <term>target</term>
///         <description>
///             The entity target(s), to change relationships with the factions.
///             Valid target values: "self", "other", "selfAOE", "otherAOE", "positionAOE".
///             In most cases you should use "self" (the player).
///             Required.
///         </description>
///     </item>
///     <item>
///         <term>range</term>
///         <description>
///             Maximum range (distance in meters) to include entities as targets.
///             Used only when the target is one of the AOE (Area Of Effect) values.
///             Optional.
///         </description>
///     </item>
///     <item>
///         <term>target_tags</term>
///         <description>
///             Adds entities with these tags, to the list of entity targets.
///             Special tag values: "party", "ally", "enemy".
///             Optional.
///         </description>
///     </item>
///     <item>
///         <term>faction</term>
///         <description>
///             The primary faction whose relationship with the target will be set.
///             Required.
///         </description>
///     </item>
///     <item>
///         <term>value</term>
///         <description>
///             The value of the new relationship between the target and faction.
///             Required.
///         </description>
///     </item>
/// </list>
/// </para>
/// <para>
/// These are the relationship values you should use for each relationship tier.
/// <list type="table">
///     <listheader>
///         <term>Relationship tier</term>
///         <description>Relationship values</description>
///     </listheader>
///     <item>
///         <term>Hate</term>
///         <description>0 - 199</description>
///     </item>
///     <item>
///         <term>Dislike</term>
///         <description>200 - 399</description>
///     </item>
///     <item>
///         <term>Neutral</term>
///         <description>400 - 599</description>
///     </item>
///     <item>
///         <term>Like</term>
///         <description>600 - 799</description>
///     </item>
///     <item>
///         <term>Love</term>
///         <description>800 - 1000 (1000 is the maximum value)</description>
///     </item>
/// </list>
/// </para>
/// </summary>
/// <example>
/// <code>
/// <!-- Sets the player's relationship with bandits to 400 ("Neutral"). -->
/// <triggered_effect trigger="onSelfBuffStart" action="SetFactionRelationshipSDX, Mods" target="self" faction="bandits" value="400" />
/// </code>
/// </example>
public class MinEventActionSetFactionRelationship : MinEventActionTargetedBase
{
    private readonly bool debug = false; // for logging when testing

    private string faction = "";

    private float value = 0f;

    public override void Execute(MinEventParams _params)
    {
        if (debug)
        {
            Debug.Log("MinEventActionSetFactionRelationshipSDX.Execute...");
        }
        for (int i = 0; i < targets.Count; i++)
        {
            EntityAlive entity = targets[i];
            if (entity != null)
            {
                Faction otherFaction = FactionManager.Instance.GetFactionByName(faction);
                if (otherFaction != null)
                {
                    otherFaction.SetRelationship(entity.factionId, value);
                    if (debug)
                    {
                        Debug.Log(string.Format("relationship to {0} set to {1}", faction, value));
                    }
                }
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        if (base.ParseXmlAttribute(_attribute))
        {
            return true;
        }

        string name = _attribute.Name.LocalName;
        if (name == "faction")
        {
            faction = _attribute.Value.Trim();
            return true;
        }
        if (name == "value")
        {
            value = StringParsers.ParseFloat(_attribute.Value);
            return true;
        }

        return false;
    }
}
