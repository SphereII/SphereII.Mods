using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Features.Fire.Harmony
{
    // Allows the spread of the particles to catch things on fire.
    [HarmonyPatch(typeof(GameStateManager))]
    [HarmonyPatch("StartGame")]
    public class GameStateManagerStartGame
    {
        public static void Postfix()
        {
            if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"
                || GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting"
                || GamePrefs.GetString(EnumGamePrefs.GameMode) == "GameModeEditWorld")
            {
                Debug.Log("Displaying Fire Manage in Play Testing / Prefab editor");
                return;
            }
            FireManager.Init();
        }
    }


}