using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     *
     * <objective type="ClearSleepers, SCore" biome="pine_forest" count="200" />
     */
    public class ChallengeObjectiveClearSleepers: BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveClearSleepers;

        public string LocalizationKey = "clearSleeperChallenge";
        public override string DescriptionText => Localization.Get(LocalizationKey);
        public string biome;

        
        public override void HandleAddHooks()
        {
            EventOnSleeperVolumeClearedUpdate.OnSleeperVolumeClearedEvent += Current_SleepersCleared;
           // QuestEventManager.Current.SleepersCleared += this.Current_SleepersCleared; 
        }

        private void Current_SleeperVolumePositionRemove(Vector3 position)
        {
           // throw new NotImplementedException();
        }

        private void Current_SleeperVolumePositionAdd(Vector3 position)
        {
          //  throw new NotImplementedException();
        }

        private void Current_SleepersCleared(Vector3 pos)
        {
            // Check all the requirements
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;
            
            var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            if (primaryPlayer == null) return;
            if (primaryPlayer.prefab == null) return;
            
            var prefabInstance = GetPrefabAt(pos);
            if (prefabInstance == null) return;
            
            if (!primaryPlayer.prefab.Equals(prefabInstance)) return;
            var biomeName = GetBiomeOfPrefab(pos);
            if (!biomeName.EqualsCaseInsensitive(biomeName)) return;
            
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;

            Current++;
            if (Current < MaxCount) return;

            Current = MaxCount;
            CheckObjectiveComplete();
        }

        private PrefabInstance GetPrefabAt(Vector3 position)
        {
            return GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(position);
        }

        private string GetBiomeOfPrefab(Vector3 position)
        {
            var biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)position.x, (int)position.z);
            return biomeAt.m_sBiomeName.ToLower();
        }

        public override void HandleRemoveHooks() {
            EventOnSleeperVolumeClearedUpdate.OnSleeperVolumeClearedEvent -= Current_SleepersCleared;
            //QuestEventManager.Current.SleepersCleared -= this.Current_SleepersCleared;
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
            if (e.HasAttribute("biome"))
            {
                this.biome = e.GetAttribute("biome").ToLower();
            }
        }

        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectiveClearSleepers {
                biome = this.biome,
                LocalizationKey = LocalizationKey
            };
        }
    }
}