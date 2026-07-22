using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ErrorChecks.Harmony {
    // Vanilla XUiC_CompanionEntryList.RefreshPartyList offsets its own grid downward by
    // (party members - 1) rows AND (companions - 1) rows. The first term is correct: the
    // companion grid shares its XML position with the party grid and must sit below the
    // party rows. The second term is wrong for this top-anchored, downward-stacking HUD
    // grid - every companion beyond the first shifts the whole companion block down one
    // more row, so the list visibly marches down the screen as companions are added
    // (XUiC_PartyEntryList never repositions itself, which is why party entries stay put).
    // This postfix recomputes the position with only the party-row term, reading the row
    // height from the grid instead of vanilla's hardcoded 40.
    public class CompanionEntryListDrift {
        [HarmonyPatch(typeof(XUiC_CompanionEntryList))]
        [HarmonyPatch(nameof(XUiC_CompanionEntryList.RefreshPartyList))]
        public class XUiCCompanionEntryListRefreshPartyList {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "FixCompanionEntryListDrift";

            public static void Postfix(XUiC_CompanionEntryList __instance) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                var player = __instance.xui.playerUI.entityPlayer;
                if (player == null || __instance.viewComponent == null) return;

                var cellHeight = 40;
                if (__instance.viewComponent is XUiV_Grid grid && grid.CellHeight > 0)
                    cellHeight = grid.CellHeight;

                var partyRows = 0;
                if (player.Party != null)
                    partyRows = player.Party.MemberList.Count - 1;
                if (partyRows < 0) partyRows = 0;

                var position = new Vector2i(
                    __instance.viewComponent.Position.x,
                    (int)__instance.yOffset - partyRows * cellHeight);
                __instance.viewComponent.Position = position;
                __instance.viewComponent.UiTransform.localPosition = new Vector3(position.x, position.y);
            }
        }
    }
}
