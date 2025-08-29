using System;
using System.Xml.Linq;
using Challenges;
using UnityEngine;

namespace Challenges
{
    /*
     * A new challenge objective to encourage players to start fires.
     *
     * <objective type="StartFire, SCore" count="20" />
     */
    public class ChallengeObjectiveStartFire : BaseChallengeObjective
    {
        // Default localization key for description
        private const string DefaultLocalizationKey = "onStartFire";

        // The localization key to retrieve the objective description
        public string LocalizationKey { get; private set; } = DefaultLocalizationKey;

        // Specify the type of challenge this is
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveStartFire;

        // Get the localized description of the challenge
        public override string DescriptionText => Localization.Get(LocalizationKey);

        // Subscribe to the fire-starting event
        public override void HandleAddHooks()
        {
            if (FireManager.Instance == null || FireManager.Instance.Enabled == false) return;
            FireManager.Instance.Events.OnFireStarted += OnFireStarted;
        }

        // Unsubscribe from the fire-starting event
        public override void HandleRemoveHooks()
        {
            if (FireManager.Instance == null || FireManager.Instance.Enabled == false) return;
            FireManager.Instance.Events.OnFireStarted -= OnFireStarted;
        }

        // Event handler for when a fire is started
        private void OnFireStarted(Vector3i position, int entityId)
        {
            if (IsFireStartedByPlayer(entityId))
            {
                IncrementObjectiveProgress();
            }
        }

        // Check if the fire was started by the local player
        private bool IsFireStartedByPlayer(int entityId)
        {
            // Early exit if entityId is invalid (-1)
            if (entityId == -1) return false;


            // Retrieve the local player
            var localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            return localPlayer.entityId == entityId;
        }

        // Increment the progress of the objective and check for completion
        private void IncrementObjectiveProgress()
        {
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;
            Current++;
            CheckObjectiveComplete();
        }

        // Parse the XML element to configure the objective
        public override void ParseElement(XElement e)
        {
            base.ParseElement(e);

            // Update the localization key if provided in the XML
            if (e.HasAttribute("description_key"))
            {
                LocalizationKey = e.GetAttribute("description_key");
            }
        }

        // Clone this objective for independent instances
        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectiveStartFire {
                LocalizationKey = this.LocalizationKey
            };
        }
    }
}