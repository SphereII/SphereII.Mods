//using UnityEngine;
//using UnityEngine.UI;
//using Lockpicking;

//public class KeyholeDemoUI : MonoBehaviour
//{

//    // Set up references to the Text objects
//    public Text lockAngleText;
//    public Text lockRangeText;
//    public Text breakTimeText;
//    public Text lockpickAngleText;
//    public Text keyholeAngleText;
//    public Text closeDistanceText;

//    public Keyhole lockpick;

//    void Start()
//    {
//        ResetLock();
//    }

//    void Update()
//    {
//        DrawText();
//    }

//    private void DrawText()
//    {
//        lockAngleText.text = "" + lockpick.LockAngle;
//        lockRangeText.text = "" + Mathf.Round(lockpick.LockAngle - lockpick.LockGive) + " to " + Mathf.Round(lockpick.LockAngle + lockpick.LockGive);
//        breakTimeText.text = "" + lockpick.BreakTimeCounter;
//        lockpickAngleText.text = "" + lockpick.LockPickAngle;
//        keyholeAngleText.text = "" + lockpick.KeyholeTurnValue;
//        closeDistanceText.text = "" + lockpick.CloseDistance;
//    }

//    public void ResetLock()
//    {
//        lockpick.ResetLock();
//    }
//}

