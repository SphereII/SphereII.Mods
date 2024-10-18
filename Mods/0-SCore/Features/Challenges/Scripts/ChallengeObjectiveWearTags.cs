using System.Globalization;
using System.Xml.Linq;

namespace Challenges {
    //             <objective type="WearTags,SCore" item_tags="armorHead"/>
    //             <objective type="WearTags,SCore" item_mod="modGunBarrelExtender"/>

    public class ChallengeObjectiveWearTags : ChallengeObjectiveWear {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveWearTags;

        private string _itemTags;
        private string _itemMod;

        public override string DescriptionText =>
            Localization.Get("challengeObjectiveWearTags", false) + " " + _itemTags + ":";

        public override void HandleAddHooks() {
            QuestEventManager.Current.WearItem -= Current_WearItem;
            CheckItems();
            QuestEventManager.Current.WearItem += Current_WearItem;
        }

        public override void HandleRemoveHooks() {
            QuestEventManager.Current.WearItem -= Current_WearItem;
        }

        private void CheckItems() {
            var xui = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player).xui;
            var fastTags = FastTags<TagGroup.Global>.Parse(_itemTags);
            foreach (var item in xui.PlayerEquipment.Equipment.GetItems())
            {
                if (item == null) continue;
                if (item.IsEmpty()) continue;
                if (item.ItemClass.HasAnyTags(fastTags))
                    Current++;
                CheckItemMods(item);
            }

            if (Current > 0)
                CheckObjectiveComplete();
        }

        private new void Current_WearItem(ItemValue itemValue) {
            if (!string.IsNullOrEmpty(_itemTags))
            {
                var fastTags = FastTags<TagGroup.Global>.Parse(_itemTags);
                if (itemValue.ItemClass.HasAnyTags(fastTags))
                    Current++;
            }

            if (!string.IsNullOrEmpty(_itemMod))
            {
                CheckItemMods(itemValue);
            }
            CheckObjectiveComplete();
        }

        private void CheckItemMods(ItemValue itemValue) {
            foreach (var item in itemValue.Modifications)
            {
                if (item == null) continue;
                if ( item.IsEmpty()) continue;
                if ( !_itemMod.Contains(item.ItemClass.GetItemName())) continue;
                Current++;
            }

            foreach (var item in itemValue.CosmeticMods)
            {
                if (item == null) continue;
                if ( item.IsEmpty()) continue;
                if ( !_itemMod.Contains(item.ItemClass.GetItemName())) continue;
                Current++;
            }
        }

        public override void ParseElement(XElement e) {
            base.ParseElement(e);
            if (e.HasAttribute("item_tags"))
            {
                _itemTags = e.GetAttribute("item_tags");
            }

            if (e.HasAttribute("item_mod"))
            {
                _itemMod = e.GetAttribute("item_mod");
            }
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveWearTags() {
                itemClassID = itemClassID,
                expectedItem = expectedItem,
                expectedItemClass = expectedItemClass,
                _itemTags = _itemTags,
                _itemMod = _itemMod
            };
        }
    }
}