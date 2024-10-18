
using System.Xml;
using Challenges;
using UnityEngine;

public class MinEventActionAutoRedeemChallenges : MinEventActionTargetedBase
{
    //        <triggered_effect trigger = "onSelfBuffUpdate" action="AutoRedeemChallenges, SCore"  />

    public override void Execute(MinEventParams _params) {
        
        foreach (var challenge in _params.Self.challengeJournal.Challenges)
        {
            if (!challenge.ReadyToComplete) continue;
            challenge.ChallengeState = Challenge.ChallengeStates.Redeemed;
            challenge.Redeem();
            QuestEventManager.Current.ChallengeCompleted(challenge.ChallengeClass, true);
        }
    }
}