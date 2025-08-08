using System.Xml.Linq;
using Challenges;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Challenges.Harmony
{
    [HarmonyPatch(typeof(ChallengeClass))]
    [HarmonyPatch(nameof(ChallengeClass.ParseElement))]
    public class ChallengesFromXmlPatches
    {
        public static void Postfix(ChallengeClass __instance, XElement e)
        {
            ChallengeRequirementManager.AddRequirements(__instance.Name.ToLower(), e);
        }
    }
    
    [HarmonyPatch(typeof(BaseChallengeObjective))]
    [HarmonyPatch(nameof(BaseChallengeObjective.CheckBaseRequirements))]
    public class BaseChallengeObjectiveCheckBaseRequirements
    {
        public static void Postfix(ref bool __result, BaseChallengeObjective __instance)
        {
            // If the CheckBaseRequirements is true, that means it's failed it's vanilla checks, so it won't run the challenge.
            // Since it's already failing here, we won't do anything at this point.
            if (__result) return;
            
            // skip if we are just initializing
            if (string.IsNullOrEmpty(__instance.Owner.ChallengeClass.Name)) return;
            
            // Check to see if we have any requirements for this.
            var results = ChallengeRequirementManager.IsValid(__instance.Owner.ChallengeClass.Name.ToLower());
            __result = !results;

        }
    }
}