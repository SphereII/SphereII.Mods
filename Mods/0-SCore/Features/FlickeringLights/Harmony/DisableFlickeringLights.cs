using HarmonyLib;
using UnityEngine;

namespace SCore.Features.FlickeringLights.Harmony
{
    
    
    public class DisableFlickeringLights
    {
        private static readonly string AdvFeatureClass = "AdvancedPrefabFeatures";
        private static readonly string Feature = "DisableFlickeringLights";

      
        [HarmonyPatch(typeof(Chunk))]
        [HarmonyPatch("GetTileEntity")]
        public class DisableFlickeringLightsChunkGetTileEntity
        {
            public static void Postfix(ref TileEntity __result) {
                if (__result is TileEntityLight _light)
                {
                    // Check if this feature is enabled.
                    if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature, true))
                        return;
                    _light.LightState = LightStateType.Static;

                }
            }
        }

        [HarmonyPatch(typeof(TileEntityLight))]
        [HarmonyPatch("CopyFrom")]
        public class DisableFlickeringLightsCopyFrom
        {
            public static void Postfix(ref TileEntityLight __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature, true))
                    return;
                __instance.LightState = LightStateType.Static;
            }
        }
        
        [HarmonyPatch(typeof(TileEntityLight))]
        [HarmonyPatch("Clone")]
        public class DisableFlickeringLightsClone
        {
            public static void Postfix(ref TileEntity __result)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature, true))
                    return ;
                if (__result is not TileEntityLight light) return;
                light.LightState = LightStateType.Static;
                
            }
        }
        
        [HarmonyPatch(typeof(SkyManager))]
        [HarmonyPatch("TriggerLightning")]
        public class DisableFlickeringLightsSkyManagerTriggerLightning
        {
            public static bool Prefix()
            {
                // Check if this feature is enabled.
                return !Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
            }
        }
   
    }
}