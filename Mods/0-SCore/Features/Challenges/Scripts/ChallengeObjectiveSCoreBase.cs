using System;
using System.Xml.Linq;
using UnityEngine;

namespace Challenges {
    public class ChallengeObjectiveSCoreBase : BaseChallengeObjective {
        private bool isDebug = false;
        public string biome;
        public string entityTag;
        public string targetName;
        public string itemClass;
        public string itemTag;
        private string item_material;
        public string blockTag;

        public void DisplayLog(string message) {
            if (!isDebug) return;
            Debug.Log(message);
        }

        public bool CheckTags(EntityAlive entityAlive) {
            if (string.IsNullOrEmpty(entityTag)) return true;

            var entityTags = FastTags<TagGroup.Global>.Parse(entityTag);
            var result = entityTags.Test_AnySet(entityAlive.EntityClass.Tags);
            DisplayLog($"Check Tags Result: {result} : {entityTags}");
            return result;
        }

        public bool CheckBlocks(BlockValue block) {
            if (string.IsNullOrEmpty(blockTag)) return true;
            var blockTags = FastTags<TagGroup.Global>.Parse(blockTag);
            return block.Block.HasAnyFastTags(blockTags);
        }

        public bool CheckItems() {
            // No items defined, so skipping.
            if (string.IsNullOrEmpty(itemClass) && string.IsNullOrEmpty(itemTag))
            {
                DisplayLog("No ItemName or tag to validate.");
                return true;
            }

            if (SCoreChallengeUtils.IsHoldingItemName(itemClass)) return true;
            if (SCoreChallengeUtils.IsHoldingItemHasTag(itemTag)) return true;
            if (SCoreChallengeUtils.IsHoldingItemHasTag(item_material)) return true;
            return false;
        }

        public bool CheckBiome() {
            if (string.IsNullOrEmpty(biome))
            {
                DisplayLog("No Biome to validate");
                return true;
            }

            return Owner.Owner.Player.biomeStandingOn.m_sBiomeName == this.biome;
        }

        public bool CheckNames(EntityAlive entityAlive) {
            var entityClassName = entityAlive.EntityClass.entityClassName;
            var localizedName = Localization.Get(entityClassName);
            if (string.IsNullOrEmpty(targetName)) return true;
            foreach (var name in targetName.Split(','))
            {
                if (entityClassName.EqualsCaseInsensitive(name)) return true;
                if (localizedName.EqualsCaseInsensitive(name)) return true;
            }

            return false;
        }

        public bool CheckRequirements(EntityAlive entityAlive) {
            DisplayLog("Check Requirements");
            if (!CheckItems()) return false;
            if (!CheckBiome()) return false;
            if (!CheckTags(entityAlive)) return false;
            if (!CheckNames(entityAlive)) return false;

            DisplayLog("End Check Requirements");
            return true;
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);

            if (e.HasAttribute("entity_tags"))
            {
                entityTag = e.GetAttribute("entity_tags");
                DisplayLog($"Entity Tags: {entityTag}");
            }

            if (e.HasAttribute("block_tags"))
            {
                blockTag = e.GetAttribute("block_tags");
                DisplayLog($"Block Tags: {blockTag}");
            }


            if (e.HasAttribute("target_name_key"))
            {
                targetName = Localization.Get(e.GetAttribute("target_name_key"), false);
                DisplayLog($"Target Name Key: {targetName}");
            }
            else if (e.HasAttribute("target_name"))
            {
                targetName = e.GetAttribute("target_name");
                DisplayLog($"Target name: {targetName}");
            }

            if (e.HasAttribute("biome"))
            {
                biome = e.GetAttribute("biome");
                DisplayLog($"Biome Restriction: {biome}");
            }

            if (e.HasAttribute("item"))
            {
                itemClass = e.GetAttribute("item");
                DisplayLog($"Item : {itemClass}");
            }

            if (e.HasAttribute("item_tags"))
            {
                itemTag = e.GetAttribute("item_tags");
                DisplayLog($"Item Tag : {itemTag}");
            }

            if (e.HasAttribute("item_material"))
            {
                item_material = e.GetAttribute("item_material");
                DisplayLog($"Material : {item_material}");
            }

            if (e.HasAttribute("debug"))
            {
                var debugBool = e.GetAttribute("debug");
                isDebug = StringParsers.ParseBool(debugBool);
                DisplayLog($"Debug is: {isDebug}");
            }
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveSCoreBase {
                biome = this.biome,
                targetName = this.targetName,
                entityTag = this.entityTag,
                itemClass = itemClass,
                itemTag = itemTag,
                isDebug = this.isDebug,
                item_material = this.item_material
            };
        }
    }
}