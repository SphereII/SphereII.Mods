using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to monitor your stealth kills
     * To pass this challenge, you must do consecutive stealth kills until you've reached the desired count.
     *
     * <!-- Kill two entities in a row with a stealth kill -->
     * <objective type="StealthStreak, SCore" count="2"/>
     */
    public class ChallengeObjectiveStealthKillStreak : ChallengeObjectiveKillWithItem {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveStealthKillStreak;

        public new string LocalizationKey = "challengeObjectiveStealthKillStreak";

        // If we pass the pre-requisite, call the base class of the KillWithTags to do the heavy lifting for us.
        protected override bool Check_EntityKill(DamageResponse _dmresponse, EntityAlive entityDamaged) {
             if (_dmresponse.Source.BonusDamageType != EnumDamageBonusType.Sneak)
             {
                 ResetComplete();
                 return false;
             }
             return base.Check_EntityKill(_dmresponse, entityDamaged);
        }


        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveStealthKillStreak() {
                entityTag = entityTag,
                entityTags = entityTags,
                biome = biome,
                targetName = targetName,
                isTwitchSpawn = isTwitchSpawn,
                killerHasBuffTag = killerHasBuffTag,
                killedHasBuffTag = killedHasBuffTag,
                ItemClass = ItemClass,
                ItemTag = ItemTag,                
                StealthCheck = StealthCheck
            };
        }
    }
}