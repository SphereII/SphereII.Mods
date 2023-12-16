//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist Scriptable Object
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


namespace VolumetricFogAndMist {

	//[CreateAssetMenu(fileName = "VolumetricFogProfile", menuName = "Volumetric Fog Profile", order = 100)]
	public class VolumetricFogProfile: ScriptableObject {

		public LIGHTING_MODEL lightingModel = LIGHTING_MODEL.Classic;

		public bool sunCopyColor = true;

		[Range(0, 1.25f)]
		public float density = 1.0f;

		[Range(0, 1f)]
		public float noiseStrength = 0.8f;

		[Range(0, 500)]
		public float height = 4f;

        [Range(0, 1f)]
        public float heightFallOff = 0.6f;

        public float baselineHeight = 0f;

		[Range(0, 1000)]
		public float distance = 0f;

		[Range(0, 5f)]
		public float distanceFallOff = 0f;

		public float maxFogLength = 1000f;

		[Range(0, 1f)]
		public float maxFogLengthFallOff = 0f;

		public bool baselineRelativeToCamera = false;

		[Range(0, 1f)]
		public float baselineRelativeToCameraDelay = 0;

		[Range(0.2f, 10f)]
		public float noiseScale = 1f;

		[Range(-0.3f, 2f)]
		public float noiseSparse = 0f;

		[Range(1f, 2f)]
		public float noiseFinalMultiplier = 1f;

		[Range(0, 1.05f)]
		public float alpha = 1f;

		public Color color = new Color(0.89f, 0.89f, 0.89f, 1);

        [Range(0, 1f)]
        public float deepObscurance = 1f;

        public Color specularColor = new Color(1, 1, 0.8f, 1);

		[Range(0, 1f)]
		public float specularThreshold = 0.6f;

		[Range(0, 1f)]
		public float specularIntensity = 0.2f;

		public Vector3 lightDirection = new Vector3(1, 0, -1);

		[Range(-1f, 3f)]
		public float lightIntensity = 0.2f;

		public Color lightColor = Color.white;

		[Range(0, 1f)]
		public float speed = 0.01f;

		public bool useRealTime;

		public Vector3 windDirection = new Vector3(-1, 0, 0);

		[Range(0, 10f)]
		public float turbulenceStrength = 0f;

		public bool useXYPlane = false;

		public Color skyColor = new Color(0.89f, 0.89f, 0.89f, 1);

		public float skyHaze = 50f;

        [Range(0, 1f)]
		public float skySpeed = 0.3f;

		[Range(0, 1f)]
		public float skyNoiseStrength = 0.1f;

        public float skyNoiseScale = 1.5f;

        [Range(0, 1f)]
		public float skyAlpha = 1f;

        [SerializeField, Range(0, 0.999f)]
        float skyDepth = 0.999f;

        public float stepping = 12f;

		public float steppingNear = 1f;

		public bool dithering = false;

		public float ditherStrength = 0.75f;

		public bool downsamplingOverride = false;

		[Range(1, 8)]
		public int downsampling = 1;

        public bool forceComposition = false;

		public bool edgeImprove = false;

		[Range(0.00001f, 0.005f)]
		public float edgeThreshold = 0.0005f;

		#region Light Scattering

		public bool lightScatteringOverride = false;
		public bool lightScatteringEnabled = false;
		[Range(0, 1f)] public float lightScatteringDiffusion = 0.7f;
		[Range(0, 1f)] public float lightScatteringSpread = 0.686f;
		[Range(4, 64)] public int lightScatteringSamples = 16;
		[Range(0, 50f)] public float lightScatteringWeight = 1.9f;
		[Range(0, 50f)] public float lightScatteringIllumination = 18f;
		[Range(0.9f, 1.1f)] public float lightScatteringDecay = 0.986f;
		[Range(0, 0.2f)] public float lightScatteringExposure = 0;
		public Color lightScatteringTint = new Color(1,1,1,0);
		[Range(0, 1f)] public float lightScatteringJittering = 0.5f;
        [Range(1,4)] public int lightScatteringBlurDownscale = 1;

		#endregion

		#region Fog Void settings

		public bool fogVoidOverride = false;
		public FOG_VOID_TOPOLOGY fogVoidTopology = FOG_VOID_TOPOLOGY.Sphere;
		[SerializeField, Range(0, 10f)] public float fogVoidFallOff = 1.0f;
		public float fogVoidRadius = 0.0f;
		public Vector3 fogVoidPosition = Vector3.zero;
		public float fogVoidDepth = 0.0f;
		public float fogVoidHeight = 0.0f;

		#endregion

		/// <summary>
		/// Applies profile settings
		/// </summary>
		/// <param name="fog">Fog.</param>
		public void Load(VolumetricFog fog) {
			// Fog Geo
			fog.density = density;
			fog.noiseStrength = noiseStrength;
			fog.height = height;
            fog.heightFallOff = heightFallOff;
			fog.baselineHeight = baselineHeight;
			fog.distance = distance;
			fog.distanceFallOff = distanceFallOff;
			fog.maxFogLength = maxFogLength;
			fog.maxFogLengthFallOff = maxFogLengthFallOff;
			fog.baselineRelativeToCamera = baselineRelativeToCamera;
			fog.baselineRelativeToCameraDelay = baselineRelativeToCameraDelay;
			fog.noiseScale = noiseScale;
			fog.noiseSparse = noiseSparse;
			fog.noiseFinalMultiplier = noiseFinalMultiplier;
			fog.useXYPlane = useXYPlane;

			// Fog Colors
			fog.lightingModel = lightingModel;
			fog.sunCopyColor = sunCopyColor;
			fog.alpha = alpha;
			fog.color = color;
            fog.deepObscurance = deepObscurance;
            fog.specularColor = specularColor;
			fog.specularThreshold = specularThreshold;
			fog.specularIntensity = specularIntensity;
			fog.lightDirection = lightDirection;
			fog.lightIntensity = lightIntensity;
			fog.lightColor = lightColor;

			// Fog animation
			fog.speed = speed;
			fog.windDirection = windDirection;
			fog.turbulenceStrength = turbulenceStrength;
			fog.useRealTime = useRealTime;

			// Fog sky
			fog.skyColor = skyColor;
			fog.skyHaze = skyHaze;
			fog.skySpeed = skySpeed;
			fog.skyNoiseStrength = skyNoiseStrength;
            fog.skyNoiseScale = skyNoiseScale;
			fog.skyAlpha = skyAlpha;
            fog.skyDepth = skyDepth;

            // Optimization
            fog.stepping = stepping;
			fog.steppingNear = steppingNear;
			fog.dithering = dithering;
			fog.ditherStrength = ditherStrength;

			if (downsamplingOverride) {
				fog.downsampling = downsampling;
                fog.forceComposition = forceComposition;
                fog.edgeImprove = edgeImprove;
				fog.edgeThreshold = edgeThreshold;
			}

			// Fog Void
			if (fogVoidOverride) {
				fog.fogVoidTopology = fogVoidTopology;
				fog.fogVoidDepth = fogVoidDepth;
				fog.fogVoidFallOff = fogVoidFallOff;
				fog.fogVoidHeight = fogVoidHeight;
				fog.fogVoidPosition = fogVoidPosition;
				fog.fogVoidRadius = fogVoidRadius;
			}

			// Light scattering
			if (lightScatteringOverride) {
				fog.lightScatteringEnabled = lightScatteringEnabled;
				fog.lightScatteringDecay = lightScatteringDecay;
				fog.lightScatteringDiffusion = lightScatteringDiffusion;
				fog.lightScatteringExposure = lightScatteringExposure;
				fog.lightScatteringIllumination = lightScatteringIllumination;
				fog.lightScatteringJittering = lightScatteringJittering;
                fog.lightScatteringBlurDownscale = lightScatteringBlurDownscale;
				fog.lightScatteringSamples = lightScatteringSamples;
				fog.lightScatteringTint = lightScatteringTint;
				fog.lightScatteringSpread = lightScatteringSpread;
				fog.lightScatteringWeight = lightScatteringWeight;
			}
		}


		/// <summary>
		/// Replaces profile settings with current fog configuration
		/// </summary>
		public void Save(VolumetricFog fog) {
			// Fog Geo
			density = fog.density;
			noiseStrength = fog.noiseStrength;
			height = fog.height;
            heightFallOff = fog.heightFallOff;
			baselineHeight = fog.baselineHeight;
			distance = fog.distance;
			distanceFallOff = fog.distanceFallOff;
			maxFogLength = fog.maxFogLength;
			maxFogLengthFallOff = fog.maxFogLengthFallOff;
			baselineRelativeToCamera = fog.baselineRelativeToCamera;
			baselineRelativeToCameraDelay = fog.baselineRelativeToCameraDelay;
			noiseScale = fog.noiseScale;
			noiseSparse = fog.noiseSparse;
			noiseFinalMultiplier = fog.noiseFinalMultiplier;
			useXYPlane = fog.useXYPlane;

			// Fog Colors
			sunCopyColor = fog.sunCopyColor;
			alpha = fog.alpha;
			color = fog.color;
            deepObscurance = fog.deepObscurance;
            specularColor = fog.specularColor;
			specularThreshold = fog.specularThreshold;
			specularIntensity = fog.specularIntensity;
			lightDirection = fog.lightDirection;
			lightIntensity = fog.lightIntensity;
			lightColor = fog.lightColor;
			lightingModel = fog.lightingModel;

			// Fog animation
			speed = fog.speed;
			windDirection = fog.windDirection;
			turbulenceStrength = fog.turbulenceStrength;
			useRealTime = fog.useRealTime;

			// Fog sky
			skyColor = fog.skyColor;
			skyHaze = fog.skyHaze;
			skySpeed = fog.skySpeed;
			skyNoiseStrength = fog.skyNoiseStrength;
            skyNoiseScale = fog.skyNoiseScale;
			skyAlpha = fog.skyAlpha;
            skyDepth = fog.skyDepth;

			// Optimization
			stepping = fog.stepping;
			steppingNear = fog.steppingNear;
			dithering = fog.dithering;
			ditherStrength = fog.ditherStrength;
			downsampling = fog.downsampling;
            forceComposition = fog.forceComposition;
            edgeImprove = fog.edgeImprove;
			edgeThreshold = fog.edgeThreshold;

			// Fog Void
			fogVoidTopology = fog.fogVoidTopology;
			fogVoidDepth = fog.fogVoidDepth;
			fogVoidFallOff = fog.fogVoidFallOff;
			fogVoidHeight = fog.fogVoidHeight;
			fogVoidPosition = fog.fogVoidPosition;
			fogVoidRadius = fog.fogVoidRadius;

			// Light scattering
			lightScatteringEnabled = fog.lightScatteringEnabled;
			lightScatteringDecay = fog.lightScatteringDecay;
			lightScatteringDiffusion = fog.lightScatteringDiffusion;
			lightScatteringExposure = fog.lightScatteringExposure;
			lightScatteringIllumination = fog.lightScatteringIllumination;
			lightScatteringJittering = fog.lightScatteringJittering;
			lightScatteringTint = fog.lightScatteringTint;
			lightScatteringSamples = fog.lightScatteringSamples;
			lightScatteringSpread = fog.lightScatteringSamples;
			lightScatteringSpread = fog.lightScatteringSpread;
			lightScatteringWeight = fog.lightScatteringWeight;
            lightScatteringBlurDownscale = fog.lightScatteringBlurDownscale;
		}

		/// <summary>
		/// Lerps between profile1 and profile2 using t as the transition amount (0..1) and assign the values to given fog
		/// </summary>
		public static void Lerp(VolumetricFogProfile profile1, VolumetricFogProfile profile2, float t, VolumetricFog fog) {
			
			if (t < 0) t = 0;
			else if (t > 1f) t = 1f;

			// Fog Geo
			fog.density = profile1.density * (1f - t) + profile2.density * t;
			fog.noiseStrength = profile1.noiseStrength * (1f - t) + profile2.noiseStrength * t;
			fog.height = profile1.height * (1f - t) + profile2.height * t;
            fog.heightFallOff = profile1.heightFallOff * (1f - t) + profile2.heightFallOff * t;
			fog.baselineHeight = profile1.baselineHeight * (1f - t) + profile2.baselineHeight * t;
            fog.distance = profile1.distance * (1f - t) + profile2.distance * t;
			fog.distanceFallOff = profile1.distanceFallOff * (1f - t) + profile2.distanceFallOff * t;
			fog.maxFogLength = profile1.maxFogLength * (1f - t) + profile2.maxFogLength * t;
			fog.maxFogLengthFallOff = profile1.maxFogLengthFallOff * (1f - t) + profile2.maxFogLengthFallOff * t;
			fog.baselineRelativeToCamera = t < 0.5f ? profile1.baselineRelativeToCamera : profile2.baselineRelativeToCamera;
			fog.baselineRelativeToCameraDelay = profile1.baselineRelativeToCameraDelay * (1f - t) + profile2.baselineRelativeToCameraDelay * t;
			fog.noiseScale = profile1.noiseScale * (1f - t) + profile2.noiseScale * t;
			fog.noiseSparse = profile1.noiseSparse * (1f - t) + profile2.noiseSparse * t;
			fog.noiseFinalMultiplier = profile1.noiseFinalMultiplier * (1f - t) + profile2.noiseFinalMultiplier * t;

			// Fog Colors
			fog.sunCopyColor = t < 0.5f ? profile1.sunCopyColor : profile2.sunCopyColor;
			fog.alpha = profile1.alpha * (1f - t) + profile2.alpha * t;
			fog.color = profile1.color * (1f - t) + profile2.color * t;
            fog.deepObscurance = profile1.deepObscurance * (1f - t) + profile2.deepObscurance * t;
            fog.specularColor = profile1.specularColor * (1f - t) + profile2.specularColor * t;
			fog.specularThreshold = profile1.specularThreshold * (1f - t) + profile2.specularThreshold * t;
			fog.specularIntensity = profile1.specularIntensity * (1f - t) + profile2.specularIntensity * t;
			fog.lightDirection = profile1.lightDirection * (1f - t) + profile2.lightDirection * t;
			fog.lightIntensity = profile1.lightIntensity * (1f - t) + profile2.lightIntensity * t;
			fog.lightColor = profile1.lightColor * (1f - t) + profile2.lightColor * t;

			// Fog animation
			fog.speed = profile1.speed * (1f - t) + profile2.speed * t;
			fog.windDirection = profile1.windDirection * (1f - t) + profile2.windDirection * t;
			fog.turbulenceStrength = profile1.turbulenceStrength * (1f - t) + profile2.turbulenceStrength * t;

			// Fog sky
			fog.skyColor = profile1.skyColor * (1f - t) + profile2.skyColor * t;
			fog.skyHaze = profile1.skyHaze * (1f - t) + profile2.skyHaze * t;
			fog.skySpeed = profile1.skySpeed * (1f - t) + profile2.skySpeed * t;
			fog.skyNoiseStrength = profile1.skyNoiseStrength * (1f - t) + profile2.skyNoiseStrength * t;
			fog.skyNoiseScale = profile1.skyNoiseScale * (1f - t) + profile2.skyNoiseScale * t;
			fog.skyAlpha = profile1.skyAlpha * (1f - t) + profile2.skyAlpha * t;
			fog.skyDepth = profile1.skyDepth * (1f - t) + profile2.skyDepth * t;
			
			// Optimization
			fog.stepping = profile1.stepping * (1f - t) + profile2.stepping * t;
			fog.steppingNear = profile1.steppingNear * (1f - t) + profile2.steppingNear * t;
			fog.dithering = t < 0.5f ? profile1.dithering : profile2.dithering;
			fog.ditherStrength = profile1.ditherStrength * (1f - t) + profile2.ditherStrength * t;

			// Fog Void
			if (profile1.fogVoidOverride && profile2.fogVoidOverride) {
				fog.fogVoidDepth = profile1.fogVoidDepth * (1f - t) + profile2.fogVoidDepth * t;
                fog.fogVoidFallOff = profile1.fogVoidFallOff * (1f - t) + profile2.fogVoidFallOff * t;
                fog.fogVoidHeight = profile1.fogVoidHeight * (1f - t) + profile2.fogVoidHeight * t;
                fog.fogVoidPosition = profile1.fogVoidPosition * (1f - t) + profile2.fogVoidPosition * t;
                fog.fogVoidRadius = profile1.fogVoidRadius * (1f - t) + profile2.fogVoidRadius * t;
			}

			// Light Scattering
			if (profile1.lightScatteringOverride && profile2.lightScatteringOverride) {
                fog.lightScatteringDecay = profile1.lightScatteringDecay * (1f - t) + profile2.lightScatteringDecay * t;
                fog.lightScatteringDiffusion = profile1.lightScatteringDiffusion * (1f - t) + profile2.lightScatteringDiffusion * t;
                fog.lightScatteringExposure = profile1.lightScatteringExposure * (1f - t) + profile2.lightScatteringExposure * t;
                fog.lightScatteringIllumination = profile1.lightScatteringIllumination * (1f - t) + profile2.lightScatteringIllumination * t;
				fog.lightScatteringTint = profile1.lightScatteringTint * (1f - t) + profile2.lightScatteringTint * t;
                fog.lightScatteringJittering = profile1.lightScatteringJittering * (1f - t) + profile2.lightScatteringJittering * t;
                fog.lightScatteringSamples = (int)(profile1.lightScatteringSamples * (1f - t) + profile2.lightScatteringSamples * t);
                fog.lightScatteringSpread = profile1.lightScatteringSpread * (1f - t) + profile2.lightScatteringSpread * t;
                fog.lightScatteringWeight = profile1.lightScatteringWeight * (1f - t) + profile2.lightScatteringWeight * t;
			}
		}


	}

}