using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ErrorChecks.Harmony {
    // SCore's pathing cubes and other "programming" sign blocks intentionally have no visual
    // renderer, so vanilla SignDataManager.TryApplyRenderingData() spams
    // "Unexpected case in Sign Data Manager: signRenderer is null" every time they update.
    // Vanilla already skips null renderers safely; this prefix runs the same logic without
    // the warnings.
    public class SignDataManagerTryApplyRenderingData {
        [HarmonyPatch(typeof(SignDataManager))]
        [HarmonyPatch(nameof(SignDataManager.TryApplyRenderingData))]
        [HarmonyPatch(new[] { typeof(GlobalSignId), typeof(SignDataManager.RenderingDataPatcher), typeof(List<SignRenderer>), typeof(SignCanvas.SignBlendMode), typeof(Camera) })]
        public class SignDataManagerTryApplyRenderingDataPatch {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "MuteSignRendererWarnings";

            public static bool Prefix(SignDataManager __instance, ref bool __result,
                GlobalSignId signId, SignDataManager.RenderingDataPatcher patcher,
                List<SignRenderer> signRenderers, SignCanvas.SignBlendMode blendMode,
                Camera targetCamera) {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

                SignDataManager.SignRenderingData signRenderingData;
                if (!__instance.TryGetRenderingData(signId, out signRenderingData)) {
                    __result = false;
                    return false;
                }

                signRenderingData.materialPropertyBlock.SetVector(SignShaderIDs._AtlasArray_ST, new Vector4(2f, 2f, -1f, -1f));
                patcher?.Invoke(signRenderingData.materialPropertyBlock);

                foreach (var signRenderer in signRenderers) {
                    if (signRenderer == null || signRenderer.Renderer == null) continue;
                    signRenderer.SetRenderParameters(signRenderingData.materialPropertyBlock, blendMode, targetCamera);
                }

                __result = true;
                return false;
            }
        }
    }
}
