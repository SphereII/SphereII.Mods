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

                // The entity performing the action is always local
                if (ent is not EntityPlayerLocal readingPlayer) return;
                if (readingPlayer.Party == null) return;

              
                // Show the tooltip for the local player.
                var unlock = stack.itemValue.ItemClass.Properties.GetString("Unlocks");
                unlock = SCoreLocalizationHelper.GetLocalization(unlock);
                var toolTipDisplay = $"{Localization.Get("sharedReadingSourceDesc")} :: {unlock}";
                GameManager.ShowTooltip(readingPlayer, toolTipDisplay);

                // Now, handle the network broadcasting.
                // This is only necessary if the player is a client connected to a server.
                if (!ConnectionManager.Instance.IsServer)
                {
                    // The client sends a package to the server to trigger the broadcast.
                    // Use the same package setup, but send it to the server.
                    var broadcastPackage = NetPackageManager.GetPackage<NetPackageMinEventSharedReading>();
                    broadcastPackage.Setup(readingPlayer.entityId, readingPlayer.entityId, MinEventTypes.onSelfSecondaryActionEnd, stack.itemValue);
                    ConnectionManager.Instance.SendToServer(broadcastPackage);
                }
                else
                {
                    // Process the event locally first. This handles the player who initiated the action.
                    var package = NetPackageManager.GetPackage<NetPackageMinEventSharedReading>();
                    package.Setup(readingPlayer.entityId, readingPlayer.entityId, MinEventTypes.onSelfSecondaryActionEnd, stack.itemValue);
                    package.ProcessPackage(readingPlayer.world, GameManager.Instance);

                }
            }
        }
    }
}