using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using SCore.Features.PlayerMetrics;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to monitor your stealth kills
     * To pass this challenge, you must do consecutive stealth kills until you've reached the desired count.
     *
     * <!-- Kill two entities in a row with a stealth kill -->
     * <objective type="StealthKillStreak, SCore" count="2" cvar="longestStreakCVar" />
     */
    public class ChallengeObjectiveStealthKillStreak : ChallengeObjectiveKillWithItem {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveStealthKillStreak;

        public string cvarName;
        public new string LocalizationKey = "challengeObjectiveStealthKillStreak";
        private string _descriptionOverride;
        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    Localization.Get(LocalizationKey);
                return Localization.Get(_descriptionOverride);
            }
        }
        // If we pass the pre-requisite, call the base class of the KillWithTags to do the heavy lifting for us.
        protected override bool Check_EntityKill(DamageResponse dmgResponse, EntityAlive entityDamaged) {
            if (dmgResponse.Source.BonusDamageType != EnumDamageBonusType.Sneak)
            {
                SCoreMetrics.UpdateCVar(cvarName, Current);
                ResetComplete();
                return false;
            }

            return base.Check_EntityKill(dmgResponse, entityDamaged);
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("cvar"))
            {
                cvarName = e.GetAttribute("cvar");
            }
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveStealthKillStreak {
                cvarName = cvarName,
                _descriptionOverride = _descriptionOverride
            };
        }
    }
}