using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to destroy certain blocks by name, material, within boimes, and specific POIs.
     *
     * <objective type="BlockDestroyed, SCore" count="20" block="myblock" biome="burn_forest" poi="traderJen" />
     * <objective type="BlockDestroyed, SCore" count="20" material="myMaterial" biome="pine_forest" poi_tags="wilderness" />
     */
    public class ChallengeObjectiveBlockDestroyed : BaseChallengeObjective {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyed;

        public string LocalizationKey = "onblockDestroyed";

        public string block;
        public string material;
        public string biome;
        public string poi;
        public string poi_tags;
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    Localization.Get(LocalizationKey);
                return Localization.Get(_descriptionOverride);
            }
        }

        public override void HandleAddHooks() {
            QuestEventManager.Current.BlockDestroy += Check_Block;
            QuestEventManager.Current.BlockChange += Check_Block;
        }
        
        
        public override void HandleRemoveHooks() {
            QuestEventManager.Current.BlockDestroy -= Check_Block;
            QuestEventManager.Current.BlockChange -= Check_Block;

        }

        private void Check_Block(Block blockold, Block blocknew, Vector3i blockpos) {
            Check_Block(blockold,  blockpos);
        }
        private void Check_Block(Block block1, Vector3i blockpos) {
            if (!HasPrerequisite(block1)) return;
            if (!HasLocationRequirement(blockpos)) return;
            Current++;
            CheckObjectiveComplete();
        }


        private bool HasPrerequisite(Block block1) {
            if (!string.IsNullOrEmpty(block))
            {
                if (string.Equals(block, block1.GetBlockName())) return true;
            }

            if (!string.IsNullOrEmpty(material))
            {
                if (string.Equals(material, block1.blockMaterial.id)) return true;
            }

            return false;
        }

        
        private bool HasLocationRequirement(Vector3i blockpos) {
            if (!string.IsNullOrEmpty(biome))
            {
                var biomeDefinition = GameManager.Instance.World.GetBiome(blockpos.x, blockpos.z);
                if (biomeDefinition.m_sBiomeName != biome) return false;
            }
            var prefabInstance = GameManager.Instance.World.GetPOIAtPosition(new Vector3(blockpos.x, blockpos.y, blockpos.z));
            if (prefabInstance == null) return false;
            if (!string.IsNullOrEmpty(poi))
            {
                if (!prefabInstance.prefab.PrefabName.Equals(poi, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }
            if (!string.IsNullOrEmpty(poi_tags))
            {
                var fasttags = FastTags<TagGroup.Poi>.Parse(poi_tags);
                if (prefabInstance.prefab.tags.Test_AnySet(fasttags)) return false;
            }

            
            return true;
        }

    


        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_key"))
                LocalizationKey =e.GetAttribute("description_key");
            
            if (e.HasAttribute("block"))
            {
                block = e.GetAttribute("block");
            }

            if (e.HasAttribute("material"))
            {
                material = e.GetAttribute("material");
            }

            if (e.HasAttribute("biome"))
            {
                biome = e.GetAttribute("biome");
            }

            if (e.HasAttribute("poiname"))
            {
                poi = e.GetAttribute("poiname");
            }

            if (e.HasAttribute("poi_tag"))
            {
                poi_tags = e.GetAttribute("poi_tag");
            }
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");


          
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveBlockDestroyed() {
                block = block,
                material = material,
                biome = biome,
                poi = poi, 
                poi_tags = poi_tags,
                LocalizationKey =LocalizationKey,
                _descriptionOverride = _descriptionOverride

                
            };
        }
    }
}