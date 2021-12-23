//using UnityEngine;
//using UnityEngine.UI;
//using Lockpicking;
//using UnityEngine.Serialization;

//public class CipherDemoUI : MonoBehaviour
//{

//    // Set up references to the Text objects
//    public Text wheel1;
//    public Text wheel2;
//    public Text wheel3;
//    public Text wheel4;
//    public Text wheel5;
//    public Text wheel6;

//    public Cipher cipher;

//    public Color highlightColor = new Color(255f,0f,171f,1f);

//    void Start()
//    {
////        ResetLock();
//    }

//    void Update()
//    {
//        DrawText();
//    }

//    private void DrawText()
//    {
//        if (Input.GetKey(KeyCode.O))
//        {
//            cipher.TryOpen();
//        }
//        if (Input.GetKeyUp(KeyCode.O))
//        {
//            cipher.StopShaking();
//        }

//        if (Mathf.Abs(cipher.DistanceLeft(0)) < cipher.closeEnough)
//            wheel1.color = highlightColor;
//        else
//            wheel1.color = Color.white;
//        if (Mathf.Abs(cipher.DistanceLeft(1)) < cipher.closeEnough)
//            wheel2.color = highlightColor;
//        else
//            wheel2.color = Color.white;
//        if (Mathf.Abs(cipher.DistanceLeft(2)) < cipher.closeEnough)
//            wheel3.color = highlightColor;
//        else
//            wheel3.color = Color.white;
//        if (Mathf.Abs(cipher.DistanceLeft(3)) < cipher.closeEnough)
//            wheel4.color = highlightColor;
//        else
//            wheel4.color = Color.white;
//        if (Mathf.Abs(cipher.DistanceLeft(4)) < cipher.closeEnough)
//            wheel5.color = highlightColor;
//        else
//            wheel5.color = Color.white;
//        if (Mathf.Abs(cipher.DistanceLeft(5)) < cipher.closeEnough)
//            wheel6.color = highlightColor;
//        else
//            wheel6.color = Color.white;
//        wheel1.text = "" + cipher.DistanceLeft(0);
//        wheel2.text = "" + cipher.DistanceLeft(1);
//        wheel3.text = "" + cipher.DistanceLeft(2);
//        wheel4.text = "" + cipher.DistanceLeft(3);
//        wheel5.text = "" + cipher.DistanceLeft(4);
//        wheel6.text = "" + cipher.DistanceLeft(5);
//    }

//    public void ResetLock()
//    {
//        cipher.ResetLock();
//    }
//}

