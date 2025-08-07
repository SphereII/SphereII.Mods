using System.Xml.Linq;

namespace Challenges
{
    /*
   * A new challenge objective to allow a player place a block to be checked by tag.
   *
   * <objective type="PlaceBlockByTag, SCore" count="2" block_tags="myTag"/>
   */
    public class ChallengeObjectivePlaceBlockByTag : BaseChallengeObjective
    {
        private string _blockTag = "";

        public override ChallengeObjectiveType ObjectiveType {
            get { return ChallengeObjectiveType.BlockPlace; }
        }

        public override string DescriptionText {
            get {
                return Localization.Get("xuiWorldPrefabsPlace", false) + " " + Localization.Get(this._blockTag, false) +
                       ":";
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
            if (string.IsNullOrEmpty(_blockTag)) return;
            var blockByName = Block.GetBlockByName(blockName);
            var tags = FastTags<TagGroup.Global>.Parse(_blockTag);
            if (!blockByName.HasAnyFastTags(tags)) return;
            Current++;
            CheckObjectiveComplete();
        }

        public override void ParseElement(XElement e)
        {
            base.ParseElement(e);
            if (e.HasAttribute("block_tags"))
            {
                _blockTag = e.GetAttribute("block_tags");
            }
        }

        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectivePlaceBlockByTag {
                _blockTag = _blockTag
            };
        }
    }
}