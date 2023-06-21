using Lockpicking;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


// This class sites between the thirdparty Keyhole script + prefabs and support scripts.
public class SphereLocks
{
    private GameObject _lockPick;

    private GameObject _lockPickAsset;

    // transforms 
    private List<string> _transforms = new List<string>();

    public void Init(BlockValue blockValue, Vector3i blockPos)
    {
        // Check if the current block has a pre-defined lockpick prefab
        var lockPrefab = "";
        if (blockValue.type != 0)
            if (blockValue.Block.Properties.Contains("LockPrefab"))
                lockPrefab = blockValue.Block.Properties.GetStringValue("LockPrefab");


        // Load up the default.
        if (string.IsNullOrEmpty(lockPrefab))
        {
            // If the globally configured lock pick cotnains Lockset01, assume its the default.
            lockPrefab = Configuration.GetPropertyValue("AdvancedLockpicking", "LockPrefab");
            if (lockPrefab.EndsWith("Lockset01"))
            {
                var locks = new List<string> { "Lockset01", "Lockset02", "Lockset03", "Lockset04", "padlock01" };
                var randomKey = Math.Abs(blockPos.x % 9);
                var randomLock = randomKey switch
                {
                    < 1 => "Lockset01",
                    < 3 => "Lockset02",
                    < 5 => "Lockset03",
                    < 7 => "Lockset04",
                    _ => "padlock01"
                };

                lockPrefab = lockPrefab.Replace("Lockset01", randomLock);
            }
        }

        if (string.IsNullOrEmpty(lockPrefab))
            return;
      
        try
        {
            _lockPickAsset = DataLoader.LoadAsset<GameObject>(lockPrefab);
            _lockPick = Object.Instantiate(_lockPickAsset);
        }
        catch( Exception ex)
        {
            Log.Out($"LockPrefab not valid. Falling back to vanilla.");
            return;
        }
        Disable();
        

        // Marked transforms
        _transforms = new List<string> { "Baseplate1", "Baseplate2", "ButtonInner", "ButtonInner", "ButtonOuter", "Padlock1_low" };
        _transforms.AddRange(new List<string> { "Padlock1_Latch_low", "Lock1Outer", "Lock2Outer", "Lock3Outer", "Lock1Inner", "Lock2Inner", "Lock3Inner" });

        if (_lockPick.GetComponent<Keyhole>() == null)
        {
            // Populate the Keyhole
            var keyhole = _lockPick.AddComponent<Keyhole>();
            keyhole.keyhole = FindTransform("Keyhole (Turnable)").gameObject;

            // attach the lock control the to top level
            LockControls lockControl;
            if (_lockPick.transform.parent != null)
                lockControl = _lockPick.transform.parent.gameObject.AddComponent<LockControls>();
            else
                lockControl = _lockPick.transform.gameObject.AddComponent<LockControls>();

            lockControl.lockpick = keyhole;

            // Lock Pick configuration
            keyhole.lockpickObject = _lockPick.transform.FindInChilds("LockpickB (Turnable)").gameObject;
            keyhole.lockpickAnimator = FindTransform("LockpickB").GetComponent<Animator>();
            keyhole.lockpickAnimator.gameObject.SetActive(true);

            keyhole.blockValue = blockValue;
            var cam = FindTransform("Cam2").GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.rect = new Rect(0.25f, 0.25f, 0.5f, 0.5f);
                var lockObjectRotation = keyhole.lockpickObject.transform.gameObject.AddComponent<LockObjectRotation>();
                lockObjectRotation.uiCam = cam;
            }

            var padlock = FindTransform("Padlock1");
            if (padlock != null)
            {
                keyhole.padlock1 = padlock.gameObject;
                keyhole.audioPadlockJiggle = FindTransform("Audio Padlock Jiggle").gameObject.AddComponent<LocksetAudio>();
                keyhole.audioPadlockOpen = FindTransform("Audio Padlock Open").gameObject.AddComponent<LocksetAudio>();
            }

            // audio configuration
            keyhole.audioTurnClick = FindTransform("Audio Turn Click").gameObject.AddComponent<LocksetAudio>();
            keyhole.audioSqueek = FindTransform("Audio Squeek").gameObject.AddComponent<LocksetAudio>();
            keyhole.audioOpen = FindTransform("Audio Open").gameObject.AddComponent<LocksetAudio>();
            keyhole.audioJiggle = FindTransform("Audio Jiggle A").gameObject.AddComponent<LocksetAudio>();
            keyhole.audioJiggle2 = FindTransform("Audio Jiggle B").gameObject.AddComponent<LocksetAudio>();
            keyhole.audioJiggle3 = FindTransform("Audio Jiggle C").gameObject.AddComponent<LocksetAudio>();

            keyhole.audioLockpickBreak = FindTransform("Audio Lockpick Break").gameObject.AddComponent<LocksetAudio>();
            keyhole.audioLockpickEnter = FindTransform("Audio Lockpick Enter").gameObject.AddComponent<LocksetAudio>();
            keyhole.audioLockpickClick = FindTransform("Audio Lockpick Click").gameObject.AddComponent<LocksetAudio>();

            var lockEmissive = _lockPick.AddComponent<LockEmissive>();
            //   lockEmissive.off = true;
            var lstRenders = new List<Renderer>();
            var tempRender = new Renderer[12];

            foreach (var transform in _transforms)
            {
                var temp = FindTransform(transform);
                if (temp)
                    lstRenders.Add(FindTransform(transform).GetComponent<MeshRenderer>());
            }

            lockEmissive.SetRenders(lstRenders.ToArray());
        }

        Enable();
    }


    public Keyhole GetScript()
    {
        return _lockPick != null ? _lockPick.GetComponent<Keyhole>() : null;
    }

    public bool IsLockOpened()
    {
        return _lockPick != null && _lockPick.GetComponent<Keyhole>().LockComplete();
    }

    public void SetPlayer(EntityPlayer player)
    {
        if (_lockPick != null) _lockPick.GetComponent<Keyhole>().player = player;
    }

    private Transform FindTransform(string target)
    {
        return _lockPick.transform.FindInChilds(target);
    }

    public void Enable()
    {
        if (_lockPick == null) return;
        _lockPick.SetActive(true);
        _lockPick.GetComponent<Keyhole>().ResetLock();
    }

    public void Disable()
    {
        if (_lockPick != null) _lockPick.SetActive(false);
    }
}