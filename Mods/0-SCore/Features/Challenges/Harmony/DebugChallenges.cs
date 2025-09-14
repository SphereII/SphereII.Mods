using Challenges;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Challenges.Harmony {
    public class DebugChallenges {
        
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "Challenges";

        
        [HarmonyPatch(typeof(ChallengeJournal))]
        [HarmonyPatch(nameof(ChallengeJournal.AddChallenge))]
        public class ChallengeJournalAddChallenge {
            public static bool Prefix(ChallengeJournal __instance,Challenge challenge) {
                __instance.ChallengeDictionary.TryAdd(challenge.ChallengeClass.Name, challenge);
                if (! __instance.Challenges.Contains(challenge))
                    __instance.Challenges.Add(challenge);
                return false;
            }
        }
        
        [HarmonyPatch(typeof(RequirementObjectiveGroupGatherIngredients))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new[] { typeof(string) })]
        public class RequirementObjectiveGroupGatherIngredientsConstructor {
            public static void Postfix(RequirementObjectiveGroupGatherIngredients __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                    
                if ( string.IsNullOrEmpty(__instance.ItemID))
                    Log.Out($"GroupGatherIngredients: ItemID is null.");
                if (__instance.itemRecipe == null)
                    Log.Out($"GroupGatherIngredients: ItemRecipe for {__instance.ItemID} was not found. Typo?");
            }
        }
        
        [HarmonyPatch(typeof(RequirementObjectiveGroupPlace))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new[] { typeof(string) })]
        public class RequirementObjectiveGroupPlaceConstructor {
            public static void Postfix(RequirementObjectiveGroupPlace __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (string.IsNullOrEmpty(__instance.ItemID))
                    Log.Out($"RequirementObjectiveGroupPlace: ItemID is null.");
                var recipe = CraftingManager.GetRecipe(__instance.ItemID);
                if ( recipe == null )
                    Log.Out($"RequirementObjectiveGroupPlace: ItemRecipe for {__instance.ItemID} was not found. Typo?");
            }
        }
        
        [HarmonyPatch(typeof(RequirementObjectiveGroupCraft))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new[] { typeof(string) })]
        public class RequirementObjectiveGroupCraftConstructor {
            public static void Postfix(RequirementObjectiveGroupCraft __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (string.IsNullOrEmpty(__instance.ItemID))
                    Log.Out($"RequirementObjectiveGroupCraft: ItemID is null.");
                var recipe = CraftingManager.GetRecipe(__instance.ItemID);
                if ( recipe == null )
                    Log.Out($"RequirementObjectiveGroupCraft: ItemRecipe for {__instance.ItemID} was not found. Typo?");
            }
        }
        //
        // [HarmonyPatch(typeof(ChallengeJournal))]
        // [HarmonyPatch("Clone")]
        // public class ChallengeJournalClone {
        //     public static void Postfix(ChallengeJournal __instance) {
        //         Debug.Log("Clone()");
        //         Debug.Log($"Groups: {__instance.ChallengeGroups.Count}");
        //         Debug.Log($"Challenges: {__instance.Challenges.Count}");
        //         
        //     }
        // }
        
        /*
        [HarmonyPatch(typeof(ChallengeJournal))]
        [HarmonyPatch("Read")]
        public class ChallengeJournalRead {
            public static void Postfix(ChallengeJournal __instance) {
                Debug.Log($"Read Groups: {__instance.ChallengeGroups.Count}");
                Debug.Log($"Read Challenges: {__instance.Challenges.Count}");
            }
        }
        
        [HarmonyPatch(typeof(ChallengeJournal))]
        [HarmonyPatch("Write")]
        public class ChallengeJournalWrite {
            public static void Postfix(ChallengeJournal __instance) {
                Debug.Log($"Write Groups: {__instance.ChallengeGroups.Count}");
                Debug.Log($"Write Challenges: {__instance.Challenges.Count}");
            }
        }
        */

    }
}