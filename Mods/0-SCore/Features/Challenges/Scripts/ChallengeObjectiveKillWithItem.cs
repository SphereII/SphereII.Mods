using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;
using UnityEngine.Animations;

namespace Challenges {
    /*
     *
     * To pass this challenge, you must killed zombies with the specified item. This extends the KillByTag objective, and thus supports
     * all those attributes as well. Multiple item="" can be listed as a comma-delimited list.
     *
     * <!-- Kill two zombies in a row with a gunHandgunT1Pistol -->
     * <objective type="KillWithItem, SCore" count="2" item="gunHandgunT1Pistol" />
     *
     * Rather than item name itself, you could also use item_tag
     * <objective type="KillWithItem, SCore" count="2" item_tags="handgunSkill"  />
     *
     * ItemName is checked first, then item tags. If either passes, then vanilla code is checked for the other tags and checks.
     *
     * You may also add the option entity tags and target_name_key for localization.
     * By default, the entity_tags and target_name_key is zombie and xuiZombies, respectively.
     * <objective type="KillWithItem, SCore" count="2" item="gunHandgunT1Pistol" entity_tags="zombie" target_name_key="xuiZombies" />
     *
     * Other attributes available are:
     *      target_name="zombieMarlene"
     *      biome="snow"
     *      killer_has_bufftag="buff_tags"
     *      killed_has_bufftag="buff_tags"
     *      is_twitch_spawn="true/false"
     */
    public class ChallengeObjectiveKillWithItem : ChallengeObjectiveKillByTag {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveKillWithItem;
    
        public string ItemClass;
        public string ItemTag;
        public bool StealthCheck = false;
        public string LocalizationKey = "";
        public bool GenerateDescription;
        private string _descriptionOverride;
        public string biome;

        public override void Init() {
            if ( string.IsNullOrEmpty(entityTag))
                entityTag = "zombie";
            if ( string.IsNullOrEmpty(targetName))
                targetName = Localization.Get("xuiZombies");

            base.Init();
        }

        public override string DescriptionText {
            get {
                if (!string.IsNullOrEmpty(_descriptionOverride))
                    return Localization.Get(_descriptionOverride);
                
                var objectiveDesc = Localization.Get("challengeKillWithItemDesc");
                objectiveDesc += $" {targetName}";
                var with = Localization.Get("challengeObjectiveWith");

                if (!string.IsNullOrEmpty(ItemClass))
                {
                    // Use a counter to know if there needs to be ,'s
                    var itemDisplay = $" {with} ";
                    var counter = 0;
                    foreach (var item in ItemClass.Split(','))
                    {
                        if (counter>0)
                            itemDisplay += ", ";
                        itemDisplay += $"{Localization.Get(item)}";
                        counter++;
                    }

                    return $"{objectiveDesc} {itemDisplay} :";
                }

                if (string.IsNullOrEmpty(ItemTag)) return $"{objectiveDesc} :";
                var itemWithTags = Localization.Get("itemWithTags");
                return $"{objectiveDesc} {with} {ItemTag} {itemWithTags}:";

            }
        }

        public override void HandleAddHooks() {
            EventOnClientKill.OnClientKillEvent += KillEntity;
        }

        public override void HandleRemoveHooks() {
            EventOnClientKill.OnClientKillEvent -= KillEntity;
        }

        private bool KillEntity(DamageResponse _dmresponse, EntityAlive entitydamaged)
        {
            var result = Check_EntityKill(_dmresponse, entitydamaged);
            if (result)
            {
                Current++;
                CheckObjectiveComplete();
            }
            return result;
        }

    
        public virtual bool HasPrerequisiteCondition(DamageResponse dmgResponse) {
            if (!dmgResponse.Fatal) return false;
            if (StealthCheck)
            {
                if (dmgResponse.Source.BonusDamageType != EnumDamageBonusType.Sneak) return false;
                
                // Sneaking is true, but there's no other checks, so let's just end it.
                if (string.IsNullOrEmpty(ItemClass) && string.IsNullOrEmpty(ItemTag)) return true;
            }

            if (SCoreChallengeUtils.IsKilledByTrap(dmgResponse, ItemClass)) return true;
            if (SCoreChallengeUtils.IsHoldingItemName(ItemClass)) return true;
            if (SCoreChallengeUtils.IsHoldingItemHasTag(ItemTag)) return true;
            return false;
        }


        // If we pass the pre-requisite, call the base class of the KillWithTags to do the heavy lifting for us.
        protected virtual bool Check_EntityKill(DamageResponse dmgResponse, EntityAlive killedEntity)
        {
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name) ) return false;
            
            if (!HasPrerequisiteCondition(dmgResponse)) return false;
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            return CheckAdditionalCondition(player, killedEntity);
        }

        private bool CheckAdditionalCondition(EntityAlive killedBy, EntityAlive killedEntity)
        {
            if (!string.IsNullOrEmpty(entityTag) && !this.entityTags.Test_AnySet(killedEntity.EntityClass.Tags))
            {
                return false;
                
            }
            if (!string.IsNullOrEmpty(biome) && this.Owner.Owner.Player.biomeStandingOn?.m_sBiomeName != this.biome)
            //if (this.biome != "" && this.Owner.Owner.Player.biomeStandingOn?.m_sBiomeName != this.biome)
            {
                return false;
            }
            if (this.isTwitchSpawn > -1)
            {
                if (this.isTwitchSpawn == 0 && killedEntity.spawnById != -1)
                {
                    return false;
                }
                if (this.isTwitchSpawn == 1 && killedEntity.spawnById == -1)
                {
                    return false;
                }
            }
            if (!this.killerHasBuffTag.IsEmpty && !killedBy.Buffs.HasBuffByTag(this.killerHasBuffTag))
            {
                return false;
            }
            if (!this.killedHasBuffTag.IsEmpty && !killedEntity.Buffs.HasBuffByTag(this.killedHasBuffTag))
            {
                return false;
            }

            return true;
        }


        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("item"))
            {
                ItemClass = e.GetAttribute("item");
            }

            if (e.HasAttribute("item_tag"))
            {
                ItemTag = e.GetAttribute("item_tag");
            }

            if (e.HasAttribute("stealth"))
            {
                var temp = e.GetAttribute("stealth");
                StringParsers.TryParseBool(temp, out StealthCheck);
            }
                
            if (e.HasAttribute("description_key"))
                 LocalizationKey =e.GetAttribute("description_key");

            if (e.HasAttribute("generate_description"))
            {
                var temp = e.GetAttribute("generate_description");
                StringParsers.TryParseBool(temp, out GenerateDescription);

            }

            if (e.HasAttribute("biome"))
            {
                biome = e.GetAttribute("biome");
            }
            if (e.HasAttribute("target_name_key"))
                targetName = Localization.Get(e.GetAttribute("target_name_key"));
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }


        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveKillWithItem {
                entityTag = entityTag,
                entityTags = entityTags,
                biome = biome,
                targetName = targetName,
                isTwitchSpawn = isTwitchSpawn,
                killerHasBuffTag = killerHasBuffTag,
                killedHasBuffTag = killedHasBuffTag,
                ItemClass = ItemClass,
                ItemTag = ItemTag,
                StealthCheck = StealthCheck,
                LocalizationKey = LocalizationKey,
                _descriptionOverride = _descriptionOverride
            };
        }
    }
}