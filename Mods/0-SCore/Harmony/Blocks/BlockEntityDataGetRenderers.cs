using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SCore.Harmony.Blocks {
    public class BlockEntityDataGetRenderers {
        
        private static readonly string AdvFeatureClass = "ErrorHandling";
        private static readonly string Feature = "BlockEntityDataGetRenderers";

        
        [HarmonyPatch(typeof(BlockEntityData))]
        [HarmonyPatch("GetRenderers")]
        public class BlockEntityDataGetRenderersPatch
        {
            /*
             * On a quest reset, sometimes the following call stack will be called.
             * NullReferenceException
                at (wrapper managed-to-native) UnityEngine.Component.get_gameObject(UnityEngine.Component)
                at UnityEngine.Component.GetComponentsInChildren[T] (System.Boolean includeInactive, System.Collections.Generic.List`1[T] result) [0x00001] in <be2cce08ca774b9684099a81093ecac0>:0 
                at BlockEntityData.GetRenderers () [0x00060] in <0a824c04d551409fad5953ac8c5c40be>:0 
             */
            public static bool Prefix(BlockEntityData __instance) {
                return true;
                if (__instance.transform != null) return true;

                if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                   Log.Out($"BlockEntityData.GetRenderers:: Transform unexpectedly null on {__instance.blockValue.Block.GetBlockName()}");
                
                __instance.matPropBlock ??= new MaterialPropertyBlock();
                if (__instance.renderers != null)
                    __instance.renderers.Clear();
                else
                    __instance.renderers = new List<Renderer>();

                return false;

            }
        }
    }
}