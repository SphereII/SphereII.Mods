using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(MinEventActionBase))]
    [HarmonyPatch(nameof(MinEventActionBase.ParseXmlAttribute))]
    public class MinEventActionBaseEnumPatcher
    {
        public static bool Prefix(ref bool __result, MinEventActionBase __instance, XAttribute _attribute)
        {
            var localName = _attribute.Name.LocalName;
            if (localName != "trigger") return true;

            
            if (!EnumUtils.TryParse<SCoreMinEventTypes>(_attribute.Value, out var scoreMinEventTypes)) return true;
            __instance.EventType = (MinEventTypes)scoreMinEventTypes;
            __result = true;
            return false;

        }
    
    }
   
}

