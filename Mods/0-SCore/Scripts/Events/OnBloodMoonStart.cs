using HarmonyLib;


public static class EventOnBloodMoonStart {
    public delegate void OnBloodMoonStartEvent();

    public static event OnBloodMoonStartEvent BloodMoonStart;

    [HarmonyPatch(typeof(AIDirectorBloodMoonComponent))]
    [HarmonyPatch("StartBloodMoon")]
    public class AIDirectorBloodMoonComponentStartBloodMoon {
        private static void Prefix() {
            BloodMoonStart?.Invoke();
        }
    }
}