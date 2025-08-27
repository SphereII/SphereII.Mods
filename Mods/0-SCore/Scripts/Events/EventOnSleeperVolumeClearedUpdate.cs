using HarmonyLib;
using UnityEngine;

public static class EventOnSleeperVolumeClearedUpdate {
    public delegate void OnSleeperVolumeClearedUpdate(Vector3 position);

    public static event OnSleeperVolumeClearedUpdate OnSleeperVolumeClearedEvent;

    [HarmonyPatch(typeof(SleeperVolume))]
    [HarmonyPatch(nameof(SleeperVolume.ClearedUpdate))]
    public class SleeperVolumeClearedUpdatePatch {
        private static void Postfix(SleeperVolume __instance)
        {
            if (__instance?.groupCountList == null) return;
            
            // Don't fire the event if there was no spawn.
            var count = 0;
            foreach (var group in __instance.groupCountList)
                count += group.count;

            // There were never any spawns
            if (count == 0) return;

            // The volume wasn't cleared. Why are we getting here?
            if (__instance.GetAliveCount() > 0) return;
            
            OnSleeperVolumeClearedEvent?.Invoke(__instance.Center);
        }
    }
}