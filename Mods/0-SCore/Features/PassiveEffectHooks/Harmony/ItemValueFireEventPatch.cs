using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch(nameof(ItemValue.FireEvent))]
    public class ItemValueFireEventPatch
    {
        public static void Postfix(ItemValue __instance, MinEventTypes _eventType, MinEventParams _eventParms)
        {
            if (!__instance.HasMods()) return;

            var originalItem = _eventParms.ItemValue;
            foreach (var mod in __instance.Modifications)
            {
                if ( mod == null ) continue;
                if ( mod.IsEmpty()) continue;
                _eventParms.ItemValue = mod;
                mod.FireEvent(_eventType, _eventParms);
            }

            _eventParms.ItemValue = originalItem;
        }
    }
}