using System.Xml;
using System.Xml.Linq;
using UnityEngine;

/// <summary>
///     <para>
///         Changes the target's relationship with the specified faction, and all the specified related
///         factions (if any), according to the relationships between the faction and the related factions.
///     </para>
///     <para>
///         Supported tag attributes:
///         <list type="bullet">
///             <item>
///                 <term>target</term>
///                 <description>
///                     The entity target(s), to change relationships with the factions.
///                     Valid target values: "self", "other", "selfAOE", "otherAOE", "positionAOE".
///                     In most cases you should use "self" (the player).
///                     Required.
///                 </description>
///             </item>
///             <item>
///                 <term>range</term>
///                 <description>
///                     Maximum range (distance in meters) to include entities as targets.
///                     Used only when the target is one of the AOE (Area Of Effect) values.
///                     Optional.
///                 </description>
///             </item>
///             <item>
///                 <term>target_tags</term>
///                 <description>
///                     Adds entities with these tags, to the list of entity targets.
///                     Special tag values: "party", "ally", "enemy".
///                     Optional.
///                 </description>
///             </item>
///             <item>
///                 <term>faction</term>
///                 <description>
///                     The primary faction whose relationship will change with the target.
///                     Required.
///                 </description>
///             </item>
///             <item>
///                 <term>value</term>
///                 <description>
///                     The amount to add to the relationship between the target and "primary" faction.
///                     Positive numbers represent amity, negative numbers represent antagonism.
///                     It takes 200 points to change a faction relationship entirely
///                     (e.g. from "neutral" to "like").
///                     Required.
///                 </description>
///             </item>
///             <item>
///                 <term>related</term>
///                 <description>
///                     One or more related factions, whose relationships will change with the target
///                     according to each one's relationship with the primary faction.
///                     It should be a comma-separated list of faction names (without spaces).
///                     You can not include either the target's faction, nor the "primary" faction.
///                     Optional; if not specified, no faction relationships are affected, other than the
///                     one between the target and primary faction.
///                     In this case, it behaves exactly like the ModifyFactionSDX action.
///                 </description>
///             </item>
///             <item>
///                 <term>scale</term>
///                 <description>
///                     A ratio by which to scale the target relationship to related factions.
///                     Suggested values are between 0 and 1 (exclusive).
///                     Optional; defaults to 1.
///                     (Ignored if "related" is not specified.)
///                 </description>
///             </item>
///         </list>
///     </para>
///     <para>
///         Default multiplier, to determine the relationship value betwen the target and each related
///         faction, by related faction's relationship with the primary faction.
///         These multipliers are then multiplied by the "scale" property (if specified).
///         <list type="table">
///             <listheader>
///                 <term>Relationship to primary faction</term>
///                 <description>Value multiplier</description>
///             </listheader>
///             <item>
///                 <term>Hate</term>
///                 <description>-0.6666667</description>
///             </item>
///             <item>
///                 <term>Dislike</term>
///                 <description>-0.3333333</description>
///             </item>
///             <item>
///                 <term>Neutral</term>
///                 <description>0 (no effect)</description>
///             </item>
///             <item>
///                 <term>Like</term>
///                 <description>0.3333333</description>
///             </item>
///             <item>
///                 <term>Love</term>
///                 <description>0.6666667</description>
///             </item>
///             <item>
///                 <term>Leader</term>
///                 <description>1.0</description>
///             </item>
///         </list>
///     </para>
/// </summary>
/// <example>
///     <code>
/// <!--
///     Gives 10 points to the player's relationship with bandits, and adjusts the player's
///     relationship with the whisperer faction according to its current relationship with bandits.
///     So if the whisperer faction loves bandits, add 6.66.. points to the player's relationship
///     with whisperers; if the whisperer faction dislikes (but doesn't hate) bandits, subtract 
///     3.33.. points from the player's relationship with whisperers.
/// -->
/// <triggered_effect trigger="onSelfBuffStart" action="ModifyRelatedFactionsSDX, SCore" target="self" faction="bandits"
///             value="10" related="whisperers" />
/// 
/// <!--
///     Gives 10 points to the player's relationship with bandits, adjusts the player's
///     relationship with the whisperer faction according to its current relationship with bandits,
///     and scales that adjustment by 0.5 (half).
///     So if the whisperer faction loves bandits, add 3.33.. points to the player's relationship
///     with whisperers; if the whisperer faction dislikes (but doesn't hate) bandits, subtract 
///     1.66.. points from the player's relationship with whisperers.
/// -->
/// <triggered_effect trigger="onSelfBuffStart" action="ModifyRelatedFactionsSDX, SCore" target="self" faction="bandits"
///             value="10" related="whisperers" scale="0.5" />
/// 
/// <!--
///     Subtracts 10 points to the player's relationship with white river, and adjust the player's
///     relationships with the bandits, red team, blue team, and green team factions according to
///     each faction's current relationship with white river.
/// -->
/// <triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyRelatedFactionsSDX, SCore" target="self"
///             faction="whiteriver" value="-10" related="bandits,redteam,blueteam,greenteam" />
/// 
/// <!--
///     Adds 10 points to the player's relationship with the red team.
///     Since the "related" attribute is omitted, it does not modify the player's relationships
///     with any other factions.
///     In this case, it behaves exactly like ModifyFactionSDX.
/// -->
/// <triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyRelatedFactionsSDX, SCore" target="self"
///             faction="redteam" value="10" />
/// </code>
/// </example>
public class MinEventActionModifyRelatedFactionsSDX : MinEventActionTargetedBase
{
    private readonly bool debug = false; // for logging when testing

    private string faction = "";

    private string[] relatedFactions;

    private float scale = 1f;

    private float value;

    public override void Execute(MinEventParams _params)
    {
        if (debug) Debug.Log("MinEventActionModifyRelatedFactionsSDX.Execute...");
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i];
            if (entity != null)
            {
                var otherFaction = FactionManager.Instance.GetFactionByName(faction);
                if (otherFaction != null)
                {
                    otherFaction.ModifyRelationship(entity.factionId, value);
                    if (debug) Debug.Log(string.Format("relationship to {0} modified by {1}", faction, value));
                    ModifyRelatedFactionRelationships(entity.factionId, otherFaction);
                }
            }
        }
    }

    private void ModifyRelatedFactionRelationships(byte targetEntityFactionId, Faction otherFaction)
    {
        if (relatedFactions == null) return;

        for (var i = 0; i < relatedFactions.Length; i++)
        {
            var relatedFaction = FactionManager.Instance.GetFactionByName(relatedFactions[i]);

            // Guard against users entering a related faction name that doesn't exist, is the
            // name of a target entity's faction, or is the same as the "faction" attribute
            if (relatedFaction != null &&
                relatedFaction.ID != otherFaction.ID &&
                relatedFaction.ID != targetEntityFactionId)
            {
                var modifier = CalculateRelatedFactionModifier(
                    relatedFaction.GetRelationship(otherFaction.ID));

                relatedFaction.ModifyRelationship(targetEntityFactionId, modifier);

                if (debug) Debug.Log(string.Format("relationship to {0} modified by {1}", relatedFactions[i], modifier));
            }
        }
    }

    private float CalculateRelatedFactionModifier(float relationshipToFaction)
    {
        float ratio = 1; // default to Leader

        if (relationshipToFaction < 200f) // Hate
            ratio = -0.6666667f;
        else if (relationshipToFaction < 400f) // Dislike
            ratio = -0.3333333f;
        else if (relationshipToFaction < 600f) // Neutral
            ratio = 0f;
        else if (relationshipToFaction < 800f) // Like
            ratio = 0.3333333f;
        else if (relationshipToFaction < 1001f) // Love
            ratio = 0.6666667f;

        var relationshipToTarget = ratio * value * scale;

        // The value 255f has a special meaning in TFP code, so avoid it
        return relationshipToTarget == 255f ? relationshipToTarget + 1 : relationshipToTarget;
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        if (base.ParseXmlAttribute(_attribute)) return true;

        var name = _attribute.Name.LocalName;
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

        if (name == "related")
        {
            relatedFactions = ParseArray(_attribute.Value);
            return true;
        }

        if (name == "scale")
        {
            scale = StringParsers.ParseFloat(_attribute.Value);
            return true;
        }

        return false;
    }

    private static string[] ParseArray(string str)
    {
        var subs = str.Split(',');
        for (var i = 0; i < subs.Length; i++)
            subs[i] = subs[i].Trim();
        return subs;
    }
}