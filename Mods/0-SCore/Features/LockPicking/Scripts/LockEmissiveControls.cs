using UnityEngine;

public class LockEmissiveControls : MonoBehaviour
{
    public Renderer[] baseplate1;
    public Renderer[] baseplate2;
    public Renderer[] lock1;
    public Renderer[] lock2;
    public Renderer[] lock3;
    public Renderer[] button;
    public Renderer[] padlock1;
    public Renderer[] padlock2;

    public void SetBaseplate1(Texture newTexture)
    {
        for (var i = 0; i < baseplate1.Length; i++) baseplate1[i].material.SetTexture("_EmissionMap", newTexture);
    }

    public void SetBaseplate2(Texture newTexture)
    {
        for (var i = 0; i < baseplate2.Length; i++) baseplate2[i].material.SetTexture("_EmissionMap", newTexture);
    }

    public void SetLock1(Texture newTexture)
    {
        for (var i = 0; i < lock1.Length; i++) lock1[i].material.SetTexture("_EmissionMap", newTexture);
    }

    public void SetLock2(Texture newTexture)
    {
        for (var i = 0; i < lock2.Length; i++) lock2[i].material.SetTexture("_EmissionMap", newTexture);
    }

    public void SetLock3(Texture newTexture)
    {
        for (var i = 0; i < lock3.Length; i++) lock3[i].material.SetTexture("_EmissionMap", newTexture);
    }

    public void SetButton(Texture newTexture)
    {
        for (var i = 0; i < button.Length; i++) button[i].material.SetTexture("_EmissionMap", newTexture);
    }

    public void SetPadlock1(Texture newTexture)
    {
        for (var i = 0; i < padlock1.Length; i++) padlock1[i].material.SetTexture("_EmissionMap", newTexture);
    }

    public void SetPadlock2(Texture newTexture)
    {
        for (var i = 0; i < padlock2.Length; i++) padlock2[i].material.SetTexture("_EmissionMap", newTexture);
    }
}