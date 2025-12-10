using UnityEngine;

namespace Challenges {
    public class SCoreChallengeUtils {

        private static ItemClass GetHoldingItem() {
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            return player == null ? null : player.inventory.holdingItem;
        }
        public static bool IsHoldingItemName(string itemName) {
            if (string.IsNullOrEmpty(itemName)) return false;
            var holdingItem = GetHoldingItem();
            if (holdingItem == null) return false;
            foreach (var item in itemName.Split(','))
            {
                if (holdingItem.GetItemName() == item)
                    return true;
            }

            return false;
        }

        public static string GenerateString(string value) {
            return string.IsNullOrEmpty(value) ? "" : $"{Localization.Get(value)} ";
        }
        public static string GetWithString() {
            return Localization.Get("challengeObjectiveWith");
        }
        public static bool IsKilledByTrap(DamageResponse _damageResponse, string itemName) {
            if (string.IsNullOrEmpty(itemName)) return false;
            if (_damageResponse.Source?.AttackingItem == null) return false;
            
            foreach (var item in itemName.Split(','))
            {
                if (_damageResponse.Source.AttackingItem.ItemClass.GetItemName() == item)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsHoldingItemHasTag(string tag) {
            if (string.IsNullOrEmpty(tag)) return false;
            var holdingItem = GetHoldingItem();
            if (holdingItem == null) return false;
            var fastTags = FastTags<TagGroup.Global>.Parse(tag);
            return holdingItem.HasAnyTags(fastTags);
        }
        
        public static bool IsHoldingItemMaterial(string material) {
            if (string.IsNullOrEmpty(material)) return false;
            var holdingItem = GetHoldingItem();
            if (holdingItem == null) return false;
            return holdingItem.MadeOfMaterial.id == material;
        }
    }
}