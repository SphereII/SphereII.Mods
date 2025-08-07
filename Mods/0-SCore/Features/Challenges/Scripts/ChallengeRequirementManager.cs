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

    public static bool IsValid(string id)
    {
        if (!ChallengeRequirements.TryGetValue(id.ToLower(), out var requirements))
        {
            return true;
        }
        var _params = new MinEventParams();
        _params.Self = GameManager.Instance.World.GetPrimaryPlayer();
        _params.Biome = _params.Self.biomeStandingOn;
        _params.ItemValue = _params.Self.inventory.holdingItemItemValue;
        foreach (var t in requirements)
        {
            if (t.IsValid(_params) == false)
            {
                return false;
            }
        }
        return true;
    }
}