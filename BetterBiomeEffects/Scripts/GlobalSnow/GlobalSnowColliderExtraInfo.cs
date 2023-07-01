using UnityEngine;
using System.Collections;

namespace GlobalSnowEffect {
    public class GlobalSnowColliderExtraInfo : MonoBehaviour {

        [Tooltip("Snow mark size for this collider.")]
        public float markSize = 0.25f;

		[Tooltip("Distance between collisions")]
        public float collisionDistanceThreshold = 0.1f;

		[Tooltip("If enabled, only a single stamp will be left on the trail, instead of a continuous line, when the character moves.")]
        public bool isFootprint;

		[Tooltip("Maximum distance for a trail segment. If set to 0, marks will be added only on collision points and no interpolation between old and new position will be computed.")]
        public float stepMaxDistance = 20f;

		[Tooltip("Rotation degrees to trigger a terrain mark.")]
        public float rotationThreshold = 3f;

        [Tooltip("Enable this option to ignore any collision on the snow caused by this collider")]
        public bool ignoreThisCollider;

        WheelCollider[] wheels;

        void Start() {
            wheels = GetComponentsInChildren<WheelCollider>();
        }

        void Update() {
            if (wheels == null) return;

            WheelHit hit;
            GlobalSnow snow = GlobalSnow.instance;
            if (snow == null) return;

            for (int k = 0; k < wheels.Length; k++) {
                WheelCollider wc = wheels[k];
                if (wc == null)
                    continue;
                if (wc.GetGroundHit(out hit)) {
                    snow.CollisionStay(wc.gameObject, null, hit.point);
                } else {
                    snow.CollisionStop(wc.gameObject);
                }
            }
        }
    }
}

