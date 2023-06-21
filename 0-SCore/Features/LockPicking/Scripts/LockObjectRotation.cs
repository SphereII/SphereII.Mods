using UnityEngine;

public class LockObjectRotation : MonoBehaviour
{
    public bool mouseMovementOn;
    [SerializeField] private float maxX = 30f; // Max rotation
    [SerializeField] private float maxY = 30f; // Max rotation

    [SerializeField] private Vector3 mousePosition; // last position of mouse
    [SerializeField] private float speed = 2160f; // speed of rotation
    [SerializeField] private float startX; // Starting rotation
    [SerializeField] private float startY; // Starting rotation
    public Camera uiCam;


    private void Start()
    {
        startX = GetAngle(transform.localEulerAngles.x);
        startY = GetAngle(transform.localEulerAngles.y);
    }

    private void Update()
    {
        MouseMovement();
        mousePosition = uiCam.ScreenToViewportPoint(Input.mousePosition); // Save mouse position every frame.
        //mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition); // Save mouse position every frame.
    }

    private void MouseMovement()
    {
        // If we are holding down the left mouse button...
        if (Input.GetMouseButton(0) || mouseMovementOn)
        {
            // Grab the new position of the mouse, 0,0 - 1,1
            var newPosition = uiCam.ScreenToViewportPoint(Input.mousePosition);
            //Vector3 newPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            // Compute the delta in the x and y positions.  Do inversion for the yDelta. Note X/Y are flipped.
            var horDelta = -(newPosition.x - mousePosition.x) * speed * Time.deltaTime;
            var verDelta = (newPosition.y - mousePosition.y) * speed * Time.deltaTime;

            // Make sure we don't go over our max range
            if (GetAngle(transform.localEulerAngles.x) + verDelta > startX + maxX || GetAngle(transform.localEulerAngles.x) + verDelta < startX - maxX)
                verDelta = 0;
            if (GetAngle(transform.localEulerAngles.y) + horDelta > startY + maxY || GetAngle(transform.localEulerAngles.y) + horDelta < startY - maxY)
                horDelta = 0;

            // Compute & assign the new angle
            var newAngles = new Vector3(transform.localEulerAngles.x + verDelta,
                transform.localEulerAngles.y + horDelta, transform.localEulerAngles.z);
            transform.localEulerAngles = newAngles;
        }
    }

    private float GetAngle(float eulerAngle)
    {
        var angle = eulerAngle;
        angle %= 360;
        if (angle > 180)
            angle -= 360;
        return angle;
    }
}