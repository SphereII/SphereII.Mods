using HarmonyLib;
using UnityEngine;

public static class EventOnSleeperVolumeClearedUpdate {
    public delegate void OnSleeperVolumeClearedUpdate(Vector3 position);

    public static event OnSleeperVolumeClearedUpdate OnSleeperVolumeClearedEvent;

    // [HarmonyPatch(typeof(SleeperVolume))]
    // [HarmonyPatch(nameof(SleeperVolume.EntityDied))]
    // public class SleeperVolumeClearedUpdatePatch {
    //     private static void Postfix(SleeperVolume __instance)
    //     {
    //         if (__instance.isSpawning) return ;
    //         if (__instance.respawnMap.Count > 0) return;
    //         if (__instance.numSpawned > 0) return;
    //         OnSleeperVolumeClearedEvent?.Invoke(__instance.Center);
    //     }
    // }
    //
    [HarmonyPatch(typeof(SleeperVolume), "ClearedUpdate")]
    public class SleeperVolume_ClearedUpdate_Patch
    {
        public static void Postfix(SleeperVolume __instance, bool __state)
        {
            if (__instance.wasCleared)
            {
                var playerId = __instance.GetPlayerTouchedToUpdateId();
                var player = GameManager.Instance.World.GetEntity(playerId) as EntityPlayer;
                var position = __instance.PrefabInstance.boundingBoxPosition;
                if (player is EntityPlayerLocal localPlayer)
                {
                    QuestEventManager.Current.ClearedSleepers(position);
                    return;
                }
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ClearSleeper, playerId,position ));
                //
                // Vector3i centerPosition = __instance.BoxMin + (__instance.BoxMax - __instance.BoxMin) / 2;
                // Log.Out($"Sleeper Volume at {centerPosition} has been cleared!");
                // Example: Fire a custom game event that other scripts can listen for.
                // GameManager.Instance.events.Dispatch("SleeperVolumeCleared", centerPosition);
            }
        }
    }
}