using System;
using UnityEngine;
using Harmony;
class BlockClockScript : MonoBehaviour
{
    Animator anim;

    int LastMinute = -1;
    int LastHour = -1;

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
        string strHour = intHour.ToString();
        string strMinute = intMinute.ToString();
        int intActualTime = -1;

        if (intMinute < 10)
        {
            intActualTime = Int32.Parse(strHour + "0" + strMinute);
        }
        else
            intActualTime = Int32.Parse(strHour + strMinute);

        if (LastMinute == intMinute)
            return;

        LastMinute = intMinute;

        if (intHour > 21 | intHour < 5)
        {
            anim.SetBool("isAM", false);
        }
        else
            anim.SetBool("isAM", true);

        anim.SetInteger("minute", intMinute);
        anim.SetInteger("actualtime", intActualTime);
        anim.SetInteger("hour", intHour);

        if (intActualTime == 1200 | intActualTime == 0)
        {
            anim.SetTrigger("trigger12");
        }
        if (intActualTime == 1300 | intActualTime == 1)
        {
            anim.SetTrigger("trigger01");
        }
        if (intActualTime == 1400 | intActualTime == 2)
        {
            anim.SetTrigger("trigger02");
        }
        if (intActualTime == 1500 | intActualTime == 3)
        {
            anim.SetTrigger("trigger03");
        }
        if (intActualTime == 1600 | intActualTime == 4)
        {
            anim.SetTrigger("trigger04");
        }
        if (intActualTime == 1700 | intActualTime == 5)
        {
            anim.SetTrigger("trigger05");
        }
        if (intActualTime == 1800 | intActualTime == 6)
        {
            anim.SetTrigger("trigger06");
        }
        if (intActualTime == 1900 | intActualTime == 7)
        {
            anim.SetTrigger("trigger07");
        }
        if (intActualTime == 2000 | intActualTime == 8)
        {
            anim.SetTrigger("trigger08");
        }
        if (intActualTime == 2100 | intActualTime == 9)
        {
            anim.SetTrigger("trigger09");
        }
        if (intActualTime == 2200 | intActualTime == 10)
        {
            anim.SetTrigger("trigger10");
        }
        if (intActualTime == 2300 | intActualTime == 11)
        {
            anim.SetTrigger("trigger11");
        }

        /*      Debug.Log(" intMinute " + intMinute);
                Debug.Log(" intHour " + intHour);
                Debug.Log(" intActualtime " + intActualTime);
                Debug.Log("   "); 
        */

    }
}
