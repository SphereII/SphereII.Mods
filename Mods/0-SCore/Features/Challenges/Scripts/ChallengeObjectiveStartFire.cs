using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to enjoy players to be pyros
     *
     * <objective type="StartFire, SCore" count="20" />
     */
    public class ChallengeObjectiveStartFire : BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveStartFire;

        public string LocalizationKey = "onStartFire";
        public override string DescriptionText => Localization.Get(LocalizationKey);

        
        public override void HandleAddHooks() {
            FireManager.Instance.OnStartFire += Check_Block;
        }

        
        public override void HandleRemoveHooks() {
            FireManager.Instance.OnStartFire -= Check_Block;
        }

        private void Check_Block() {
            Current++;
            CheckObjectiveComplete();
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveStartFire();
        }
    }
}