using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCore.Harmony.GameManagerPatches
{
    public class GameManagerPatch_BC
    {

        // copy of firehook
        [HarmonyPatch(typeof(GameStateManager))]
        [HarmonyPatch("StartGame")]
        public class GameStateManagerStartGame
        {
            public static void Postfix()
            {
                Broadcastmanager.Init();
            }
        }
    }

}

