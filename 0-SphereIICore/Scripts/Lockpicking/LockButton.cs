using UnityEngine;
using UnityEngine.Events;

public class LockButton : MonoBehaviour
{
    
    [Header("Animation Trigger Strings")]
    public string pressDownTrigger = "ButtonPressDown";
    public string pressUpTrigger = "ButtonPressUp";

    [Header("Plumbing")]
    [SerializeField] private Animator animator = null;
    public LocksetAudio audioButtonPressed;
    public LocksetAudio audioButtonReset;
    
    private bool _isUp = true;
    private int _pressDownTrigger;
    private int _pressUpTrigger;
    
    UnityEvent buttonPressed = new UnityEvent();
    UnityEvent buttonReset = new UnityEvent();

    void Awake()
    {
        _pressDownTrigger = Animator.StringToHash(pressDownTrigger);
        _pressUpTrigger = Animator.StringToHash(pressUpTrigger);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ToggleButton();
    }

    public void ToggleButton()
    {
        if (_isUp)
        {
            animator.SetTrigger(_pressDownTrigger);
            buttonPressed.Invoke();
            audioButtonPressed.PlayOnce();
        }
        else
        {
            animator.SetTrigger(_pressUpTrigger);
            buttonReset.Invoke();
            audioButtonReset.PlayOnce();
        }

        _isUp = !_isUp;
    }

    public void ToggleButton(bool up)
    {
        if (up && !_isUp || !up && _isUp)
            ToggleButton();
    }
}

