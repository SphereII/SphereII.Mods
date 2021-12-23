using UnityEngine;
public class LeaderCollision : MonoBehaviour
{
    private void OnCollisionExit(Collision other)
    {
        Debug.Log("Turning collider back on.");
        enabled = true;
    }
}
