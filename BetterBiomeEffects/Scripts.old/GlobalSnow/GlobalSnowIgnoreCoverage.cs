using System;
using System.Collections;
using UnityEngine;

namespace GlobalSnowEffect {

    [ExecuteInEditMode]
    public class GlobalSnowIgnoreCoverage : MonoBehaviour {

        [SerializeField]
        [Tooltip("If this gameobject or any of its children can receive snow.")]
        bool _receiveSnow;

        public bool receiveSnow {
            get { return _receiveSnow;  }
            set { if (_receiveSnow != value) { _receiveSnow = value; UpdateSettings(); } }
        }


        [SerializeField]
        [Tooltip("If this gameobject or any of its children block snow down.")]
        bool _blockSnow;
        public bool blockSnow {
            get { return _blockSnow; }
            set { _blockSnow = value; }
        }


        [SerializeField]
        [Tooltip("If enabled, Global Snow will use a fast mask shader to exclude snow from this object. If disabled, Global Snow will use the object material and shader, which can be a bit slower but more accurate. If shader uses displacement or vertex animation, disable this option.")]
        bool _useFastMaskShader = true;
        public bool useFastMaskShader {
            get { return _useFastMaskShader; }
            set { _useFastMaskShader = value; }
        }

        [SerializeField]
        [Tooltip("Exclusion alpha cut-off")]
        [Range(0, 1)]
        float _exclusionCutOff;
        public float exclusionCutOff {
            get { return _exclusionCutOff; }
            set { if (_exclusionCutOff != value) { _exclusionCutOff = value; UpdateSettings(); } }
        }

        [NonSerialized]
        public int layer;

        [NonSerialized]
        public Renderer[] renderers;

        [NonSerialized, HideInInspector]
        public int[] renderersLayers;

        GlobalSnow snow;

        void OnEnable() {
            renderers = GetComponentsInChildren<Renderer>(true);
            renderersLayers = new int[renderers.Length];
            snow = GlobalSnow.instance;
            if (snow != null) {
                snow.IgnoreGameObject(this);
            }
        }

        void OnValidate() {
            UpdateSettings();
        }

        void Start() {
            if (Application.isPlaying && snow == null) {
                snow = GlobalSnow.instance;
                if (snow != null) {
                    snow.IgnoreGameObject(this);
                } else {
                    StartCoroutine(DelayIgnoreObject());
                }
            }
        }

        IEnumerator DelayIgnoreObject() {
            WaitForEndOfFrame w = new WaitForEndOfFrame();
            while (snow == null) {
                snow = GlobalSnow.instance;
                yield return w;
            }
            snow.IgnoreGameObject(this);
        }


        void OnDisable() {
            if (snow != null) {
                snow.UseGameObject(this);
            }
        }


        void UpdateSettings() {
            if (snow != null) {
                snow.RefreshExcludedObjects();
            }
        }


    }
}