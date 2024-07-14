using UnityEngine;

namespace GlobalSnowEffect {

    public class GlobalSnowVolume : MonoBehaviour {

        [Tooltip("Set collider that will trigger this snow volume. If not set, this snow volume will react to any collider which has the main camera. If you use a third person controller, assign the character collider here.")]
        public Collider targetCollider;

        [Tooltip("When enabled, a console message will be printed whenever this fog volume is entered or exited.")]
        public bool debugMode;

        [Tooltip("Assign target Global Snow component that will be affected by this volume.")]
        public GlobalSnow targetGlobalSnow;


        bool cameraInside;

        void Start() {
            if (targetGlobalSnow == null)
                targetGlobalSnow = GlobalSnow.instance;
        }

        void OnTriggerEnter(Collider other) {
            if (cameraInside || targetGlobalSnow == null)
                return;
            // Check if other collider has the main camera attached
            if (other == targetCollider || other.gameObject.transform.GetComponentInChildren<Camera>() == targetGlobalSnow.snowCamera) {
                cameraInside = true;
                targetGlobalSnow.enabled = false;
                if (debugMode) {
                    Debug.Log("Global Snow Volume entered by " + other.name);
                }
            }
        }

        void OnTriggerExit(Collider other) {
            if (!cameraInside || targetGlobalSnow == null)
                return;
            if (other == targetCollider || other.gameObject.transform.GetComponentInChildren<Camera>() == targetGlobalSnow.snowCamera) {
                cameraInside = false;
                targetGlobalSnow.enabled = true;
                if (debugMode) {
                    Debug.Log("Global Snow Volume exited by " + other.name);
                }
            }
        }

    }

}