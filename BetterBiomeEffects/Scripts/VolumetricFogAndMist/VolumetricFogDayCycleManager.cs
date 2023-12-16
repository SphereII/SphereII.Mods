using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumetricFogAndMist {

    [ExecuteInEditMode]
    public class VolumetricFogDayCycleManager : MonoBehaviour {

        [Range(0, 24)]
        public float currentTime;

#if ENVIRO_HD || ENVIRO_LW
        public bool timeDrivenByEnviro = true;
#endif

        public Gradient colorOverTime;
        public AnimationCurve densityOverTime;

        int prevTime;
        VolumetricFog[] fogs;

        private void OnEnable() {
            fogs = FindObjectsOfType<VolumetricFog>();
            if (colorOverTime == null) {
                colorOverTime = new Gradient();
                GradientColorKey[] keys = new GradientColorKey[2];
                keys[0].color = Color.white;
                keys[0].time = 0;
                keys[1].color = Color.white;
                keys[1].time = 1f;
                colorOverTime.colorKeys = keys;
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0].alpha = 1f;
                alphaKeys[0].time = 0;
                alphaKeys[1].alpha = 1f;
                alphaKeys[1].time = 1f;
                colorOverTime.alphaKeys = alphaKeys;
            }
            if (densityOverTime == null) {
                densityOverTime = new AnimationCurve();
                densityOverTime.AddKey(0, 1f);
                densityOverTime.AddKey(24, 1f);
            }
        }

        void Update() {
            currentTime = GetCurrentTime();
            int iTime = (int)(currentTime * 60);
            if (iTime == prevTime && Application.isPlaying) return;
            prevTime = iTime;

            Color color = colorOverTime.Evaluate(currentTime / 24f);
            float density = densityOverTime.Evaluate(currentTime);

            for (int k = 0; k < fogs.Length; k++) {
                bool changes = false;
                if (fogs[k] == null) continue;
                if (fogs[k].temporaryProperties.color != color) {
                    fogs[k].color = color;
                    changes = true;
                }
                if (fogs[k].temporaryProperties.density != density) {
                    fogs[k].density = density;
                    changes = true;
                }
                if (changes) {
                    fogs[k].UpdateMaterialProperties();
                }
            }
        }



        float GetCurrentTime() {
#if ENVIRO_HD || ENVIRO_LW
            if (timeDrivenByEnviro) {
                return EnviroSkyMgr.instance.GetUniversalTimeOfDay();
            } else {
                return currentTime;
            }
#else
            return currentTime;
#endif


        }
    }

}