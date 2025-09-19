using System;
using System.Xml.Linq;
using SCore.Features.ItemDegradation.Harmony;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;
//            <triggered_effect trigger="onSelfItemDegrade" action="DegradeItemValueMod, SCore"/>
//            <triggered_effect trigger="onSelfItemDegrade" action="DegradeItemValueMod, SCore" DegradeOveride="50"/>
public class MinEventActionDegradeItemValueMod: MinEventActionTargetedBase
{
    private int _degradeOverride = 0;

    public override void Execute(MinEventParams _params)
    {
        var player = _params.Self as EntityPlayerLocal;
        if (player == null) return;

        var itemValue = _params.ItemValue;
        if (itemValue == null) return;

        // Only Work on a mod.
     //   if (!itemValue.IsMod) return;
        ItemDegradationHelpers.CheckModification(itemValue, player, _degradeOverride);
        
    }
    
    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var handledByBase = base.ParseXmlAttribute(_attribute);
        if (handledByBase) return true;

        var localName = _attribute.Name.LocalName;

        if (localName == "DegradeOverride")
        {
            _degradeOverride = StringParsers.ParseSInt32(_attribute.Value);
        }

        return false;
    }
}