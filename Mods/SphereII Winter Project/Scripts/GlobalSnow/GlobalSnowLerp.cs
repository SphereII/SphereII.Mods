using System;
using UnityEngine;

namespace GlobalSnowEffect {



    [ExecuteInEditMode]
    public class GlobalSnowLerp : MonoBehaviour {

        [Tooltip("Assign target Global Snow component that will be affected by this volume.")]
        public GlobalSnow targetGlobalSnow;

        [Range(0,1)]
        public float transition;

        public KeyCode leftKey, rightKey;
        public float keySpeed = 1f;

        public GlobalSnowProfile profile1;
        public GlobalSnowProfile profile2;


        void OnEnable() {
            if (targetGlobalSnow == null) {
                targetGlobalSnow = GlobalSnow.instance;
            }
            if (targetGlobalSnow != null && profile1.snowAmount == 0 && profile1.minimumAltitude == 0 && profile1.groundCoverage == 0) {
                // grab default values from current configuration
                profile1.CopyFrom(targetGlobalSnow);
            }
        }

        private void OnValidate() {
            UpdateSettings();
        }

        public void UpdateSettings() {
            if (targetGlobalSnow == null) return;
            GlobalSnowProfile.Lerp(targetGlobalSnow, profile1, profile2, transition);
        }

        private void Update() {
            if (Input.GetKey(leftKey)) {
                transition -= keySpeed * Time.deltaTime;
                if (transition < 0) transition = 0;
                UpdateSettings();
            }
            if (Input.GetKey(rightKey)) {
                transition += keySpeed * Time.deltaTime;
                if (transition < 1) transition = 1f;
                UpdateSettings();
            }

        }

    }

}