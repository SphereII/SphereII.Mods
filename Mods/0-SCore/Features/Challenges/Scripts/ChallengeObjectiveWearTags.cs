using System.Globalization;
using System.Xml.Linq;

namespace Challenges {
    //             <objective type="WearTags,SCore" item_tags="armorHead"/>
    //             <objective type="WearTags,SCore" item_mod="modGunBarrelExtender"/>
    //             <objective type="WearTags,SCore" installable_tags="turretRanged"/>
    //             <objective type="WearTags,SCore" modifier_tags="barrelAttachment"/>

    public class ChallengeObjectiveWearTags : ChallengeObjectiveWear {
        public override ChallengeObjectiveType ObjectiveType =>
            (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveWearTags;

        private string _itemTags;
        private string _itemMod;
        private string _installableTags;
        private string _modifierTags;
        private string _descriptionOverride;

        public override string DescriptionText {
            get {
                if (string.IsNullOrEmpty(_descriptionOverride))
                    return Localization.Get("challengeObjectiveWearTags", false) +
                           $" {_itemTags} {_installableTags} {_modifierTags} tags:";
                return Localization.Get(_descriptionOverride);
            }
        }


        public override void HandleAddHooks() {
            QuestEventManager.Current.WearItem -= CheckItemMods;
            CheckItems();
            QuestEventManager.Current.WearItem += CheckItemMods;
        }

        public override void HandleRemoveHooks() {
            QuestEventManager.Current.WearItem -= CheckItemMods;
        }

        private void CheckItems() {
            var xui = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player).xui;
            foreach (var item in xui.PlayerEquipment.Equipment.GetItems())
            {
                CheckItemMods(item);
            }

            CheckObjectiveComplete();
        }

        private void CheckItemModsTags(ItemValue itemValue) {
            if (itemValue == null) return;
            if (itemValue.IsEmpty()) return;
            FindItemTags(itemValue, _installableTags);
            FindItemTags(itemValue, _modifierTags);
        }

        private void FindItemTags(ItemValue itemValue, string tag) {
            if (itemValue.ItemClass is not ItemClassModifier itemValueModifier) return;
            if (string.IsNullOrEmpty(tag)) return;
            var tags = FastTags<TagGroup.Global>.Parse(tag);
            if (itemValueModifier.ItemTags.Test_AnySet(tags))
            {
                Current++;
            }

            if (itemValueModifier.InstallableTags.Test_AnySet(tags))
            {
                Current++;
            }

            if (string.IsNullOrEmpty(_itemMod)) return;
            if (_itemMod.Contains(itemValueModifier.GetItemName()))
            {
                Current++;
            }
        }

        private void CheckItemMods(ItemValue itemValue) {
            if (itemValue == null) return;
            if (itemValue.IsEmpty()) return;
            if (!string.IsNullOrEmpty(_itemTags))
            {
                var fastTags = FastTags<TagGroup.Global>.Parse(_itemTags);
                if (itemValue.ItemClass.HasAnyTags(fastTags))
                    Current++;
            }

            foreach (var item in itemValue.Modifications)
            {
                CheckItemModsTags(item);
            }

            foreach (var item in itemValue.CosmeticMods)
            {
                CheckItemModsTags(item);
            }

            CheckObjectiveComplete();
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

            if (e.HasAttribute("installable_tags"))
            {
                _installableTags = e.GetAttribute("installable_tags");
            }

            if (e.HasAttribute("modifier_tags"))
            {
                _modifierTags = e.GetAttribute("modifier_tags");
            }
            
            if (e.HasAttribute("description_override"))
                _descriptionOverride = e.GetAttribute("description_override");
        }

        public override BaseChallengeObjective Clone() {
            return new ChallengeObjectiveWearTags() {
                itemClassID = itemClassID,
                expectedItem = expectedItem,
                expectedItemClass = expectedItemClass,
                _itemTags = _itemTags,
                _itemMod = _itemMod,
                _installableTags = _installableTags,
                _modifierTags = _modifierTags,
                _descriptionOverride = _descriptionOverride
            };
        }
    }
}