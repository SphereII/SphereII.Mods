using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace SCore.Harmony.ZombieFeatures.UMA
{
    public class UMATweaks
    {
        private static string AdvFeatureClass = "AdvancedZombieFeatures";
        private static string Feature = "UMATweaks";

        // Disable the pregeneration of UMAs, since this can be time consuming and resource intensive, causing menu lag, etc. 
        // Create the directory if it doesn't exist, so when they are populated on demand, they'll be saved.
        [HarmonyPatch(typeof(Archetypes))]
        [HarmonyPatch("PregenStatic")]
        public class SphereII_PregenStatic
        {
            public static bool Prefix(Archetype __instance, ref List<Archetype> ___archetypes)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                // Create the folder if it doesn't exist
                string strUMAFolder = Path.Combine(Application.dataPath, "..", "Data", "UMATextures");
                if (!Directory.Exists(strUMAFolder))
                    Directory.CreateDirectory(strUMAFolder);

                // No not pre-generating the UMAs. Let them be created on the fly when needed, but saved for future use.
                return false;
            }
        }

        [HarmonyPatch(typeof(Archetypes))]
        [HarmonyPatch("GetArchetype")]
        public class SphereII_Archetypes_GetArcheType_Random
        {
            static bool DisplayLogs = false;


            public static bool Prefix(Archetype __result, string _name, List<Archetype> ___archetypes)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (DisplayLogs) Debug.Log("GetArchetype(): " + _name);
                int MaxArchetypes = ___archetypes.Count - 1;
                if (_name == "Random")
                {
                    GameRandom random = GameRandomManager.Instance.CreateGameRandom();
                    if (DisplayLogs) Debug.Log("GetArcheType(): Randomizing UMA. Randomized Pool Size: " + ___archetypes.Count);

                    int intRandom = random.RandomRange(0, MaxArchetypes);
                    if (DisplayLogs) Debug.Log("Random Range: " + intRandom);
                    __result = ___archetypes[intRandom].Clone();

                    return false;
                }

                return true;
            }
        }


    }
}
