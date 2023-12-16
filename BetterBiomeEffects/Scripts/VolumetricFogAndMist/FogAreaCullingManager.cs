using UnityEngine;

namespace VolumetricFogAndMist {

	[AddComponentMenu("")]
	public class FogAreaCullingManager : MonoBehaviour {

		public VolumetricFog fog;

		void OnEnable() {
			if (fog == null) {
				fog = GetComponent<VolumetricFog>();
				if (fog == null) {
					fog = gameObject.AddComponent<VolumetricFog>();
				}
			}
		}

		void OnBecameVisible() {
			if (fog != null)
				fog.enabled = true;
		}

		void OnBecameInvisible() {
			if (fog != null)
				fog.enabled = false;
		}
	
	}
}
