using HarmonyLib;
using UnityEngine;

public class EventOnCVarAdded {
    public delegate void OnCVarAdded(EntityAlive entityAlive,string cvarName, float cvarValue);

    public static event OnCVarAdded CVarAdded;

    [HarmonyPatch(typeof(EntityBuffs))]
    [HarmonyPatch(nameof(EntityBuffs.SetCustomVar))]
    public class BuffManagerSetCustomVar {
        private static void Postfix(EntityBuffs __instance, string _name, float _value)
        {
            // Don't process the _ cvars. Those are super spammy.
            if (_name.StartsWith("_")) return;
            CVarAdded?.Invoke(__instance.parent, _name, _value);
        }
    }
}