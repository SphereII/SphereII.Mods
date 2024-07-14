using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionAnimatorSetFloatSDX : MinEventActionTargetedBase
{
    private string _property;
    private float _value;
    private bool _cvarRef;
    private string _refCvarName = string.Empty;

    public override void Execute(MinEventParams @params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            if (targets[i].emodel == null || this.targets[i].emodel.avatarController == null) continue;
            if (_cvarRef)
            {
                _value = targets[i].Buffs.GetCustomVar(_refCvarName, 0f);
            }
            targets[i].emodel.avatarController.UpdateFloat(_property, _value, true);
        }
    }

    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag) return true;
        var localName = attribute.Name.LocalName;
        switch (localName)
        {
            case "property":
                _property = attribute.Value;
                return true;
            case "value":
            {
                if (attribute.Value.StartsWith("@"))
                {
                    _cvarRef = true;
                    _refCvarName = attribute.Value.Substring(1);
                }
                else
                {
                    _value = StringParsers.ParseFloat(attribute.Value, 0, -1, NumberStyles.Any);
                }
                return true;
            }
        }
        return false;
    }
}