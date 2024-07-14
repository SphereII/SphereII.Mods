using UnityEngine;

namespace GlobalSnowEffect {
    public class GlobalSnowCollisionDetector : MonoBehaviour {

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