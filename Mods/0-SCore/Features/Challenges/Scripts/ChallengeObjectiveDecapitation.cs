using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    /*
     * A new challenge objective to monitor your Decapitation
     *
     * <!-- Kill two entities by removing their heads. -->
     * <objective type="Decapitation, SCore" count="2" />
     */
    public class ChallengeObjectiveDecapitation : ChallengeObjectiveKillWithItem {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveDecapitation;

        public string LocalizationKey = "challengeDecapZombies";
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    Localization.Get(LocalizationKey);
                return Localization.Get(_descriptionOverride);
            }
        }
        protected override bool Check_EntityKill(DamageResponse dmgResponse, EntityAlive entityDamaged) {
            if (!dmgResponse.Dismember) return false;
            if (entityDamaged.emodel.avatarController is not AvatarZombieController controller) return false;
            if (!controller.headDismembered) return false;
            var result = base.Check_EntityKill(dmgResponse, entityDamaged);
            if (result)
            {
                Current++;
                CheckObjectiveComplete();
            }
            return result;
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }
        
        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveDecapitation {
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
                LocalizationKey = LocalizationKey
                ,
                _descriptionOverride = _descriptionOverride
            };
        }

    }
}