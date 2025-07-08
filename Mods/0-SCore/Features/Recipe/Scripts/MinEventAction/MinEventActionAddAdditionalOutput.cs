using System.Xml;
using System.Xml.Linq;
using UnityEngine;

//        <triggered_effect trigger = "onSelfBuffUpdate" action="AddAdditionalOutput, SCore" target="selfAOE" range="4" buff="buffAnimalFertility"  />
//        <triggered_effect trigger = "onSelfBuffUpdate" action="AddAdditionalOutput, SCore" target="selfAOE" range="4" mustmatch="true" buff="buffAnimalFertility"  />
public class MinEventActionAddAdditionalOutput : MinEventActionBuffModifierBase
{
    private string _createItem = string.Empty;
    private int _createItemCount = 1;
    
    public override void Execute(MinEventParams _params)
    {
        
    }

    public ItemStack GetItemStack()
    {
        if ( string.IsNullOrEmpty(_createItem))
            return ItemStack.Empty;
        var item= ItemClass.GetItem(_createItem);
        return new ItemStack(item, _createItemCount);
    }
    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;

        switch (name)
        {
            case "item":
                _createItem = _attribute.Value;
                return true;
            case "count":
                _createItemCount = (int)StringParsers.ParseFloat(_attribute.Value);
                return true;
            default:
                return false;
        }
    }
}