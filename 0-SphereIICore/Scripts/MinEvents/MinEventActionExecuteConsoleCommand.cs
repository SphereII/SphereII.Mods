using System.Xml;

// Authored by Soleil Plein, and InnocuousChaos
// Example: <triggered_effect trigger="onSelfBuffStart" action="ExecuteConsoleCommand, Mods" command="st night"/>
public class MinEventActionExecuteConsoleCommand : MinEventActionBase
{
    string command;
    ClientInfo _cInfo;
    public override void Execute(MinEventParams _params)
    {
        if (command == null)
        {
            return;
        }
        else
        {
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, null);
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(GameManager.Instance.World.GetPrimaryPlayerId(), command), false);
            }
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool xmlAttribute = base.ParseXmlAttribute(_attribute);
        if (xmlAttribute || !(_attribute.Name == "command"))
            return xmlAttribute;
        this.command = _attribute.Value;
        return true;
    }
}