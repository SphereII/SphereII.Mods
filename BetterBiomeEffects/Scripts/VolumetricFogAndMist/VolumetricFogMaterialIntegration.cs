using UnityEngine;

namespace VolumetricFogAndMist {

    [ExecuteAlways, RequireComponent(typeof(VolumetricFog))]
    public class VolumetricFogMaterialIntegration : MonoBehaviour {

        enum PropertyType {
            Float,
            Vector,
            Color,
            Texture2D,
            FloatArray,
            Float4Array,
            ColorArray,
            Matrix4x4
        }

        struct Properties {
            public int id;
            public PropertyType type;
        }

        static readonly Properties[] props =  {
            new Properties { id = Shader.PropertyToID("_NoiseTex"), type = PropertyType.Texture2D },
            new Properties { id = Shader.PropertyToID("_BlueNoise"), type = PropertyType.Texture2D },
            new Properties { id = Shader.PropertyToID("_FogAlpha"), type = PropertyType.Float },
            new Properties { id = Shader.PropertyToID("_FogColor"), type = PropertyType.Color },
            new Properties { id = Shader.PropertyToID("_FogDistance"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogData"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_DeepObscurance"), type = PropertyType.Float },
            new Properties { id = Shader.PropertyToID("_FogWindDir"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogStepping"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_BlurTex"), type = PropertyType.Texture2D },
            new Properties { id = Shader.PropertyToID("_FogVoidPosition"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogVoidData"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogAreaPosition"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogAreaData"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogOfWar"), type = PropertyType.Texture2D },
            new Properties { id = Shader.PropertyToID("_FogOfWarCenter"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogOfWarSize"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogOfWarCenterAdjusted"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogPointLightPosition"), type = PropertyType.Float4Array },
            new Properties { id = Shader.PropertyToID("_FogPointLightColor"), type = PropertyType.ColorArray },
            new Properties { id = Shader.PropertyToID("_SunPosition"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_SunDir"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_SunColor"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogScatteringData"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogScatteringData2"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_FogScatteringTint"), type = PropertyType.Color },
            new Properties { id = Shader.PropertyToID("_VolumetricFogSunDepthTexture"), type = PropertyType.Texture2D },
            new Properties { id = Shader.PropertyToID("_VolumetricFogSunDepthTexture_TexelSize"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_VolumetricFogSunProj"), type = PropertyType.Matrix4x4 },
            new Properties { id = Shader.PropertyToID("_VolumetricFogSunWorldPos"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_VolumetricFogSunShadowsData"), type = PropertyType.Vector },
            new Properties { id = Shader.PropertyToID("_Jitter"), type = PropertyType.Float },
            new Properties { id = Shader.PropertyToID("_ClipDir"), type = PropertyType.Vector }
        };

        static readonly string[] keywords = {
            "FOG_DISTANCE_ON", "FOG_AREA_SPHERE", "FOG_AREA_BOX", "FOG_VOID_SPHERE", "FOG_VOID_BOX", "FOG_OF_WAR_ON", "FOG_SCATTERING_ON", "FOG_BLUR_ON", "FOG_POINT_LIGHTS", "FOG_SUN_SHADOWS_ON"
        };

        [Tooltip("The fog renderer")]
        public VolumetricFog fog;

        [Tooltip("Assign at least one renderer in the scene using a material you wish to add the fog effect")]
        public Renderer[] materials;

        void OnEnable() {
            fog = GetComponent<VolumetricFog>();
        }


        void OnPreRender() {
            if (fog == null) return;
            Material fogMat = fog.fogMat;
            if (fogMat == null || materials == null || materials.Length == 0) return;
    

            // sync uniforms
            for (int k = 0; k < props.Length; k++) {
                if (!fogMat.HasProperty(props[k].id)) continue;
                switch (props[k].type) {
                    case PropertyType.Color:
                        Color color = fogMat.GetColor(props[k].id);
                        for (int m = 0; m < materials.Length; m++) {
                            if (materials[m] != null && materials[m].sharedMaterial != null)
                                materials[m].sharedMaterial.SetColor(props[k].id, color);
                        }
                        break;
                    case PropertyType.ColorArray:
                        Color[] colors = fogMat.GetColorArray(props[k].id);
                        if (colors != null) {
                            for (int m = 0; m < materials.Length; m++) {
                                if (materials[m] != null && materials[m].sharedMaterial != null)
                                    materials[m].sharedMaterial.SetColorArray(props[k].id, colors);
                            }
                        }
                        break;
                    case PropertyType.FloatArray:
                        float[] floats = fogMat.GetFloatArray(props[k].id);
                        if (floats != null) {
                            for (int m = 0; m < materials.Length; m++) {
                                if (materials[m] != null && materials[m].sharedMaterial != null)
                                    materials[m].sharedMaterial.SetFloatArray(props[k].id, floats);
                            }
                        }
                        break;
                    case PropertyType.Float4Array:
                        Vector4[] vectors = fogMat.GetVectorArray(props[k].id);
                        if (vectors != null) {
                            for (int m = 0; m < materials.Length; m++) {
                                if (materials[m] != null && materials[m].sharedMaterial != null)
                                    materials[m].sharedMaterial.SetVectorArray(props[k].id, vectors);
                            }
                        }
                        break;
                    case PropertyType.Float:
                        float f = fogMat.GetFloat(props[k].id);
                        for (int m = 0; m < materials.Length; m++) {
                            if (materials[m] != null && materials[m].sharedMaterial != null)
                                materials[m].sharedMaterial.SetFloat(props[k].id, f);
                        }
                        break;
                    case PropertyType.Vector:
                        Vector4 v = fogMat.GetVector(props[k].id);
                        for (int m = 0; m < materials.Length; m++) {
                            if (materials[m] != null && materials[m].sharedMaterial != null)
                                materials[m].sharedMaterial.SetVector(props[k].id, v);
                        }
                        break;
                    case PropertyType.Matrix4x4:
                        Matrix4x4 matrix = fogMat.GetMatrix(props[k].id);
                        for (int m = 0; m < materials.Length; m++) {
                            if (materials[m] != null && materials[m].sharedMaterial != null)
                                materials[m].sharedMaterial.SetMatrix(props[k].id, matrix);
                        }
                        break;
                    case PropertyType.Texture2D:
                        Texture tex = fogMat.GetTexture(props[k].id);
                        for (int m = 0; m < materials.Length; m++) {
                            if (materials[m] != null && materials[m].sharedMaterial != null)
                                materials[m].sharedMaterial.SetTexture(props[k].id, tex);
                        }
                        break;
                }
            }
             
            // sync shader keywords
            for (int k = 0; k < keywords.Length; k++) {
                if (fogMat.IsKeywordEnabled(keywords[k])) {
                    for (int m = 0; m < materials.Length; m++) {
                        if (materials[m] != null && materials[m].sharedMaterial != null)
                            materials[m].sharedMaterial.EnableKeyword(keywords[k]);
                    }
                } else {
                    for (int m = 0; m < materials.Length; m++) {
                        if (materials[m] != null && materials[m].sharedMaterial != null)
                            materials[m].sharedMaterial.DisableKeyword(keywords[k]);
                    }
                }

            }

        }
    }

}