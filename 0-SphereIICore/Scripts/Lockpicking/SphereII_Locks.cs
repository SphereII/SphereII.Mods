using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lockpicking;
using UnityEngine;



// This class sites between the thirdparty Keyhole script + prefabs and support scripts.
public class SphereII_Locks
{
    public GameObject LockPickAsset;
    public GameObject lockPick;
    // transforms 
    List<String> transforms = new List<string>();

    public void Init(BlockValue _blockValue, Vector3i _blockPos)
    {
        // Check if the current block has a pre-defined lockpick prefab
        String LockPrefab = "";
        if (_blockValue.type != 0)
        {
            if (_blockValue.Block.Properties.Contains("LockPrefab"))
                LockPrefab = _blockValue.Block.Properties.GetStringValue("LockPrefab");
        }

        
        // Load up the default.
        if (String.IsNullOrEmpty(LockPrefab))
        {
            // If the globally configured lock pick cotnains Lockset01, assume its the default.
            LockPrefab = Configuration.GetPropertyValue("AdvancedLockpicking", "LockPrefab");
            if (LockPrefab.EndsWith("Lockset01"))
            {
                List<String> Locks = new List<string>() { "Lockset01", "Lockset02", "Lockset03", "Lockset04", "padlock01" };
                String randomLock;
                int RandomKey = Math.Abs( _blockPos.x % 9 );
                if (RandomKey < 1)
                    randomLock = "Lockset01";
                else if (RandomKey < 3)
                    randomLock = "Lockset02";
                else if (RandomKey < 5)
                    randomLock = "Lockset03";
                else if (RandomKey < 7)
                    randomLock = "Lockset04";
                else 
                    randomLock = "padlock01";

                LockPrefab = LockPrefab.Replace("Lockset01", randomLock);

            }
        }

        if (String.IsNullOrEmpty(LockPrefab))
            return;
        LockPickAsset = DataLoader.LoadAsset<GameObject>(LockPrefab);
        lockPick = UnityEngine.Object.Instantiate<GameObject>(LockPickAsset);
        Disable();

        // Marked transforms
        transforms = new List<string>() { "Baseplate1", "Baseplate2", "ButtonInner", "ButtonInner", "ButtonOuter", "Padlock1_low" };
        transforms.AddRange(new List<String>() { "Padlock1_Latch_low", "Lock1Outer", "Lock2Outer", "Lock3Outer", "Lock1Inner", "Lock2Inner", "Lock3Inner" });

        if (lockPick.GetComponent<Keyhole>() == null)
        {
            // Populate the Keyhole
            Keyhole keyhole = lockPick.AddComponent<Keyhole>();
            keyhole.keyhole = FindTransform("Keyhole (Turnable)").gameObject;

            // attach the lock control the to top level
            LockControls lockControl;
            if (lockPick.transform.parent != null)
            {
                lockControl = lockPick.transform.parent.gameObject.AddComponent<LockControls>();
            }
            else
            {
                lockControl = lockPick.transform.gameObject.AddComponent<LockControls>();

            }

            lockControl.lockpick = keyhole;

            // Lock Pick configuration
            keyhole.lockpickObject = lockPick.transform.FindInChilds("LockpickB (Turnable)").gameObject;
            keyhole.lockpickAnimator = FindTransform("LockpickB").GetComponent<Animator>();
            keyhole.lockpickAnimator.gameObject.SetActive(true);

            keyhole.blockValue = _blockValue;
            Camera cam = FindTransform("Cam2").GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.rect = new Rect(0.25f, 0.25f, 0.5f, 0.5f);
                LockObjectRotation lockObjectRotation = keyhole.lockpickObject.transform.gameObject.AddComponent<LockObjectRotation>();
                lockObjectRotation.uiCam = cam;
            }

            Transform padlock = FindTransform("Padlock1");
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

            LockEmissive lockEmissive = lockPick.AddComponent<LockEmissive>();
         //   lockEmissive.off = true;
            List<Renderer> lstRenders = new List<Renderer>();
            Renderer[] tempRender = new Renderer[12];

            foreach (String transform in transforms)
            {
                Transform temp = FindTransform(transform);
                if (temp)
                    lstRenders.Add(FindTransform(transform).GetComponent<MeshRenderer>());
            }
            lockEmissive.SetRenders(lstRenders.ToArray());

            //foreach (Transform child in lockPick.transform)
            //{
            //    Debug.Log("\t Transform: " + child.transform.ToString() + " Active? " + child.gameObject.activeInHierarchy);
            //}
        }
        Enable();

    }

  

    public Keyhole GetScript()
    {
        if (lockPick != null)
            return lockPick.GetComponent<Keyhole>();
        return null;
    }
   
    public bool IsLockOpened()
    {
        if (lockPick != null)
            return lockPick.GetComponent<Keyhole>().LockComplete();
        return false;
    }

    public void SetPlayer(EntityPlayer player)
    {
        if (lockPick != null)
        {
            lockPick.GetComponent<Keyhole>().player = player;

        }
    }
    public Transform FindTransform(String target)
    {
        return lockPick.transform.FindInChilds(target, false);
    }

    public void Enable()
    {
        if (lockPick != null)
        {
            lockPick.SetActive(true);
            lockPick.GetComponent<Keyhole>().ResetLock();

        }
    }
    public void Disable()
    {
        if (lockPick != null)
        {
            lockPick.SetActive(false);
        }
    }
}
