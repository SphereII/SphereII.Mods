using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective that fires when you hire an npc
     *
     * <objective type="HireNPC, SCore" count="20" />
     */
    public class ChallengeObjectiveHireNPC : ChallengeObjectiveSCoreBase {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveHireNPC;

        public string LocalizationKey = "onHireNPC";
        public override string DescriptionText => Localization.Get(LocalizationKey);

        public override void HandleAddHooks() {
            EntityUtilities.OnHire += OnHireNPC;
        }

        public override void HandleRemoveHooks() {
            EntityUtilities.OnHire -= OnHireNPC;
        }

        private bool OnHireNPC(int hiredId, int ownerID) {
            var entityAlive = GameManager.Instance.World.GetEntity(hiredId) as EntityAlive;
            if (!CheckRequirements(entityAlive)) return false;
            Current++;
            CheckObjectiveComplete();
            return true;
        }

        // public override void ParseElement(XElement e) {
        //     base.ParseElement(e);
        //     if (e.HasAttribute("description_key"))
        //         LocalizationKey = e.GetAttribute("description_key");
        // }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveHireNPC {
                biome = this.biome,
                targetName = this.targetName,
                entityTag = this.entityTag,
                itemClass = itemClass,
                itemTag = itemTag
            };
        }
    }
}