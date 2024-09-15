using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to encourage players to be pyros
     *
     * <objective type="ExtinguishFire, SCore" count="20" />
     */
    public class ChallengeObjectiveExtinguishFire : BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveExtinguishFire;

        public string LocalizationKey = "onExtinguish";
        public override string DescriptionText => Localization.Get(LocalizationKey);

        
        public override void HandleAddHooks() {
            FireManager.Instance.OnExtinguish += Check_Block;
        }
        
        
        public override void HandleRemoveHooks() {
            FireManager.Instance.OnExtinguish -= Check_Block;
        }

        private void Check_Block(int count) {
            Current = count;
            CheckObjectiveComplete();
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveExtinguishFire();
        }
    }
}