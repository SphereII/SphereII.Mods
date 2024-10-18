using System;
using HarmonyLib;
using UnityEngine;


public static class EventOnBuffAdded {
    public delegate void OnBuffAdded(BuffClass buffClass);

    public static event OnBuffAdded BuffAdded;

    [HarmonyPatch(typeof(EntityBuffs))]
    [HarmonyPatch(nameof(EntityBuffs.AddBuff))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3i), typeof(int), typeof(bool), typeof(bool), typeof(float) })]
    public class BuffManagerAddBuff {
        private static void Prefix(string _name) {
            var buff = BuffManager.GetBuff(_name);
            if (buff == null)
            {
                Debug.Log($"OnBuffAdded,SCore: No such Buff: {_name}");
                return;
            }
            BuffAdded?.Invoke(buff);
        }
    }
}