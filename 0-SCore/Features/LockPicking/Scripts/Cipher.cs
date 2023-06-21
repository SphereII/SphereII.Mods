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
    //[ExecuteInEditMode]
    [RequireComponent(typeof(LockEmissive))]
    public class Cipher : MonoBehaviour
    {
        [Header("Speed Settings")]
        [Tooltip("Maximum shake distance per shake change.")]
        [SerializeField]
        private float maxShake = 0.5f;

        [Tooltip("Amount of time between shake changes when shaking.")]
        [SerializeField]
        private float shakeTime = 0.1f;

        [Tooltip("Speed of the wheel when turning.")]
        public float turnSpeed = 180f;


        [Header("Lock Setup")]
        [Tooltip("Number of symbols on the wheel. Should be spaced evenly.")]
        public int symbolCount = 18;

        [Tooltip("How close do we have to be to perfectly aligned for the placement of a wheel to count?")]
        public float closeEnough = 3f;

        [Tooltip("If true, wheels will \"snap\" into a new random spot on setup. If false, they will rotate to those spots.")]
        public bool quickReset;

        [Tooltip("If true, the active wheel will be highlighted using the emissive map of the material.")]
        public bool highlightActiveWheel = true;

        [Tooltip("If true, wheels will move to the closest spot when the player stops input movement.")]
        public bool moveToClosestSpot;


        [Header("Lock Details")]
        [Tooltip("When true, the lock has been opened.")]
        public bool isOpen;

        [Tooltip("When true, the lock will reset itself on awake. Set this to false if you are choosing a specific combination.")]
        public bool resetOnAwake;


        [Header("Animation Trigger Strings")] public string openTrigger = "Open";

        public string closeTrigger = "Close";

        // Audio Settings
        [Range(0f, 1f)] public float clickVolumeMin = 0.1f;
        [Range(0f, 1f)] public float clickVolumeMax = 0.4f;
        [Range(0f, 1f)] public float squeekVolumeMin = 0.1f;
        [Range(0f, 1f)] public float squeekVolumeMax = 0.4f;
        [Range(0, 100)] public int squeekChance = 50;
        [Range(0f, 15f)] public float squeekRate = 3.5f;
        public float[] unlockAngle; // All the unlock angles for each wheel
        public float[] startAngle; // All the starting angles for each wheel
        public float[] turnedDistance; // amount each wheel has been turned
        public float[] turnedDistancePrev; // amount each wheel has been turned
        public bool[] isMoving; // Records whether the wheels are moving
        public bool playAudioOnSetup = true;

        [Header("Plumbing")] public GameObject[] wheels;

        public LocksetAudio audioClick;
        public LocksetAudio audioSqueek;
        public LocksetAudio audioJiggle;
        public LocksetAudio audioJiggle2;
        public LocksetAudio audioJiggle3;
        public LocksetAudio audioOpen;

        // Events
        private readonly UnityEvent lockOpen = new UnityEvent();

        // Private variables
        private int _activeWheel;
        private int _closeTrigger;
        private LocksetAudio _locksetAudio;


        // Private animation hashes
        private int _openTrigger;
        private Animator animator;
        private float getReadyPostReadyTimer = 0.3f;
        private bool isReady; // WHen true, player can interact
        private bool isShaking; // When true, the lock is shaking (trying to be opened but in the wrong combination)

        private LockEmissive lockEmissive;
        private Vector3 preshake; // Saves the pre-shake angles

        private float shakeTimer; // Counter for the shake Time

        //   private float _speed;
        private float squeekTimer;

        public bool VisualizeSolution { get; set; }

        public bool VisualizeStart { get; set; }

        public float ActiveWheelStart
        {
            get => startAngle[ActiveWheel()];
            set => startAngle[ActiveWheel()] = value;
        }

        public float ActiveWheelSolution
        {
            get => unlockAngle[ActiveWheel()];
            set => unlockAngle[ActiveWheel()] = value;
        }

        private void Awake()
        {
            squeekTimer = squeekRate;
            _locksetAudio = GetComponent<LocksetAudio>();
            animator = GetComponent<Animator>();
            lockEmissive = GetComponent<LockEmissive>();
            if (!highlightActiveWheel)
                lockEmissive.highlightRenderer = null;

            _openTrigger = Animator.StringToHash(openTrigger);
            _closeTrigger = Animator.StringToHash(closeTrigger);

            if (resetOnAwake) ResetLock();
        }

        private void Update()
        {
            // Do actions to get ready, if we are not ready
            if (!isReady)
            {
                GetReady();
                if (playAudioOnSetup)
                    DoAudio();
                return;
            }

            if (getReadyPostReadyTimer > 0)
                getReadyPostReadyTimer -= Time.deltaTime;

            if (moveToClosestSpot)
                MoveToClosestSpot();
            ResetMovement();

            if (highlightActiveWheel) lockEmissive.SetHighlightRenderer(wheels[_activeWheel].GetComponent<Renderer>());

            DoAudio();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (unlockAngle.Length != wheels.Length )
            {
                Debug.Log("Updating Lengths");
                unlockAngle = new float[wheels.Length];
                startAngle = new float[wheels.Length];
                turnedDistance = new float[wheels.Length];
                isMoving = new bool[wheels.Length];
            }
            
            if (_visualizeSolution)
                SetRotationToSolution();
            else if (_visualizeStart)
                SetRotationToStart();
            else
                SetRotationToZero();

            for (int i = 0; i < wheels.Length; i++)
            {
                unlockAngle[i] = ClosestSpot(unlockAngle[i]);
            }
#endif
        }


        public int ActiveWheel()
        {
            return _activeWheel;
        }

        public void EditorOnValidate()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            OnValidate();
        }

        private void SetRotationToSolution()
        {
            for (var i = 0; i < wheels.Length; i++)
            {
                var eulerAngles = wheels[i].transform.localEulerAngles;
                eulerAngles.x = unlockAngle[i];
                wheels[i].transform.localEulerAngles = eulerAngles;
            }
        }

        private void SetRotationToStart()
        {
            for (var i = 0; i < wheels.Length; i++)
            {
                var eulerAngles = wheels[i].transform.localEulerAngles;
                eulerAngles.x = startAngle[i];
                wheels[i].transform.localEulerAngles = eulerAngles;
            }
        }

        private void SetRotationToZero()
        {
            for (var i = 0; i < wheels.Length; i++) wheels[i].transform.localEulerAngles = new Vector3();
        }

        public float DistanceLeft(int i)
        {
            if (turnedDistance.Length >= i + 1)
            {
                var left = unlockAngle[i] - turnedDistance[i];
                if (left < -350)
                    left += 360;
                if (left > 350)
                    left -= 360;

                return left;
            }

            return 9999;
        }

        private void DoAudio()
        {
            for (var i = 0; i < turnedDistance.Length; i++)
            {
                var turnSpeed = 0f;
                if (turnedDistance[i] != turnedDistancePrev[i])
                {
                    if (getReadyPostReadyTimer <= 0 || playAudioOnSetup)
                    {
                        if (audioSqueek)
                            // Squeek
                            if (squeekRate > 0)
                            {
                                squeekTimer -= Time.deltaTime;
                                if (squeekTimer <= 0)
                                {
                                    if (Random.Range(0, 100) < squeekChance) audioSqueek.PlayAudioClip(Random.Range(squeekVolumeMin, squeekVolumeMax));
                                    squeekTimer = squeekRate;
                                }
                            }

                        if (audioClick)
                        {
                            // Clicks
                            turnSpeed = turnedDistance[i] - turnedDistancePrev[i];

                            var turnedMod = turnedDistance[i] % (360 / symbolCount);
                            var turnedModPrev = turnedDistancePrev[i] % (360 / symbolCount);

                            if ((turnSpeed > 0 || turnSpeed < -350) && (turnedMod < turnedModPrev || turnedMod > 0 && turnedModPrev < 0))
                                audioClick.PlayAudioClip(Random.Range(clickVolumeMin, clickVolumeMax));
                            else if ((turnSpeed < 0 || turnSpeed > 350) && (turnedMod > turnedModPrev || turnedMod < 0 && turnedModPrev > 0))
                                audioClick.PlayAudioClip(Random.Range(clickVolumeMin, clickVolumeMax));
                            else if (turnedMod == 0) audioClick.PlayAudioClip(Random.Range(clickVolumeMin, clickVolumeMax));
                        }
                    }

                    turnedDistancePrev[i] = turnedDistance[i];
                }
            }
        }

        public int NextWheelIndex()
        {
            return _activeWheel + 1 < wheels.Length ? _activeWheel + 1 : 0;
        }

        public int PrevWheelIndex()
        {
            return _activeWheel - 1 < 0 ? wheels.Length - 1 : _activeWheel - 1;
        }

        public void SelectWheel(int value)
        {
            _activeWheel = value < wheels.Length && value >= 0 ? value : 0;
        }

        public void ResetMovement()
        {
            for (var i = 0; i < wheels.Length; i++) isMoving[i] = false;
        }

        public void MoveToClosestSpot()
        {
            for (var i = 0; i < wheels.Length; i++)
                // Only run this if the wheel is not moving (i.e. being moved by the player)
                if (!isMoving[i])
                {
                    if (turnedDistance[i] < 0) turnedDistance[i] += 360;

                    /*
                    float smallestDistance = 360f;
                    float closestRotation = 0f;

                    for (int d = 0; d < 360 / symbolCount; d++)
                    {
                        float diff = d * (360 / symbolCount);
                        if (Mathf.Abs(Mathf.Abs(turnedDistance[i]) - diff) < smallestDistance)
                        {
                            smallestDistance = Mathf.Abs(Mathf.Abs(turnedDistance[i]) - diff);
                            closestRotation = diff;
                        }
                    }
                    MoveTowards(i, closestRotation);
                    */
                    MoveTowards(i, ClosestSpot(Mathf.Abs(turnedDistance[i])));
                }
        }

        private float ClosestSpot(float value)
        {
            var smallestDistance = 360f;
            var closestRotation = 0f;

            for (var d = -360; d < 360 / symbolCount; d++)
            {
                float diff = d * (360 / symbolCount);
                if (Mathf.Abs(value - diff) < smallestDistance)
                {
                    smallestDistance = Mathf.Abs(value - diff);
                    closestRotation = diff;
                }
            }

            return closestRotation;
        }

        public void TryOpen()
        {
            if (!isShaking)
                preshake = transform.localEulerAngles;

            if (!isOpen)
            {
                for (var i = 0; i < wheels.Length; i++)
                    if (Mathf.Abs(DistanceLeft(i)) > closeEnough)
                    {
                        Shake();
                        return;
                    }

                isOpen = true;
                animator.SetTrigger(_openTrigger);

                audioOpen.PlayOnce();

                // Invoke the event for any other scripts that are listening
                lockOpen.Invoke();
            }
        }

        public void StopShaking()
        {
            transform.localEulerAngles = preshake;
            isShaking = false;

            if (audioJiggle)
            {
                audioJiggle.StopLoop();
                if (audioJiggle2 != null)
                    audioJiggle2.StopLoop();
                if (audioJiggle3 != null)
                    audioJiggle3.StopLoop();
            }
        }

        private void Shake()
        {
            if (!isShaking)
                isShaking = true;

            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0)
            {
                // Start with the current values
                var newShake = preshake;

                // Add some modification
                //newShake.z += Random.Range(-maxShake, maxShake);
                newShake.x += Random.Range(-maxShake, maxShake);
                newShake.y += Random.Range(-maxShake, maxShake);

                // Set the value + modification
                transform.localEulerAngles = newShake;

                // Reset the timer
                shakeTimer = shakeTime;
            }

            if (audioJiggle)
            {
                audioJiggle.PlayLoop();
                if (audioJiggle2 != null)
                    audioJiggle2.PlayLoop();
                if (audioJiggle3 != null)
                    audioJiggle3.PlayLoop();
            }
        }

        public void GetReady()
        {
            var readyCount = 0;
            for (var i = 0; i < wheels.Length; i++)
            {
                var speed = 1;
                var distanceLeft = startAngle[i] - turnedDistance[i];

                if (VisualizeSolution) distanceLeft = unlockAngle[i] - turnedDistance[i];

                if (distanceLeft < 0) speed = -1;

                var turnAmount = speed * turnSpeed * Time.deltaTime;
                if (Mathf.Abs(distanceLeft) < Mathf.Abs(turnAmount))
                {
                    turnAmount = distanceLeft;
                    readyCount++;
                }

                if (quickReset || VisualizeSolution)
                {
                    turnAmount = distanceLeft;
                    readyCount++;
                }

                SetTurnedDistance(i, turnAmount);
                RotateWheel(wheels[i].transform, turnAmount);
            }

            if (readyCount == wheels.Length)
                isReady = true;
        }

        public void MoveActiveWheel(int speed)
        {
            MoveWheel(_activeWheel, speed);
        }

        public void MoveWheel(int i, int speed, float destination = 999f)
        {
            if (isReady)
            {
                isMoving[i] = true; // Mark this wheel as currently moving
                var turnAmount = speed * turnSpeed * Time.deltaTime;
                if (destination != 999)
                    if (Mathf.Abs(destination - turnedDistance[i]) < Mathf.Abs(turnAmount))
                        turnAmount = destination - turnedDistance[i];

                SetTurnedDistance(i, turnAmount);
                RotateWheel(wheels[i].transform, turnAmount);
            }
        }

        private void RotateWheel(Transform transform, float value)
        {
            transform.Rotate(value, 0.0f, 0.0f, Space.Self);
        }

        public void MoveTowards(int i, float closestRotation)
        {
            if (turnedDistance[i] > closestRotation)
                MoveWheel(i, -1, closestRotation);
            else
                MoveWheel(i, 1, closestRotation);
        }

        public void SetTurnedDistance(int i, float distance)
        {
            turnedDistance[i] += distance;
            if (turnedDistance[i] > 360)
                turnedDistance[i] -= 360;
            if (turnedDistance[i] < -360)
                turnedDistance[i] += 360;
        }

        public void ResetLock()
        {
            Debug.Log("ResetLock");
            isOpen = false;
            isReady = false;
            animator.SetTrigger(_closeTrigger);

            // Shuffle each wheel
            for (var i = 0; i < wheels.Length; i++)
            {
                unlockAngle[i] = RandomAngle();
                startAngle[i] = RandomAngle();
            }
        }

        private float RandomAngle()
        {
            return Random.Range(0, symbolCount) * (360 / symbolCount) - 180;
        }
    }
}