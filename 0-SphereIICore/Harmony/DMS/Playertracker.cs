using DynamicMusic;
using HarmonyLib;
using MusicUtils.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SphereII_DynamicMusic_PlayerTracker
{

	// Skip the determineTrader if its a non-default trader; this will avoid the WRN: **** is not a known trader to DMS
    [HarmonyPatch(typeof(DynamicMusic.PlayerTracker))]
    [HarmonyPatch("determineTrader")]
    public class SphereII_DynamicMusic_PlayerTrack
    {
        public static bool Prefix(ref SectionType __result, PlayerTracker __instance, Vector3 ___boundingBoxRange, List<Entity> ___npcs)
        {
			GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityNPC), new Bounds(GameManager.Instance.World.GetPrimaryPlayer().position, ___boundingBoxRange), ___npcs);
			if (___npcs.Count > 0)
			{
				EntityNPC entityNPC = ___npcs[0] as EntityNPC;
				if (entityNPC != null)
				{
					string npcID = entityNPC.npcID;
					if (npcID == "traitorjoel")
					{
						return true;
					}
					if (npcID == "traderjen")
					{
						return true;
					}
					if (npcID == "traderbob")
					{
						return true;
					}
					if (npcID == "traderhugh")
					{
						return true;
					}
					if (npcID == "traderrekt")
					{
						return true;
					}
				}
			}

			__result= SectionType.None;
			return false;
		}
    }

}