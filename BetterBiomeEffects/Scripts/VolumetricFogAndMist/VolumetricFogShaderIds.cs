//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------

using UnityEngine;


namespace VolumetricFogAndMist {
    public partial class VolumetricFog : MonoBehaviour {

        static class ShaderParams {
            public static int FogWindDir = Shader.PropertyToID("_FogWindDir");
            public static int FogSkyData = Shader.PropertyToID("_FogSkyData");
            public static int LightMatrix = Shader.PropertyToID("_VolumetricFogLightMatrix");
            public static int CookieSize = Shader.PropertyToID("_VolumetricFogCookieSize");
            public static int CookieTex = Shader.PropertyToID("_VolumetricFogLightCookie");
            public static int FlickerFreeCamPos = Shader.PropertyToID("_FlickerFreeCamPos");
            public static int ClipDir = Shader.PropertyToID("_ClipDir");
            public static int ClipToWorld = Shader.PropertyToID("_ClipToWorld");
            public static int ShaftTex = Shader.PropertyToID("_ShaftTex");
            public static int DownsampledDepth = Shader.PropertyToID("_DownsampledDepth");
            public static int MainTex = Shader.PropertyToID("_MainTex");
            public static int FogDownsampled = Shader.PropertyToID("_FogDownsampled");
            public static int FogPointLightColor = Shader.PropertyToID("_FogPointLightColor");
            public static int FogPointLightPosition = Shader.PropertyToID("_FogPointLightPosition");
            public static int ScreenMaskTexture = Shader.PropertyToID("_VolumetricFogScreenMaskTexture");
            public static int SunPosition = Shader.PropertyToID("_SunPosition");
            public static int SunPositionRightEye = Shader.PropertyToID("_SunPositionRightEye");
            public static int FogScatteringData = Shader.PropertyToID("_FogScatteringData");
            public static int FogScatteringData2 = Shader.PropertyToID("_FogScatteringData2");
            public static int FogScatteringTint = Shader.PropertyToID("_FogScatteringTint");
            public static int SunDir = Shader.PropertyToID("_SunDir");
            public static int SunColor = Shader.PropertyToID("_SunColor");
            public static int FogSkyColor = Shader.PropertyToID("_FogSkyColor");
            public static int FogSkyNoiseScale = Shader.PropertyToID("_FogSkyNoiseScale");
            public static int NoiseTex = Shader.PropertyToID("_NoiseTex");
            public static int FogData = Shader.PropertyToID("_FogData");
            public static int FogSkyHaze = Shader.PropertyToID("_FogSkyHaze");
            public static int FogVoidPosition = Shader.PropertyToID("_FogVoidPosition");
            public static int FogAreaPosition = Shader.PropertyToID("_FogAreaPosition");
            public static int DeepObscurance = Shader.PropertyToID("_DeepObscurance");
            public static int Jitter = Shader.PropertyToID("_Jitter");
            public static int FogStepping = Shader.PropertyToID("_FogStepping");
            public static int FogAlpha = Shader.PropertyToID("_FogAlpha");
            public static int FogDistance = Shader.PropertyToID("_FogDistance");
            public static int FogVoidData = Shader.PropertyToID("_FogVoidData");
            public static int FogAreaData = Shader.PropertyToID("_FogAreaData");
            public static int FogOfWarTexture = Shader.PropertyToID("_FogOfWar");
            public static int FogOfWarCenter = Shader.PropertyToID("_FogOfWarCenter");
            public static int FogOfWarSize = Shader.PropertyToID("_FogOfWarSize");
            public static int FogOfWarCenterAdjusted = Shader.PropertyToID("_FogOfWarCenterAdjusted");
            public static int PointLightsInsideAtten = Shader.PropertyToID("_PointLightInsideAtten");
            public static int FogBlurDepth = Shader.PropertyToID("_FogBlurDepth");
            public static int VFM_CutOff = Shader.PropertyToID("_VFM_CutOff");
            public static int FogColor = Shader.PropertyToID("_FogColor");
            public static int BlurDepth = Shader.PropertyToID("_BlurDepth");
            public static int BlurTex = Shader.PropertyToID("_BlurTex");
            public static int Amount = Shader.PropertyToID("_Amount");
            public static int GlobalDepthTexture = Shader.PropertyToID("_VolumetricFogDepthTexture");
            public static int GlobalShadowBias = Shader.PropertyToID("_VF_ShadowBias");
            public static int GlobalSunProjection = Shader.PropertyToID("_VolumetricFogSunProj");
            public static int GlobalSunDepthTexture = Shader.PropertyToID("_VolumetricFogSunDepthTexture");
            public static int GlobalSunWorldPos = Shader.PropertyToID("_VolumetricFogSunWorldPos");
            public static int GlobalSunShadowsCameraWorldPos = Shader.PropertyToID("_VolumetricFogSunShadowsCameraWorldPos");
            public static int GlobalSunShadowsData = Shader.PropertyToID("_VolumetricFogSunShadowsData");
            public static int SurfaceCaptureMatrix = Shader.PropertyToID("_SurfaceCaptureMatrix");
            public static int SurfaceDepthTexture = Shader.PropertyToID("_SurfaceDepthTexture");
            public static int SurfaceData = Shader.PropertyToID("_SurfaceData");
            public static int BlueNoiseTexture = Shader.PropertyToID("_BlueNoise");
            public static int BlurEdgeThreshold = Shader.PropertyToID("_BlurEdgeThreshold");
        }

    }
}


