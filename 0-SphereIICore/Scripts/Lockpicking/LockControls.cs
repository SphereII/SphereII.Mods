using Lockpicking;
using UnityEngine;

public class LockControls : MonoBehaviour
{
    public Keyhole lockpick;

    void Update()
    {
        if (!lockpick)
            return;

        if (lockpick.gameObject.activeSelf)
        {
            InputControls();
        }
     
    }

    private void InputControls()
    {
        ResetValues();

        if (Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                lockpick.openPressure = 0.4f;
            else
                lockpick.openPressure = 1f;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            lockpick.lockpickPressure = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            lockpick.lockpickPressure = 1f;
        }
    }

    private void ResetValues()
    {
        lockpick.openPressure = 0f;
        lockpick.lockpickPressure = 0f;
    }
}
