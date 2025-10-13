using DynamicMusic;
using HarmonyLib;
using UnityEngine;

namespace Harmony.NPCFeatures
{
    [HarmonyPatch(typeof(PlayerTracker))]
    [HarmonyPatch(nameof(PlayerTracker.IsTraderAreaOpen))]
    public class DynamicMusicPlayerTrackerIsTraderAreaOpen
    {
        // Sometimes NPCs will be picked up when the DynamicMusic is scanning for EntityTraders, and it'll
        // pick an invalid trader (aka, a non-trader NPC.). This patch checks for it.
        private static bool Prefix(ref bool __result, TraderArea _ta, PlayerTracker __instance)
        {
            var vector = _ta.Position.ToVector3() + _ta.PrefabSize.ToVector3() / 2f;
            var bounds = new Bounds(vector, _ta.PrefabSize.ToVector3());
            GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityTrader), bounds, __instance.traders);
            if (__instance.traders.Count <= 0) 
            {
                __result = false;
                return false;
            }

            var isOpen = false;
            foreach (var entity in __instance.traders)
            {
                if (entity is not EntityTrader entityTrader) continue;
                var traderData = entityTrader.TileEntityTrader?.TraderData;
                if (traderData == null) continue;
                if ( traderData.TraderID == -1) continue;
                if (TraderInfo.traderInfoList.Length < traderData.TraderID)
                {
                    Debug.Log($"Total Traders: {TraderInfo.traderInfoList.Length} < {traderData.TraderID}");
                    continue;
                }
                if ( traderData.TraderInfo == null) continue;
                if (!traderData.TraderInfo.IsOpen) continue;
                isOpen = true;
                break;
            }
         
            __instance.traders.Clear();
            __result = isOpen;
            return false;

        }
    }
}