using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public static class ChallengeRequirementManager
{
    public static Dictionary<string, List<IRequirement>> ChallengeRequirements =
        new Dictionary<string, List<IRequirement>>();

    public static void AddRequirements(string id, XElement e)
    {
        var requirements = new List<IRequirement>();
        foreach (var xelement in e.Elements("requirement"))
        {
            var requirement = RequirementBase.ParseRequirement(xelement);
            if (requirement != null)
            {
                requirements.Add(requirement);
            }
        }

        if (requirements.Count > 0)
        {
            ChallengeRequirements.TryAdd(id.ToLower(), requirements);
        }

    }

    public static bool IsValid(string id, MinEventParams minEventContext = null) 
    {
        if (!ChallengeRequirements.TryGetValue(id.ToLower(), out var requirements))
        {
            return true;
        }
        var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
        if ( minEventContext == null )
            minEventContext = primaryPlayer.MinEventContext;

        minEventContext.Biome = primaryPlayer.biomeStandingOn;
        
        foreach (var t in requirements)
        {
            if (t.IsValid(minEventContext) == false)
            {
                return false;
            }
        }
        return true;
    }
}