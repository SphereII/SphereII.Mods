//------------------------------------------------------------------------------------------------------------------
// Global Snow
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;


namespace GlobalSnowEffect {

    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [HelpURL("https://kronnect.com/support")]
    public class GlobalSnowImageEffect : MonoBehaviour {

        GlobalSnow snow;

        void OnEnable() {
            snow = GetComponent<GlobalSnow>();
        }

        [ImageEffectOpaque] // comment out to force camera frost effect to render after transparent objects
        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (!enabled || snow == null) {
                Graphics.Blit(source, destination);
                return;
            }

            Material snowMat = snow.composeMat;
            Camera cameraEffect = snow.snowCamera;
            if (snowMat == null || cameraEffect == null || (!snow.showSnowInSceneView && Camera.current.cameraType == CameraType.SceneView)) {
                Graphics.Blit(source, destination);
                return;
            }

            if (snow.distanceOptimization && !snow.deferred && snow.distantSnowMat != null) {
                RenderTexture rtDistantSnow = RenderTexture.GetTemporary(cameraEffect.pixelWidth, cameraEffect.pixelHeight, 24, RenderTextureFormat.ARGB32);
                snow.distantSnowMat.SetMatrix(GlobalSnow.ShaderParams.ClipToWorldMatrix, cameraEffect.cameraToWorldMatrix);
                snow.distantSnowMat.SetMatrix(GlobalSnow.ShaderParams.CamToWorld, cameraEffect.cameraToWorldMatrix);
                Graphics.Blit(source, rtDistantSnow, snow.distantSnowMat);
                snowMat.SetTexture(GlobalSnow.ShaderParams.DistanceTexture, rtDistantSnow);
                RenderTexture.ReleaseTemporary(rtDistantSnow);
            }
            bool frosted = snow.cameraFrost && snow.snowAmount > 0;
            snowMat.SetVector(GlobalSnow.ShaderParams.FrostIntensity, new Vector3(frosted ? snow.cameraFrostIntensity * snow.snowAmount * 5f : 0, 5.1f - snow.cameraFrostSpread, snow.cameraFrostDistortion * 0.01f));
            snowMat.SetColor(GlobalSnow.ShaderParams.FrostTintColor, snow.cameraFrostTintColor);
            int renderPass = snow.debugSnow ? 1 : 0;
            Graphics.Blit(source, destination, snowMat, renderPass);
        }
    }

}