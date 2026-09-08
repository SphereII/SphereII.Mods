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

                var journal = _params?.Self?.challengeJournal;
                if (journal == null) return;

                // Challenge.Redeem() is client-side code. It builds an analytics event out of
                // Owner.Player - which is an EntityPlayerLocal - and reads Player.totalTimePlayed
                // with no guard of its own.
                //
                // ChallengeJournal.FireEvent also runs on the server: opening a loot container
                // reaches it through LockManager.LockRequestServer, TEFeatureStorage.PopulateTE
                // and LootManager.LootContainerOpened. A server has no local player for a remote
                // player's journal, so Player is null there and redeeming throws inside vanilla,
                // taking down the package handler with it.
                //
                // No local player means nothing to redeem for on this machine, so bail.
                if (journal.Player == null) return;

                foreach (var challenge in journal.Challenges)
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