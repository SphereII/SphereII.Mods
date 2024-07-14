//------------------------------------------------------------------------------------------------------------------
// Global Snow
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace GlobalSnowEffect {

    [CreateAssetMenu(menuName = "Global Snow Profile", fileName = "Global Snow Profile", order = 100)]
    public partial class GlobalSnowProfile : ScriptableObject {

        public float minimumAltitude = -10f;
        public float minimumAltitudeVegetationOffset = 0f;
        public Color snowTint = Color.white;

        [Range(0, 250f)]
        public float altitudeScatter = 20f;

        [Range(0, 500f)]
        public float altitudeBlending = 25f;

        public bool distanceOptimization;

        public float detailDistance = 100f;

        public Color distanceSnowColor = new Color(0.7882f, 0.8235f, 0.9921f);

        public bool distanceIgnoreNormals;

        public bool distanceIgnoreCoverage;

        public bool smoothCoverage = true;

        [Range(1, 4)]
        public int coverageExtension = 1;

        [Range(1, 4)]
        public int coverageResolution = 1;

        [Range(0, 0.5f)]
        public float groundCoverage;

        [Range(0, 1f)]
        public float slopeThreshold = 0.7f;

        [Range(0, 1f)]
        public float slopeSharpness = 0.5f;

        [Range(0, 1f)]
        public float slopeNoise = 0.5f;

        public Texture2D snowNormalsTex, noiseTex;

        [Range(0, 2)]
        public float snowNormalsStrength = 1f;

        public float noiseTexScale = 0.1f;

        [Range(0, 1f)]
        public float distanceSlopeThreshold = 0.7f;

        public bool coverageMask;
        public Texture2D coverageMaskTexture;

        public Vector3 coverageMaskWorldSize = new Vector3(2000, 0, 2000);

        public Vector3 coverageMaskWorldCenter = new Vector3(0, 0, 0);

        public SNOW_QUALITY snowQuality = SNOW_QUALITY.ReliefMapping;

        public SNOW_COVERAGE_UPDATE_METHOD coverageUpdateMethod = SNOW_COVERAGE_UPDATE_METHOD.Discrete;

        [Range(0.05f, 0.3f)]
        public float reliefAmount = 0.3f;

        public bool occlusion = true;

        [Range(0.01f, 5f)]
        public float occlusionIntensity = 1.2f;

        public float glitterStrength = 0.75f;

        public bool snowfall = true;

        [Range(0.001f, 1f)]
        public float snowfallIntensity = 0.1f;

        public float snowfallSpeed = 1f;

        [Range(0, 2)]
        public float snowfallWind;

        [Range(10, 200)]
        public float snowfallDistance = 100f;

        public bool snowfallUseIllumination;

        public bool snowfallReceiveShadows;

        [Range(0f, 1f)]
        public float snowdustIntensity;

        [Range(0f, 2f)]
        public float snowdustVerticalOffset = 0.5f;

        [Range(0f, 2f)]
        public float maxExposure = 0.85f;

        [Range(0f, 1f)]
        public float smoothness = 0.9f;

        [Range(0f, 2f)]
        public float snowAmount = 1f;

        public bool cameraFrost = true;

        [Range(0.001f, 1.5f)]
        public float cameraFrostIntensity = 0.35f;

        [Range(1f, 5f)]
        public float cameraFrostSpread = 1.2f;

        [Range(0f, 1f)]
        public float cameraFrostDistortion = 0.25f;

        public Color cameraFrostTintColor = Color.white;

        public LayerMask
                        layerMask = -1;

        public bool
                        excludedCastShadows = true;

        public LayerMask
                        zenithalMask = -1;

        [Range(0f, 2f)]
        public float billboardCoverage = 1.4f;

        [Range(0f, 1f)]
        public float grassCoverage = 0.75f;


        public void ApplyTo(GlobalSnow snow) {
            snow.minimumAltitude = minimumAltitude;
            snow.minimumAltitudeVegetationOffset = minimumAltitudeVegetationOffset;
            snow.snowTint = snowTint;
            snow.altitudeScatter = altitudeScatter;
            snow.altitudeBlending = altitudeBlending;
            snow.distanceOptimization = distanceOptimization;
            snow.detailDistance = detailDistance;
            snow.distanceSnowColor = distanceSnowColor;
            snow.distanceIgnoreNormals = distanceIgnoreNormals;
            snow.distanceIgnoreCoverage = distanceIgnoreCoverage;
            snow.smoothCoverage = smoothCoverage;
            snow.coverageExtension = coverageExtension;
            snow.coverageResolution = coverageResolution;
            snow.groundCoverage = groundCoverage;
            snow.slopeThreshold = slopeThreshold;
            snow.slopeSharpness = slopeSharpness;
            snow.slopeNoise = slopeNoise;
            snow.snowNormalsTex = snowNormalsTex;
            snow.snowNormalsStrength = snowNormalsStrength;
            snow.noiseTexScale = noiseTexScale;
            snow.distanceSlopeThreshold = distanceSlopeThreshold;
            snow.coverageMask = coverageMask;
            snow.coverageMaskTexture = coverageMaskTexture;
            snow.coverageMaskWorldSize = coverageMaskWorldSize;
            snow.coverageMaskWorldCenter = coverageMaskWorldCenter;
            snow.snowQuality = snowQuality;
            snow.coverageUpdateMethod = coverageUpdateMethod;
            snow.reliefAmount = reliefAmount;
            snow.occlusion = occlusion;
            snow.occlusionIntensity = occlusionIntensity;
            snow.glitterStrength = glitterStrength;
            snow.snowfall = snowfall;
            snow.snowfallIntensity = snowfallIntensity;
            snow.snowfallSpeed = snowfallSpeed;
            snow.snowfallWind = snowfallWind;
            snow.snowfallDistance = snowfallDistance;
            snow.snowfallUseIllumination = snowfallUseIllumination;
            snow.snowfallReceiveShadows = snowfallReceiveShadows;
            snow.snowdustIntensity = snowdustIntensity;
            snow.snowdustVerticalOffset = snowdustVerticalOffset;
            snow.maxExposure = maxExposure;
            snow.smoothness = smoothness;
            snow.snowAmount = snowAmount;
            snow.cameraFrost = cameraFrost;
            snow.cameraFrostIntensity = cameraFrostIntensity;
            snow.cameraFrostSpread = cameraFrostSpread;
            snow.cameraFrostDistortion = cameraFrostDistortion;
            snow.cameraFrostTintColor = cameraFrostTintColor;
            snow.layerMask = layerMask;
            snow.excludedCastShadows = excludedCastShadows;
            snow.zenithalMask = zenithalMask;
            snow.billboardCoverage = billboardCoverage;
            snow.grassCoverage = grassCoverage;
            snow.UpdateMaterialPropertiesNow();
        }

        public void CopyFrom(GlobalSnow snow) {
            minimumAltitude = snow.minimumAltitude;
            minimumAltitudeVegetationOffset = snow.minimumAltitudeVegetationOffset;
            snowTint = snow.snowTint;
            altitudeScatter = snow.altitudeScatter;
            altitudeBlending = snow.altitudeBlending;
            distanceOptimization = snow.distanceOptimization;
            detailDistance = snow.detailDistance;
            distanceSnowColor = snow.distanceSnowColor;
            distanceIgnoreNormals = snow.distanceIgnoreNormals;
            distanceIgnoreCoverage = snow.distanceIgnoreCoverage;
            smoothCoverage = snow.smoothCoverage;
            coverageExtension = snow.coverageExtension;
            coverageResolution = snow.coverageResolution;
            groundCoverage = snow.groundCoverage;
            slopeThreshold = snow.slopeThreshold;
            slopeSharpness = snow.slopeSharpness;
            slopeNoise = snow.slopeNoise;
            snowNormalsTex = snow.snowNormalsTex;
            snowNormalsStrength = snow.snowNormalsStrength;
            noiseTexScale = snow.noiseTexScale;
            distanceSlopeThreshold = snow.distanceSlopeThreshold;
            coverageMask = snow.coverageMask;
            coverageMaskTexture = snow.coverageMaskTexture;
            coverageMaskWorldSize = snow.coverageMaskWorldSize;
            coverageMaskWorldCenter = snow.coverageMaskWorldCenter;
            snowQuality = snow.snowQuality;
            coverageUpdateMethod = snow.coverageUpdateMethod;
            reliefAmount = snow.reliefAmount;
            occlusion = snow.occlusion;
            occlusionIntensity = snow.occlusionIntensity;
            glitterStrength = snow.glitterStrength;
            snowfall = snow.snowfall;
            snowfallIntensity = snow.snowfallIntensity;
            snowfallSpeed = snow.snowfallSpeed;
            snowfallWind = snow.snowfallWind;
            snowfallDistance = snow.snowfallDistance;
            snowfallUseIllumination = snow.snowfallUseIllumination;
            snowfallReceiveShadows = snow.snowfallReceiveShadows;
            snowdustIntensity = snow.snowdustIntensity;
            snowdustVerticalOffset = snow.snowdustVerticalOffset;
            maxExposure = snow.maxExposure;
            smoothness = snow.smoothness;
            snowAmount = snow.snowAmount;
            cameraFrost = snow.cameraFrost;
            cameraFrostIntensity = snow.cameraFrostIntensity;
            cameraFrostSpread = snow.cameraFrostSpread;
            cameraFrostDistortion = snow.cameraFrostDistortion;
            cameraFrostTintColor = snow.cameraFrostTintColor;
            layerMask = snow.layerMask;
            excludedCastShadows = snow.excludedCastShadows;
            zenithalMask = snow.zenithalMask;
            billboardCoverage = snow.billboardCoverage;
            grassCoverage = snow.grassCoverage;
        }


        public static void Lerp(GlobalSnow gs, GlobalSnowProfile profile1, GlobalSnowProfile profile2, float t) {
            if (gs == null || profile1 == null || profile2 == null) return;
            gs.minimumAltitude = Mathf.Lerp(profile1.minimumAltitude, profile2.minimumAltitude, t);
            gs.minimumAltitudeVegetationOffset = Mathf.Lerp(profile1.minimumAltitudeVegetationOffset, profile2.minimumAltitudeVegetationOffset, t);
            gs.snowTint = Color.Lerp(profile1.snowTint, profile2.snowTint, t);
            gs.altitudeScatter = Mathf.Lerp(profile1.altitudeScatter, profile2.altitudeScatter, t);
            gs.altitudeBlending = Mathf.Lerp(profile1.altitudeBlending, profile2.altitudeBlending, t);
            gs.distanceOptimization = t < 0.5f ? profile1.distanceOptimization : profile2.distanceOptimization;
            gs.detailDistance = Mathf.Lerp(profile1.detailDistance, profile2.detailDistance, t);
            gs.distanceSnowColor = Color.Lerp(profile1.distanceSnowColor, profile2.distanceSnowColor, t);
            gs.distanceIgnoreNormals = t < 0.5f ? profile1.distanceIgnoreNormals : profile2.distanceIgnoreNormals;
            gs.distanceIgnoreCoverage = t < 0.5f ? profile1.distanceIgnoreCoverage : profile2.distanceIgnoreCoverage;
            gs.smoothCoverage = t < 0.5f ? profile1.smoothCoverage : profile2.smoothCoverage;
            gs.coverageExtension = (int)Mathf.Lerp(profile1.coverageExtension, profile2.coverageExtension, t);
            gs.coverageResolution = (int)Mathf.Lerp(profile1.coverageResolution, profile2.coverageResolution, t);
            gs.groundCoverage = Mathf.Lerp(profile1.groundCoverage, profile2.groundCoverage, t);
            gs.slopeThreshold = Mathf.Lerp(profile1.slopeThreshold, profile2.slopeThreshold, t);
            gs.slopeSharpness = Mathf.Lerp(profile1.slopeSharpness, profile2.slopeSharpness, t);
            gs.slopeNoise = Mathf.Lerp(profile1.slopeNoise, profile2.slopeNoise, t);
            gs.snowNormalsTex = t < 0.5f ? profile1.snowNormalsTex : profile2.snowNormalsTex;
            gs.snowNormalsStrength = Mathf.Lerp(profile1.snowNormalsStrength, profile2.snowNormalsStrength, t);
            gs.noiseTexScale = Mathf.Lerp(profile1.noiseTexScale, profile2.noiseTexScale, t);
            gs.distanceSlopeThreshold = Mathf.Lerp(profile1.distanceSlopeThreshold, profile2.distanceSlopeThreshold, t);
            gs.coverageMask = t < 0.5f ? profile1.coverageMask : profile2.coverageMask;
            gs.coverageMaskTexture = t < 0.5f ? profile1.coverageMaskTexture : profile2.coverageMaskTexture;
            gs.coverageMaskWorldSize = Vector3.Lerp(profile1.coverageMaskWorldSize, profile2.coverageMaskWorldSize, t);
            gs.coverageMaskWorldCenter = Vector3.Lerp(profile1.coverageMaskWorldCenter, profile2.coverageMaskWorldCenter, t);
            gs.snowQuality = t < 0.5f ? profile1.snowQuality : profile2.snowQuality;
            gs.coverageUpdateMethod = t < 0.5f ? profile1.coverageUpdateMethod : profile2.coverageUpdateMethod;
            gs.reliefAmount = Mathf.Lerp(profile1.reliefAmount, profile2.reliefAmount, t);
            gs.occlusion = t < 0.5f ? profile1.occlusion : profile2.occlusion;
            gs.occlusionIntensity = Mathf.Lerp(profile1.occlusionIntensity, profile2.occlusionIntensity, t);
            gs.glitterStrength = Mathf.Lerp(profile1.glitterStrength, profile2.glitterStrength, t);
            gs.snowfall = t < 0.5f ? profile1.snowfall : profile2.snowfall;
            gs.snowfallIntensity = Mathf.Lerp(profile1.snowfallIntensity, profile2.snowfallIntensity, t);
            gs.snowfallSpeed = Mathf.Lerp(profile1.snowfallSpeed, profile2.snowfallSpeed, t);
            gs.snowfallWind = Mathf.Lerp(profile1.snowfallWind, profile2.snowfallWind, t);
            gs.snowfallDistance = Mathf.Lerp(profile1.snowfallDistance, profile2.snowfallDistance, t);
            gs.snowfallUseIllumination = t < 0.5f ? profile1.snowfallUseIllumination : profile2.snowfallUseIllumination;
            gs.snowfallReceiveShadows = t < 0.5f ? profile1.snowfallReceiveShadows : profile2.snowfallReceiveShadows;
            gs.snowdustIntensity = Mathf.Lerp(profile1.snowdustIntensity, profile2.snowdustIntensity, t);
            gs.snowdustVerticalOffset = Mathf.Lerp(profile1.snowdustVerticalOffset, profile2.snowdustVerticalOffset, t);
            gs.maxExposure = Mathf.Lerp(profile1.maxExposure, profile2.maxExposure, t);
            gs.smoothness = Mathf.Lerp(profile1.smoothness, profile2.smoothness, t);
            gs.snowAmount = Mathf.Lerp(profile1.snowAmount, profile2.snowAmount, t);
            gs.cameraFrost = t < 0.5f ? profile1.cameraFrost : profile2.cameraFrost;
            gs.cameraFrostIntensity = Mathf.Lerp(profile1.cameraFrostIntensity, profile2.cameraFrostIntensity, t);
            gs.cameraFrostSpread = Mathf.Lerp(profile1.cameraFrostSpread, profile2.cameraFrostSpread, t);
            gs.cameraFrostDistortion = Mathf.Lerp(profile1.cameraFrostDistortion, profile2.cameraFrostDistortion, t);
            gs.cameraFrostTintColor = Color.Lerp(profile1.cameraFrostTintColor, profile2.cameraFrostTintColor, t);
            gs.layerMask = t < 0.5f ? profile1.layerMask : profile2.layerMask;
            gs.excludedCastShadows = t < 0.5f ? profile1.excludedCastShadows : profile2.excludedCastShadows;
            gs.zenithalMask = t < 0.5f ? profile1.zenithalMask : profile2.zenithalMask;
            gs.billboardCoverage = Mathf.Lerp(profile1.billboardCoverage, profile2.billboardCoverage, t);
            gs.grassCoverage = Mathf.Lerp(profile1.grassCoverage, profile2.grassCoverage, t);
            gs.UpdateMaterialPropertiesNow();
        }

    }

}