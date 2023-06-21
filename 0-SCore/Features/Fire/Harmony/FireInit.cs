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
            FireManager.Init();
        }
    }


}