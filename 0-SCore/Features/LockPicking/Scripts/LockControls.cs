using Lockpicking;
using Platform;
using UnityEngine;

public class LockControls : MonoBehaviour
{
    public Keyhole lockpick;
    PlayerActionsLocal playerActions;

    private void Update()
    {
        if (!lockpick)
            return;

        if (playerActions == null)
        {
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player != null)
                playerActions = player.PlayerUI.playerInput;
           
        } 
        if (lockpick.gameObject.activeSelf) InputControls();
    }

    private bool Left()
    {
        if (Input.GetKey(KeyCode.JoystickButton4))  return true;
        if (Input.GetKey(KeyCode.A)) return true;
        if (Input.GetKey(KeyCode.LeftArrow)) return true;
        if (Input.mouseScrollDelta.y < 0) return true;
        return false;
    }

    private bool Right()
    {
        if (Input.GetKey(KeyCode.JoystickButton5)) return true;
        if (Input.GetKey(KeyCode.D)) return true;
        if (Input.GetKey(KeyCode.RightArrow)) return true;
        if (Input.mouseScrollDelta.y > 0) return true;
        return false;

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

        if (playerActions == null) return;

        if (Left())
            lockpick.lockpickPressure = -1f;

        if (Right())
            lockpick.lockpickPressure = 1f;

        //if (Input.GetKey(KeyCode.LeftArrow))
        //else if (Input.GetKey(KeyCode.RightArrow)) lockpick.lockpickPressure = 1f;
    }

    private void ResetValues()
    {
        lockpick.openPressure = 0f;
        lockpick.lockpickPressure = 0f;
    }
}