using HarmonyLib;
public static class EventOnRallyPointActivated {
    public delegate void OnRallyPointActivated();

    public static event OnRallyPointActivated OnActivated;

    [HarmonyPatch(typeof(ObjectiveRallyPoint))]
    [HarmonyPatch("RallyPointActivated")]
    public class ObjectiveRallyPointRallyPointActivated {
        private static void Postfix() {
            OnActivated?.Invoke();
        }
    }
}