using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Harmony.EntityAlive {
    // In vanilla, the jiggle script gets turned off at a pretty aggressive level.
    // This will allow each entity to customize it's jiggle distance.
    [HarmonyPatch(typeof(EModelBase))]
    [HarmonyPatch("JiggleOn")]
    public class EModelBaseJiggleOn {
        private static bool Prefix(EModelBase __instance, ref bool _on) {
            if (__instance.entity is not global::EntityAlive entityAlive) return true;

            if (entityAlive.EntityClass.Properties.GetBool("AlwaysJiggle"))
                _on = true;
            
            if (entityAlive.EntityClass.Properties.GetBool("NeverScaleAI"))
                entityAlive.aiActiveScale = 1f;
            
            return true;

        }
    }
}