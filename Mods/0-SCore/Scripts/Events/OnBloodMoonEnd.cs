using HarmonyLib;


public static class EventOnBloodMoonEnd {
    public delegate void OnBloodMoonEndEvent();

    public static event OnBloodMoonEndEvent BloodMoonEnd;

    [HarmonyPatch(typeof(AIDirectorBloodMoonComponent))]
    [HarmonyPatch("EndBloodMoon")]
    public class AIDirectorBloodMoonComponentEndBloodMoon {
        private static void Prefix() {
            BloodMoonEnd?.Invoke();
        }
    }
}