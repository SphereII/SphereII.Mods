using System.Xml.Linq;

// "Skill Level Up! You are a better Demolitions Expert!
// <triggered_effect trigger="onSelfBuffStart" action="ShowPerkLevelUp, SCore" perk="perkPummelPete"  sound="read_skillbook_final" />

public class MinEventActionShowPerkLevelUp: MinEventActionShowToolbeltMessage
{
    private string _perkName;
    public override void Execute(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(_perkName)) return;

        foreach (var t in this.targets)
        {
            if (t is not EntityPlayerLocal player) continue;
            message = $"{Localization.Get("LearnByDoingLevelUp")} {Localization.Get($"{_perkName}Name", true)}!";
            if (sound != null)
            {
                GameManager.ShowTooltip(player, message, string.Empty, this.sound);
            }
            else
            {
                GameManager.ShowTooltip(player, message);
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var localName = _attribute.Name.LocalName;
            if (localName == "perk")
            {
                _perkName = _attribute.Value.ToLower();
                return true;
            }
        }

        return flag;
    }

}