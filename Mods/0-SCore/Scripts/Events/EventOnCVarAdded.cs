using HarmonyLib;

public class EventOnCVarAdded {
    public delegate void OnCVarAdded(EntityAlive entityAlive,string cvarName, float cvarValue);

    public static event OnCVarAdded CVarAdded;

    [HarmonyPatch(typeof(EntityBuffs))]
    [HarmonyPatch(nameof(EntityBuffs.SetCustomVar))]
    public class BuffManagerSetCustomVar {
        private static void Postfix(EntityBuffs __instance, string _name, float _value) {
            CVarAdded?.Invoke(__instance.parent, _name, _value);
        }
    }
}