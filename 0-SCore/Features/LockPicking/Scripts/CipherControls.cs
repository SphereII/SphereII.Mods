using Lockpicking;
using UnityEngine;
using UnityEngine.Serialization;

public class CipherControls : MonoBehaviour
{
    [FormerlySerializedAs("cypher")] public Cipher cipher;

    // Update is called once per frame
    private void Update()
    {
        if (cipher.gameObject.activeSelf)
            InputControls();
    }

    private void InputControls()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                cipher.SelectWheel(cipher.PrevWheelIndex());
            else
                cipher.SelectWheel(cipher.NextWheelIndex());
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            cipher.SelectWheel(cipher.PrevWheelIndex());

        if (Input.GetKeyDown(KeyCode.RightArrow))
            cipher.SelectWheel(cipher.NextWheelIndex());

        if (Input.GetKey(KeyCode.UpArrow))
            cipher.MoveActiveWheel(-1);

        if (Input.GetKey(KeyCode.DownArrow))
            cipher.MoveActiveWheel(1);

        if (Input.GetKey(KeyCode.Q))
            cipher.MoveWheel(0, -1);

        if (Input.GetKey(KeyCode.A))
            cipher.MoveWheel(0, 1);

        if (Input.GetKey(KeyCode.W))
            cipher.MoveWheel(1, -1);

        if (Input.GetKey(KeyCode.S))
            cipher.MoveWheel(1, 1);

        if (Input.GetKey(KeyCode.E))
            cipher.MoveWheel(2, -1);

        if (Input.GetKey(KeyCode.D))
            cipher.MoveWheel(2, 1);

        if (Input.GetKey(KeyCode.R))
            cipher.MoveWheel(3, -1);

        if (Input.GetKey(KeyCode.F))
            cipher.MoveWheel(3, 1);

        if (Input.GetKey(KeyCode.T))
            cipher.MoveWheel(4, -1);

        if (Input.GetKey(KeyCode.G))
            cipher.MoveWheel(4, 1);

        if (Input.GetKey(KeyCode.Y))
            cipher.MoveWheel(5, -1);

        if (Input.GetKey(KeyCode.H))
            cipher.MoveWheel(5, 1);
    }
}