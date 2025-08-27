using System;
using System.Xml.Linq;

namespace Challenges {
    public class ChallengeObjectiveCVarV2 : BaseChallengeObjective {
        // When myCVar is set to 20
        
       // <objective type="CVarV2, SCore" cvar="player_m_desert" count="5000" cvar_override="xuiCVar" description_key="xuiTravel"/>

        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveCVarV2;

        private string _cvarName;
        public string LocalizationKey = "challengeObjectiveOnCVar";
        public string cvar_override = "";
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                   return Localization.Get($"{LocalizationKey}") + " " + Localization.Get(cvar_override);
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
            // Check all the requirements
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;
            
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
            if (e.HasAttribute("cvar_override"))
                cvar_override = e.GetAttribute("cvar_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveCVarV2 {
                _cvarName = _cvarName,
                _descriptionOverride = _descriptionOverride,
                cvar_override = cvar_override,
                LocalizationKey =  LocalizationKey

            };
        }
    }
}