using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        
        [HarmonyPatch(typeof(GameManager))]
        [HarmonyPatch("SpawnParticleEffectServer")]
        public class GameManagerSpawnParticleEffectServer
        {
            public static bool Prefix(ref ParticleEffect _pe)
            {
                if (_pe.debugName == "confetti")
                {
                    _pe = new ParticleEffect("confetti", _pe.pos, _pe.lightValue, Color.white, null, null, false);
                }
                return true;
            }
        }
    }

}

