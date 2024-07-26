using HarmonyLib;


public static class EventOnEnterPoi {
    public delegate void OnEnterPoi(PrefabInstance prefabInstance);

    public static event OnEnterPoi EnterPoi;

    [HarmonyPatch(typeof(EntityPlayer))]
    [HarmonyPatch("onNewPrefabEntered")]
    public class EntityPlayerOnNewPrefabEntered {
        private static void Prefix(PrefabInstance _prefabInstance) {
            if (_prefabInstance == null) return;
            EnterPoi?.Invoke(_prefabInstance);
        }
    }
}