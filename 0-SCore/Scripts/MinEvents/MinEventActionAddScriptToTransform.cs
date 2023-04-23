using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Xml;
using UnityEngine;


/*
<configs>
<append xpath="/entity_classes/entity_class[@name='playerMale']">
    <effect_group>
        <triggered_effect trigger="onSelfEnteredGame" action="AddScriptToTransform, SCore" transform="Camera" script="GlobalSnowEffect.GlobalSnow, BetterBiomeEffects"/>
    </effect_group>
</append>
</configs>
*/

// <triggered_effect trigger="onSelfFirstSpawn" action="AddScriptToTransform, SCore" component="Animator" script="GlobalSnowEffect.GlobalSnowIgnoreCoverage, SphereII_Winter_Project"/>
// <triggered_effect trigger="onSelfFirstSpawn" action="AddScriptToTransform, SCore" component="RigidBody" script="GlobalSnowEffect.GlobalSnowIgnoreCoverage, SphereII_Winter_Project"/>
// <triggered_effect trigger="onSelfFirstSpawn" action="AddScriptToTransform, SCore" component="Renderer" script="GlobalSnowEffect.GlobalSnowIgnoreCoverage, SphereII_Winter_Project"/>
// <triggered_effect trigger="onSelfFirstSpawn" action="AddScriptToTransform, SCore" component="EntityAlive" script="GlobalSnowEffect.GlobalSnowIgnoreCoverage, SphereII_Winter_Project"/>

//<triggered_effect trigger="onSelfBuffStart" action="AddScriptToTransform, SCore" transform="Head" script="whatever, SCore" /> 
public class MinEventActionAddScriptToTransform : MinEventActionBuffModifierBase
{
    private string _transform;
    private string _script;
    private string _component;

    public override void Execute(MinEventParams _params)
    {
        var type = Type.GetType(_script);
        if (type == null)
        {
            Debug.Log($"No Such Script: {_script}");
            return;
        }

        if (_params.Self == null || _params.Self.RootTransform == null) return;

        var root = _params.Self.RootTransform;
        if (!string.IsNullOrEmpty(_transform))
            AddScriptToTransform(root, type);

        if (!string.IsNullOrEmpty(_component))
        {
            if (_component == "Animator")
            {
                foreach (var component in root.GetComponentsInChildren<Animator>())
                    CheckComponentForScript(component, type);
            }
        
            if (_component == "RigidBody")
            {
                foreach (var component in root.GetComponentsInChildren<Rigidbody>())
                    CheckComponentForScript(component, type);
            }
            if (_component == "EntityAlive")
            {
                foreach (var component in root.GetComponentsInChildren<EntityAlive>())
                    CheckComponentForScript(component, type);
            }
            
            if (_component == "Renderer")
            {
                foreach (var component in root.GetComponentsInChildren<Renderer>())
                    CheckComponentForScript(component, type);
            }
            
            if (_component == "Collider")
            {
                foreach (var component in root.GetComponentsInChildren<Collider>())
                    CheckComponentForScript(component, type);
            }
        }

        base.Execute(_params);
    }

    private void AddScriptToTransform(Transform root, Type type)
    {
        if (string.IsNullOrEmpty(_transform)) return;

        var childs = new List<Transform>();
        GetAllChildren(root, ref childs);
        foreach (var child in childs)
        {
            if (child.name != _transform) continue;
            
            var component = child.gameObject.GetComponent(_script);
            if (component != null) continue;

         //   Debug.Log($"Adding {_script} to {child.name}");
            child.gameObject.AddComponent(type);
            
            
        }
    }
    private void CheckComponentForScript(Component component, Type type)
    {
        var component2 = component.transform.gameObject.GetComponent(_script);
        if (component2 != null) return; // Already attached
      //  Debug.Log($"Adding {_script}");

        component.transform.gameObject.AddComponent(type);
    }

    private static void GetAllChildren(Transform parent, ref List<Transform> transforms)
    {
        foreach (Transform t in parent)
        {
            transforms.Add(t);
            GetAllChildren(t, ref transforms);
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name;

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
            case "component":
                _component = _attribute.Value;
                return true;
            default:
                return false;
        }
    }
}