using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCore.Harmony.GameManagerPatches
{
    public class GameManagerPatch
    {

        // Allows the spread of the particles to catch things on fire.
        [HarmonyPatch(typeof(GameManager))]
        [HarmonyPatch("SaveAndCleanupWorld")]
        public class GameManagerSaveAndCleanupWorld
        {
            public static void Postfix()
            {
                if (FireManager.Instance != null)
                {
                    Log.Out("Cleaning up Fire Manager");
                    if (FireManager.Instance.Enabled == false) return;
                    FireManager.Instance.CleanUp();
                }




            }
        }
    }
}

