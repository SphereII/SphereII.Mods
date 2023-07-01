using System;
using UnityEngine;

namespace GlobalSnowEffect {

    [RequireComponent(typeof(SphereCollider))]
    public class GlobalSnowCollisionDetectorFootPrints : MonoBehaviour 
    {
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

        void Start()
        {
            isFootprint = true;
            Debug.Log("Loaded Footprint script.");
            // var collider = GetComponent<SphereCollider>();
            // if ( collider == null) Debug.Log("No Collider");
            // else
            // {
            //     Debug.Log("Collider Found.");
            // }
        }
        
        
        void OnCollisionStay(Collision collision) {
            GlobalSnow snow = GlobalSnow.instance;
            if (snow != null) {
                snow.CollisionStay(collision.gameObject, collision, Vector3.zero);
            }
        }

        private void OnCollisionExit(Collision collision) {
            GlobalSnow snow = GlobalSnow.instance;
            if (snow != null) {
                snow.CollisionStop(collision.gameObject);
            }
        }
       
       
    }

}