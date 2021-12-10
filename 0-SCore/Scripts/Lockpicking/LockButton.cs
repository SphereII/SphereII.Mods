using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class LockButton : MonoBehaviour
{
    [Header("Animation Trigger Strings")] public string pressDownTrigger = "ButtonPressDown";

    public string pressUpTrigger = "ButtonPressUp";

    [Header("Plumbing")] [SerializeField] private Animator animator;

    public LocksetAudio audioButtonPressed;
    public LocksetAudio audioButtonReset;

    private readonly UnityEvent buttonPressed = new UnityEvent();
    private readonly UnityEvent buttonReset = new UnityEvent();

    private bool _isUp = true;
    private int _pressDownTrigger;
    private int _pressUpTrigger;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        _pressDownTrigger = Animator.StringToHash(pressDownTrigger);
        _pressUpTrigger = Animator.StringToHash(pressUpTrigger);
    }

    private void Update()
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