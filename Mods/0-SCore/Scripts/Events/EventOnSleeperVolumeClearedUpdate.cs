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
        public static void Postfix(SleeperVolume __instance, bool __state)
        {
            if (__instance.wasCleared)
            {
                var playerId = __instance.GetPlayerTouchedToUpdateId();
                var player = GameManager.Instance.World.GetEntity(playerId) as EntityPlayer;
                var position = __instance.PrefabInstance.boundingBoxPosition;
                if (player is EntityPlayerLocal localPlayer)
                {
                    Debug.Log($"Local Player: Position: {position}");
                    SleeperVolumeCleared(position);
                    return;
                }
                
                Debug.Log($"Remote Player: Position: {position}");
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSleeperVolumeCleared>().Setup(position,playerId ));
              
            }
        }
    }
}