using System;
using System.Xml.Linq;

namespace Challenges {
    public class ChallengeObjectiveCVar : BaseChallengeObjective {
        // When myCVar is set to 20
        // <objective type="CVar, SCore" cvar="myCVar" count="20" description_key="onCraft" />


        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveCVar;

        private string _cvarName;
        public string LocalizationKey = "challengeObjectiveOnCVar";

        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                   return Localization.Get($"{LocalizationKey}", false) + " " + _cvarName + ":";
                return Localization.Get(_descriptionOverride);
            }
        }

        public override void HandleAddHooks() {
            EventOnCVarAdded.CVarAdded += CheckCVar;
        }

        private void CheckCVar(EntityAlive entityAlive, string cvarname, float cvarvalue) {
            // Quick check to make sure that we are the local player, rather than querying the the game manager.
            if (entityAlive is not EntityPlayerLocal player) return;
            if (string.IsNullOrEmpty(cvarname)) return;
            if (string.IsNullOrEmpty(_cvarName)) return;

            if (!string.Equals(cvarname, _cvarName, StringComparison.CurrentCultureIgnoreCase)) return;
            Current = (int)cvarvalue;
            CheckObjectiveComplete();
        }

        public override void HandleRemoveHooks() {
            EventOnCVarAdded.CVarAdded -= CheckCVar;
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("cvar"))
            {
                _cvarName = e.GetAttribute("cvar");
            }

            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveCVar {
                _cvarName = _cvarName
                ,
                _descriptionOverride = _descriptionOverride


            };
        }
    }
}