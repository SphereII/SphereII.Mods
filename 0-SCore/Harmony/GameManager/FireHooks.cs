﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCore.Harmony.GameManagerPatches
{
    public class GameManagerPatch
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

        // Light reduction patch from ocbMaurice
        [HarmonyPatch(typeof(ParticleEffect))]
        [HarmonyPatch("SpawnParticleEffect")]
        public class spawnParticleEffect
        {
            static readonly int odds = 4;
            static int count = odds;
            public static void Postfix(ref Transform __result)
            {
                if (__result?.GetComponentInChildren<Light>() is Light light)
                {
                    if (count == odds)
                    {
                        count = 0;
                    }
                    else
                    {
                        light.enabled = false;
                        light.gameObject.transform.parent = null;
                        UnityEngine.Object.Destroy(light.gameObject);
                        count += 1;
                    }
                }
            }
        }

    }
}

