using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using Lockpicking;
using Platform;
using UnityEngine;

public class LockControls : MonoBehaviour {
    public Keyhole lockpick;
    PlayerActionsLocal playerActions;

    private static readonly string AdvFeatureClass = "AdvancedLockpicking";

    private List<KeyCode> _leftKeys;
    private List<KeyCode> _rightKeys;
    private List<KeyCode> _turnKeys;

    private Vector2 _centerScreen;
    private void Start() {
        var leftKey = Configuration.GetPropertyValue(AdvFeatureClass, "Left");
        _leftKeys = ParseKeys(leftKey);

        var rightKey = Configuration.GetPropertyValue(AdvFeatureClass, "Right");
        _rightKeys = ParseKeys(rightKey);

        var turnKey = Configuration.GetPropertyValue(AdvFeatureClass, "Turn");
        _turnKeys = ParseKeys(turnKey);

        _centerScreen = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    private static List<KeyCode> ParseKeys(string keyStrings) {
        var keys = new List<KeyCode>();
        foreach (var keyString in keyStrings.Split(','))
        {
            if (!Enum.TryParse(keyString, out KeyCode key)) continue;
            if (keys.Contains(key)) continue;
            keys.Add(key);
        }

        return keys;
    }

    private void Update() {
        if (!lockpick)
            return;

        
        if (playerActions == null)
        {
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            playerActions = player.PlayerUI.playerInput;
        }

        if (lockpick.gameObject.activeSelf) InputControls();
    }

    private bool Left() {
        if (Input.GetKey(KeyCode.A)) return true;
        if (Input.GetKey(KeyCode.LeftArrow)) return true;
        if (Input.mouseScrollDelta.y < 0) return true;
        if (Input.GetAxis("Horizontal") < 0) return true;
        if (Input.GetAxis("Mouse X") < 0)
        {
            var mousePosition = Input.mousePosition;
            return mousePosition.x < _centerScreen.x;
        } 
        if (playerActions?.LookLeft) return true;
        if (playerActions?.MoveLeft) return true;

        return _leftKeys.Any(Input.GetKey);
    }

    private bool Right() {
        if (Input.GetKey(KeyCode.D)) return true;
        if (Input.GetKey(KeyCode.RightArrow)) return true;
        if (Input.mouseScrollDelta.y > 0) return true;
        if (Input.GetAxis("Horizontal") > 0) return true;
        if (Input.GetAxis("Mouse X") > 0)
        {
            var mousePosition = Input.mousePosition;
            return mousePosition.x > _centerScreen.x;
        } 
        if (playerActions?.LookRight) return true;
        if (playerActions?.MoveRight) return true;
        return _rightKeys.Any(Input.GetKey);
    }

    private bool Turn() {
        if (Input.GetKey(KeyCode.Space)) return true;
        if (Input.GetAxis("Vertical") != 0) return true;
        if (Input.GetKey(KeyCode.Joystick1Button0)) return true;
        if (Input.GetKey(KeyCode.Joystick1Button1)) return true;
        if (Input.GetMouseButtonDown(0)) return true;
        
        return _turnKeys.Any(Input.GetKey);
    }

    private void TurnLock() {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            lockpick.openPressure = 0.4f;
        else
            lockpick.openPressure = 1f;
    }

    private void InputControls() {
        ResetValues();

        if (Turn() ) 
            TurnLock();

        if (playerActions == null) return;

        if (Left())
            lockpick.lockpickPressure = -1f;

        if (Right())
            lockpick.lockpickPressure = 1f;
    }

    private void ResetValues() {
        lockpick.openPressure = 0f;
        lockpick.lockpickPressure = 0f;
    }
}