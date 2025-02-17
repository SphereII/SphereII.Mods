using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;


/*
<configs>
<append xpath="/entity_classes/entity_class[@name='playerMale']">
    <effect_group>
        <triggered_effect trigger="onSelfEnteredGame" action="AddScriptToArmour, SCore" transform="Camera" script="GlobalSnowEffect.GlobalSnow, BetterBiomeEffects"/>
    </effect_group>
</append>
</configs>
*/
//<triggered_effect trigger="onSelfBuffStart" action="AddScriptToArmour, SCore" transform="Head" script="whatever, SCore" /> 
public class MinEventActionAddScriptToArmour : MinEventActionBuffModifierBase
{
    private string _transform;
    private string _script;

    public override void Execute(MinEventParams _params)
    {
        var type = Type.GetType(_script);
        if (type == null)
        {
            Debug.Log($"No Such Script: {_script}");
            return;
        }
        
        if ( _params.Transform != null )
            Debug.Log($"Transform: {_params.Transform.name}");
        
        if (_params.Self == null || _params.Self.RootTransform == null) return;
        // int slotCount = _params.Self.equipment.GetSlotCount();
        // for (int j = 0; j < slotCount; j++)
        // {
        //     ItemValue slotItem = _params.Self.equipment.GetSlotItem(j);
        //     if (slotItem != null && slotItem.ItemClass != null && slotItem.ItemClass.SDCSData != null)
        //     {
        //         var childs = new List<Transform>();
        //         GetAllChildren(_params.Self.RootTransform, ref childs);
        //         foreach (var child in childs)
        //         {
        //             if (child.name != _transform) continue;
        //             var component = child.gameObject.GetComponent(_script);
        //             if (component != null)
        //             {
        //                 Debug.Log($"Already Exists {_script} to {child.name} for {_params.Self.EntityName}");
        //                 continue;
        //             }
        //
        //             Debug.Log($"Adding {_script} to {child.name} for {_params.Self.EntityName}");
        //             // child.gameObject.GetOrAddComponent<SphereCollider>();
        //             child.gameObject.AddComponent(type);
        //         }
        //
        //         base.Execute(_params);
        //     }
        //
        // }
    }

    public static void GetAllChildren(Transform parent, ref List<Transform> transforms)
            {
                foreach (Transform t in parent)
                {
                    transforms.Add(t);
                    GetAllChildren(t, ref transforms);
                }
            }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;

        switch (name)
        {
            case null:
                return flag;
            case "transform":
                _transform = _attribute.Value;
                return true;
            case "script":
                _script = _attribute.Value;
                return true;
            default:
                return false;
        }
    }
}