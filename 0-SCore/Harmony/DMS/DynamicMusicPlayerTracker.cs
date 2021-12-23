using DynamicMusic;
using HarmonyLib;
using MusicUtils.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Harmony.DMS
{
    public class SCoreDynamicMusicPlayerTracker
    {
        // Skip the determineTrader if its a non-default trader; this will avoid the WRN: **** is not a known trader to DMS
        [HarmonyPatch(typeof(PlayerTracker))]
        [HarmonyPatch("determineTrader")]
        public class SCoreDynamicMusicPlayerTrack
        {
            public static bool Prefix(ref SectionType __result, PlayerTracker __instance, Vector3 ___boundingBoxRange, List<Entity> ___npcs)
            {
                GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityNPC), new Bounds(GameManager.Instance.World.GetPrimaryPlayer().position, ___boundingBoxRange), ___npcs);
                if (___npcs.Count > 0)
                {
                    var entityNpc = ___npcs[0] as EntityNPC;
                    if (entityNpc != null)
                    {
                        var npcID = entityNpc.npcID;
                        switch (npcID)
                        {
                            case "traitorjoel":
                            case "traderjen":
                            case "traderbob":
                            case "traderhugh":
                            case "traderrekt":
                                return true;
                        }
                    }
                }

                __result = SectionType.None;
                return false;
            }
        }
    }
}