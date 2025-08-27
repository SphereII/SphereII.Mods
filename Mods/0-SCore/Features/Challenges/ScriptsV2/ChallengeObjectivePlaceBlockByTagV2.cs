using System.Xml.Linq;
using UnityEngine;

namespace Challenges
{
    /*
   * A new challenge objective to allow a player place a block to be checked by tag.
   *
   * <objective type="PlaceBlockByTagV2, SCore" count="2" />
   */
    public class ChallengeObjectivePlaceBlockByTagV2 : BaseChallengeObjective
    {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectivePlaceBlockByTagV2;
      
        public string LocalizationKey = "challengePlaceBlockByTag";
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                return Localization.Get($"{LocalizationKey}") ;
            }
        }
        public override void HandleAddHooks()
        {
            QuestEventManager.Current.BlockPlace += this.Current_BlockPlace;
        }

        public override void HandleRemoveHooks()
        {
            QuestEventManager.Current.BlockPlace -= this.Current_BlockPlace;
        }

        public void Current_BlockPlace(string blockName, Vector3i blockPos)
        {
            var blockValue = GameManager.Instance.World.GetBlock(blockPos);
            var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            primaryPlayer.MinEventContext.BlockValue = blockValue;
            
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;
            Current += 1;
            if (Current >= MaxCount)
            {
                Current = MaxCount;
                CheckObjectiveComplete();
            }
        }
        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectivePlaceBlockByTagV2 {
                _descriptionOverride = _descriptionOverride,
                LocalizationKey = LocalizationKey
            };
        }
        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("description_key"))
                LocalizationKey = e.GetAttribute("description_key");
        }

       
    }
}