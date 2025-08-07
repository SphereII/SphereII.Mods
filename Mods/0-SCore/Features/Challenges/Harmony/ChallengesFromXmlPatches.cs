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
    
    // [HarmonyPatch(typeof(BaseChallengeObjective))]
    // [HarmonyPatch(nameof(BaseChallengeObjective.CheckBaseRequirements))]
    // public class BaseChallengeObjectiveCheckBaseRequirements
    // {
    //     public static void Postfix(ref bool __result, BaseChallengeObjective __instance)
    //     {
    //         if (__result) return;
    //         
    //         if (string.IsNullOrEmpty(__instance.Owner.ChallengeClass.Name)) return;
    //         var results = ChallengeRequirementManager.IsValid(__instance.Owner.ChallengeClass.Name.ToLower());
    //         Debug.Log($"Requirement is {results}");
    //         
    //
    //     }
    // }
}