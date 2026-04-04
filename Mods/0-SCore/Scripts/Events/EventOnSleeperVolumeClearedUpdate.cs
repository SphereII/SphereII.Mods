using HarmonyLib;
using UnityEngine;

public static class EventOnSleeperVolumeClearedUpdate {
    public delegate void OnSleeperVolumeClearedUpdate(Vector3 position);

    public static event OnSleeperVolumeClearedUpdate OnSleeperVolumeClearedEvent;

    public static void SleeperVolumeCleared(Vector3 pos)
    {
        OnSleeperVolumeClearedEvent?.Invoke(pos);
    }
    [HarmonyPatch(typeof(SleeperVolume), "ClearedUpdate")]
    public class SleeperVolume_ClearedUpdate_Patch
    {
        public static void Postfix(SleeperVolume __instance)
        {
            // 1. Basic null and state check
            if (__instance == null || !__instance.wasCleared) return;

            // 2. Get the POI instance directly from the SleeperVolume
            // In 7DTD, SleeperVolumes usually have a reference to their parent prefab
            var poiInstance = __instance.prefabInstance;
            if (poiInstance == null) return;

            var position = poiInstance.boundingBoxPosition;
            var playerId = __instance.GetPlayerTouchedToUpdateId();
            var entity = GameManager.Instance.World.GetEntity(playerId);

            // 3. Handle Local Player logic
            if (entity is EntityPlayerLocal localPlayer)
            {
                // Instead of checking localPlayer.prefab (which is null if they stepped out),
                // we just trigger the event because the volume itself was cleared.
                SleeperVolumeCleared(position);
                return;
            }

            // 4. Handle Multiplayer/Server logic
            if (SingletonMonoBehaviour<ConnectionManager>.Instance != null)
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageSleeperVolumeCleared>().Setup(position, playerId)
                );
            }
        }
    }
}