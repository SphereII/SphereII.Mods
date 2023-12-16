using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumetricFogAndMist {

    [ExecuteInEditMode]
    public class FogOfWarHole : MonoBehaviour {

        public enum HoleShape {
            Disc,
            Box
        }

        public VolumetricFog fog;
        public HoleShape shape;
		[Range(0, 1f), Tooltip("The transparency of the fog")]
        public float alpha;
		[Range(0, 1f), Tooltip("The smoothness/harshness of the hole's border")]
        public float smoothness = 0.85f;

        HoleShape lastShape;
        Vector3 lastPosition = Vector3.zero;
        Vector3 lastScale;


        void Start() {
            if (fog == null) fog = VolumetricFog.instance;
            StampHole(transform.position, shape, transform.localScale.x, transform.localScale.z);
        }


        void RestoreHole(Vector3 position, HoleShape shape, float sizeX, float sizeZ) {
            if (fog == null) {
                fog = VolumetricFog.instance;
                if (fog == null) return;
            }

			fog.fogOfWarEnabled = true;
            switch (shape) {
                case HoleShape.Box:
                    fog.ResetFogOfWarAlpha(position, sizeX * 0.5f, sizeZ * 0.5f);
                    break;
                case HoleShape.Disc:
                    fog.ResetFogOfWarAlpha(position, Mathf.Max(sizeX, sizeZ) * 0.5f);
                    break;
            }
        }


        void StampHole(Vector3 position, HoleShape shape, float sizeX, float sizeZ) {
            if (fog == null) {
                fog = VolumetricFog.instance;
                if (fog == null) return;
            }

            fog.fogOfWarEnabled = true;
            switch (shape) {
                case HoleShape.Box:
                    fog.SetFogOfWarAlpha(new Bounds(position, new Vector3(sizeX, 0, sizeZ)), alpha, false, 0, smoothness, 0, 0);
                    break;
                case HoleShape.Disc:
                    fog.SetFogOfWarAlpha(position, Mathf.Max(sizeX, sizeZ) * 0.5f, alpha, false, 0, smoothness, 0, 0);
                    break;
            }
            lastPosition = position;
            lastShape = shape;
            lastScale = transform.localScale;
        }

        public void Refresh() {
            RestoreHole(lastPosition, lastShape, lastScale.x, lastScale.z);
            StampHole(transform.position, shape, transform.localScale.x, transform.localScale.z);
        }


#if UNITY_EDITOR

        void OnEnable() {
            if (!Application.isPlaying) {
                StampHole(transform.position, shape, transform.localScale.x, transform.localScale.z);
            }
        }

        void OnDisable() {
            RestoreHole(lastPosition, lastShape, lastScale.x, lastScale.z);
        }

        void OnValidate() {
            if (fog == null) {
                fog = VolumetricFog.instance;
                if (fog == null) return;
            }
            RestoreHole(lastPosition, lastShape, lastScale.x, lastScale.z);
            StampHole(transform.position, shape, transform.localScale.x, transform.localScale.z);
        }

        void LateUpdate() {
            if (!Application.isPlaying) {
                if (lastPosition != transform.position || lastScale != transform.localScale) {
                    Refresh();
                }
            }
        }

        void OnDrawGizmos() {
            Gizmos.color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            switch (shape) {
                case HoleShape.Disc:
                    Gizmos.DrawWireSphere(transform.position, Mathf.Max(transform.localScale.x, transform.localScale.z) * 0.5f);
                    break;
                case HoleShape.Box:
                    Gizmos.DrawWireCube(transform.position, new Vector3(transform.localScale.x, 0.1f, transform.localScale.z));
                    break;
            }
        }
#endif


    }

}