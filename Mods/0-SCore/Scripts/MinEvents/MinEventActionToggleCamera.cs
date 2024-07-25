using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionToggleCamera : MinEventActionBuffModifierBase
{
    private string Camera = "Main Camera";

    private bool CameraStatus = true;

    //  <triggered_effect trigger="onSelfBuffStart" action="ToggleCamera, SCore" cameraName="Camera" value="false" />
    //  <triggered_effect trigger="onSelfBuffFinish" action="ToggleCamera, SCore" cameraName="Camera" value="true" />
    public override void Execute(MinEventParams _params)
    {
        foreach (var temp in Object.FindObjectsOfType<GameObject>())
            //        Debug.Log(temp.name);
            if (temp.GetComponent<Camera>() != null)
                if (Camera.Contains(temp.name))
                {
                    Debug.Log("Camera: " + temp.name);
                    temp.GetComponent<Camera>().enabled = CameraStatus;
                }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
            {
                if (name == "cameraName")
                {
                    Camera = _attribute.Value;
                    return true;
                }

                if (name == "value")
                {
                    CameraStatus = StringParsers.ParseBool(_attribute.Value);
                    return true;
                }
            }
        }

        return flag;
    }
}