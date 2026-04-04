using HarmonyLib;

namespace SCore.Harmony.XUIC
{
    /// <summary>
    /// When the looting window closes, check whether it was showing a trader NPC's harvest
    /// inventory (opened via NetPackageHarvestInventoryData on a dedicated-server client).
    /// If so, serialise whatever items the player left behind and send them back to the server
    /// via NetPackageHarvestInventoryUpdate so the server-side HarvestManager stays accurate.
    ///
    /// On a listen server the host opens the HarvestManager container directly (same object),
    /// so item changes are reflected in-place and no packet is needed — ClientPendingEntityId
    /// remains -1 and this patch is a no-op for that path.
    /// </summary>
    public class XUiC_LootWindowGroup_OnClose
    {
        [HarmonyPatch(typeof(XUiC_LootWindowGroup))]
        [HarmonyPatch(nameof(XUiC_LootWindowGroup.OnClose))]
        public class Patch
        {
            public static void Postfix(XUiC_LootWindowGroup __instance)
            {
                if (HarvestManager.ClientPendingEntityId == -1) return;
                if (HarvestManager.ClientPendingContainer == null) return;

                // Guard: only act when the window being closed is our harvest container,
                // not an unrelated loot window that opened after ours.
                if (__instance.te != HarvestManager.ClientPendingContainer) return;

                var serialized = EntitySyncUtils.SerializeItemStackArray(
                    HarvestManager.ClientPendingContainer.items);

                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                    NetPackageManager.GetPackage<NetPackageHarvestInventoryUpdate>()
                        .Setup(HarvestManager.ClientPendingEntityId, serialized));

                HarvestManager.ClientPendingEntityId  = -1;
                HarvestManager.ClientPendingContainer = null;
            }
        }
    }
}
