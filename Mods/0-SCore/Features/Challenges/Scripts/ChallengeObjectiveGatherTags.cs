using System.Globalization;
using System.Xml.Linq;

namespace Challenges
{
    //            <objective type="GatherTags, SCore" item_tags="junk" count="10"/>

    public class ChallengeObjectiveGatherTags : BaseChallengeObjective
    {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveGatherTags;

        public string itemTags;

        private string _descriptionOverride;
        private string _lootList;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    return Localization.Get("challengeObjectiveGatherTags", false) + " " + itemTags + ":";
                return Localization.Get(_descriptionOverride);
            }
        }


        public override void HandleAddHooks()
        {
            EntityPlayerLocal player = this.Owner.Owner.Player;
            XUiM_PlayerInventory playerInventory =
                LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player).xui.PlayerInventory;
            playerInventory.Backpack.OnBackpackItemsChangedInternal -= CheckItems;
            playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= CheckItems;
            playerInventory.Backpack.OnBackpackItemsChangedInternal += CheckItems;
            playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += CheckItems;
            player.DragAndDropItemChanged -= CheckItems;
            player.DragAndDropItemChanged += CheckItems;
            base.HandleAddHooks();
            CheckObjectiveComplete();
        }


        public override bool CheckObjectiveComplete(bool handleComplete = true)
        {
            if (this.CheckForNeededItem())
            {
                base.Complete = true;
                base.Current = this.MaxCount;
                if (handleComplete)
                {
                    this.Owner.HandleComplete();
                }

                return true;
            }

            base.Complete = false;
            return base.CheckObjectiveComplete(handleComplete);
        }

        public override void HandleRemoveHooks()
        {
            EntityPlayerLocal player = this.Owner.Owner.Player;
            if (player == null)
            {
                return;
            }

            LocalPlayerUI.GetUIForPlayer(player);
            XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
            playerInventory.Backpack.OnBackpackItemsChangedInternal -= CheckItems;
            playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= CheckItems;
            player.DragAndDropItemChanged -= CheckItems;
        }

        private void CheckItems()
        {
            if (string.IsNullOrEmpty(_lootList))
            {
                CheckObjectiveComplete();
                return;
            }

            var player = Owner.Owner.Player;
            if (player == null) return;
            var xui = LocalPlayerUI.GetUIForPlayer(player).xui;
            if (xui == null) return;
            if (xui.lootContainer == null) return;
            foreach (var loot in _lootList.Split(','))
            {
                if (xui.lootContainer.lootListName == loot)
                {
                    CheckObjectiveComplete();
                    return;
                }
            }
        }


        private bool CheckForNeededItem()
        {
            var num = 0;
            if (string.IsNullOrEmpty(this.itemTags)) return false;
            
            var tags = FastTags<TagGroup.Global>.Parse(itemTags);
            var playerInventory = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player).xui.PlayerInventory;
            foreach (var item in playerInventory.Backpack.GetSlots())
            {
                if (item.IsEmpty()) continue;
                if (item.itemValue.ItemClass.HasAnyTags(tags))
                    num += item.count;
            }

            foreach (var item in playerInventory.Toolbelt.GetSlots())
            {
                if (item.IsEmpty()) continue;
                if (item.itemValue.ItemClass.HasAnyTags(tags))
                    num += item.count;
            }

            if (num > MaxCount)
            {
                num = MaxCount;
            }

            if (current != num)
            {
                Current = num;
            }

            return num == MaxCount;
        }


        public override void ParseElement(XElement e)
        {
            base.ParseElement(e);
            if (e.HasAttribute("item_tags"))
            {
                itemTags = e.GetAttribute("item_tags");
            }

            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");

            if (e.HasAttribute("loot_list"))
                _lootList = e.GetAttribute("loot_list");
        }

        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectiveGatherTags() {
                itemTags = itemTags,
                _descriptionOverride = _descriptionOverride,
                _lootList = _lootList
            };
        }
    }
}