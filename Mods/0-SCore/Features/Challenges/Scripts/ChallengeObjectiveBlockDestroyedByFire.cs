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
     * <objective type="BlockDestroyedByFire, SCore" count="20" />
     */
    public class ChallengeObjectiveBlockDestroyedByFire : BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyedByFire;

        public string LocalizationKey = "onblockDestroyedByFire";
        public override string DescriptionText => Localization.Get(LocalizationKey);

        
        public override void HandleAddHooks()
        {
            if (FireManager.Instance == null || FireManager.Instance.Enabled == false) return;
            FireManager.Instance.Events.OnBlockDestroyedCount += Check_Block;
        }

        
        public override void HandleRemoveHooks() {
            if (FireManager.Instance == null || FireManager.Instance.Enabled == false) return;
            FireManager.Instance.Events.OnBlockDestroyedCount -= Check_Block;
        }

        private void Check_Block(int count) {
            Current += count;
            CheckObjectiveComplete();
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveBlockDestroyedByFire();
        }
    }
}