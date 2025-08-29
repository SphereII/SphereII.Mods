using System;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * This script is designed to trigger a random animation on an associated Animator component every five minutes of in-game time.
 * It is a time-based animation controller, useful for adding subtle, random environmental or character animations without relying
 * on complex state machines or frequent, performance-heavy checks.
 */
class RandomAnimationScript : MonoBehaviour
{
    Animator anim;
    int intLastMinute = -1;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.World == null) return;
        if (GameManager.Instance.IsPaused())
            return;

        ulong worldTime = GameManager.Instance.World.worldTime;
        int intHour = (int)(worldTime / 1000UL) % 24;
        int intMinute = (int)(worldTime * 0.06) % 60;

        if (intLastMinute == intMinute)
            return;

        if ((intLastMinute - intMinute) >= 5 || (intMinute - intLastMinute) >= 5 || (intLastMinute == -1))
        {
            intLastMinute = intMinute;
            RunAnim();
        }

        void RunAnim()
        {
            int randomNumber = Random.Range(1, 7);
            anim.SetTrigger("Random0" + randomNumber);
            Debug.Log(randomNumber);
            Debug.Log(intLastMinute);
        }
    }
}