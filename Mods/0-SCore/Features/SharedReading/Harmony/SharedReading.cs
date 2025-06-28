using HarmonyLib;
using UnityEngine;

namespace SCore.Features.SharedReading.Harmony
{
    public class SharedReadingPatches
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "SharedReading";

        [HarmonyPatch(typeof(ItemActionEat))]
        [HarmonyPatch(nameof(ItemActionEat.ExecuteInstantAction))] 
        public class ItemActionEatExecuteInstantActionPatch
        {
            public static void Postfix(EntityAlive ent, ItemStack stack)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (!stack.itemValue.ItemClass.Properties.Contains("Unlocks")) return;
                if (stack.itemValue.ItemClass.Properties.Contains("NoSharedReading")) return;

                if (ent is not EntityPlayerLocal readingPlayer) return;
                if (readingPlayer.Party == null) return;

                foreach (var member in readingPlayer.Party.MemberList)
                {
                    if (ent.entityId == member.entityId) continue;
                    var package = NetPackageManager.GetPackage<NetPackageMinEventSharedReading>();
                    package.Setup(member.entityId, readingPlayer.entityId, MinEventTypes.onSelfSecondaryActionEnd, stack.itemValue);
                    ConnectionManager.Instance.SendToClientsOrServer(package);
                }
                var unlock = stack.itemValue.ItemClass.Properties.GetString("Unlocks");
                unlock = SCoreLocalizationHelper.GetLocalization(unlock);
                var toolTipDisplay = $"{Localization.Get("sharedReadingSourceDesc")} :: {unlock}";
                GameManager.ShowTooltip(readingPlayer, toolTipDisplay);

            }
        }
    }
}