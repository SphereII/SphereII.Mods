using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

namespace Challenges {
    public class ChallengeObjectiveSCoreBase : BaseChallengeObjective ,IChallengeObjective{
        protected bool isDebug = false;
        public string biome;
        public string entityTag;
        public string targetName;
        public string itemClass;
        public string itemTag;
        protected string item_material;
        public string blockTag;
        public string blockName;
        protected string neededResourceID;
        protected int neededResourceCount = 0;
        public string descriptionOveride;
        public string LocalizationKey;
        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(descriptionOveride))
                {
                    var display = Localization.Get(LocalizationKey);
                    display += $"{SCoreChallengeUtils.GenerateString(entityTag)}";
                    display += $"{SCoreChallengeUtils.GenerateString(targetName)}";
                    display += $"{SCoreChallengeUtils.GenerateString(item_material)}";
                    display += $"{SCoreChallengeUtils.GenerateString(itemClass)}";
                    display += $"{SCoreChallengeUtils.GenerateString(blockName)}";
                    display += $"{SCoreChallengeUtils.GenerateString(blockTag)}";
                    display += $"{SCoreChallengeUtils.GenerateString(biome)}";
                    return display;
                }
                return Localization.Get(descriptionOveride);
            }
        }
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

        public bool CheckBlockName(string expectedBlock) {
            // We aren't looking for any blocks here.
            if (string.IsNullOrEmpty(expectedBlock)) return true;
            if (string.IsNullOrEmpty(blockName)) return true;
            
            if (blockName.EqualsCaseInsensitive(expectedBlock)) return true;

            if (expectedBlock.Contains(":") &&
                blockName.EqualsCaseInsensitive(expectedBlock.Substring(0, expectedBlock.IndexOf(':'))))
            {
                return true;
            }
                
            var blockByName = Block.GetBlockByName(blockName, true);
            if (blockByName == null) return false;
            return blockByName.SelectAlternates && blockByName.ContainsAlternateBlock(expectedBlock);
        }

        public bool CheckBlockTags(BlockValue block) {
            if (string.IsNullOrEmpty(blockTag)) return true;
            var blockTags = FastTags<TagGroup.Global>.Parse(blockTag);
            return block.Block.HasAnyFastTags(blockTags);
        }
        public bool CheckBlockTags(string _blockName) {
            if (string.IsNullOrEmpty(blockTag)) return true;
            var blockTags = FastTags<TagGroup.Global>.Parse(blockTag);
            var blockByName = Block.GetBlockByName(_blockName, true);
            return blockByName.HasAnyFastTags(blockTags);
        }

        public bool CheckHoldingItems() {
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

        public override void HandleOnCreated() {
            base.HandleOnCreated();
            CreateRequirements();
        }

        private void CreateRequirements() {
            if (!ShowRequirements)
            {
                return;
            }

            if (string.IsNullOrEmpty(itemClass)) return;
            if (string.IsNullOrEmpty(neededResourceID)) return;
            if (neededResourceCount == 0) return;
            Owner.SetRequirementGroup(new RequirementObjectiveGroupBlockUpgrade(this.itemClass, this.neededResourceID,
                this.neededResourceCount));
        }

        public bool CheckRequirements(EntityAlive entityAlive) {
            DisplayLog("Check Requirements");
            if (!CheckHoldingItems()) return false;
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
            
            if (e.HasAttribute("block_tag"))
            {
                blockTag = e.GetAttribute("block_tag");
                DisplayLog($"Block Tags: {blockTag}");
            }

            if (e.HasAttribute("block"))
            {
                blockName = e.GetAttribute("block");
                DisplayLog($"Block  {blockName}");
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

            if (e.HasAttribute("item_tag"))
            {
                itemTag = e.GetAttribute("item_tag");
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

            if (e.HasAttribute("needed_resource"))
            {
                neededResourceID = e.GetAttribute("needed_resource");
            }

            if (e.HasAttribute("needed_resource_count"))
            {
                neededResourceCount = StringParsers.ParseSInt32(e.GetAttribute("needed_resource_count"));
            }

            // Helper to make it easier to deal with held vs item=
            if (e.HasAttribute("held"))
            {
                itemClass = e.GetAttribute("held");
            }
            if (e.HasAttribute("description_override"))
                descriptionOveride = e.GetAttribute("description_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveSCoreBase {
                biome = this.biome,
                targetName = this.targetName,
                entityTag = this.entityTag,
                itemClass = itemClass,
                itemTag = itemTag,
                isDebug = this.isDebug,
                item_material = this.item_material,
                blockName = this.blockName,
                blockTag = this.blockTag,

                neededResourceID = this.neededResourceID,
                neededResourceCount = this.neededResourceCount,
                descriptionOveride = this.descriptionOveride
            };
        }
    }
}