
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
/*
    <triggered_effect trigger="onSelfBuffUpdate" 
        action="AdjustTransformValues, SCore"  
        parent_transform="AK47"
        local_offset="-0.05607828,0.07183618,-0.02150292" 
        local_rotation="-3.98,-9.826,-5.901"
        <!-- Optional. Defaults to false -->
        debug="true"  
        />
*/
public class MinEventActionAdjustTransformValues : MinEventActionBuffModifierBase {
    private string _transform;
    private Vector3 _localOffset;
    private Quaternion _localRotation;
    
    // This is just used for debugging so the same message doesn't get spammed.
    private static string LastMessage = string.Empty;
    private bool _debug;

    private void DisplayLog(MinEventParams _params) {
        var message =
            $"{_params.Self.EntityName} : Adjusting {_transform}: Local Offset: {_localOffset} Local Rotation: {_localRotation}";
        if (LastMessage == message) return;
        LastMessage = message;
        Debug.Log(message);
    }

    public override void Execute(MinEventParams _params) {
        if (_params.Self != null || _params.Self.RootTransform != null)
        {
            var children = new List<Transform>();
            GetAllChildren(_params.Self.RootTransform, ref children);
            foreach (var child in children)
            {
                if (child.name != _transform) continue;
                if ( _debug)
                    DisplayLog(_params);
                child.localPosition = _localOffset;
                child.localRotation = _localRotation;
                break;
            }
        }

        base.Execute(_params);
    }

    public static void GetAllChildren(Transform parent, ref List<Transform> transforms) {
        foreach (Transform t in parent)
        {
            transforms.Add(t);
            GetAllChildren(t, ref transforms);
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute) {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;

        switch (name)
        {
            case null:
                return flag;
            case "parent_transform":
                _transform = _attribute.Value;
                return true;
            case "debug":
                _debug = StringParsers.ParseBool(_attribute.Value);
                return true;
            case "local_offset":
                _localOffset = StringParsers.ParseVector3(_attribute.Value);
                return true;
            case "local_rotation":
                var rotation = StringParsers.ParseVector3(_attribute.Value);
                var newQuaternion = new Quaternion();
                newQuaternion.Set(rotation.x, rotation.y, rotation.z,1);
                _localRotation = newQuaternion; 
                return true;

            default:
                return false;
        }
    }
}