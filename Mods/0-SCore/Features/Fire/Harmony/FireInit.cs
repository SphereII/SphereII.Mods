using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = System.Object;

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
                Debug.Log("Disabling Fire Manager in Play Testing / Prefab editor");
                return;
            } 
            
            var fireManager = GameManager.Instance.transform.gameObject.GetOrAddComponent<FireManager>();
            if (fireManager != null)
            {
                fireManager.Init();
            }
            else
            {
                Log.Out("No Fire Manager Available.");
            }
        }
    }
    
    [HarmonyPatch(typeof(GameStateManager))]
    [HarmonyPatch("EndGame")]
    public class GameStateManagerEndGame
    {
        public static void Prefix()
        {
            var fireManager = GameManager.Instance.transform.gameObject.GetComponent<FireManager>();
            if (fireManager == null) return;
            fireManager.ForceStop();
        }
    }
}