using Challenges;
using HarmonyLib;
using UnityEngine;

namespace SCore.Harmony.PlayerFeatures {
    public class AutoRedeemChallenges {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "AutoRedeemChallenges";

        [HarmonyPatch(typeof(ChallengeJournal))]
        [HarmonyPatch(nameof(ChallengeJournal.FireEvent))]
        public class AutoRedeemChallengesFireEvent {
            public static void Postfix(ChallengeJournal __instance,MinEventTypes _eventType, MinEventParams _params) {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature, true))
                {
                    return;
                }
                foreach (var challenge in _params.Self.challengeJournal.Challenges)
                {
                    if (!challenge.ReadyToComplete) continue;
                    challenge.ChallengeState = Challenge.ChallengeStates.Redeemed;
                    challenge.Redeem();
                    QuestEventManager.Current.ChallengeCompleted(challenge.ChallengeClass, true);
                }
            }
        }
    }
}