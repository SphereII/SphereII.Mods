using System;
using System.Xml.Linq;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A challenge objective to encourage players to extinguish fires.
     *
     * <objective type="ExtinguishFire, SCore" count="20" />
     */
    public class ChallengeObjectiveExtinguishFire : BaseChallengeObjective {
        // Default localization key for the challenge description
        private const string DefaultLocalizationKey = "onExtinguish";

        // The localization key for this objective, can be customized via XML
        public string LocalizationKey { get; private set; } = DefaultLocalizationKey;

        // The objective type specific to extinguishing fires
        public override ChallengeObjectiveType ObjectiveType => 
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveExtinguishFire;

        // Retrieves the localized description of the objective
        public override string DescriptionText => Localization.Get(LocalizationKey);

        // Adds the event listener for when a fire is extinguished
        public override void HandleAddHooks() {
            if (FireManager.Instance == null || FireManager.Instance.Enabled == false) return;
                FireManager.Instance.Events.OnFireExtinguished += OnFireExtinguished;
            
        }

        // Removes the event listener when no longer needed
        public override void HandleRemoveHooks() {
            if (FireManager.Instance == null || FireManager.Instance.Enabled == false) return;
                FireManager.Instance.Events.OnFireExtinguished -= OnFireExtinguished;
        }

        // Event handler for when a fire is extinguished
        private void OnFireExtinguished(Vector3i position, int entityId) {
            if (IsExtinguishedByPlayer(entityId)) {
                UpdateProgress(1);
            }
        }

        // Checks if the fire was extinguished by the local player
        private bool IsExtinguishedByPlayer(int entityId) {
            // Early exit if entityId is invalid (-1)
            if (entityId == -1) return false;

            // Retrieve the local player and check if they match the entity who extinguished the fire
            var localPlayer = Owner.Owner.Player;
            return localPlayer.entityId == entityId;
        }

        // Updates the objective progress and checks for completion
        private void UpdateProgress(int extinguishCount) {
            Current += extinguishCount;
            CheckObjectiveComplete();
        }

        // Parses the XML element to configure the objective, particularly the description key
        public override void ParseElement(XElement e) {
            base.ParseElement(e);

            // If a custom description key is provided in the XML, update it
            if (e.HasAttribute("description_key")) {
                LocalizationKey = e.GetAttribute("description_key");
            }
        }

        // Creates a clone of the objective for independent instances
        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveExtinguishFire {
                LocalizationKey = this.LocalizationKey
            };
        }
    }
}
