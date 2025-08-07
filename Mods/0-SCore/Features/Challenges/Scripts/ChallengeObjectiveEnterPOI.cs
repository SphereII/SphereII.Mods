
using System;
using System.Xml.Linq;
using HarmonyLib;
using Challenges;
using UnityEngine;

namespace Challenges {
    public class ChallengeObjectiveEnterPOI : BaseChallengeObjective {
        private string _prefabName;
        private FastTags<TagGroup.Poi> _poiTags;

        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveEnterPOI;

        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    return  Localization.Get("challengeObjectiveEnter") + " " + Localization.Get(_prefabName);
                return Localization.Get(_descriptionOverride);
            }
        }
        public override void Init() {
        }

        public override void HandleAddHooks() {
            EventOnEnterPoi.EnterPoi += Current_PrefabEnter;
        }

        public override void HandleRemoveHooks() {
            EventOnEnterPoi.EnterPoi -= Current_PrefabEnter;
        }

        private bool isValidPOI(PrefabInstance prefabInstance) {
            // if both are set, validate them both.
            if (!string.IsNullOrEmpty(_prefabName) && !_poiTags.IsEmpty)
            {
                var count = 0;
                if (prefabInstance.prefab.PrefabName.Equals(_prefabName, StringComparison.InvariantCultureIgnoreCase))
                    count++;
                if (prefabInstance.prefab.tags.Test_AnySet(_poiTags))
                    count++;
                return count == 2;
            }

            if (!string.IsNullOrEmpty(_prefabName))
            {
                if (prefabInstance.prefab.PrefabName.Equals(_prefabName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            if (!_poiTags.IsEmpty)
            {
                if (prefabInstance.prefab.tags.Test_AnySet(_poiTags))
                    return true;
            }

            return false;
        }

        private void Current_PrefabEnter(PrefabInstance prefabInstance)
        {
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name) ) return ;
            var isValid = isValidPOI(prefabInstance);
            if (!isValid) return;

            Current++;
            if (Current < MaxCount) return;
            Current = MaxCount;
            CheckObjectiveComplete();
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("prefab"))
            {
                _prefabName = e.GetAttribute("prefab");
            }

            if (e.HasAttribute("tags"))
            {
                _poiTags = FastTags<TagGroup.Poi>.Parse(e.GetAttribute("tags"));
            }
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveEnterPOI {
                _prefabName = this._prefabName,
                _poiTags =  this._poiTags
                ,
                _descriptionOverride = _descriptionOverride

            };
        }
    }
}