using System;
using HarmonyLib;


public static class EventOnBuffAdded {
    public delegate void OnBuffAdded(BuffClass buffClass);

    public static event OnBuffAdded BuffAdded;

    [HarmonyPatch(typeof(EntityBuffs))]
    [HarmonyPatch(nameof(EntityBuffs.AddBuff))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3i), typeof(int), typeof(bool), typeof(bool), typeof(float) })]
    public class BuffManagerAddBuff {
        private static void Prefix(string _name) {
            var buff = BuffManager.GetBuff(_name);
            BuffAdded?.Invoke(buff);
        }
    }
}