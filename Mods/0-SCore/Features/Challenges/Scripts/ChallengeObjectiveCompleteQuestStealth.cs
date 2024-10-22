using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using SCore.Features.PlayerMetrics;
using UnityEngine;

namespace Challenges {
    
    /*
     * A new challenge objective to monitor your stealth kills during a quest.
     * To pass this challenge, you must do consecutive stealth kills until you've reached the desired count.
     * If the intention is that the full quest be done 100% stealth, set the count to be higher than the expected number of zombies
     * Once the Sleeper volumes are all cleared for the QuestObjectiveClear, then the challenge will complete, regardless if
     * the Count is equaled to the count specified. 
     *
     * <!-- Kill two entities in a row with a stealth kill, during a quest. -->
     * <objective type="CompleteQuestStealth, SCore" count="2"/>
     * 
     * <!-- Kill all entities in a row with a stealth kill, during a quest. -->
     * <objective type="CompleteQuestStealth, SCore" count="1000"/>

     */
    public class ChallengeObjectiveCompleteQuestStealth : BaseChallengeObjective {

        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveCompleteQuestStealth;

        public string LocalizationKey = "ChallengeObjectiveCompleteQuestStealth";
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    Localization.Get(LocalizationKey);
                return Localization.Get(_descriptionOverride);
            }
        }
        public override void Init() {
        }

        public override void HandleAddHooks() {
            EventOnRallyPointActivated.OnActivated += OnQuestActivated;
        }

        private void Reset() {
            Current = 0;
            Complete = false;
            HandleRemoveHooks();
        }

        public override void HandleRemoveHooks() {
            EventOnRallyPointActivated.OnActivated -= OnQuestActivated;
            QuestEventManager.Current.SleepersCleared -= Current_SleepersCleared;
            EventOnClientKill.OnClientKillEvent -= Current_EntityDamaged;
        }

        // Start listening to the other hooks only after we've activated a quest.
        private void OnQuestActivated() {
            Reset();
            EventOnClientKill.OnClientKillEvent += Current_EntityDamaged;
            QuestEventManager.Current.SleepersCleared += Current_SleepersCleared;
        }

        // The only way this event should still be listened to here, is if we didn't end the kill streak.
        private void Current_SleepersCleared(Vector3 prefabPos) {
            Current = MaxCount;
            CheckObjectiveComplete();
        }
        private bool Current_EntityDamaged(DamageResponse _dmresponse, EntityAlive entityDamaged) {
            // We didn't damage the zombie. Don't count it.
            var entityId = _dmresponse.Source.getEntityId();
            if (entityId == -1) return false;

            var player = GameManager.Instance.World.GetEntity(entityId) as EntityPlayerLocal;
            if (player == null) return false;

            // Is the zombies still without the bounds?
            var position = entityDamaged.position;
            position.y = position.z;
            if (!player.ZombieCompassBounds.Contains(position)) return false;

            // Were we sneaking?
            if (_dmresponse.Source.BonusDamageType != EnumDamageBonusType.Sneak)
            {
                Reset();
                return false;
            }
            
            // Check to see if we have a max counter to complete the challenge.
            Current++;
            CheckObjectiveComplete();
            return true;
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveCompleteQuestStealth {
                _descriptionOverride = _descriptionOverride
            };
        }

    }
}