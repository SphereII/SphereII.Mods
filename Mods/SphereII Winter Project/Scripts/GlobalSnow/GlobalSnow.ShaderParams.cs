using UnityEngine;


namespace GlobalSnowEffect {

    public partial class GlobalSnow : MonoBehaviour {

        public static class ShaderParams {

            public static int WorldPos = Shader.PropertyToID("_WorldPos");
            public static int TargetUV = Shader.PropertyToID("_TargetUV");
            public static int DrawDist = Shader.PropertyToID("_DrawDist");
            public static int EraseSpeed = Shader.PropertyToID("_EraseSpeed");
            public static int TargetCount = Shader.PropertyToID("_TargetCount");
            public static int TargetUVArray = Shader.PropertyToID("_TargetUVArray");
            public static int WorldPosArray = Shader.PropertyToID("_WorldPosArray");
            public static int GlobalDecalTexture = Shader.PropertyToID("_GS_DecalTex");
            public static int GlobalReliefFlag = Shader.PropertyToID("_GS_ReliefFlag");
            public static int SnowedScene = Shader.PropertyToID("_SnowedScene");
            public static int SnowedScene2 = Shader.PropertyToID("_SnowedScene2");
            public static int GlobalSunDir = Shader.PropertyToID("_GS_SunDir");
            public static int GlobalSnowData1 = Shader.PropertyToID("_GS_SnowData1");
            public static int GlobalSnowData2 = Shader.PropertyToID("_GS_SnowData2");
            public static int GlobalSnowData3 = Shader.PropertyToID("_GS_SnowData3");
            public static int GlobalSnowData4 = Shader.PropertyToID("_GS_SnowData4");
            public static int GlobalSnowData5 = Shader.PropertyToID("_GS_SnowData5");
            public static int GlobalSnowData6 = Shader.PropertyToID("_GS_SnowData6");
            public static int FlickerFreeCamPos = Shader.PropertyToID("_FlickerFreeCamPos");
            public static int ClipToWorldMatrix = Shader.PropertyToID("_ClipToWorld");
            public static int GlobalSnowTint = Shader.PropertyToID("_GS_SnowTint");
            public static int GlobalDepthMaskTexture = Shader.PropertyToID("_GS_DepthMask");
            public static int GlobalDepthMaskWorldSize = Shader.PropertyToID("_GS_DepthMaskWorldSize");
            public static int GlobalDetailTexture = Shader.PropertyToID("_GS_DetailTex");
            public static int Color = Shader.PropertyToID("_Color");
            public static int Distance01 = Shader.PropertyToID("_Distance01");
            public static int DistanceSlopeThreshold = Shader.PropertyToID("_DistanceSlopeThreshold");
            public static int MainTex = Shader.PropertyToID("_MainTex");
            public static int MaskCutOff = Shader.PropertyToID("_GS_MaskCutOff");
            public static int MaskTexture = Shader.PropertyToID("_GS_MaskTexture");
            public static int EraseCullMode = Shader.PropertyToID("_EraseCullMode");
            public static int FrostIntensity = Shader.PropertyToID("_FrostIntensity");
            public static int FrostTintColor = Shader.PropertyToID("_FrostTintColor");
            public static int CamToWorld = Shader.PropertyToID("_CamToWorld");
            public static int GlobalSnowTex = Shader.PropertyToID("_GS_SnowTex");
            public static int GlobalSnowNormalsTex = Shader.PropertyToID("_GS_SnowNormalsTex");
            public static int GlobalNoiseTexture = Shader.PropertyToID("_GS_NoiseTex");
            public static int GlobalDepthTexture = Shader.PropertyToID("_GS_DepthTexture");
            public static int GlobalCamPos = Shader.PropertyToID("_GS_SnowCamPos");
            public static int GlobalFootprintTex = Shader.PropertyToID("_GS_FootprintTex");
            public static int DistanceTexture = Shader.PropertyToID("_DistantSnow");

        }

    }

}