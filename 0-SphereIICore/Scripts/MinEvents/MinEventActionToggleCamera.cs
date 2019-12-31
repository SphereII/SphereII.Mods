using System.Xml;
using UnityEngine;
public class MinEventActionToggleCamera: MinEventActionRemoveBuff
{
    string Camera = "Main Camera";
    bool CameraStatus = true;
    //  <triggered_effect trigger="onSelfBuffStart" action="ToggleCamera, Mods" cameraName="Camera" value="false" />
    //  <triggered_effect trigger="onSelfBuffFinish" action="ToggleCamera, Mods" cameraName="Camera" value="true" />
    public override void Execute(MinEventParams _params)
    {
        foreach (GameObject temp in GameObject.FindObjectsOfType<GameObject>())
        {
    //        Debug.Log(temp.name);
            if (temp.GetComponent<Camera>() != null)
            {
                if (Camera.Contains(temp.name))
                {
                    Debug.Log("Camera: " + temp.name);
                    temp.GetComponent<Camera>().enabled = CameraStatus;
                }
            }
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "cameraName" )
                {
                    Camera = _attribute.Value;
                    return true;
                }
                if (name == "value")
                {
                    CameraStatus = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
                    return true;
                }

            }
        }
        return flag;
    }
}
