//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;



namespace VolumetricFogAndMist {

    /// <summary>
    /// Important: do not change these values directly as it's managed by inspector editor. This value needs to match the value in VolumetricFogOptions.cginc file.
    /// </summary>
    public partial class VolumetricFog : MonoBehaviour {
        
public const int MAX_POINT_LIGHTS = 6;

public const bool LIGHT_SCATTERING_BLUR_ENABLED = false;

public const bool USE_UNITY_SHADOW_MAP = true;

public const bool USE_DIRECTIONAL_LIGHT_COOKIE = false;

public const bool LIGHT_DIFFUSION_ENABLED = true;



    }

}
