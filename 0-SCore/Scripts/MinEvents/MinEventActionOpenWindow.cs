using Platform;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
public class MinEventActionOpenWindow : MinEventActionRemoveBuff
{
    private string window;
    //  		<triggered_effect trigger="onSelfDamagedBlock" action="OpenWindow, SCore" window="SCoreCompanionsGroup"/>
    public override void Execute(MinEventParams _params)
    {
        if (_params.Self is not EntityPlayerLocal player) return;
        
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player);
        uiforPlayer.windowManager.OpenIfNotOpen(window, true);
    }
    
    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;
        if (name != "window") return true;
        window = _attribute.Value;
        return true;

    }
}