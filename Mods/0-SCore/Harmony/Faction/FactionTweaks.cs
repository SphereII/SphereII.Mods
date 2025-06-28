using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Harmony.Faction
{
    /**
     * SCoreFaction_Tweaks
     * 
     * This class includes a Harmony patches to enable or improve functionality in the factions. It includes allowing the factions to be saved to disk, as well
     * as fixing a casting bug when setting the relationship (A18/A19).
     */
    internal class FactionTweaks
    {
        // Fixing for a reverse condition for isGameStarted
        [HarmonyPatch(typeof(FactionManager))]
        [HarmonyPatch(nameof(FactionManager.Update))]
        public class FactionUpdate
        {

            public static bool Prefix(FactionManager __instance, ref float ___saveTime, ThreadManager.ThreadInfo ___dataSaveThreadInfo)
            {
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || GameManager.Instance.World == null || GameManager.Instance.World.Players == null || GameManager.Instance.World.Players.Count == 0 )
                    return false;

                ___saveTime -= Time.deltaTime;
                if (___saveTime <= 0f && (___dataSaveThreadInfo == null || ___dataSaveThreadInfo.HasTerminated()))
                {
                    ___saveTime = 60f;
                    __instance.Save();
                }
                return false;
            }
        }

        // Fixing casting bug
        [HarmonyPatch(typeof(global::Faction))]
        [HarmonyPatch("SetRelationship")]
        public class SetRelationship
        {
            public static bool Prefix(global::Faction __instance, byte _factionId, float _value)
            {
               //if ( __instance.Relationships.Contains(_factionId))
                    __instance.Relationships[_factionId] = Mathf.Clamp(_value, 0f, 1000f);
                return false;
            }
        }
        
        // Working around missing faction bug
        [HarmonyPatch(typeof(FactionManager))]
        [HarmonyPatch("GetFactionByName")]
        public class FactionGetFactionByName
        {
            public static bool Prefix(ref global::Faction __result, FactionManager __instance, string _name) {
                global::Faction defaultFaction = null;
                for (var i = 0; i < __instance.Factions.Length; i++)
                {
                    // If no faction is found, use this one.
                    if (__instance.Factions[i]?.Name == "undead")
                        defaultFaction = __instance.Factions[i];

                    if (__instance.Factions[i]?.Name != _name) continue;
                    __result = __instance.Factions[i];
                    return false;
                }
               
                Debug.Log($"FactionManager: Requested this Faction: {_name} but it was not defined in the npc.xml. Defaulting to Undead faction.");
                __result = defaultFaction;
                return false;
            }
        }
   
    }
}