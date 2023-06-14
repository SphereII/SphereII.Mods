using UnityEngine;
using UnityEngine.Events;

/* 
 * Thank you for using my Lockpicking asset! I hope it works great in your game. You can extend it to suit your game by
 * manipulating some of the code below. For instance, if your player can have a various level of "lockpicking" skill,
 * you may consider multiplying the value of lockGive by their skill, so that a higher skilled player would find it
 * easier to open a lock.
 *
 * Enjoy!
 */

namespace Lockpicking
{
    [RequireComponent(typeof(LockEmissive))]
    public class Keyhole : MonoBehaviour
    {
        // 7 Days To Die stuff
        public int NumLockPicks;
        public EntityPlayer player;
        public BlockValue blockValue;

        [Header("Plumbing")] public GameObject keyhole; // The keyhole with lockpick A that turns the entire keyhole object to open it

        public GameObject lockpickObject; // The lockpick that turns to match the secret lockAngle
        public Animator lockpickAnimator; // Animator on the lockpick in the lockpickObject
        public Animator _padlockAnimator;
        public GameObject padlock1; // Link to the padlock 1 game object
        public GameObject button;
        public LocksetAudio audioTurnClick;
        public LocksetAudio audioSqueek;
        public LocksetAudio audioOpen;
        public LocksetAudio audioJiggle;
        public LocksetAudio audioJiggle2;
        public LocksetAudio audioJiggle3;
        public LocksetAudio audioPadlockOpen;
        public LocksetAudio audioPadlockJiggle;
        public LocksetAudio audioLockpickBreak;
        public LocksetAudio audioLockpickEnter;
        public LocksetAudio audioLockpickClick;

        // Audio Settings
        [Range(0f, 1f)] public float clickVolumeMin = 0.1f;
        [Range(0f, 1f)] public float clickVolumeMax = 0.4f;
        [Range(0, 100)] public int clickChance = 100;
        [Range(0f, 15f)] public float clickRate = 10f;
        [Range(0f, 1f)] public float squeekVolumeMin = 0.1f;
        [Range(0f, 1f)] public float squeekVolumeMax = 0.4f;
        [Range(0, 100)] public int squeekChance = 50;
        [Range(0f, 360f)] public float squeekRate = 20f;
        public bool buttonDown;
        private LockEmissive _lockEmissive; // Link to the lockEmissive script on this object
        private float _lockpickAnglePrev;

        // Private variables
        private float breakCounter; // Counter for taking a break after a broken lockpick
        private float breakTimeCounter;
        private bool isShaking; // Whether we are currently shaking or not
        private Vector3 preshakeKeyhole; // Saves the pre-shake angles
        private Vector3 preshakeLockpick; // Saves the pre-shake angles
        private float shakeTimer; // Counter for the shake Time
        private float squeekTimer;

        private void Awake()
        {
            squeekTimer = squeekRate;

            _pickAnglesDefault = LockpickAngles();

            _lockEmissive = gameObject.GetComponent<LockEmissive>();
            if (_lockEmissive == null)
                gameObject.AddComponent<LockEmissive>();

            if (padlock1 != null)
            {
                _padlockAnimator = padlock1.gameObject.GetComponent<Animator>();
                if (_padlockAnimator == null)
                    _padlockAnimator = padlock1.gameObject.AddComponent<Animator>();
            }

            _closeTrigger = Animator.StringToHash(closeTrigger);
            _openTrigger = Animator.StringToHash(openTrigger);
            _lockpickBreakTrigger = Animator.StringToHash(lockPickBreakTrigger);
            _lockpickInsertTrigger = Animator.StringToHash(lockPickInsertTrigger);

            if (resetOnAwake)
                ResetLock();
        }

        private void Update()
        {
            if (NumLockPicks > 0)
            {
                PassValuesToEmissiveScript();

                if (BreakingForAnimation())
                    return;
                HandlePlayerInput();
            }
            else
            {
                RefreshLockPicks();
            }
        }


        private void OnEnable()
        {
            RefreshLockPicks();
            ResetLock();
            if (NumLockPicks > 0)
                UpdateLockPicks(true);
            else
                UpdateLockPicks(false);
        }

        private void OnDisable()
        {
            NumLockPicks = 0;
        }

        private void OnValidate()
        {
            LockAngle = LockAngle;
            LockGive = LockGive;
            CloseDistance = CloseDistance;

            _pickAngleMin = Mathf.Clamp(_pickAngleMin, minLockAngle, maxLockAngle);
            _pickAngleMax = Mathf.Clamp(_pickAngleMax, minLockAngle, maxLockAngle);

            minGiveAmount = Mathf.Clamp(minGiveAmount, 1f, 360f);
            maxGiveAmount = Mathf.Clamp(maxGiveAmount, 1f, 360f);
            minCloseDistance = Mathf.Clamp(minCloseDistance, 5f, 360f);
            maxCloseDistance = Mathf.Clamp(maxCloseDistance, 5f, 360f);
        }

        public float BreakTimeCounter()
        {
            return breakTimeCounter;
        }

        public float LockPickAngle()
        {
            return GetAngle(LockpickAngles().z);
        }

        public float KeyholeAngle()
        {
            return GetAngle(KeyholeAngles().z);
        }

        public void EditorOnValidate()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            OnValidate();
        }

        private void UpdateLockPicks(bool enable)
        {
            // Hide the lock picks or show them.
            keyhole.transform.FindInChilds("LockpickB (Turnable)").gameObject.SetActive(enable);
            keyhole.transform.FindInChilds("LockpickA").gameObject.SetActive(enable);
        }


        private void RefreshLockPicks()
        {
            if (player == null)
                return;

            var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
            var playerInventory = uiforPlayer.xui.PlayerInventory;
            var item = ItemClass.GetItem("resourceLockPick");
            if (item != null)
                NumLockPicks = playerInventory.GetItemCount(item);

            if (NumLockPicks > 0)
                UpdateLockPicks(true);
            else
                UpdateLockPicks(false);
        }

        private void HandlePlayerInput()
        {
            if (_lockIsOpen) return;
            if (openPressure > 0)
            {
                TryToTurnKeyhole();
            }
            else
            {
                StopShaking();
                ReturnKeyholeToDefaultPosition();
                TurnLockpick(turnSpeedLockpick * lockpickPressure);
            }
        }

        private bool BreakingForAnimation()
        {
            if (breakCounter > 0f)
            {
                breakCounter -= Time.deltaTime;
                ReturnKeyholeToDefaultPosition();
                return true;
            }

            return false;
        }

        private void ReturnKeyholeToDefaultPosition()
        {
            TurnKeyhole(-returnSpeedKeyhole);
        }

        private void TryToTurnKeyhole()
        {
            if (LockCanTurn())
            {
                TurnKeyhole(turnSpeedKeyhole * openPressure);

                if (KeyholeTurnValue() <= 0 && LockpickIsInPosition())
                    OpenLock();
            }
            else
            {
                Shake();
            }
        }

        private void PassValuesToEmissiveScript()
        {
            _lockEmissive.breakpointValue = Mathf.Clamp(breakTimeCounter / breakTime, 0, 1);
            _lockEmissive.successValue = Mathf.Clamp(KeyholeTurnValue(), 0, 1);
        }

        private bool LockpickIsInPosition()
        {
            return LockPickAngle() < _lockAngle + _lockGive && LockPickAngle() > _lockAngle - _lockGive;
        }

        private void Shake()
        {
            // If we are not already shaking, save the original rotations.
            if (!isShaking)
            {
                if (audioPadlockJiggle && padlock1 != null && padlock1.activeSelf)
                {
                    audioPadlockJiggle.PlayLoop();
                }
                else
                {
                    if (audioJiggle)
                        audioJiggle.PlayLoop();
                    if (audioJiggle2 != null)
                        audioJiggle2.PlayLoop();
                    if (audioJiggle3 != null)
                        audioJiggle3.PlayLoop();
                }

                preshakeKeyhole = KeyholeAngles();
                preshakeLockpick = LockpickAngles();
                isShaking = true;
            }

            // Check breakTimeCounter to stop shaking at the right time
            breakTimeCounter += Time.deltaTime;
            Log.Out($"Break Time Counter: {breakTimeCounter} Total BreakTime: {breakTime}");
            if (breakTimeCounter > breakTime)
            {
                StopShaking();
                BreakLockpick();
                return;
            }

            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0)
            {
                // Start with the current values
                var newShakeKeyhole = preshakeKeyhole;
                var newShakeLockpick = preshakeLockpick;

                // Add some modification
                newShakeKeyhole.z += Random.Range(-maxShake, maxShake);
                newShakeLockpick.z += Random.Range(-maxShake, maxShake);

                // Set the value + modification
                SetKeyholeAngles(newShakeKeyhole);
                SetLockpickAngles(newShakeLockpick);

                // Reset the timer
                shakeTimer = shakeTime;
            }
        }

        private void StopShaking()
        {
            if (isShaking)
            {
                if (audioPadlockJiggle && padlock1 != null && padlock1.activeSelf)
                {
                    audioPadlockJiggle.StopLoop();
                }
                else if (audioJiggle)
                {
                    audioJiggle.StopLoop();
                    if (audioJiggle2 != null)
                        audioJiggle2.StopLoop();
                    if (audioJiggle3 != null)
                        audioJiggle3.StopLoop();
                }

                SetKeyholeAngles(preshakeKeyhole);
                SetLockpickAngles(preshakeLockpick);
                isShaking = false;
            }
        }

        private void SetKeyholeAngles(Vector3 value)
        {
            keyhole.transform.localEulerAngles = value;
        }

        private void SetLockpickAngles(Vector3 value)
        {
            lockpickObject.transform.localEulerAngles = value;
        }

        private void BreakLockpick()
        {
            if (audioLockpickBreak) audioLockpickBreak.PlayOnce();
            breakCounter = breakPause; // Set so we can't do any actions for a short time
            breakTimeCounter = 0f; // Reset the breakCounter
            ResetLockpickPosition(); // Reset the lockpick position
            lockpickAnimator.SetTrigger(_lockpickBreakTrigger); // Play the break animation
            lockpickBroke.Invoke(); // Invoke this event in case other scripts are listening

            if (audioLockpickEnter && audioLockpickEnter.isActiveAndEnabled) audioLockpickEnter.DelayPlay(1f);

            // Remove the broke pick lock.
            if (player != null)
            {
                var playerUI = (player as EntityPlayerLocal).PlayerUI;
                var playerInventory = playerUI.xui.PlayerInventory;

                var item = ItemClass.GetItem("resourceLockPick");
                var itemStack = new ItemStack(item, 1);
                playerInventory.RemoveItem(itemStack);
                RefreshLockPicks();
            }

            if (NumLockPicks > 0)
                UpdateLockPicks(true);
            else
                UpdateLockPicks(false);
        }


        /// <summary>
        ///     Call this when the lock is open successfully.
        /// </summary>
        public void OpenLock()
        {
            if (!_lockIsOpen)
            {
                if (audioPadlockOpen && padlock1 != null && padlock1.activeInHierarchy)
                {
                    audioPadlockOpen.PlayOnce();
                    if (_padlockAnimator != null)
                        _padlockAnimator.SetTrigger(_openTrigger);
                }
                else if (audioOpen)
                {
                    audioOpen.PlayOnce();
                }

                // Invoke the event for any other scripts that are listening
                lockOpen.Invoke();
                _lockIsOpen = true;
            }
        }

        private void DoSqueekAudio(float speed)
        {
            if (audioSqueek)
                if (squeekRate > 0)
                {
                    squeekTimer -= Mathf.Abs(speed) * Time.deltaTime;
                    if (squeekTimer <= 0)
                    {
                        if (Random.Range(0, 100) < squeekChance) audioSqueek.PlayAudioClip(Random.Range(squeekVolumeMin, squeekVolumeMax));
                        squeekTimer = squeekRate;
                    }
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

        private void TurnLockpick(float speed)
        {
            // If we are at or outside of our max range, return
            if (LockPickAngle() >= _pickAngleMax && speed > 0 || LockPickAngle() <= _pickAngleMin && speed < 0)
                return;


            // Set the new angle
            var newAngle = new Vector3(LockpickAngles().x, LockpickAngles().y,
                LockpickAngles().z + speed * Time.deltaTime);

            SetLockpickAngles(newAngle);

            DoClickAudio(speed, newAngle);
        }

        private void DoClickAudio(float speed, Vector3 newAngle)
        {
            var angleMod = newAngle.z % clickRate;
            var prevMod = _lockpickAnglePrev % clickRate;

            if (speed > 0 && angleMod < prevMod || speed < 0 && angleMod > prevMod)
                if (audioTurnClick != null)
                    audioTurnClick.PlayAudioClip(Random.Range(clickVolumeMin, clickVolumeMax));

            _lockpickAnglePrev = newAngle.z;
        }

        private Vector3 LockpickAngles()
        {
            return lockpickObject.transform.localEulerAngles;
        }

        private void TurnKeyhole(float speed)
        {
            // If we are at or outside of our max range, return
            if (KeyholeAngle() >= _keyholeAngleMax && speed > 0 || KeyholeAngle() <= _keyholeAngleDefault && speed < 0)
                return;

            // Set the new angle
            SetKeyholeAngles(new Vector3(KeyholeAngles().x, KeyholeAngles().y, KeyholeAngles().z + speed * Time.deltaTime));

            DoSqueekAudio(speed);
        }

        private Vector3 KeyholeAngles()
        {
            return keyhole.transform.localEulerAngles;
        }

        public float KeyholeTurnValue()
        {
            return (_keyholeAngleMax - KeyholeAngle()) / (_keyholeAngleMax - _keyholeAngleDefault);
        }

        public void SetLock(float newLockAngle, float newLockGive, float newCloseDistance)
        {
            _lockAngle = newLockAngle;
            _lockGive = newLockGive;
            _closeDistance = newCloseDistance;
            if (audioLockpickEnter && audioLockpickEnter.isActiveAndEnabled) audioLockpickEnter.DelayPlay(0.7f);

            ResetLockpickPosition();
            lockpickAnimator.SetTrigger(_lockpickInsertTrigger); // Play the  animation
        }

        public void SetLock(float lockAngleMin, float lockAngleMax, float lockGiveMin,
            float lockGiveMax, float closeDistanceMin, float closeDistanceMax)
        {
            SetLock(Random.Range(lockAngleMin, lockAngleMax),
                Random.Range(lockGiveMin, lockGiveMax),
                Random.Range(closeDistanceMin, closeDistanceMax));
        }

        public void ResetLock()
        {
            LockIsOpen = false;
            ProgressionValue progressionValue = null;
            var difficulty = 0;

            var prevBreakTime = breakTime;
            var prevMaxGive = maxGiveAmount;

            var healthLeft = blockValue.Block.MaxDamage - blockValue.damage;
            // Adjust the difficulty based on the lock damage
            if (healthLeft <= 10000)
                difficulty = 3;
            if (healthLeft <= 2500)
                difficulty = 2;
            if (healthLeft <= 400)
                difficulty = 1;
            if (healthLeft <= 200)
                difficulty = 0;

            // give more time to avoid breaking pick locks.
            if (player != null)
            {
                var secureBlock = Block.list[blockValue.type];
                if (secureBlock != null)
                {

                    if (secureBlock.Properties.Values.ContainsKey("LockPickDifficulty"))
                        difficulty = int.Parse(secureBlock.Properties.Values["LockPickDifficulty"]);
                }
                var maxGiveAmounts = Configuration.GetPropertyValue("AdvancedLockpicking", "MaxGiveAmount").Split(',');
                var breakTimes = Configuration.GetPropertyValue("AdvancedLockpicking", "BreakTime").Split(',');

                // Default values.
                maxGiveAmount = 10f;
                breakTime = 2f;
                if (maxGiveAmounts.Length >= difficulty)
                {
                    maxGiveAmount = StringParsers.ParseFloat(maxGiveAmounts[difficulty]);
                    breakTime = StringParsers.ParseFloat(breakTimes[difficulty]);
                }

                progressionValue = player.Progression.GetProgressionValue("perkLockPicking");
                if (progressionValue is {Level: > 0})
                {
                    prevBreakTime = breakTime;
                    prevMaxGive = maxGiveAmount;
                    breakTime += (float)progressionValue.Level / 5;
                    maxGiveAmount += (float)progressionValue.Level * 2 ;
                }
            }


            SetLock(minLockAngle, maxLockAngle,
                minGiveAmount, maxGiveAmount,
                minCloseDistance, maxCloseDistance);

            if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
            {
                var progression = " Progression: ";
                progression = progressionValue != null ? $"{progression} Level {progressionValue.Level} BreakTime Before: {prevBreakTime} MaxGiveAmount Before: {prevMaxGive}" : $"{progression} N/A";

                Log.Out("");
                Log.Out("-------------------------------------------");
                Log.Out($"Configured Lock Pick: Break Time: {breakTime}  MaxGiveAmount: {maxGiveAmount} Lock Difficulty: {difficulty} Block Damage: {healthLeft} {progression} ");
                Log.Out("Lock Angle: " + _lockAngle + " Give: " + _lockGive + " Close distance: " + _closeDistance);
                Log.Out($"MinLockAngle: {minLockAngle} MaxLockAngle: {maxLockAngle} Min Give Amount: {minGiveAmount} Max Give Amount: {maxGiveAmount} Min Close distance: {minCloseDistance} Max Close Distance: {maxCloseDistance}");
                Log.Out(" To Adjust, run console command   lock 2 34");
                Log.Out(" For breaktime of 2 and maxgive of 34");
                Log.Out("-------------------------------------------");

            }

            RefreshLockPicks();
        }

        public void ResetLockpickPosition()
        {
            SetLockpickAngles(_pickAnglesDefault);
            if (_padlockAnimator != null)
                _padlockAnimator.SetTrigger(_closeTrigger);
        }

        public bool LockCanTurn()
        {
            return !(LockPickAngle() < GetAngle(_lockAngle) - _lockGive - _closeDistance * KeyholeTurnValue()) &&
                   !(LockPickAngle() > GetAngle(_lockAngle) + _lockGive + _closeDistance * KeyholeTurnValue());
        }


        public bool LockComplete()
        {
            if (!LockIsOpen)
                return false;


            if (padlock1 != null && padlock1.activeInHierarchy)
            {
                if (_padlockAnimator != null)
                    // If the padlock is fully animated
                    if (_padlockAnimator.GetCurrentAnimatorStateInfo(0).IsName("Padlock1Opened"))
                        return true;
            }
            else
            {
                return !audioOpen.isAudioPlaying();
            }

            return false;
        }

        #region ThirdParty

        // Events
        private readonly UnityEvent lockpickBroke = new UnityEvent();
        private readonly UnityEvent lockOpen = new UnityEvent();


        [Header("Player Input")] public float openPressure;

        public float lockpickPressure;


        [Header("Speed Settings")]
        [Tooltip("Speed of the lockpick when input value is full.")]
        [Range(1f, 720f)]
        public float turnSpeedLockpick = 50f;

        [Tooltip("Speed of the entire keyhole when input value is full.")]
        [Range(1f, 720f)]
        public float turnSpeedKeyhole = 50f;

        [Tooltip("Speed at which the lock will return to normal when the input value is 0.")]
        [Range(1f, 720f)]
        public float returnSpeedKeyhole = 150f;

        [Tooltip("Maximum shake distance per shake change.")]
        [SerializeField]
        private float maxShake = 0.5f;

        [Tooltip("Amount of time between shake changes when shaking.")]
        [SerializeField]
        private float shakeTime = 0.1f;


        [Header("Pick Settings")]
        [Tooltip("Starting angle of the lock pick.")]
        [SerializeField]
        private Vector3 _pickAnglesDefault;

        [Tooltip("Minimum angle the lock pick can travel to.")]
        [SerializeField]
        private float _pickAngleMin = -90f;

        [Tooltip("Maximum angle the lock pick can travel to.")]
        [SerializeField]
        private float _pickAngleMax = 90f;


        [Header("Keyhole Settings")]
        [Tooltip("Starting angle of the keyhole.")]
        [SerializeField]
        private float _keyholeAngleDefault = 0f;

        [Tooltip("Maximum angle of the keyhole. At this angle, the lock will open.")]
        [SerializeField]
        private float _keyholeAngleMax = 90f;


        [Header("Lock Settings")]
        [Tooltip("If true, lock details will be randomized on awake")]
        public bool resetOnAwake = true;

        [Tooltip("Minimum angle the lock can be set to.")]
        [Range(0f, 180f)]
        public float minLockAngle = -90f;

        [Tooltip("Maximum angle the lock can be set to.")]
        [Range(0f, 180f)]
        public float maxLockAngle = 90f;

        [Tooltip("Minimum distance (plus and minus) from the lock angle that the lock will open.")]
        [Range(1f, 180f)]
        public float minGiveAmount = 1f;

        [Tooltip("Maximum distance (plus and minus) from the lock angle that the lock will open.")]
        [Range(1f, 180f)]
        public float maxGiveAmount = 30f;

        [Tooltip("Minimum distance for the pick to be in for the lock will turn partially.")]
        [Range(5f, 180f)]
        public float minCloseDistance = 5f;

        [Tooltip("Maximum distance for the pick to be in for the lock will turn partially.")]
        [Range(5f, 180f)]
        public float maxCloseDistance = 10f;

        [Tooltip("Amount of time to ignore player input after a lock pick breaks.")]
        [Range(0f, 5f)]
        public float breakPause = 2f;


        [Header("Lock Details")]
        [Tooltip("True if the lock is already open (unlocked).")]
        [SerializeField]
        private bool _lockIsOpen;

        public bool LockIsOpen
        {
            get => _lockIsOpen;
            set => _lockIsOpen = value;
        }


        [Tooltip("The exact angle the lock is set to.")]
        private float _lockAngle;

        public float LockAngle
        {
            get => _lockAngle;
            set => _lockAngle = Mathf.Clamp(value, minLockAngle, maxLockAngle);
        }

        [Tooltip("The distance to/from the LockAngle the lock pick needs to be in for the lock to open successfully.")]
        [SerializeField]
        private float _lockGive;

        public float LockGive
        {
            get => _lockGive;
            set => _lockGive = Mathf.Clamp(value, 1, maxGiveAmount);
        }

        [Tooltip("If the lock pick is within this distance to the angle range which the lock will open, the lock will turn partially when an open attempt is made.")]
        [SerializeField]
        private float _closeDistance;

        public float CloseDistance
        {
            get => _closeDistance;
            set => _closeDistance = Mathf.Clamp(value, 5, maxCloseDistance);
        }

        [Tooltip("The amount of time before a lock pick breaks when the lock is unable to be opened, but the player is attempting to open it.")]
        [Range(0f, 5f)]
        public float breakTime = 1f;


        [Header("Animation Trigger Strings")] public string openTrigger = "OpenPadlock";

        public string closeTrigger = "ClosePadlock";
        public string lockPickBreakTrigger = "BreakLockpick1";
        public string lockPickInsertTrigger = "InsertLockpick";

        // Private animation hashes
        private int _openTrigger;
        private int _closeTrigger;
        private int _lockpickBreakTrigger;
        private int _lockpickInsertTrigger;

        #endregion
    }
}