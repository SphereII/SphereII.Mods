using Challenges;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Challenges.Harmony {
    public class DebugChallenges {
        [HarmonyPatch(typeof(ChallengeJournal))]
        [HarmonyPatch("AddChallenge")]
        public class ChallengeJournalAddChallenge {
            public static bool Prefix(ChallengeJournal __instance,Challenge challenge) {
                __instance.ChallengeDictionary.TryAdd(challenge.ChallengeClass.Name, challenge);
                if (! __instance.Challenges.Contains(challenge))
                    __instance.Challenges.Add(challenge);
                return false;
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