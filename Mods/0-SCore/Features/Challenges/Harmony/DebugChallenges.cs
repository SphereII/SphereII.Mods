// using Challenges;
// using HarmonyLib;
// using UnityEngine;
//
// namespace SCore.Features.Challenges.Harmony {
//     public class DebugChallenges {
//         // [HarmonyPatch(typeof(XUiC_ChallengeGroupEntry))]
//         // [HarmonyPatch("OnOpen")]
//         // public class ChallengeReadObjective {
//         //     public static void Postfix(XUiC_ChallengeGroupEntry __instance) {
//         //         Debug.Log($"Total Challenges: {__instance.player.challengeJournal.Challenges.Count}");
//         //         Debug.Log($"Total Dict Challenges: {__instance.player.challengeJournal.ChallengeDictionary.Count}");
//         //         // foreach (var challenge in __instance.player.challengeJournal.Challenges)
//         //         // {
//         //         //     Debug.Log($"Challenge: {challenge.ChallengeClass.Name}");
//         //         //     Debug.Log($" Group: {challenge.ChallengeGroup.Name}");
//         //         // }
//         //         
//         //     }
//         // }
//         //
//         // [HarmonyPatch(typeof(ChallengeJournal))]
//         // [HarmonyPatch("Clone")]
//         // public class ChallengeJournalClone {
//         //     public static void Postfix(ChallengeJournal __instance) {
//         //         Debug.Log("Clone()");
//         //         Debug.Log($"Groups: {__instance.ChallengeGroups.Count}");
//         //         Debug.Log($"Challenges: {__instance.Challenges.Count}");
//         //         
//         //     }
//         // }
//         
//         /*
//         [HarmonyPatch(typeof(ChallengeJournal))]
//         [HarmonyPatch("Read")]
//         public class ChallengeJournalRead {
//             public static void Postfix(ChallengeJournal __instance) {
//                 Debug.Log($"Read Groups: {__instance.ChallengeGroups.Count}");
//                 Debug.Log($"Read Challenges: {__instance.Challenges.Count}");
//             }
//         }
//         
//         [HarmonyPatch(typeof(ChallengeJournal))]
//         [HarmonyPatch("Write")]
//         public class ChallengeJournalWrite {
//             public static void Postfix(ChallengeJournal __instance) {
//                 Debug.Log($"Write Groups: {__instance.ChallengeGroups.Count}");
//                 Debug.Log($"Write Challenges: {__instance.Challenges.Count}");
//             }
//         }
//         */
//
//     }
// }